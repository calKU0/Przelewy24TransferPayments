using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Models
{
    public class DispatchTransactionResult
    {
        public List<DispatchTransactionResultDetails> Result { get; set; }
        public Error Error { get; set; }
    }
    public class Error
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DispatchTransactionResultDetails
    {
        public long OrderId { get; set; }
        public long OrderIdNew { get; set; }
        public string SessionId { get; set; }
        public int SellerId { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
    }
}
