using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using YoutubePlayer.Extensions;

namespace YoutubePlayer.Components
{
    [RequireComponent(typeof(VideoPlayer))]
    public class InvidiousVideoPlayer : MonoBehaviour
    {
        public InvidiousInstance InvidiousInstance;

        public string VideoId;

        [Tooltip("The itag of the video to play. If not set, 720p video will be played.")]
        public string Itag = null;

        /// <summary>
        /// VideoPlayer component associated with the current YoutubePlayer instance
        /// </summary>
        public VideoPlayer VideoPlayer { get; private set; }

        void Awake()
        {
            VideoPlayer = GetComponent<VideoPlayer>();
        }

        async void OnEnable()
        {
            if (VideoPlayer.playOnAwake)
                await PlayVideoAsync();
        }

        public async Task PrepareVideoAsync(CancellationToken cancellationToken = default)
        {
            if (InvidiousInstance == null)
            {
                throw new System.InvalidOperationException("InvidiousInstance is not set");
            }

            // TODO: use destroyCancellationToken in 2022.3

            // WebGL will not work because of YouTube's CORS. We need to use the proxy option.
            // TODO: WebGL will not work before CORS fix https://github.com/iv-org/invidious/pull/4571


            var proxyVideos = false;
#if UNITY_WEBGL && !UNITY_EDITOR
            var proxyVideos = true;
#endif
            var videoUrl = await InvidiousInstance.GetVideoUrl(VideoId, proxyVideos, Itag, cancellationToken);

            VideoPlayer.source = VideoSource.Url;

            //Resetting the same url restarts the video...
            if (VideoPlayer.url != videoUrl)
            {
                Debug.Log($"Setting video url to {videoUrl}");
                VideoPlayer.url = videoUrl;
            }

            await VideoPlayer.PrepareAsync(cancellationToken);
        }

        public async Task PlayVideoAsync(CancellationToken cancellationToken = default)
        {
            await PrepareVideoAsync(cancellationToken);
            VideoPlayer.Play();
        }
    }
}
