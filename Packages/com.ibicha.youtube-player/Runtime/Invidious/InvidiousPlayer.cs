using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

namespace YoutubePlayer
{
    [RequireComponent(typeof(VideoPlayer))]
    public class InvidiousPlayer : MonoBehaviour
    {
        [Header("Automatically find an invidious instance")]
        public bool AutomaticInstance = true;

        [Header("Use a custom instance.")]
        [Header("Used if AutomaticInstance is set to false")]
        public string CustomInstance;

        [Header("Indicates the quality of the video")]
        public string Itag = "22";

        [Header("Stream the video through instance.")]
        [Header("Use if video is geoblocked. (fails to load)")]
        public bool UseProxy = false;

        [Header("Video identifier (not the full url)")]
        public string videoId;

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
            if (string.IsNullOrEmpty(Itag))
            {
                throw new ArgumentNullException(nameof(Itag));
            }
            if (string.IsNullOrEmpty(videoId))
            {
                throw new System.ArgumentNullException(nameof(videoId));
            }
            if (videoId.StartsWith("http"))
            {
                throw new ArgumentException("Please do not provide a link to the video, only the ID.", nameof(videoId));
            }


            var instance = "";
            if (AutomaticInstance)
            {
                instance = await InvidiousApi.GetDefaultInstanceAsync(cancellationToken);
            }
            else
            {
                instance = CustomInstance;
            }

            var url = InvidiousApi.GetVideoUrl(videoId, instance, Itag, UseProxy);

            VideoPlayer.source = VideoSource.Url;

            //Resetting the same url restarts the video...
            if (VideoPlayer.url != url)
                VideoPlayer.url = url;

            await VideoPlayer.PrepareAsync(cancellationToken);
        }

        public async Task PlayVideoAsync(CancellationToken cancellationToken = default)
        {
            await PrepareVideoAsync(cancellationToken);
            VideoPlayer.Play();
        }
    }
}
