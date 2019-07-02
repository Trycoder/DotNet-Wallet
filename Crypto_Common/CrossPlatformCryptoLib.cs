using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Common
{
    public enum Schema : short { V0, V1, V2, V3 };
    public enum AesMode : short { CTR, CBC };
    public enum Pbkdf2Prf : short { SHA1 };
    public enum HmacAlgorithm : short { SHA1, SHA256 };
    public enum Algorithm : short { AES };
    public enum Options : short { V0, V1 };

    public struct PayloadComponents
    {
        public byte[] schema;
        public byte[] options;
        public byte[] salt;
        public byte[] hmacSalt;
        public byte[] iv;
        public int headerLength;
        public byte[] hmac;
        public byte[] ciphertext;
    };

    abstract public class Cryptor
    {
        protected AesMode aesMode;
        protected Options options;
        protected bool hmac_includesHeader;
        protected bool hmac_includesPadding;
        protected HmacAlgorithm hmac_algorithm;

        protected const Algorithm algorithm = Algorithm.AES;
        protected const short saltLength = 8;
        protected const short ivLength = 16;
        protected const Pbkdf2Prf pbkdf2_prf = Pbkdf2Prf.SHA1;
        protected const int pbkdf2_iterations = 10000;
        protected const short pbkdf2_keyLength = 32;
        protected const short hmac_length = 32;

        /// <summary>
        ///Gets or sets the Encoding
        /// </summary>
        public Encoding TextEncoding { set; get; }

        public Cryptor()
        {
            // set default encoding for UTF8
            TextEncoding = Encoding.UTF8;
        }

        protected void configureSettings(Schema schemaVersion)
        {
            switch (schemaVersion)
            {
                case Schema.V0:
                    aesMode = AesMode.CTR;
                    options = Options.V0;
                    hmac_includesHeader = false;
                    hmac_includesPadding = true;
                    hmac_algorithm = HmacAlgorithm.SHA1;
                    break;

                case Schema.V1:
                    aesMode = AesMode.CBC;
                    options = Options.V1;
                    hmac_includesHeader = false;
                    hmac_includesPadding = false;
                    hmac_algorithm = HmacAlgorithm.SHA256;
                    break;

                case Schema.V2:
                case Schema.V3:
                    aesMode = AesMode.CBC;
                    options = Options.V1;
                    hmac_includesHeader = true;
                    hmac_includesPadding = false;
                    hmac_algorithm = HmacAlgorithm.SHA256;
                    break;
            }
        }

        protected byte[] generateHmac(PayloadComponents components, string password)
        {
            List<byte> hmacMessage = new List<byte>();
            if (this.hmac_includesHeader)
            {
                hmacMessage.AddRange(this.assembleHeader(components));
            }
            hmacMessage.AddRange(components.ciphertext);

            byte[] key = this.generateKey(components.hmacSalt, password);

            HMAC hmacAlgo = null;
            switch (this.hmac_algorithm)
            {
                case HmacAlgorithm.SHA1:
                    hmacAlgo = new HMACSHA1(key);
                    break;

                case HmacAlgorithm.SHA256:
                    hmacAlgo = new HMACSHA256(key);
                    break;
            }
            List<byte> hmac = new List<byte>();
            hmac.AddRange(hmacAlgo.ComputeHash(hmacMessage.ToArray()));

            if (this.hmac_includesPadding)
            {
                for (int i = hmac.Count; i < Cryptor.hmac_length; i++)
                {
                    hmac.Add(0x00);
                }
            }

            return hmac.ToArray();
        }

        protected byte[] assembleHeader(PayloadComponents components)
        {
            List<byte> headerBytes = new List<byte>();
            headerBytes.AddRange(components.schema);
            headerBytes.AddRange(components.options);
            headerBytes.AddRange(components.salt);
            headerBytes.AddRange(components.hmacSalt);
            headerBytes.AddRange(components.iv);

            return headerBytes.ToArray();
        }

        protected byte[] generateKey(byte[] salt, string password)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Cryptor.pbkdf2_iterations);
            return pbkdf2.GetBytes(Cryptor.pbkdf2_keyLength);
        }

        protected byte[] encryptAesCtrLittleEndianNoPadding(byte[] plaintextBytes, byte[] key, byte[] iv)
        {
            byte[] counter = this.computeAesCtrLittleEndianCounter(plaintextBytes.Length, iv);
            byte[] encrypted = this.encryptAesEcbNoPadding(counter, key);
            return this.bitwiseXOR(plaintextBytes, encrypted);
        }

        private byte[] computeAesCtrLittleEndianCounter(int payloadLength, byte[] iv)
        {
            byte[] incrementedIv = new byte[iv.Length];
            iv.CopyTo(incrementedIv, 0);

            int blockCount = (int)Math.Ceiling((decimal)payloadLength / (decimal)iv.Length);

            List<byte> counter = new List<byte>();

            for (int i = 0; i < blockCount; ++i)
            {
                counter.AddRange(incrementedIv);

                // Yes, the next line only ever increments the first character
                // of the counter string, ignoring overflow conditions.  This
                // matches CommonCrypto's behavior!
                incrementedIv[0]++;
            }

            return counter.ToArray();
        }

        private byte[] encryptAesEcbNoPadding(byte[] plaintext, byte[] key)
        {
            byte[] encrypted;

            var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            var encryptor = aes.CreateEncryptor(key, null);

            using (var ms = new MemoryStream())
            {
                using (var cs1 = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs1.Write(plaintext, 0, plaintext.Length);
                }
                encrypted = ms.ToArray();
            }
            return encrypted;
        }

        private byte[] bitwiseXOR(byte[] first, byte[] second)
        {
            byte[] output = new byte[first.Length];
            ulong klen = (ulong)second.Length;
            ulong vlen = (ulong)first.Length;
            ulong k = 0;
            ulong v = 0;
            for (; v < vlen; v++)
            {
                output[v] = (byte)(first[v] ^ second[k]);
                k = (++k < klen ? k : 0);
            }
            return output;
        }

        protected string hex_encode(byte[] input)
        {
            string hex = "";
            foreach (byte c in input)
            {
                hex += String.Format("{0:x2}", c);
            }
            return hex;
        }

    }

    public class Encryptor : Cryptor
    {
        private Schema defaultSchemaVersion = Schema.V2;

        public string Encrypt(string plaintext, string password)
        {
            return this.Encrypt(plaintext, password, this.defaultSchemaVersion);
        }

        public string Encrypt(string plaintext, string password, Schema schemaVersion)
        {
            this.configureSettings(schemaVersion);

            byte[] plaintextBytes = TextEncoding.GetBytes(plaintext);

            PayloadComponents components = new PayloadComponents();
            components.schema = new byte[] { (byte)schemaVersion };
            components.options = new byte[] { (byte)this.options };
            components.salt = this.generateRandomBytes(Cryptor.saltLength);
            components.hmacSalt = this.generateRandomBytes(Cryptor.saltLength);
            components.iv = this.generateRandomBytes(Cryptor.ivLength);

            byte[] key = this.generateKey(components.salt, password);

            switch (this.aesMode)
            {
                case AesMode.CTR:
                    components.ciphertext = this.encryptAesCtrLittleEndianNoPadding(plaintextBytes, key, components.iv);
                    break;

                case AesMode.CBC:
                    components.ciphertext = this.encryptAesCbcPkcs7(plaintextBytes, key, components.iv);
                    break;
            }

            components.hmac = this.generateHmac(components, password);

            List<byte> binaryBytes = new List<byte>();
            binaryBytes.AddRange(this.assembleHeader(components));
            binaryBytes.AddRange(components.ciphertext);
            binaryBytes.AddRange(components.hmac);

            return Convert.ToBase64String(binaryBytes.ToArray());
        }

        private byte[] encryptAesCbcPkcs7(byte[] plaintext, byte[] key, byte[] iv)
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            var encryptor = aes.CreateEncryptor(key, iv);

            byte[] encrypted;

            using (var ms = new MemoryStream())
            {
                using (var cs1 = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs1.Write(plaintext, 0, plaintext.Length);
                }

                encrypted = ms.ToArray();
            }

            return encrypted;
        }

        private byte[] generateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);

            return randomBytes;
        }
    }

    public class Decryptor : Cryptor
    {

        public string Decrypt(string encryptedBase64, string password)
        {
            PayloadComponents components = this.unpackEncryptedBase64Data(encryptedBase64);

            if (!this.hmacIsValid(components, password))
            {
                return "";
            }

            byte[] key = this.generateKey(components.salt, password);

            byte[] plaintextBytes = new byte[0];

            switch (this.aesMode)
            {
                case AesMode.CTR:
                    // Yes, we are "encrypting" here.  CTR uses the same code in both directions.
                    plaintextBytes = this.encryptAesCtrLittleEndianNoPadding(components.ciphertext, key, components.iv);
                    break;

                case AesMode.CBC:
                    plaintextBytes = this.decryptAesCbcPkcs7(components.ciphertext, key, components.iv);
                    break;
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        private byte[] decryptAesCbcPkcs7(byte[] encrypted, byte[] key, byte[] iv)
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            var decryptor = aes.CreateDecryptor(key, iv);

            string plaintext;
            using (MemoryStream msDecrypt = new MemoryStream(encrypted))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }

            return TextEncoding.GetBytes(plaintext);
        }

        private PayloadComponents unpackEncryptedBase64Data(string encryptedBase64)
        {
            List<byte> binaryBytes = new List<byte>();
            binaryBytes.AddRange(Convert.FromBase64String(encryptedBase64));

            PayloadComponents components;
            int offset = 0;

            components.schema = binaryBytes.GetRange(0, 1).ToArray();
            offset++;

            this.configureSettings((Schema)binaryBytes[0]);

            components.options = binaryBytes.GetRange(1, 1).ToArray();
            offset++;

            components.salt = binaryBytes.GetRange(offset, Cryptor.saltLength).ToArray();
            offset += components.salt.Length;

            components.hmacSalt = binaryBytes.GetRange(offset, Cryptor.saltLength).ToArray();
            offset += components.hmacSalt.Length;

            components.iv = binaryBytes.GetRange(offset, Cryptor.ivLength).ToArray();
            offset += components.iv.Length;

            components.headerLength = offset;

            components.ciphertext = binaryBytes.GetRange(offset, binaryBytes.Count - Cryptor.hmac_length - components.headerLength).ToArray();
            offset += components.ciphertext.Length;

            components.hmac = binaryBytes.GetRange(offset, Cryptor.hmac_length).ToArray();

            return components;

        }

        private bool hmacIsValid(PayloadComponents components, string password)
        {
            byte[] generatedHmac = this.generateHmac(components, password);

            if (generatedHmac.Length != components.hmac.Length)
            {
                return false;
            }

            for (int i = 0; i < components.hmac.Length; i++)
            {
                if (generatedHmac[i] != components.hmac[i])
                {
                    return false;
                }
            }
            return true;
        }

    }

    public class CryptoLib
    {
        //TODO : Key to encrypt and decrypt
        static string key = "Deen@09Au5tr@l1a";
        public static string Encrypt(string text)
        {
            string encryptedText = string.Empty;

            Encryptor encryptor = new Encryptor();
            encryptedText = encryptor.Encrypt(text, key);

            return encryptedText;
        }

        public static string Decrypt(string text)
        {
            string decryptedText = string.Empty;

            Decryptor decryptor = new Decryptor();
            decryptedText = decryptor.Decrypt(text, key);

            return decryptedText;
        }
    }
}
