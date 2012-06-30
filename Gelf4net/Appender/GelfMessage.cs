using System;
using Newtonsoft.Json;

namespace Esilog.Gelf4net.Appender
{
    [JsonObject(MemberSerialization.OptIn)]
    class GelfMessage
    {
        [JsonProperty("facility")]
        public string Facility { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("full_message")]
        public string FullMesage { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("line")]
        public string Line { get; set; }

        [JsonProperty("short_message")]
        public string ShortMessage { get; set; }

        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}