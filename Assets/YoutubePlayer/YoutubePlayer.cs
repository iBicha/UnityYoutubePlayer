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
            if (!YoutubeClient.TryParseVideoId(url, out var videoId))
                return;

            youtubeUrl = url;
            
            var streamInfoSet = await youtubeClient.GetVideoMediaStreamInfosAsync(videoId);
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
          
            videoPlayer.source = VideoSource.Url;
            if (videoPlayer.url != streamInfo.Url)
            {
                videoPlayer.url = streamInfo.Url;
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
            var trackInfo = trackInfos.First(t => t.Language.Code == "en");
            return await youtubeClient.GetClosedCaptionTrackAsync(trackInfo);
        }
        
    }
}
