using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Common
{
    public struct CryptoCurrencyShortForm
    {
        public const string BTC = "BTC";
        public const string LTC = "LTC";
        public const string ETH = "ETH";
        public const string XRP = "XRP";
        public const string BCH = "BCH";
        public const string ETC = "ETC";
        public const string OMG = "OMG";
        public const string PLA = "PLA";
    }
    public struct CryptoCurrencyTypes
    {
        public const string BITCOIN = "BITCOIN";
        public const string BCH = "BITCOINCASH";
        public const string ETH = "ETHEREUM";
        public const string LTC = "LITECOIN";
        public const string XRP = "RIPPLE";
        public const string PLA = "PLAYCHIP";
    }
    public struct EtherRPC
    {
        public const string PROJECT_ID = "77e813b071894ff6863a43e8e7d99c7e";
        public const string MAINNET = "https://mainnet.infura.io/v3/id";
        public const string TESTNET = "https://ropsten.infura.io/v3/id";

    }
}
