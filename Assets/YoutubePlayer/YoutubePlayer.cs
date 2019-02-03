using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using YoutubeExplode;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubePlayer
{
    [RequireComponent(typeof(VideoPlayer))]
    public class YoutubePlayer : MonoBehaviour
    {
        public string youtubeUrl;
        
        private VideoPlayer videoPlayer;
        private YoutubeClient youtubeClient;
        
        private void Awake()
        {
            youtubeClient = new YoutubeClient();
            videoPlayer = GetComponent<VideoPlayer>();
        }

        private async void OnEnable()
        {
            if(videoPlayer.playOnAwake)
                await PlayVideoAsync(youtubeUrl);
        }

        public async Task PlayVideoAsync(string url)
        {
            try
            {
                if (!YoutubeClient.TryParseVideoId(url, out var videoId))
                    throw new ArgumentException("Invalid youtube url", nameof(url));
               
                youtubeUrl = url;
            
                var streamInfoSet = await youtubeClient.GetVideoMediaStreamInfosAsync(videoId);
                var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
                if (streamInfo == null)
                    throw new NotSupportedException($"No muxed streams in youtube video '{url}'");
                
                videoPlayer.source = VideoSource.Url;
                if (videoPlayer.url != streamInfo.Url)
                {
                    videoPlayer.url = streamInfo.Url;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public async Task<ClosedCaptionTrack> DownloadClosedCaptions(string videoId = null)
        {
            if (string.IsNullOrEmpty(videoId))
            {
                if (!YoutubeClient.TryParseVideoId(youtubeUrl, out videoId))
                    return null;
            }
            
            var trackInfos = await youtubeClient.GetVideoClosedCaptionTrackInfosAsync(videoId);
            if (trackInfos?.Count == 0)
                return null;
            
            var trackInfo = trackInfos.FirstOrDefault(t => t.Language.Code == "en") ?? trackInfos.First();
            return await youtubeClient.GetClosedCaptionTrackAsync(trackInfo);
        }
        
    }
}
