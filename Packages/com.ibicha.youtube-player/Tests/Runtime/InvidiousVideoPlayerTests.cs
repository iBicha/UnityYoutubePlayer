using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Video;
using YoutubePlayer.Components;

namespace YoutubePlayer.Tests
{
    public class InvidiousVideoPlayerTests : InvidiousTestFixture
    {
        const string InvidiousTestingUrl = "http://192.168.1.119:8095";

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            SetInstanceUrl(InvidiousTestingUrl);
        }

        static IEnumerable PlayVideoTestCases()
        {
            yield return "jNQXAC9IVRw";
            yield return "1PuGuqpHQGo";
        }

        [UnityTest]
        public IEnumerator PlayVideo([ValueSource(nameof(PlayVideoTestCases))] string videoId)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            yield return AwaitTask(Test(cancellationTokenSource.Token), 10000, cancellationTokenSource);
            async Task Test(CancellationToken cancellationToken)
            {
                var go = new GameObject("VideoPlayer");

                try
                {
                    var videoPlayer = go.AddComponent<VideoPlayer>();
                    videoPlayer.playOnAwake = false;

                    var invidiousVideoPlayer = go.AddComponent<InvidiousVideoPlayer>();
                    invidiousVideoPlayer.InvidiousInstance = InvidiousInstance;
                    invidiousVideoPlayer.VideoId = videoId;

                    _ = invidiousVideoPlayer.PlayVideoAsync(cancellationToken);

                    await WaitForVideoPlayerAsync(invidiousVideoPlayer.VideoPlayer);

                    Assert.IsTrue(videoPlayer.isPlaying);
                }
                finally
                {
                    Object.Destroy(go);
                }
            }
        }

        Task WaitForVideoPlayerAsync(VideoPlayer videoPlayer)
        {
            var tcs = new TaskCompletionSource<bool>();

            void OnErrorReceived(VideoPlayer source, string message)
            {
                tcs.TrySetException(new System.Exception(message));

                videoPlayer.errorReceived -= OnErrorReceived;
                videoPlayer.started -= OnStarted;
            }

            void OnStarted(VideoPlayer source)
            {
                tcs.TrySetResult(true);

                videoPlayer.errorReceived -= OnErrorReceived;
                videoPlayer.started -= OnStarted;
            }

            if (videoPlayer.isPlaying)
            {
                tcs.TrySetResult(true);
            }
            else
            {
                videoPlayer.errorReceived += OnErrorReceived;
                videoPlayer.started += OnStarted;
            }
            return tcs.Task;
        }
    }
}
