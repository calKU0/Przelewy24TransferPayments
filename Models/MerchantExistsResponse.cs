using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Models
{
    public class MerchantExistsResponse
    {
        public List<string> Data { get; set; }

        public string Error { get; set; }
    }
}
