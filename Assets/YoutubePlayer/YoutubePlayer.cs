using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using YoutubeExplode.Models.ClosedCaptions;

namespace YoutubePlayer
{
    [RequireComponent(typeof(VideoPlayer))]
    public class YoutubePlayer : MonoBehaviour
    { 
        public int ServerPort;
        
        private VideoPlayer videoPlayer;
        private YoutubeServer youtubeServer;
        
        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }


        private void OnEnable()
        {
            //We start a server
            youtubeServer = new YoutubeServer(ServerPort);
            //And get the port back, in case we chose a random port
            ServerPort = youtubeServer.Port;
            
            //And feed that back to the player
            videoPlayer.url = GetLocalHostUrl(videoPlayer.url, ServerPort);
        }

        public float GetDownloadProgressForVideo(string videoId)
        {
            return youtubeServer.GetDownloadProgressForVideo(videoId);
        }

        public async Task<ClosedCaptionTrack> DownloadClosedCaptions(string videoId)
        {
            return await youtubeServer.DownloadClosedCaptions(videoId);
        }

        private static string GetLocalHostUrl(string url, int port)
        {
            var builder = new UriBuilder(new Uri(url));
            builder.Scheme = "http";
            builder.Host = "localhost";
            builder.Port = port;
            return builder.Uri.ToString();
        }
        
        private void OnDisable()
        {
            youtubeServer.Dispose();
        }
    }
}
