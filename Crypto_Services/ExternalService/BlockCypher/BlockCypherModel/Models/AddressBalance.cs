using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class AddressBalance : BaseObject
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("balance")]
        public Satoshi Balance { get; set; }

        [JsonProperty("final_balance")]
        public Satoshi FinalBalance { get; set; }

        [JsonProperty("final_n_tx")]
        public int FinalTx { get; set; }

        [JsonProperty("n_tx")]
        public int TotalTransactions { get; set; }

        [JsonProperty("txrefs")]
        public IList<TxReference> Transactions { get; set; }

        [JsonProperty("tx_url")]
        public string TxUrl { get; set; }

        [JsonProperty("unconfirmed_balance")]
        public int UnconfirmedBalance { get; set; }

        [JsonProperty("unconfirmed_n_tx")]
        public int UnconfirmedTx { get; set; }
    }
}
