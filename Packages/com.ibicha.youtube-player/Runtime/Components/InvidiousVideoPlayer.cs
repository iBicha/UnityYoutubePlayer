using System.Collections.Generic;
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
        public enum ProxyVideoType
        {
            Never = 0,
            OnlyIfNeeded = 1,
            Always = 2
        }

        public InvidiousInstance InvidiousInstance;

        public string VideoId;

        [Tooltip("Whether to use a proxy to play the video. This is useful when a video gets geoblocked. Not used in WebGL builds.")]
        public ProxyVideoType ProxyVideo = ProxyVideoType.OnlyIfNeeded;

        [Tooltip("The itag of the video to play. If not set, 720p video will be played.")]
        public string Itag = null;

        /// <summary>
        /// VideoPlayer component associated with the current YoutubePlayer instance
        /// </summary>
        public VideoPlayer VideoPlayer { get; private set; }

        // Because video player can fail in some cases, we need to try multiple urls. E.g.
        // - 1. Video without proxying
        // - 2. If that fails, video with proxying
        // This list contains the urls that we will try to play, depending on the ProxyVideo setting.
        List<string> m_PlayingVideoUrls = new List<string>();
        int m_PlayingVideoUrlIndex;
        string m_StartedPlayingVideoId;
        string m_PlayingVideoId;

        void Awake()
        {
            VideoPlayer = GetComponent<VideoPlayer>();
            VideoPlayer.errorReceived += OnVideoError;
            VideoPlayer.started += OnVideoStarted;
        }

        async void OnEnable()
        {
            if (VideoPlayer.playOnAwake)
                await PlayVideoAsync();
        }

        public async Task PrepareVideoAsync(CancellationToken cancellationToken = default)
        {
            // TODO: use destroyCancellationToken in 2022.3

            m_PlayingVideoId = VideoId;
            m_PlayingVideoUrlIndex = 0;
            m_StartedPlayingVideoId = null;
            await PrepareVideoUrls(cancellationToken);

            VideoPlayer.source = VideoSource.Url;
            SetVideoPlayerUrl(m_PlayingVideoUrls[m_PlayingVideoUrlIndex]);
            await VideoPlayer.PrepareAsync(cancellationToken);
        }

        async Task PrepareVideoUrls(CancellationToken cancellationToken = default)
        {
            if (InvidiousInstance == null)
            {
                throw new System.InvalidOperationException("InvidiousInstance is not set");
            }

            m_PlayingVideoUrls.Clear();

            switch (ProxyVideo)
            {
                case ProxyVideoType.Never:
                    var videoUrl = await InvidiousInstance.GetVideoUrl(VideoId, false, Itag, cancellationToken);
                    m_PlayingVideoUrls.Add(videoUrl);
                    break;

                case ProxyVideoType.OnlyIfNeeded:
                    videoUrl = await InvidiousInstance.GetVideoUrl(VideoId, false, Itag, cancellationToken);
                    m_PlayingVideoUrls.Add(videoUrl);

#if UNITY_EDITOR || !UNITY_WEBGL
                    // WebGL ignores the proxyVideo parameter, and uses true anyway.
                    // So no need to add the url twice.
                    videoUrl = await InvidiousInstance.GetVideoUrl(VideoId, true, Itag, cancellationToken);
                    m_PlayingVideoUrls.Add(videoUrl);
#endif
                    break;

                case ProxyVideoType.Always:
                    videoUrl = await InvidiousInstance.GetVideoUrl(VideoId, true, Itag, cancellationToken);
                    m_PlayingVideoUrls.Add(videoUrl);
                    break;

                default:
                    throw new System.InvalidOperationException($"Unknown ProxyVideoType: {ProxyVideo}");
            }
        }

        void SetVideoPlayerUrl(string videoUrl)
        {
            // Resetting the same url restarts the video...
            if (VideoPlayer.url != videoUrl)
            {
                Debug.Log($"Setting video url to {videoUrl}");
                VideoPlayer.url = videoUrl;
            }
        }

        public async Task PlayVideoAsync(CancellationToken cancellationToken = default)
        {
            await PrepareVideoAsync(cancellationToken);
            VideoPlayer.Play();
        }

        async void OnVideoError(VideoPlayer source, string message)
        {
            if (m_StartedPlayingVideoId == m_PlayingVideoId)
            {
                // The video was successfully played, so ignore the error.
                // We do this because we do not have access to the exact error code,
                // but we want to capture 403 errors.
                // Instead, we rely on the fact that the video was successfully played.
                // Any other error that happens later (video decoding, slow internet, etc)
                // will not trigger trying a different url.
                return;
            }

            m_PlayingVideoUrlIndex++;
            if (m_PlayingVideoUrlIndex < m_PlayingVideoUrls.Count)
            {
                SetVideoPlayerUrl(m_PlayingVideoUrls[m_PlayingVideoUrlIndex]);
                await VideoPlayer.PrepareAsync();
                VideoPlayer.Play();
            }
            else
            {
                Debug.LogError($"Error playing video {m_PlayingVideoId}: {message}");
            }
        }

        void OnVideoStarted(VideoPlayer source)
        {
            m_StartedPlayingVideoId = m_PlayingVideoId;
        }

        void OnDestroy()
        {
            VideoPlayer.errorReceived -= OnVideoError;
            VideoPlayer.started -= OnVideoStarted;
        }
    }
}
