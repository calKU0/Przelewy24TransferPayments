using Microsoft.Extensions.Options;
using Przelewy24TransferPayments.Contracts.Clients;
using Przelewy24TransferPayments.Contracts.DTOs;
using Przelewy24TransferPayments.Contracts.Settings;
using System.Net.Http.Json;
using System.Text.Json;

namespace Przelewy24TransferPayments.Infrastructure.Clients
{
    public class Przelewy24ApiClient : IPrzelewy24ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public Przelewy24ApiClient(HttpClient httpClient, IOptions<Przelewy24ApiSettings> options)
        {
            _httpClient = httpClient;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }
        public async Task<DispatchTransactionResult> DispatchTransaction(DispachTransactionRequest request)
        {
            string endpoint = $"api/v1/multiStore/dispatchTransaction";
            var response = await PostDataAsync<DispatchTransactionResult>(endpoint, request);

            return response;
        }

        public async Task<MerchantExistsResponse> GetMerchant(MerchantExistsRequest request)
        {
            string endpoint = $"api/v1/merchant/exists/{request.IdentificationType}/{request.IdentificationNumber}";
            var response = await GetDataAsync<MerchantExistsResponse>(endpoint);
            return response;
        }

        public async Task<List<TransactionDetails>> GetTransacions(GetTransactionsRequest request)
        {
            string endpoint = $"api/v1/report/history?dateFrom={request.DateFrom}&dateTo={request.DateTo}";
            if (!string.IsNullOrEmpty(request.Type))
            {
                endpoint += $"&type={request.Type}";
            }

            var response = await GetDataAsync<TransactionHistory>(endpoint);
            return response.Data;
        }

        private async Task<T> GetDataAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        private async Task<T> PostDataAsync<T>(string endpoint, object requestBody, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }
    }
}
