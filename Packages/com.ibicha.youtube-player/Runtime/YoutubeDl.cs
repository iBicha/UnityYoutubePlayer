using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace YoutubePlayer
{
    public class YoutubeDl
    {
        const string k_DefaultServer = "http://localhost:3000";
        
        public static async Task<YoutubeVideoMetaData> GetVideoMetaDataAsync(string youtubeUrl)
        {
            return await GetVideoMetaDataAsync(youtubeUrl, YoutubeDlOptions.Default);
        }

        public static async Task<YoutubeVideoMetaData> GetVideoMetaDataAsync(string youtubeUrl, YoutubeDlOptions options)
        {
            var optionFlags = new List<string>();
            if (string.IsNullOrWhiteSpace(options.Format))
            {
                optionFlags.Add($"-f '{options.Format}'");
            }
            if (options.Subtitles)
            {
                optionFlags.Add("--sub-format str");
                optionFlags.Add("--sub-lang end");
            }

            var requestUrl = $"{k_DefaultServer}/v1/video?url={youtubeUrl}";
            if (optionFlags.Count > 0)
            {
                requestUrl += $"&options={HttpUtility.UrlEncode(string.Join(" ", optionFlags))}";
            }

            var request = UnityWebRequest.Get(requestUrl);
            var tcs = new TaskCompletionSource<YoutubeVideoMetaData>();
            request.SendWebRequest().completed += operation =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    tcs.TrySetException(new Exception(request.error));
                    return;
                }

                var text = request.downloadHandler.text;
                var video = JsonConvert.DeserializeObject<YoutubeVideoMetaData>(text);
                tcs.TrySetResult(video);
            };
            return await tcs.Task;
        }
        
    }
}