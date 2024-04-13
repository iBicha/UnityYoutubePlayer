using UnityEngine;
using YoutubePlayer.Components;
using YoutubePlayer.Extensions;

namespace YoutubePlayer.Samples.VideoInfo
{
    public class PrintVideoInfo : MonoBehaviour
    {
        public InvidiousInstance InvidiousInstance;

        public string VideoId;

        async void Start()
        {
            Debug.Log("Fetching video info for video ID " + VideoId);
            var videoInfo = await InvidiousInstance.GetVideoInfo(VideoId);

            var infoString = $"Title: {videoInfo.Title}\n";
            infoString += $"Author: {videoInfo.Author}\n";
            infoString += $"Description: {videoInfo.Description}\n";
            infoString += $"Published: {videoInfo.PublishedText}\n";
            infoString += $"View count: {videoInfo.ViewCount}\n";
            infoString += $"Length: {videoInfo.LengthSeconds} seconds\n";

            Debug.Log(infoString);
        }
    }
}
