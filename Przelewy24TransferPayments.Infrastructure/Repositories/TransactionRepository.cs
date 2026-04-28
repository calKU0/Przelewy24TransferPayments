using Przelewy24TransferPayments.Contracts.Repositories;
using Przelewy24TransferPayments.Infrastructure.Data;

namespace Przelewy24TransferPayments.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IDbExecutor _db;

        public TransactionRepository(IDbExecutor db)
        {
            _db = db;
        }

        public async Task AddTransaction(long orderId, string sessionId, int merchantId, long amount, string error = null)
        {
            const string sql = @"INSERT INTO [kkur].[Przelewy24Logs]
                    ([OrderId], [SessionId], [Amount], [Currency], [MerchantId], [Success], [ErrorMessage])
                    VALUES (@OrderId, @SessionId, @Amount, @Currency, @MerchantId, @Success, @ErrorMessage)";
            var result = await _db.ExecuteAsync(sql, new
            {
                OrderId = orderId,
                SessionId = sessionId,
                Amount = amount,
                Currency = "PLN",
                MerchantId = merchantId,
                Success = string.IsNullOrEmpty(error) ? 1 : 0,
                ErrorMessage = string.IsNullOrEmpty(error) ? (object)DBNull.Value : error
            });
        }

        public async Task<bool> IsTransactionTransfered(long orderId)
        {
            const string sql = @"SELECT 1 FROM [kkur].[Przelewy24Logs] WHERE OrderId = @OrderId AND Success = 1";
            var result = await _db.QuerySingleOrDefaultAsync<int?>(sql, new { OrderId = orderId });
            return result.HasValue;
        }
    }
}
