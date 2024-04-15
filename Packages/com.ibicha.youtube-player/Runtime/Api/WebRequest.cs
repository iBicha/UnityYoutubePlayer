using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using YoutubePlayer.Extensions;

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
    }
}
