using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class TxOutput
    {
        [JsonProperty("addresses")]
        public IList<string> Addresses { get; set; }

        [JsonProperty("value")]
        public Satoshi Value { get; set; }
    }
}
