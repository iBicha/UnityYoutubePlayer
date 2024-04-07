using System.Threading;
using System.Threading.Tasks;
using YoutubePlayer.Api;
using YoutubePlayer.Components;
using YoutubePlayer.Models;

namespace YoutubePlayer.Extensions
{
    public static class InvidiousInstanceExtensions
    {
        public static async Task<string> GetVideoUrl(this InvidiousInstance invidiousInstance, string videoId, bool proxyVideo, CancellationToken cancellationToken = default)
        {
            var instanceUrl = await invidiousInstance.GetInstanceUrl(cancellationToken);

            if (proxyVideo)
            {
                return $"{instanceUrl}/latest_version?id={videoId}&local=true";
            }
            else
            {
                return $"{instanceUrl}/latest_version?id={videoId}";
            }
        }

        public static async Task<VideoInfo> GetVideoInfo(this InvidiousInstance invidiousInstance, string videoId, CancellationToken cancellationToken = default)
        {
            var instanceUrl = await invidiousInstance.GetInstanceUrl(cancellationToken);
            return await InvidiousApi.GetVideoInfo(instanceUrl, videoId, cancellationToken);
        }
    }
}
