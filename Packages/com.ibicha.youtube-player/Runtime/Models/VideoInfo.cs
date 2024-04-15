using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubePlayer.Models
{
    public class VideoInfo
    {
        [JsonProperty("videoId")]
        public string VideoId { get; set;}

        [JsonProperty("title")]
        public string Title { get; set;}

        [JsonProperty("description")]
        public string Description { get; set;}

        [JsonProperty("published")]
        public long Published { get; set;}

        [JsonProperty("publishedText")]
        public string PublishedText { get; set;}

        [JsonProperty("viewCount")]
        public long ViewCount { get; set;}

        [JsonProperty("lengthSeconds")]
        public int LengthSeconds { get; set;}

        [JsonProperty("author")]
        public string Author { get; set;}

        [JsonProperty("authorId")]
        public string AuthorId { get; set;}

        [JsonProperty("videoThumbnails")]
        public List<VideoThumbnailInfo> VideoThumbnails { get; set;}

        [JsonProperty("formatStreams")]
        public List<VideoFormatInfo> FormatStreams { get; set;}

        [JsonProperty("adaptiveFormats")]
        public List<VideoFormatInfo> AdaptiveFormats { get; set;}

        public override string ToString()
        {
            return $"VideoId: {VideoId}, Title: {Title}";
        }
    }
}
