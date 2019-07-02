using Crypto_Common;
using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Hex.HexConvertors.Extensions;

using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Services.CryptoService
{
    public class ETHLib
    {
        public const string Path = "m/44'/60'/0'/0/x";

        public dynamic CreateNewHDWallet(string walletName, string passPhrase)
        {
            Wallet wallet = new Wallet(Wordlist.English, WordCount.Fifteen, passPhrase);
            dynamic response = new ExpandoObject();
            var masterKey = new ExtKey(wallet.Seed);
            var wif = masterKey.PrivateKey.ToBytes().ToHex();
            response.wif = wif;
            response.mnemo = string.Join(" ", wallet.Words);
            return response;

        }

        public string GetNewDepositAddress(string mneumonicWords, string passphrase, long addressIndex)
        {
            var ethECKey = GetECKeyPair(mneumonicWords, passphrase, addressIndex);
            string address = ethECKey.GetPublicAddress();
            return address;
        }

        public string GetPrivateKey(string mneumonicWords, string passphrase, long addressIndex)
        {
            var ethECKey = GetECKeyPair(mneumonicWords, passphrase, addressIndex);
            string privateKey = ethECKey.GetPrivateKey();
            return privateKey;
        }

        private EthECKey GetECKeyPair(string mneumonicWords, string passphrase, long addressIndex)
        {
            Mnemonic mneumonic = new Mnemonic(mneumonicWords);
            string seed = mneumonic.DeriveSeed(passphrase).ToHex();
            ExtKey masterKey = new ExtKey(seed);
            NBitcoin.KeyPath keyPath = new NBitcoin.KeyPath(GetIndexPath(addressIndex));
            ExtKey childkey = masterKey.Derive(keyPath);
            byte[] privateKeyBytes = childkey.PrivateKey.ToBytes();
            EthECKey ethECKey = new EthECKey(privateKeyBytes, true);
            return ethECKey;
        }

        public Account GetAccount(string mneumonicWords, string passphrase, long addressIndex)
        {
            Wallet wallet = new Wallet(mneumonicWords, passphrase);
            var account = wallet.GetAccount(Convert.ToInt32(addressIndex));
            return account;
        }

        private string GetIndexPath(long index)
        {
            return Path.Replace("x", index.ToString());
        }

        public dynamic SendEtherToExchange(dynamic transactionDto, dynamic walletDto, string toAddress)
        {
            try
            {
                dynamic obj = null;
                string url = GetRPCUrl(walletDto.Network);
                string mKey = CryptoLib.Decrypt(walletDto.MasterKey);
                string mneumonicWords = CryptoLib.Decrypt(walletDto.Mnemonic);
                string passphrase = CryptoLib.Decrypt(walletDto.PassPhrase);
                var account = GetAccount(mneumonicWords, passphrase, transactionDto.AddressIndex);
                Web3 web3 = new Web3(account, url);
                var gasPrice = Nethereum.Signer.Transaction.DEFAULT_GAS_PRICE;
                // var gasPrice = Web3.Convert.ToWei(1.5, UnitConversion.EthUnit.Gwei);
                var gasLimit = Nethereum.Signer.Transaction.DEFAULT_GAS_LIMIT;
                var balance = web3.Eth.GetBalance.SendRequestAsync(account.Address).Result;

                //TODO: Check fee and use default fee
                var fee = gasPrice * gasLimit;
                var maxfee = Web3.Convert.ToWei(transactionDto.MaxFee.GetValueOrDefault(), UnitConversion.EthUnit.Ether);

                if (fee >= maxfee)
                {
                    fee = Web3.Convert.ToWei(transactionDto.DefaultFees.GetValueOrDefault(), UnitConversion.EthUnit.Ether);
                }
                var amountToSend = balance - fee;
                var amountToSendInEther = UnitConversion.Convert.FromWei(amountToSend, 18);

                TransactionReceipt receipt = web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(toAddress, amountToSendInEther, UnitConversion.Convert.FromWei(gasPrice, 9), gasLimit).Result;
                if (receipt != null)
                {
                    if (receipt.Status.Value == 1)
                    {
                        obj = new ExpandoObject();
                        obj.TransactionHash = receipt.TransactionHash;
                        obj.Fees = UnitConversion.Convert.FromWei(fee, 18);
                        obj.Amount = amountToSendInEther;
                        obj.TotalAmount = UnitConversion.Convert.FromWei(balance, 18);
                    }
                }
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        private string GetRPCUrl(string network)
        {
            var projectId =EtherRPC.PROJECT_ID;
            string url = EtherRPC.TESTNET;
            if (network.ToLower() == "main")
            {
                url = EtherRPC.MAINNET;

            }
            return url.Replace("id", projectId);
        }

        public decimal ConvertEthFromWei(long weiAmount)
        {
            return UnitConversion.Convert.FromWei(weiAmount, 18);
        }
    }
}
