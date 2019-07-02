using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class BasicTransaction
    {
        [JsonProperty("inputs")]
        public IList<TxInput> Inputs { get; set; }

        [JsonProperty("outputs")]
        public IList<TxOutput> Outputs { get; set; }

        //[JsonProperty("fees")]
        //public Satoshi Fees { get; set; }
        [JsonProperty("preference")]
        public string Preference { get; set; }
        
    }
}
