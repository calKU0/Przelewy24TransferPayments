using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Services
{
    public sealed class HttpClientService
    {
        private static readonly Lazy<HttpClient> _httpClientLazy = new Lazy<HttpClient>(() =>
        {
            try
            {
                string user = ConfigurationManager.AppSettings["ApiUser"];
                string secret = ConfigurationManager.AppSettings["ApiSecret"];
                string baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(baseUrl))
                    throw new InvalidOperationException("Missing API credentials or base URL in App.config");

                var client = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl)
                };

                // Basic Auth: Base64(user:secret)
                string authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{secret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                return client;
            }
            catch (ConfigurationErrorsException configEx)
            {
                Log.Error(configEx, "Configuration error while initializing HttpClient.");
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while initializing HttpClient.");
                throw;
            }
        });


        public static HttpClient Client => _httpClientLazy.Value;

        // Prevent instantiation
        private HttpClientService() { }
    }

}
