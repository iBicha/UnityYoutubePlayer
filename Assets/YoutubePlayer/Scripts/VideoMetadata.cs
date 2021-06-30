using Newtonsoft.Json;
using UnityEngine;

namespace YoutubePlayer
{
    public class VideoMetadata : MonoBehaviour
    {
        // This an example on how to extend the YoutubeVideoMetaData class to get more details about a video.
        // Information about the video is provided by youtube-dl.
        public class MyYoutubeVideoMetadata : YoutubeVideoMetaData
        {
            public MyYoutubeVideoMetadata() : base()
            {
            }

            [JsonProperty("description")]
            public string Description;
            
            [JsonProperty("view_count")]
            public long ViewCount;
        }

        public string videoUrl;

        async void Start()
        {
            Debug.Log("Loading...");
            var metadata = await YoutubeDl.GetVideoMetaDataAsync<MyYoutubeVideoMetadata>(videoUrl);
            Debug.Log($"Video description: {metadata.Description}");
            Debug.Log($"View count: {metadata.ViewCount}");
        }
    }
}