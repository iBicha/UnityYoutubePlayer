using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Video;

namespace YoutubePlayer.Extensions
{
    static class VideoPlayerExtensions
    {
        public static async Task PrepareAsync(this VideoPlayer videoPlayer, CancellationToken cancellationToken = default)
        {
            if (videoPlayer.isPrepared) return;

            var tcs = new TaskCompletionSource<bool>();

            void OnPrepare(VideoPlayer source)
            {
                videoPlayer.prepareCompleted -= OnPrepare;
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.TrySetCanceled(cancellationToken);
                }
                else if (source.isPrepared)
                {
                    tcs.TrySetResult(true);
                }
                else
                {
                    tcs.TrySetException(new System.Exception("Failed to prepare video player"));
                }
            }

            cancellationToken.Register(obj => OnPrepare((VideoPlayer)obj), videoPlayer);

            videoPlayer.prepareCompleted += OnPrepare;
            videoPlayer.Prepare();
            await tcs.Task;
        }
    }
}
