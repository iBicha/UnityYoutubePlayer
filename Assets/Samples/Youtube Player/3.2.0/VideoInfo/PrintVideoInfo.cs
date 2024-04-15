using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YoutubePlayer.Components;
using YoutubePlayer.Extensions;

namespace YoutubePlayer.Samples.VideoInfo
{
    public class PrintVideoInfo : MonoBehaviour
    {
        public InvidiousInstance InvidiousInstance;

        public string VideoId;

        public Image Thumbnail;

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

            Debug.Log("Loading thumbnail into texture");

            var thumbnail = videoInfo.VideoThumbnails.First(thumbnail => thumbnail.Quality == "medium");
            var texture = await thumbnail.ToTextureAsync();
            Thumbnail.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Thumbnail.preserveAspect = true;

            Debug.Log("Thumbnail loaded");

            Debug.Log("You can also access the thumbnail using: " + InvidiousInstance.GetVideoThumbnailUrl(VideoId));
        }

        void OnDestroy()
        {
            if (Thumbnail != null && Thumbnail.sprite != null)
            {
                Destroy(Thumbnail.sprite.texture);
                Destroy(Thumbnail.sprite);
            }
        }
    }
}
