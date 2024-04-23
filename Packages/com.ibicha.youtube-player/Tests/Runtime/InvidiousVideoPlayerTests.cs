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

        [UnityTest]
        public IEnumerator PlayVideoWithFallback()
        {
            // video known to be blocked when stream access is from a different location than the server
            var videoId = "5jlI4uzZGjU";
            var instanceUrl = "https://invidious.fdn.fr";

            var cancellationTokenSource = new CancellationTokenSource();
            yield return AwaitTask(Test(cancellationTokenSource.Token), 20000, cancellationTokenSource);
            async Task Test(CancellationToken cancellationToken)
            {
                var go = new GameObject("VideoPlayer");

                try
                {
                    var videoPlayer = go.AddComponent<VideoPlayer>();
                    videoPlayer.playOnAwake = false;

                    var invidiousInstance = go.AddComponent<InvidiousInstance>();
                    invidiousInstance.InstanceType = InvidiousInstance.InvidiousInstanceType.Custom;
                    invidiousInstance.CustomInstanceUrl = instanceUrl;

                    var invidiousVideoPlayer = go.AddComponent<InvidiousVideoPlayer>();
                    invidiousVideoPlayer.InvidiousInstance = invidiousInstance;
                    invidiousVideoPlayer.VideoId = videoId;

                    // ignore error message about video being blocked
                    LogAssert.ignoreFailingMessages = true;

                    _ = invidiousVideoPlayer.PlayVideoAsync(cancellationToken);

                    await WaitForVideoPlayerAsync(invidiousVideoPlayer.VideoPlayer);

                    Assert.IsTrue(videoPlayer.isPlaying);
                }
                finally
                {
                    Object.Destroy(go);
                    LogAssert.ignoreFailingMessages = false;
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
