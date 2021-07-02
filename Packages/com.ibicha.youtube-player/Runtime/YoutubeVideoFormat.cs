using Newtonsoft.Json;

namespace YoutubePlayer
{
    public class YoutubeVideoFormat
    {
        public YoutubeVideoFormat()
        {
        }

        [JsonProperty("format_id")]
        public string FormatId;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("ext")]
        public string Extension;

        [JsonProperty("width")]
        public int? Width;

        [JsonProperty("height")]
        public int? Height;

        [JsonProperty("fps")]
        public int? Fps;

        [JsonProperty("vcodec")]
        public string AudioCodec;

        [JsonProperty("acodec")]
        public string VideoCodec;
    }
}
