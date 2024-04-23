using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using YoutubePlayer.Extensions;
using UnityEngine;

namespace YoutubePlayer
{
    class WebRequest
    {
        public static async Task<T> GetAsync<T>(string requestUrl, CancellationToken cancellationToken = default)
        {
            var request = UnityWebRequest.Get(requestUrl);
            try
            {
                await request.SendWebRequestAsync(cancellationToken);
                var text = request.downloadHandler.text;
                return JsonConvert.DeserializeObject<T>(text);
            }
            finally
            {
                request.Dispose();
            }
        }

        public static async Task<long> HeadAsync(string requestUrl, CancellationToken cancellationToken = default)
        {
            var request = UnityWebRequest.Head(requestUrl);
            try
            {
                await request.SendWebRequestAsync(cancellationToken);
                return request.responseCode;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return request.responseCode;
            }
            finally
            {
                request.Dispose();
            }
        }
    }
}
