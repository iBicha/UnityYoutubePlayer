using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace YoutubePlayer
{
    public static class VideoPlayerExtensions 
    {
        public static async Task PrepareAsync(this VideoPlayer videoPlayer, CancellationToken cancellationToken = default)
        {
            if(videoPlayer.isPrepared) return;

            var tcs = new TaskCompletionSource<bool>();
            
            void OnPrepare(VideoPlayer source)
            {
                videoPlayer.prepareCompleted -= OnPrepare;
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.TrySetCanceled(cancellationToken);
                }
                else
                {
                    tcs.TrySetResult(true);
                }
            }

            cancellationToken.Register(obj => OnPrepare((VideoPlayer)obj), videoPlayer);

            videoPlayer.prepareCompleted += OnPrepare;
            videoPlayer.Prepare();
            await tcs.Task;
        }

        public static async Task PlayYoutubeVideoAsync(this VideoPlayer videoPlayer, string youtubeUrl, CancellationToken cancellationToken = default)
        {
            var metaData = await YoutubeDl.GetVideoMetaDataAsync(youtubeUrl, YoutubeDlOptions.Default, cancellationToken);
            var rawUrl = metaData.Url;
                
            videoPlayer.source = VideoSource.Url;

            //Resetting the same url restarts the video...
            if (videoPlayer.url != rawUrl)
                videoPlayer.url = rawUrl;

            await videoPlayer.PrepareAsync(cancellationToken);
            videoPlayer.Play();
        }
    }
 
}
