using Crypto_Services.ExternalService.BlockCypher;
using Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Enums;
using Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Services.CryptoService
{
    public class LTCLib
    {
        public string GetNewDepositAddress(string masterKey, string network, long addressIndex)
        {
            Network selectedNetwork = GetNetwork(network);
            BitcoinExtKey exkey = new BitcoinExtKey(ExtKey.Parse(masterKey, selectedNetwork), selectedNetwork);
            ExtKey addressKey1 = exkey.ExtKey.Derive(new KeyPath(@"m/44'/2'/0'/0/" + addressIndex));
            string address = addressKey1.PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy,selectedNetwork).ToString();
            // string privatekey = GetPrivateKey(masterKey, network, addressIndex);
            return address;
        }

        public dynamic CreateNewHDWallet(string walletName, string network, string passPhrase)
        {
            Network ltcNetwork = null;


            if (network.ToLower() == "main")
                ltcNetwork = NBitcoin.Altcoins.Litecoin.Instance.Mainnet;
            else
                ltcNetwork = NBitcoin.Altcoins.Litecoin.Instance.Testnet;


            Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Fifteen);
            ExtKey hdroot = mnemo.DeriveExtKey(passPhrase);
            var wif = hdroot.GetWif(ltcNetwork);

            dynamic response = new ExpandoObject();
            response.wif = wif;
            response.mnemo = mnemo;

            response.Network = network.ToLower() == "main" ? "Main" : "TestNet";

            return response;
        }

        private Network GetNetwork(string selectedNetwork)
        {
            Network network = NBitcoin.Altcoins.Litecoin.Instance.Testnet;
            switch (selectedNetwork.ToLower())
            {
                case "main":
                    network = NBitcoin.Altcoins.Litecoin.Instance.Mainnet;
                    break;
            }
            return network;
        }

        public string GetPrivateKey(string masterKey, string network, long addressIndex)
        {
            Network selectedNetwork = GetNetwork(network);
            BitcoinExtKey exkey = new BitcoinExtKey(ExtKey.Parse(masterKey, selectedNetwork), selectedNetwork);
            ExtKey addressKey1 = exkey.ExtKey.Derive(new KeyPath(@"m/44'/2'/0'/0/" + addressIndex));
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
                    endpoint = Endpoint.LtcMain;
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
                Money fee = Money.Coins(transactionDto.DefaultFees / 100);
                Money totalAmount = Money.Coins(transactionDto.Amount / 100);
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

        public dynamic SendToTestNet(dynamic transactionDto, string toAddress, string privateKey, string network, string token)
        {

            try
            {
                var btcnetwork = GetNetwork(network);


                BitcoinSecret secret = new BitcoinSecret(privateKey, btcnetwork);
                NBitcoin.Transaction tx = NBitcoin.Transaction.Create(btcnetwork);
                TxIn input = new TxIn
                {
                    PrevOut = new OutPoint(new uint256(transactionDto.TransactionID), transactionDto.Vout),
                    ScriptSig = secret.GetAddress(ScriptPubKeyType.Legacy).ScriptPubKey
                };
                tx.Inputs.Add(input);

                TxOut output = new TxOut();
                Money fee = Money.Coins(transactionDto.DefaultFees);
                Money totalAmount = Money.Coins(transactionDto.Amount);
                output.Value = totalAmount - fee;
                BitcoinAddress toaddress = BitcoinAddress.Create(toAddress, btcnetwork);
                output.ScriptPubKey = toaddress.ScriptPubKey;
                tx.Outputs.Add(output);

                var coins = tx.Inputs.Select(txin => new Coin(txin.PrevOut, new TxOut { ScriptPubKey = txin.ScriptSig }));
                tx.Sign(secret, coins.ToArray());

                /* Push Data to So Chain*/
                string api = "https://chain.so/api/v2/send_tx/LTCTEST";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(api);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = HttpMethod.Post.Method;
                dynamic reqObj = new ExpandoObject();
                reqObj.tx_hex = tx.ToHex();
                string payload = JsonConvert.SerializeObject(reqObj);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(payload);
                    var ss = streamWriter.ToString();
                    streamWriter.Flush();

                }
                string responseData = string.Empty;
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    responseData = streamReader.ReadToEnd();
                }
                dynamic response = new ExpandoObject();
                if (!string.IsNullOrWhiteSpace(responseData))
                {
                    response = JsonConvert.DeserializeObject(responseData);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

    }
}
