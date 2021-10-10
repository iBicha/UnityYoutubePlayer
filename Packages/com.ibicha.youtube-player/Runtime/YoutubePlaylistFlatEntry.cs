using Newtonsoft.Json;

namespace YoutubePlayer
{
    public class YoutubePlaylistFlatEntry
    {
        public YoutubePlaylistFlatEntry()
        {
        }

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("url")]
        public string Url;
    }
}
