using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class RawTransaction
    {
        [JsonProperty("tx")]
        public string TxHex { get; set; }
    }
}
