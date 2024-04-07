using System.Threading;
using System.Threading.Tasks;
using YoutubePlayer.Api;
using YoutubePlayer.Components;
using YoutubePlayer.Models;

namespace YoutubePlayer.Extensions
{
    public static class InvidiousInstanceExtensions
    {
        public static async Task<string> GetVideoUrl(this InvidiousInstance invidiousInstance, string videoId, bool proxyVideo = false, string itag = null, CancellationToken cancellationToken = default)
        {
            var url = await invidiousInstance.GetInstanceUrl(cancellationToken);

            url = $"{url}/latest_version?id={videoId}";

            if (proxyVideo)
            {
                url += "&local=true";
            }

            if (!string.IsNullOrEmpty(itag))
            {
                url += $"&itag={itag}";
            }

            return url;
        }

        public static async Task<VideoInfo> GetVideoInfo(this InvidiousInstance invidiousInstance, string videoId, CancellationToken cancellationToken = default)
        {
            var instanceUrl = await invidiousInstance.GetInstanceUrl(cancellationToken);
            return await InvidiousApi.GetVideoInfo(instanceUrl, videoId, cancellationToken);
        }
    }
}
