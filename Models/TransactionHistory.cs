using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Models
{
    public class TransactionHistory
    {
        public List<TransactionDetails> Data { get; set; }
        public string Token { get; set; }
        public int ResponseCode { get; set; }
        public PageInformation PageInformation { get; set; }
    }

    public class PageInformation
    {
        public int RecordsOnPage { get; set; }
        public int RecordsAll { get; set; }
        public int PageCount { get; set; }
    }

    public class Detail
    {
        // For 'transaction' type
        public int SettledAmount { get; set; }
        public long OrderId { get; set; }
        public string SessionId { get; set; }
        public int Status { get; set; }
        public string Date { get; set; }
        public string DateOfTransaction { get; set; }
        public string DateOfVerification { get; set; }
        public string ClientEmail { get; set; }
        public string AccountChecksum { get; set; }
        public int? PaymentMethod { get; set; }
        public string Description { get; set; }
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string ClientCity { get; set; }
        public string ClientZip { get; set; }
        public string Statement { get; set; }
        public int? Fee { get; set; }

        // For 'refund' type
        public int? RefundId { get; set; }
        public string RequestId { get; set; }
        public string TransactionSessionId { get; set; }
        public string DateIn { get; set; }
        public string DateOut { get; set; }

        // For 'batch' type
        public string DateInBatch { get; set; }
        public string DateOutBatch { get; set; }
        public int? BatchId { get; set; }
        public int? Commission { get; set; }
        public int? Charge { get; set; }
        public int? Refund { get; set; }
    }


    public class TransactionDetails
    {
        public string Type { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public Detail Details { get; set; }
    }
}
