using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class UnsignedTransaction : BaseObject
    {
        [JsonProperty("pubkeys")]
        public IList<string> PubKeys { get; set; }

        [JsonProperty("signatures")]
        public IList<string> Signatures { get; set; }

        [JsonProperty("tosign")]
        public IList<string> ToSign { get; set; }

        [JsonProperty("tx")]
        public UnsignedInner Transactions { get; set; }
    }
}
