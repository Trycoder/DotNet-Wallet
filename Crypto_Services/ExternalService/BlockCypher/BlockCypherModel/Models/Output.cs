using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class Output
    {
        [JsonProperty("addresses")]
        public IList<string> Addresses { get; set; }

        [JsonProperty("script")]
        public string Script { get; set; }

        [JsonProperty("script_type")]
        public string ScriptType { get; set; }

        [JsonProperty("spent_by")]
        public string SpentBy { get; set; }

        [JsonProperty("value")]
        public Satoshi Value { get; set; }
    }
}
