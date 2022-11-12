using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace YoutubePlayer
{
    class InvidiousApi
    {
        const string INSTANCE_API = "https://api.invidious.io/instances.json?sort_by=type,health";

        static string s_DefaultInstance;

        public static async Task<string> GetDefaultInstanceAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(s_DefaultInstance))
            {
                return s_DefaultInstance;
            }

            var instances = await WebRequest.GetAsync<JArray>(INSTANCE_API, cancellationToken);
            return instances[0][1]["uri"].ToObject<string>();
        }

        public static string GetVideoUrl(string videoId, string instance, string itag, bool useProxy = false)
        {
            var url = $"{instance}/latest_version?id={videoId}&itag={itag}";
            if (useProxy)
            {
                url += "&local=true";
            }
            return url;
        }
    }
}
