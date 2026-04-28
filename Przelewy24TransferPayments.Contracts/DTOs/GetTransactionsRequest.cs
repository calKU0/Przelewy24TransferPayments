namespace Przelewy24TransferPayments.Contracts.DTOs
{
    public class GetTransactionsRequest
    {
        public string DateFrom { get; set; } = string.Empty;
        public string DateTo { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
