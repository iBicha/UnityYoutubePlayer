using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using YoutubePlayer.Components;

namespace YoutubePlayer.Tests
{
    [TestFixture]
    public abstract class InvidiousTestFixture
    {
        public const string InvidiousTestingUrl = "http://192.168.18.5:8095";

        public InvidiousInstance InvidiousInstance;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            var go = new GameObject("InvidiousInstance");
            InvidiousInstance = go.AddComponent<InvidiousInstance>();
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            Object.Destroy(InvidiousInstance.gameObject);
        }

        public void SetInstanceUrl(string url)
        {
            InvidiousInstance.InstanceType = InvidiousInstance.InvidiousInstanceType.Custom;
            InvidiousInstance.CustomInstanceUrl = url;
        }

        public IEnumerator AwaitTask(Task task, int timeoutMilliseconds = -1, CancellationTokenSource cancellationTokenSource = default)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            while (!task.IsCompleted)
            {
                yield return null;

                if (timeoutMilliseconds > 0 && stopwatch.ElapsedMilliseconds > timeoutMilliseconds)
                {
                    Assert.Fail($"Task did not complete within {timeoutMilliseconds} milliseconds");
                    cancellationTokenSource?.Cancel();
                    break;
                }
            }

            if (task.IsFaulted)
            {
                Assert.Fail(task.Exception.ToString());
            }
        }
    }
}
