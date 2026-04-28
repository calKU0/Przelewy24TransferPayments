namespace Przelewy24TransferPayments.Contracts.DTOs
{
    public class DispachTransactionRequest
    {
        public int BatchId { get; set; }
        public List<DispachTransactionRequestDetails> Details { get; set; }
    }

    public class DispachTransactionRequestDetails
    {
        public long OrderId { get; set; }
        public string SessionId { get; set; }
        public int SellerId { get; set; }
        public int Amount { get; set; }
    }
}
