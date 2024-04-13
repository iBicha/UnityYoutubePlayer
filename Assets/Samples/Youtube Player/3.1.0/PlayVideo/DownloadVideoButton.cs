using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YoutubePlayer.Components;
using YoutubePlayer.Extensions;

namespace YoutubePlayer.Samples.PlayVideo
{
    public class DownloadVideoButton : MonoBehaviour
    {
        public InvidiousVideoPlayer videoPlayer;

        public async void Download()
        {
            var videoId = videoPlayer.VideoId;

            var videoUrl = await videoPlayer.InvidiousInstance.GetVideoUrl(videoId);

            var filePath = System.IO.Path.Combine(Application.persistentDataPath, $"{videoId}.mp4");

            try
            {
                Debug.Log($"Downloading video ${videoId} to {filePath}");
                await DownloadAsync(videoUrl, filePath);
                Debug.Log($"Downloaded video to {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download video: {e.Message}");
            }
        }

        static Task DownloadAsync(string videoUrl, string filePath, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();
            var request = UnityWebRequest.Get(videoUrl);
            cancellationToken.Register(o => request.Abort(), true);
            request.downloadHandler = new DownloadHandlerFile(filePath);
            request.SendWebRequest().completed += operation => {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    tcs.TrySetException(new Exception(request.error));
                    return;
                }
                tcs.TrySetResult(true);
            };
            return tcs.Task;
        }
    }
}
