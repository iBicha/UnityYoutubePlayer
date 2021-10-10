using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace YoutubePlayer
{
    class RemoteYoutubeDl : IYoutubeDl
    {
        public async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options, IEnumerable<string> schema, CancellationToken cancellationToken = default)
        {
            var requestUrl = BuildRequestUrl(youtubeUrl, options, schema);
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

            return await tcs.Task;
        }

        string BuildRequestUrl(string youtubeUrl, YoutubeDlOptions options, IEnumerable<string> schema)
        {
            var optionFlags = new List<string>();
            if (!string.IsNullOrWhiteSpace(options.Format))
            {
                optionFlags.Add($"-f \"{options.Format}\"");
            }
            if (options.UserAgent != null)
            {
                optionFlags.Add($"--user-agent \"{options.UserAgent}\"");
            }
            if (options.Custom != null)
            {
                optionFlags.Add(options.Custom);
            }

            var requestUrl = $"{YoutubeDl.ServerUrl}/v1/video?url={youtubeUrl}";
            if (optionFlags.Count > 0)
            {
                requestUrl += $"&options={UnityWebRequest.EscapeURL(string.Join(" ", optionFlags))}";
            }

            if (schema != null)
            {
                foreach (var schemaKey in schema)
                {
                    requestUrl += $"&schema={schemaKey}";
                }
            }

            return requestUrl;
        }
    }
}
