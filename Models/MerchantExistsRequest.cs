using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Models
{
    public class MerchantExistsRequest
    {
        public string IdentificationType { get; set; }

        public string IdentificationNumber { get; set; }
    }
}
