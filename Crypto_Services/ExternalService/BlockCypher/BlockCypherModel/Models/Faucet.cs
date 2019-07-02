using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class Faucet : BaseObject
    {
        [JsonProperty("tx_ref")]
        public string TxReference { get; set; }
    }
}
