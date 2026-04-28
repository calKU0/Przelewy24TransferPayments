using Microsoft.Extensions.Options;
using Przelewy24TransferPayments.Service.Services;
using Przelewy24TransferPayments.Service.Settings;

namespace Przelewy24TransferPayments.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppSettings _appSettings;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _appSettings = appSettings.Value;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker starting at: {Time}", DateTime.Now);
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var interval = TimeSpan.FromSeconds(_appSettings.WorkerIntervalSeconds);

                try
                {
                    _logger.LogInformation("Worker running at: {Time}", DateTime.Now);
                    using var scope = _scopeFactory.CreateScope();
                    var syncService = scope.ServiceProvider.GetRequiredService<Przelewy24Service>();

                    await syncService.TransferTransactions(stoppingToken);
                    _logger.LogInformation("Job executed at: {Time}", DateTime.Now);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker execution failed.");
                }
                finally
                {
                    await Task.Delay(interval, stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stopping at: {Time}", DateTime.Now);
            await base.StopAsync(cancellationToken);
        }
    }
}
