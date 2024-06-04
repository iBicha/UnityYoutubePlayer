using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace YoutubePlayer.Extensions
{
    static class UnityWebRequestExtensions
    {
        public static async Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();

            void OnComplete(AsyncOperation obj)
            {
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    tcs.TrySetException(new Exception(request.error));
                }
                else if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    string error = request.error;
                    if (request.downloadHandler != null && request.downloadHandler is DownloadHandlerBuffer)
                    {
                        error += "\nResponseError:" + (request.downloadHandler as DownloadHandlerBuffer).text;
                    }
                    tcs.TrySetException(new Exception(error));
                }
                else
                {
                    tcs.TrySetResult(request);
                }
            }

            var op = request.SendWebRequest();

            using (var registration = cancellationToken.Register(obj =>
            {
                op.completed -= OnComplete;
                tcs.TrySetCanceled(cancellationToken);
                var request = (UnityWebRequest)obj;
                request.Abort();
            }, request))
            {
                op.completed += OnComplete;
                return await tcs.Task;
            }
        }
    }
}
