namespace Przelewy24TransferPayments.Contracts.Repositories
{
    public interface ITransactionRepository
    {
        Task<bool> IsTransactionTransfered(long orderId);
        Task AddTransaction(long orderId, string sessionId, int merchantId, long amount, string error = null);
    }
}
