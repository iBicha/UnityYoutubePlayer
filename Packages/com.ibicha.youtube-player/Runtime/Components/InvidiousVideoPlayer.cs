using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using YoutubePlayer.Api;
using YoutubePlayer.Extensions;
using YoutubePlayer.Models;

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

        [Tooltip("Check for blocked streams by making a HEAD request. If a stream is blocked, it will try to play a different one.")]
        public bool CheckForBlockedStreams = true;

        /// <summary>
        /// VideoPlayer component associated with the current YoutubePlayer instance
        /// </summary>
        public VideoPlayer VideoPlayer { get; private set; }

        // Because video player can fail in some cases, we need to try multiple urls. E.g.
        // - 1. Video without proxying
        // - 2. If that fails, video with proxying
        // A queue contains the urls that we will try to play, depending on the ProxyVideo setting.

        struct VideoUrl
        {
            public string Url;
            public bool IsProxied;
        }

        Queue<VideoUrl> m_PlayingVideoUrls = new Queue<VideoUrl>();
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
            m_StartedPlayingVideoId = null;
            await PrepareVideoUrls(cancellationToken);

            VideoPlayer.source = VideoSource.Url;
            await TrySetVideoPlayerUrl();
            await VideoPlayer.PrepareAsync(cancellationToken);
        }

        async Task PrepareVideoUrls(CancellationToken cancellationToken = default)
        {
            m_PlayingVideoUrls.Clear();

            if (InvidiousInstance == null)
            {
                throw new InvalidOperationException("InvidiousInstance is not set");
            }

            var instanceUrl = await InvidiousInstance.GetInstanceUrl(cancellationToken);
            var videoInfo = await InvidiousApi.GetVideoInfo(instanceUrl, VideoId, cancellationToken);
            var format = GetCompatibleVideoFormat(videoInfo);

            var proxyVideo = ProxyVideo;

            if (!Application.isEditor && Application.platform == RuntimePlatform.WebGLPlayer)
            {
                proxyVideo = ProxyVideoType.Always;
            }

            switch (proxyVideo)
            {
                case ProxyVideoType.Never:
                    m_PlayingVideoUrls.Enqueue(new VideoUrl { Url = format.Url, IsProxied = false });
                    break;

                case ProxyVideoType.OnlyIfNeeded:
                    m_PlayingVideoUrls.Enqueue(new VideoUrl { Url = format.Url, IsProxied = false });
                    m_PlayingVideoUrls.Enqueue(new VideoUrl { Url = GetProxiedUrl(format.Url, instanceUrl), IsProxied = true });
                    break;

                case ProxyVideoType.Always:
                    m_PlayingVideoUrls.Enqueue(new VideoUrl { Url = GetProxiedUrl(format.Url, instanceUrl), IsProxied = true });
                    break;

                default:
                    throw new InvalidOperationException($"Unknown ProxyVideoType: {proxyVideo}");
            }
        }

        VideoFormatInfo GetCompatibleVideoFormat(VideoInfo videoInfo)
        {
            var itag = Itag;
            if (string.IsNullOrEmpty(itag))
            {
                // 720p, the highest quality available with the video and audio combined
                itag = "22";
            }

            var format = videoInfo.FormatStreams.Find(f => f.Itag == itag);
            if (format != null)
            {
                return format;
            }

            // Maybe we're looking for an adaptive format?
            format = videoInfo.AdaptiveFormats.Find(f => f.Itag == itag);
            if (format != null)
            {
                return format;
            }

            return videoInfo.FormatStreams.Last();
        }

        string GetProxiedUrl(string url, string invidiousUrl)
        {
            var uri = new Uri(url);
            var invidiousUri = new Uri(invidiousUrl);
            var builder = new UriBuilder(uri)
            {
                Host = invidiousUri.Host,
                Scheme = invidiousUri.Scheme,
                Port = invidiousUri.Port
            };
            return builder.Uri.ToString();
        }

        async Task<bool> TrySetVideoPlayerUrl()
        {
            if (!m_PlayingVideoUrls.TryDequeue(out var videoUrl))
            {
                return false;
            }

            if (CheckForBlockedStreams && !videoUrl.IsProxied)
            {
                Debug.Log($"Checking if stream is blocked: {videoUrl.Url}");
                if (await WebRequest.HeadAsync(videoUrl.Url) == 403)
                {
                    Debug.LogWarning($"Blocked stream: {videoUrl.Url}");
                    return await TrySetVideoPlayerUrl();
                }
            }

            // Resetting the same url restarts the video...
            if (VideoPlayer.url != videoUrl.Url)
            {
                Debug.Log($"Setting video url to {videoUrl.Url}");
                VideoPlayer.url = videoUrl.Url;
            }

            return true;
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

            if (await TrySetVideoPlayerUrl())
            {
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
