using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine;

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
#if UNITY_2020_2_OR_NEWER
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
#else
                if (request.isNetworkError)
#endif
                {
                    tcs.TrySetException(new Exception(request.error));
                    return;
                }

                var text = request.downloadHandler.text;

#if UNITY_2020_2_OR_NEWER
                if (request.result == UnityWebRequest.Result.ProtocolError)
#else
                if (request.isHttpError)
#endif
                {
                    tcs.TrySetException(new Exception(request.error + "\nResponseError:" + text));
                    return;
                }

                var video = JsonConvert.DeserializeObject<T>(text);
                tcs.TrySetResult(video);
            };

            cancellationToken.Register(obj =>
            {
                ((UnityWebRequest)obj).Abort();
                tcs.TrySetCanceled(cancellationToken);
            }, request);

            return tcs.Task;
        }
    }
}
