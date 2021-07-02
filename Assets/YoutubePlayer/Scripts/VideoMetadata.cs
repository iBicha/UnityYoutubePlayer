using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

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

        public Text Info;

        async void Start()
        {
            Log("Loading...");

            // Optimize by specifying the fields we're interested in, to avoid downloading everything.
            string[] fields = {"title", "description", "view_count"};

            var metadata = await YoutubeDl.GetVideoMetaDataAsync<MyYoutubeVideoMetadata>(videoUrl, fields);
            Log($"- Video title: {metadata.Title}");
            Log($"- Video description: {metadata.Description}");
            Log($"- View count: {metadata.ViewCount}");

            // You can also omit the fields, and the JsonProperty attributes will be used instead.
            // var metadata = await YoutubeDl.GetVideoMetaDataAsync<MyYoutubeVideoMetadata>(videoUrl);
        }

        void Log(string message)
        {
            Debug.Log(message);
            Info.text += message + '\n';
        }
    }
}
