using Przelewy24TransferPayments.Interfaces;
using Przelewy24TransferPayments.Logging;
using Przelewy24TransferPayments.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Przelewy24TransferPayments.Services
{
    public class Przelewy24Service : IPrzelewy24Service
    {
        private readonly HttpClient _client = HttpClientService.Client;
        public Przelewy24Service() { }

        public async Task<MerchantExistsResponse> GetMerchant(MerchantExistsRequest merchant)
        {
            var result = new MerchantExistsResponse();
            string endpoint = $"merchant/exists/{merchant.IdentificationType}/{merchant.IdentificationNumber}";
            var segments = endpoint.Split('/');
            string firstTwoSegments = string.Join("_", segments.Take(2));

            try
            {
                var response = await _client.GetAsync(endpoint);
                string content = await response.Content.ReadAsStringAsync();

                ResponseLogger.SaveResponseToFile(content, firstTwoSegments);

                response.EnsureSuccessStatusCode();
                result = JsonSerializer.Deserialize<MerchantExistsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Wystąpił błąd przy próbie pobrania danych merchanta");
            }

            return result;
        }

        public async Task<DispatchTransactionResult> DispatchTransaction(List<DispachTransactionRequestDetails> details)
        {
            var result = new DispatchTransactionResult
            {
                Result = new List<DispatchTransactionResultDetails>()
            };

            string endpoint = $"multiStore/dispatchTransaction";

            try
            {
                int batchId = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % int.MaxValue);
                var request = new DispachTransactionRequest
                {
                    BatchId = batchId,
                    Details = details
                };

                var response = await _client.PostAsJsonAsync(endpoint, request);
                string content = await response.Content.ReadAsStringAsync();

                ResponseLogger.SaveResponseToFile(content, endpoint);

                try
                {
                    var deserialized = JsonSerializer.Deserialize<DispatchTransactionResult>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });

                    if (deserialized != null && deserialized.Result != null)
                    {
                        return deserialized;
                    }
                }
                catch (JsonException jsonEx)
                {
                    Log.Warning(jsonEx, "Deserialization failed. Response content may not be in expected format.");
                }

                result.Result.Add(new DispatchTransactionResultDetails
                {
                    OrderId = 0,
                    Amount = 0,
                    SessionId = "",
                    SellerId = 0,
                    Error = $"Failed to deserialize response. Status: {response.StatusCode}, Content: {content}"
                });
            }
            catch (Exception ex)
            {
                result.Result.Add(new DispatchTransactionResultDetails
                {
                    OrderId = 0,
                    Amount = 0,
                    SessionId = "",
                    SellerId = 0,
                    Error = $"Exception: {ex.Message}"
                });
            }

            return result;
        }

        public async Task<List<TransactionDetails>> GetTransacions(string dateFrom, string dateTo, string type = "")
        {
            var result = new TransactionHistory();
            string endpoint = $"report/history?dateFrom={dateFrom}&dateTo={dateTo}";
            var segments = endpoint.Split('?')[0].Split('/');
            string firstTwoSegments = string.Join("_", segments.Take(2));

            if (!string.IsNullOrEmpty(type))
            {
                endpoint += $"&type={type}";
            }

            try
            {
                var response = await _client.GetAsync(endpoint);
                string content = await response.Content.ReadAsStringAsync();

                ResponseLogger.SaveResponseToFile(content, firstTwoSegments);

                response.EnsureSuccessStatusCode();
                result = JsonSerializer.Deserialize<TransactionHistory>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Wystąpił błąd przy próbie pobrania histori transakcji");
            }

            return result.Data;
        }
    }
}
