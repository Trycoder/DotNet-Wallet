using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class SatoshiConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var types = new[] {
                typeof(int),
                typeof(long)
            };

            return types.Any(t => t == objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
                throw new ArgumentException(string.Format("Expected integer, got {0}", reader.TokenType));

            return new Satoshi
            {
                Value = (long)reader.Value
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((long)((Satoshi)value).Value);
        }
    }
}
