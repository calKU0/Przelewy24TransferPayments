using Przelewy24TransferPayments.Contracts.DTOs;

namespace Przelewy24TransferPayments.Contracts.Clients
{
    public interface IPrzelewy24ApiClient
    {
        Task<MerchantExistsResponse> GetMerchant(MerchantExistsRequest request);
        Task<List<TransactionDetails>> GetTransacions(GetTransactionsRequest request);
        Task<DispatchTransactionResult> DispatchTransaction(DispachTransactionRequest request);
    }
}
