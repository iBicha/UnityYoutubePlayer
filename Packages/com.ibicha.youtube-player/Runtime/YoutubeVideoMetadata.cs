using Newtonsoft.Json;

namespace YoutubePlayer
{
    public class YoutubeVideoMetaData
    {
        public YoutubeVideoMetaData()
        {
        }

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("ext")]
        public string Extension;

        [JsonProperty("requested_formats")]
        public YoutubeVideoFormat[] requestedFormats;

        [JsonProperty("formats")]
        public YoutubeVideoFormat[] Formats;

        [JsonProperty("_filename")]
        public string FileName;
    }
}
