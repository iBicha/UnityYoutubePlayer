using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using YoutubePlayer.Api;
using YoutubePlayer.Extensions;
using YoutubePlayer.Models;

namespace YoutubePlayer.Tests
{
    public class InvidiousApiTests : InvidiousTestFixture
    {
        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            SetInstanceUrl(InvidiousTestingUrl);
        }

        static IEnumerable GetVideoInfoTestCases()
        {
            yield return new VideoInfo { VideoId = "jNQXAC9IVRw", Title = "Me at the zoo", Author = "jawed", AuthorId = "UC4QobU6STFB0P71PMvOGN5A" };
            yield return new VideoInfo { VideoId = "1PuGuqpHQGo", Title = "Buried Memories Volume 1: Yggdrasil - Icon Pack - Teaser", Author = "Unity", AuthorId = "UCG08EqOAXJk_YXPDsAvReSg" };
        }

        [UnityTest]
        public IEnumerator GetVideoInfo([ValueSource(nameof(GetVideoInfoTestCases))] VideoInfo testCase)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            yield return AwaitTask(Test(cancellationTokenSource.Token), 10000, cancellationTokenSource);
            async Task Test(CancellationToken cancellationToken)
            {
                var videoId = testCase.VideoId;
                var expectedTitle = testCase.Title;
                var expectedAuthor = testCase.Author;
                var expectedAuthorId = testCase.AuthorId;

                var videoInfo = await InvidiousInstance.GetVideoInfo(videoId, cancellationToken);

                Assert.IsNotNull(videoInfo);
                Assert.AreEqual(videoId, videoInfo.VideoId);
                Assert.AreEqual(expectedTitle, videoInfo.Title);
                Assert.AreEqual(expectedAuthor, videoInfo.Author);
                Assert.AreEqual(expectedAuthorId, videoInfo.AuthorId);
                Assert.IsNotEmpty(videoInfo.FormatStreams);

                for (int i = 0; i < videoInfo.FormatStreams.Count; i++)
                {
                    var formatStream = videoInfo.FormatStreams[i];
                    Assert.IsNotEmpty(formatStream.Url);
                    Assert.IsNotEmpty(formatStream.Itag);
                }
            }
        }

        [UnityTest]
        public IEnumerator GetPublicInstances()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            yield return AwaitTask(Test(cancellationTokenSource.Token), 10000, cancellationTokenSource);
            async Task Test(CancellationToken cancellationToken)
            {
                var instances = await InvidiousApi.GetPublicInstances(cancellationToken);
                Assert.IsNotEmpty(instances);

                foreach (var instance in instances)
                {
                    Assert.IsNotEmpty(instance.Uri);
                    Assert.AreEqual("https", instance.Type);
                    Assert.IsTrue(instance.Api);
                    Assert.IsTrue(instance.Cors);
                }
            }
        }
    }
}
