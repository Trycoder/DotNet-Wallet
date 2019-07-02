using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Crypto_Common
{
    public static class UtilHelper
    {
        static string _key = "TstcC0!n5L@k3b@";
        static string _salt = "C0!bDeen@";

        public static void CopyProperties<T1, T2>(T1 src, ref T2 des)
        {
            if (src != null)
            {
                if (des == null)
                {
                    des = Activator.CreateInstance<T2>();
                }

                PropertyInfo[] srcProps = src.GetType().GetProperties();
                PropertyInfo[] desProps = des.GetType().GetProperties();
                foreach (PropertyInfo prop in srcProps)
                {
                    if (prop.PropertyType.IsValueType || prop.PropertyType.Name == "String")
                    {
                        PropertyInfo desProp = desProps.FirstOrDefault(p => p.Name.ToLower() == prop.Name.ToLower());
                        if (desProp != null)
                        {
                            if (prop.PropertyType.Name == "Int64" && desProp.PropertyType.Name == "String")
                            {
                                desProp.SetValue(des, prop.GetValue(src).ToString());
                            }
                            else if (prop.PropertyType.Name == "String" && desProp.PropertyType.Name == "Int64")
                            {
                                desProp.SetValue(des, Convert.ToInt64(prop.GetValue(src)));
                            }

                            else
                            {
                                desProp.SetValue(des, prop.GetValue(src));
                            }
                        }
                    }
                }
            }

        }

        public static string GetTwoPhrase()
        {
            string[] phrases = { "aback" , "abase", "boon", "boot", "bop", "born", "bort","bot", "bow", "bowl", "boy", "bran", "bred", "brit", "clew", "clip", "clod", "clog", "clot", "clue", "co", "coat",
            "code", "cogs", "col", "des", "dry", "drys", "dubs", "due", "dun", "duo", "dusk", "dust", "duty", "dx", "dye", "ear", "earn", "eat", "gun",
            "guy", "hen", "heat", "ice", "ink", "joe", "jam", "james", "kit", "kat", "kite", "kin", "kith", "low", "life", "lie", "mid", "no", "pen",
            "quick", "rain", "sit", "toe", "too", "vin", "woo", "xmas", "yoo", "zoo"};

            string pos = GenerateRandomNos(2);

            string twoPhrase = phrases[pos[0]] + ' ' + phrases[pos[1]];
            return twoPhrase;
        }

        public static string GenerateRandomNos(int textLength)
        {
            string randomText = string.Empty;
            string digits = "0123456789"; ;
            int index = 0;
            Random rnd = new Random();

            for (int i = randomText.Length; i < textLength; i++)
            {
                index = rnd.Next(0, digits.Length - 1);
                randomText += digits[index];
            }

            return randomText;
        }

        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailaddress))
                {
                    return false;
                }
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsValidCryptoCurrency(string currency)
        {
            bool isValid = false;

            try
            {
                if (!string.IsNullOrEmpty(currency))
                {
                    string[] currencies = typeof(CryptoCurrencyTypes).GetFields()
                          .Select(a => a.GetRawConstantValue()
                          .ToString()).ToArray();

                    string result = currencies.FirstOrDefault(x => x.ToLower().Trim() == currency.ToLower().Trim());
                    if (!string.IsNullOrEmpty(result))
                    {
                        isValid = true;
                    }
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }

        public static string EscapeXml(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            }
            return text;
        }

        public static string EncryptQueryString(string inputText)
        {
            byte[] plainText = Encoding.UTF8.GetBytes(inputText);

            using (RijndaelManaged rijndaelCipher = new RijndaelManaged())
            {
                PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(_key), Encoding.ASCII.GetBytes(_salt));
                using (ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainText, 0, plainText.Length);
                            cryptoStream.FlushFinalBlock();
                            string base64 = Convert.ToBase64String(memoryStream.ToArray());

                            // Generate a string that won't get screwed up when passed as a query string.
                            string urlEncoded = HttpUtility.UrlEncode(base64);
                            return urlEncoded;
                        }
                    }
                }
            }
        }

        public static string DecryptQueryString(string inputText)
        {
            byte[] encryptedData = Convert.FromBase64String(inputText);
            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(_key), Encoding.ASCII.GetBytes(_salt));

            using (RijndaelManaged rijndaelCipher = new RijndaelManaged())
            {
                using (ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                {
                    using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            byte[] plainText = new byte[encryptedData.Length];
                            cryptoStream.Read(plainText, 0, plainText.Length);
                            string utf8 = Encoding.UTF8.GetString(plainText);
                            return utf8;
                        }
                    }
                }
            }
        }

        public static string GetClientIpAddress(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return IPAddress.Parse(((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress).ToString();
            }
            if (request.Properties.ContainsKey("MS_OwinContext"))
            {
                return IPAddress.Parse(((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress).ToString();
            }
            return String.Empty;
        }

        public static dynamic GetDynamicFromJSON(string json)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            dynamic obj = serializer.Deserialize(json, typeof(object));
            return obj;
        }

        public static DateTime GetAEDTTimeFromUTC(DateTime utcTime)
        {
            TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            bool isDaylight = tst.IsDaylightSavingTime(utcTime);
            if (isDaylight)
            {
                return utcTime.AddHours(11);
            }
            else
            {
                return utcTime.AddHours(10);
            }
        }

        public static string GetFullFormCurrencyCode(string shortForm)
        {
            switch (shortForm.ToUpper().Trim())
            {
                case CryptoCurrencyShortForm.BTC:
                    return CryptoCurrencyTypes.BITCOIN;
                case CryptoCurrencyShortForm.ETH:
                    return CryptoCurrencyTypes.ETH;
                case CryptoCurrencyShortForm.LTC:
                    return CryptoCurrencyTypes.LTC;
                case CryptoCurrencyShortForm.BCH:
                    return CryptoCurrencyTypes.BCH;
                case CryptoCurrencyShortForm.XRP:
                    return CryptoCurrencyTypes.XRP;
                case CryptoCurrencyShortForm.PLA:
                    return CryptoCurrencyTypes.PLA;
                default:
                    return string.Empty;
            }
        }

        public static string GetShortFormCurrencyCode(string FullForm)
        {
            switch (FullForm.ToUpper().Trim())
            {
                case CryptoCurrencyTypes.BITCOIN:
                    return CryptoCurrencyShortForm.BTC;
                case CryptoCurrencyTypes.ETH:
                    return CryptoCurrencyShortForm.ETH;
                case CryptoCurrencyTypes.LTC:
                    return CryptoCurrencyShortForm.LTC;
                case CryptoCurrencyTypes.BCH:
                    return CryptoCurrencyShortForm.BCH;
                case CryptoCurrencyTypes.XRP:
                    return CryptoCurrencyShortForm.XRP;
                case CryptoCurrencyTypes.PLA:
                    return CryptoCurrencyShortForm.PLA;
                default:
                    return string.Empty;
            }
        }

        public static bool IsValidCryptoShortForm(string currrencyType)
        {

            bool isValid = false;

            try
            {
                if (!string.IsNullOrEmpty(currrencyType))
                {
                    string[] currencies = typeof(CryptoCurrencyShortForm).GetFields()
                          .Select(a => a.GetRawConstantValue()
                          .ToString()).ToArray();

                    string result = currencies.FirstOrDefault(x => x == currrencyType);
                    if (!string.IsNullOrEmpty(result))
                    {
                        isValid = true;
                    }
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;

        }

        public static bool IsValidCurrencyShortForm(string currrencyType)
        {

            bool isValid = false;

            try
            {
                if (!string.IsNullOrEmpty(currrencyType))
                {
                    string[] currencies = typeof(CryptoCurrencyShortForm).GetFields()
                          .Select(a => a.GetRawConstantValue()
                          .ToString()).ToArray();

                    string result = currencies.FirstOrDefault(x => x == currrencyType);
                    if (!string.IsNullOrEmpty(result))
                    {
                        isValid = true;
                    }
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;

        }

        public static string GetDevice()
        {
            HttpContext ctx = HttpContext.Current;
            string deviceName = string.Empty;

            string u = ctx.Request.ServerVariables["HTTP_USER_AGENT"];
            Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if ((b.IsMatch(u) || v.IsMatch(u.Substring(0, 4))))
            {
                deviceName = "Mobile";
            }
            else
            {
                deviceName = "Web";
            }

            return deviceName;
        }

        public static string ToComputeHash(this Guid guid)
        {
            byte[] source = guid.ToByteArray();
            var encoder = new SHA256Managed();
            byte[] encoded = encoder.ComputeHash(source);
            return Convert.ToBase64String(encoded);
        }

        public static string ToEncoded(this string inputString)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(inputString));
        }

        public static string ToDecoded(this string encodedString)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
        }

        public static bool IsValidCurrencyTypesShortForm(string currencyShortForm)
        {
            bool isValid = false;

            try
            {
                if (!string.IsNullOrEmpty(currencyShortForm))
                {
                    string[] currencies = typeof(CryptoCurrencyShortForm).GetFields()
                          .Select(a => a.GetRawConstantValue()
                          .ToString()).ToArray();

                    string result = currencies.FirstOrDefault(x => x.ToLower().Trim() == currencyShortForm.ToLower().Trim());
                    if (!string.IsNullOrEmpty(result))
                    {
                        isValid = true;
                    }
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }
       
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string EncryptEmailUserName(string email)
        {
            string result = string.Empty;
            if (!string.IsNullOrWhiteSpace(email))
            {
                List<string> data = email.Split('@').ToList();
                if (data.Count == 2)
                {
                    result = ComputeSha256Hash(data[0]) + "@" + data[1];
                }
            }
            return result;
        }

        //public static List<APIResponseDTO> GetIPLocation(List<string> IPs)
        //{
        //    List<APIResponseDTO> locations = new List<APIResponseDTO>();
        //    ParameterDTO key = ParameterHelper.GetParameterByName(Parameters.IP_LOC_KEY);
        //    string url = "http://ip-api.com/batch/?key=" + key.Value + "&fields=country,city,query";
        //    string payLoad = "[";
        //    string response = string.Empty;
        //    IPs = IPs.Distinct().ToList();
        //    int requestCount = 0;

        //    for (int index = 1; index <= IPs.Count; index++)
        //    {
        //        payLoad += "{\"query\": \"" + IPs[index - 1] + "\"},";
        //        if (index % 100 == 0 || index == IPs.Count)
        //        {
        //            payLoad = payLoad.Remove(payLoad.Length - 1);
        //            payLoad += "]";
        //            response = APIHelper.Post(url, payLoad);
        //            payLoad = "[";
        //            requestCount++;
        //            dynamic locs = GetDynamicFromJSON(response);
        //            if (response != null)
        //            {
        //                locations.AddRange(((IEnumerable<dynamic>)locs).Select(x => new APIResponseDTO()
        //                {
        //                    Status = x.query,
        //                    Message = x["city"] != null && x["country"] != null ? x.city.ToString() + "," + x.country.ToString() : null
        //                }).ToList());
        //            }

        //            if (requestCount % 10 == 0)
        //            {
        //                System.Threading.Thread.Sleep(5000);
        //            }

        //        }
        //    }
        //    return locations;
        //}

        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public static string EncryptGUID(Guid MID)
        {
            return ToEncoded(MID.ToString());
        }

        public static Guid DecryptGUID(string MIDStr)
        {
            return Guid.Parse(ToDecoded(MIDStr));
        }
    }
}
