using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Interfaces
{
    public interface IDatabaseService
    {
        Task<bool> IsTransactionTransfered(long orderId);
    }
}
