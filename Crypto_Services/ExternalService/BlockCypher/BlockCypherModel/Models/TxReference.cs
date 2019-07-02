using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class TxReference : BaseObject
    {
        [JsonProperty("block_height")]
        public int BlockHeight { get; set; }

        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }

        [JsonProperty("confirmed")]
        public DateTime Confirmed { get; set; }

        [JsonProperty("double_spend")]
        public bool DoubleSpend { get; set; }

        [JsonProperty("spent")]
        public bool Spent { get; set; }

        [JsonProperty("spent_by")]
        public string SpentBy { get; set; }

        [JsonProperty("tx_hash")]
        public string TxHash { get; set; }

        [JsonProperty("tx_input_n")]
        public int TxInputN { get; set; }

        [JsonProperty("tx_output_n")]
        public int TxOutputN { get; set; }

        [JsonProperty("value")]
        public Satoshi Value { get; set; }
    }
}
