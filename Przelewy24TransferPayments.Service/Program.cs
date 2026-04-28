using Microsoft.Extensions.Options;
using Przelewy24TransferPayments.Contracts.Clients;
using Przelewy24TransferPayments.Contracts.Repositories;
using Przelewy24TransferPayments.Contracts.Settings;
using Przelewy24TransferPayments.Infrastructure.Clients;
using Przelewy24TransferPayments.Infrastructure.Data;
using Przelewy24TransferPayments.Infrastructure.Logging;
using Przelewy24TransferPayments.Infrastructure.Repositories;
using Przelewy24TransferPayments.Service;
using Przelewy24TransferPayments.Service.Constants;
using Przelewy24TransferPayments.Service.Services;
using Przelewy24TransferPayments.Service.Settings;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = ServiceConstants.ServiceName;
    })
    .UseSerilog((hostContext, _, loggerConfiguration) =>
    {
        loggerConfiguration.ConfigureServiceLogging(hostContext.Configuration, ServiceConstants.ServiceName);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Configuration
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<Przelewy24ApiSettings>(configuration.GetSection("Przelewy24ApiSettings"));

        // Database context
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddSingleton<IDbExecutor>(sp => new DapperDbExecutor(connectionString));

        // HttpClients
        services.AddHttpClient<IPrzelewy24ApiClient, Przelewy24ApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<Przelewy24ApiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            var byteArray = Encoding.ASCII.GetBytes($"{settings.User}:{settings.Secret}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        // Repositories
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Services
        services.AddScoped<Przelewy24Service>();

        // Background worker
        services.AddHostedService<Worker>();

        // Host options
        services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(15));
    })
    .Build();

host.Run();