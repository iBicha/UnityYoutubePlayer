using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace YoutubePlayer
{
    public class YoutubeDl
    {
        public static string ServerUrl { get; set; } = "https://unity-youtube-dl-server.herokuapp.com";

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, IEnumerable<string> schema,
            CancellationToken cancellationToken = default)
        {
            return await GetVideoMetaDataAsync<T>(youtubeUrl, YoutubeDlOptions.Default, schema, cancellationToken);
        }

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, CancellationToken cancellationToken = default)
        {
            return await GetVideoMetaDataAsync<T>(youtubeUrl, YoutubeDlOptions.Default, cancellationToken);
        }

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            CancellationToken cancellationToken = default)
        {
            var schema = GetJsonSchema<T>();
            return await GetVideoMetaDataAsync<T>(youtubeUrl, options, schema, cancellationToken);
        }

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            IEnumerable<string> schema = null, CancellationToken cancellationToken = default)
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

            var requestUrl = $"{ServerUrl}/v1/video?url={youtubeUrl}";
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

        static IEnumerable<string> GetJsonSchema<T>()
        {
            return typeof(T).GetFields()
                .Select(fieldInfo => fieldInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true))
                .Where(attributes => attributes.Length > 0)
                .Select(attributes => ((JsonPropertyAttribute)attributes.First()).PropertyName)
                .Where(propertyName => !string.IsNullOrWhiteSpace(propertyName))
                .ToArray();
        }
    }
}
