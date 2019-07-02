using NBitcoin;
using Crypto_Services.ExternalService.BlockCypher;
using Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Enums;
using Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Services.CryptoService
{
    public class BTCLib
    {
        public string GetNewDepositAddress(string masterKey, string network, long addressIndex)
        {
            Network selectedNetwork = GetNetwork(network);
            BitcoinExtKey exkey = new BitcoinExtKey(ExtKey.Parse(masterKey, selectedNetwork), selectedNetwork);
            ExtKey addressKey1 = exkey.ExtKey.Derive(new KeyPath(@"m/44'/0'/0'/0/" + addressIndex));
            string address = addressKey1.PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy,selectedNetwork).ToString();
            // string privatekey = GetPrivateKey(masterKey, network, addressIndex);
            return address;
        }

        public dynamic CreateNewHDWallet(string walletName, string network, string passPhrase)
        {
            Network btcNetwork = null;


            if (network.ToLower() == "main")
                btcNetwork = Network.Main;
            else
                btcNetwork = Network.TestNet;


            Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Fifteen);
            ExtKey hdroot = mnemo.DeriveExtKey(passPhrase);
            var wif = hdroot.GetWif(btcNetwork);

            dynamic response = new ExpandoObject();
            response.wif = wif;
            response.mnemo = mnemo;
            response.btcNetwork = btcNetwork;

            return response;
        }

        private Network GetNetwork(string selectedNetwork)
        {
            Network network = Network.TestNet;
            switch (selectedNetwork.ToLower())
            {
                case "main":
                    network = Network.Main;
                    break;
            }
            return network;
        }

        public string GetPrivateKey(string masterKey, string network, long addressIndex)
        {
            Network selectedNetwork = GetNetwork(network);
            BitcoinExtKey exkey = new BitcoinExtKey(ExtKey.Parse(masterKey, selectedNetwork), selectedNetwork);
            ExtKey addressKey1 = exkey.ExtKey.Derive(new KeyPath(@"m/44'/0'/0'/0/" + addressIndex));
            return addressKey1.PrivateKey.GetWif(selectedNetwork).ToString();
        }

        public UnsignedTransaction SendTransactionToExchage(dynamic transactionDto, string toAddress, string privateKey, string network, string token)
        {
            try
            {
                var btcnetwork = GetNetwork(network);
                Endpoint endpoint = Endpoint.BtcTest3;
                if (network.ToUpper() == "MAIN")
                {
                    endpoint = Endpoint.BtcMain;
                }
                Blockcypher blockCypherLib = new Blockcypher(token, endpoint);
                BitcoinSecret secret = new BitcoinSecret(privateKey, btcnetwork);
                NBitcoin.Transaction tx = NBitcoin.Transaction.Create(btcnetwork);
                TxIn input = new TxIn
                {
                    PrevOut = new OutPoint(new uint256(transactionDto.TransactionID), transactionDto.Vout),
                    ScriptSig = secret.GetAddress(ScriptPubKeyType.Legacy).ScriptPubKey
                };
                tx.Inputs.Add(input);

                TxOut output = new TxOut();
                Money fee = Money.Coins(transactionDto.DefaultFees.GetValueOrDefault());
                Money totalAmount = Money.Coins(transactionDto.Amount);
                output.Value = totalAmount - fee;
                BitcoinAddress toaddress = BitcoinAddress.Create(toAddress, btcnetwork);
                output.ScriptPubKey = toaddress.ScriptPubKey;
                tx.Outputs.Add(output);

                var coins = tx.Inputs.Select(txin => new Coin(txin.PrevOut, new TxOut { ScriptPubKey = txin.ScriptSig }));
                tx.Sign(secret, coins.ToArray());
                var response = blockCypherLib.PushTransaction(tx.ToHex()).Result;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
    }

}
