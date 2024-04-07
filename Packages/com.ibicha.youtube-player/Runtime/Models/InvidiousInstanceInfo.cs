using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YoutubePlayer.Extensions;

namespace YoutubePlayer.Models
{
    public class InvidiousInstanceInfo
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("flag")]
        public string Flag { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("cors")]
        public bool Cors { get; set; }

        [JsonProperty("api")]
        public bool Api { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonIgnore]
        public string Version { get; set; }

        [JsonIgnore]
        public int Users { get; set; }

        [JsonIgnore]
        public bool Signup { get; set; }

        [JsonIgnore]
        public string Health { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> m_AdditionalData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            var stats = m_AdditionalData["stats"];
            Version = stats.SelectToken("$.software.version").ToValidValue(JTokenType.String, "N/A");
            Users = stats.SelectToken("$.usage.users.total").ToValidValue(JTokenType.Integer, 0);
            Signup = stats.SelectToken("$.openRegistrations").ToValidValue(JTokenType.Boolean, false);

            var monitor = m_AdditionalData["monitor"];
            Health = monitor.SelectToken("$.30dRatio.ratio").ToValidValue(JTokenType.String, "N/A");

            m_AdditionalData = null;
        }

        public override string ToString()
        {
            return $"{Uri} ({Region}) - version {Version} - {Users} users - health: {Health}";
        }
    }
}
