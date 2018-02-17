using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DerpBot
{
    public class Config
    {
        [JsonProperty("bottoken")]
        public string BotToken { get; set; }
        [JsonProperty("prefix")]
        public string Prefix { get; set; }
    }
}
