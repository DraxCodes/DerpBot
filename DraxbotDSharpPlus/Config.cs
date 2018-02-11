using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DraxbotDSharpPlus
{
    public class Config
    {
        [JsonProperty("bottoken")]
        public string BotToken { get; set; }
        [JsonProperty("prefix")]
        public string Prefix { get; set; }
    }
}
