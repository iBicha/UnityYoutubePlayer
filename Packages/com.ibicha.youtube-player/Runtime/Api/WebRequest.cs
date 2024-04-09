using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace YoutubePlayer
{
    class WebRequest
    {
        public static Task<T> GetAsync<T>(string requestUrl, CancellationToken cancellationToken = default)
        {
            var request = UnityWebRequest.Get(requestUrl);
            var tcs = new TaskCompletionSource<T>();
            request.SendWebRequest().completed += operation =>
            {
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    tcs.TrySetException(new Exception(request.error));
                    request.Dispose();
                    return;
                }

                var text = request.downloadHandler.text;

                if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    tcs.TrySetException(new Exception(request.error + "\nResponseError:" + text));
                    request.Dispose();
                    return;
                }

                var video = JsonConvert.DeserializeObject<T>(text);
                tcs.TrySetResult(video);
                request.Dispose();
            };

            cancellationToken.Register(obj =>
            {
                tcs.TrySetCanceled(cancellationToken);
                var request = (UnityWebRequest)obj;
                request.Abort();
                request.Dispose();
            }, request);

            return tcs.Task;
        }
    }
}
