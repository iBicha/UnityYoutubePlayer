using Newtonsoft.Json;

namespace YoutubePlayer
{
    public class YoutubeVideoMetaData
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("ext")]
        public string Extension;

    }
}