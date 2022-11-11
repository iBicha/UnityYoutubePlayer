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
        public Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            IEnumerable<string> schema, YoutubeDlCli cli, CancellationToken cancellationToken = default)
        {
            var requestUrl = BuildRequestUrl(youtubeUrl, options, schema, cli);
            return WebRequest.GetAsync<T>(requestUrl, cancellationToken);
        }

        string BuildRequestUrl(string youtubeUrl, YoutubeDlOptions options,
            IEnumerable<string> schema, YoutubeDlCli cli)
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

            var requestUrl = $"{YoutubeDl.ServerUrl}/v1/video?url={youtubeUrl}&cli={cli.Value}";
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
