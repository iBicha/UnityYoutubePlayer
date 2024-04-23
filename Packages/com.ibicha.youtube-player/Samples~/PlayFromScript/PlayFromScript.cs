using UnityEngine;
using UnityEngine.Video;

namespace YoutubePlayer.Samples.PlayFromScript
{
    public class PlayFromScript : MonoBehaviour
    {
        async void Start()
        {
            const string videoId = "1PuGuqpHQGo";

            var go = new GameObject("InvidiousVideoPlayer");
            var videoPlayer = go.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = Camera.main;
            videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

            var invidiousInstance = go.AddComponent<YoutubePlayer.Components.InvidiousInstance>();
            // Important: make sure to choose the instance to use. For the sample to work, we are using a public instance.
            // To avoid additional latency, set the instance type to Custom and specify the url:
            // invidiousInstance.InstanceType = YoutubePlayer.Components.InvidiousInstance.InvidiousInstanceType.Custom;
            // invidiousInstance.CustomInstanceUrl = "https://my-invidious-instance.com";

            var invidiousVideoPlayer = go.AddComponent<YoutubePlayer.Components.InvidiousVideoPlayer>();
            invidiousVideoPlayer.InvidiousInstance = invidiousInstance;
            invidiousVideoPlayer.VideoId = videoId;
            await invidiousVideoPlayer.PlayVideoAsync();
        }
    }
}
