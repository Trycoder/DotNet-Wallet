﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto_Services.ExternalService.BlockCypher.BlockCypherModel.Models
{
    public class BaseObject
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        public string[] Errors
        {
            get
            {
                if (Extra == null || !Extra.ContainsKey("errors"))
                    return null;

                return Extra["errors"].Select(a => a["error"].ToString()).ToArray();
            }
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> Extra { get; set; }

        public bool IsError
        {
            get { return !string.IsNullOrEmpty(Error) || Errors != null; }
        }
    }
}
