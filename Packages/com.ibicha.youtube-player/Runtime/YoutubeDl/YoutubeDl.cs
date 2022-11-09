using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace YoutubePlayer
{
    public class YoutubeDl
    {
        /// <summary>
        /// When possible, trying using youtube-dl locally before sending web requests to youtube-dl-server
        /// Currently only supported on desktop platforms.
        /// </summary>
        public static bool UseLocalInstance { get; set; } = true;

        public static string ServerUrl { get; set; } = "https://unity-youtube-dl-server.herokuapp.com";

        static IYoutubeDl s_RemoteYoutubeDl = new RemoteYoutubeDl();
        static IYoutubeDl s_LocalYoutubeDl = new LocalYoutubeDl();

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, CancellationToken cancellationToken = default)
        {
            return await GetVideoMetaDataAsync<T>(youtubeUrl, YoutubeDlOptions.Default, cancellationToken);
        }

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, IEnumerable<string> schema,
            CancellationToken cancellationToken = default)
        {
            return await GetVideoMetaDataAsync<T>(youtubeUrl, YoutubeDlOptions.Default, schema, cancellationToken);
        }

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            CancellationToken cancellationToken = default)
        {
            var schema = GetJsonSchema<T>();
            return await GetVideoMetaDataAsync<T>(youtubeUrl, options, schema, cancellationToken);
        }

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            YoutubeDlCli cli, CancellationToken cancellationToken = default)
        {
            var schema = GetJsonSchema<T>();
            return await GetVideoMetaDataAsync<T>(youtubeUrl, options, schema, cli, cancellationToken);
        }

        public static Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            IEnumerable<string> schema, CancellationToken cancellationToken = default)
        {
            return GetVideoMetaDataAsync<T>(youtubeUrl, options, schema, YoutubeDlCli.YtDlp, cancellationToken);
        }

        public static async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            IEnumerable<string> schema, YoutubeDlCli cli, CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (UseLocalInstance)
            {
                try
                {
                    return await s_LocalYoutubeDl.GetVideoMetaDataAsync<T>(youtubeUrl, options, schema, cli, cancellationToken);
                }
                catch (FileNotFoundException e) when (Path.GetFileNameWithoutExtension(e.FileName) == "youtube-dl")
                {
                    Debug.LogException(e);
                    Debug.LogWarning("local youtube-dl does not exist, trying remote server...");
                }
                catch (FileNotFoundException e) when (Path.GetFileNameWithoutExtension(e.FileName) == "yt-dlp")
                {
                    Debug.LogException(e);
                    Debug.LogWarning("local yt-dlp does not exist, trying remote server...");
                }
            }
#endif
            return await s_RemoteYoutubeDl.GetVideoMetaDataAsync<T>(youtubeUrl, options, schema, cli, cancellationToken);
        }

        static IEnumerable<string> GetJsonSchema<T>()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (UseLocalInstance)
            {
                return null;
            }
#endif
            var keys = new List<string>();
            var fieldInfos = typeof(T).GetFields();
            foreach (var fieldInfo in fieldInfos)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                if (attributes.Length == 0)
                {
                    keys.Add(fieldInfo.Name);
                    continue;
                }
                var propertyName = ((JsonPropertyAttribute)attributes.First()).PropertyName;
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    keys.Add(fieldInfo.Name);
                    continue;
                }
                keys.Add(propertyName);
            }
            return keys;
        }
    }
}
