using Newtonsoft.Json;

namespace YoutubePlayer.Models
{
    public class VideoInfo
    {
        [JsonProperty("videoId")]
        public string VideoId { get; set;}

        [JsonProperty("title")]
        public string Title { get; set;}

        [JsonProperty("author")]
        public string Author { get; set;}

        [JsonProperty("authorId")]
        public string AuthorId { get; set;}

        public override string ToString()
        {
            return $"VideoId: {VideoId}, Title: {Title}";
        }
    }
}
