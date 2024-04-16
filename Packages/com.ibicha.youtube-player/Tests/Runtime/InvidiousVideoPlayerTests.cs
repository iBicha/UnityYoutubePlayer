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

        public struct PlayVideoTestCase
        {
            public string VideoId;
            public InvidiousVideoPlayer.ProxyVideoType ProxyVideo;

            public override string ToString()
            {
                return $"{VideoId} - proxy: {ProxyVideo}";
            }
        }

        static IEnumerable PlayVideoTestCases()
        {
            foreach (var videoId in new[] {"jNQXAC9IVRw", "1PuGuqpHQGo"})
            {
                foreach (var proxyVideo in System.Enum.GetValues(typeof(InvidiousVideoPlayer.ProxyVideoType)))
                {
                    yield return new PlayVideoTestCase { VideoId = videoId, ProxyVideo = (InvidiousVideoPlayer.ProxyVideoType)proxyVideo };
                }
            }
        }

        [UnityTest]
        public IEnumerator PlayVideo([ValueSource(nameof(PlayVideoTestCases))] PlayVideoTestCase testCase)
        {
            var videoId = testCase.VideoId;
            var proxyVideo = testCase.ProxyVideo;

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
                    invidiousVideoPlayer.ProxyVideo = proxyVideo;

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
