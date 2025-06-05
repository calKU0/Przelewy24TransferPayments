using Przelewy24TransferPayments.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Interfaces
{
    public interface IPrzelewy24Service
    {
        Task<MerchantExistsResponse> GetMerchant(MerchantExistsRequest merchant);
        Task<List<TransactionDetails>> GetTransacions(string dateFrom, string dateTo, string type = "");
        Task<DispatchTransactionResult> DispatchTransaction(List<DispachTransactionRequestDetails> request);
    }
}
