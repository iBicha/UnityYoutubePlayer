using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace YoutubePlayer.Tests
{
    public class WebRequestTests : InvidiousTestFixture
    {
        [UnityTest]
        public IEnumerator GetAsyncCancels()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            yield return AwaitTask(Test(cancellationTokenSource.Token), 10000, cancellationTokenSource);
            async Task Test(CancellationToken cancellationToken)
            {
                var task = WebRequest.GetAsync<object>(InvidiousTestingUrl + "/api/v1/videos/1PuGuqpHQGo", cancellationToken);
                cancellationTokenSource.Cancel();

                await task;

                Assert.IsTrue(task.IsCanceled);
            }
        }
    }
}
