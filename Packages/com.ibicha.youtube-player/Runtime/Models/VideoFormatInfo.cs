using Newtonsoft.Json;

namespace YoutubePlayer.Models
{
    public class VideoFormatInfo
    {
        [JsonProperty("url")]
        public string Url { get; set;}

        [JsonProperty("itag")]
        public string Itag { get; set;}

        public override string ToString()
        {
            return $"Itag: {Itag}, Url: {Url}";
        }
    }
}
