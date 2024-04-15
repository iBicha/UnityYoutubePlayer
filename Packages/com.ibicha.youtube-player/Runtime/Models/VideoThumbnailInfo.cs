using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubePlayer.Models
{
    public class VideoThumbnailInfo
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("quality")]
        public string Quality { get; set; }
    }
}
