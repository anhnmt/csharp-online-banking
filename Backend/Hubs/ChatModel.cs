using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backend.Hubs
{
    public class ChatModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("fromId")]
        public int? FromId { get; set; }

        [JsonProperty("toId")]
        public int? ToId { get; set; }
    }
}