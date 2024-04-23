using System;
using System.Linq;
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
#if UNITY_WEBGL && !UNITY_EDITOR
            // /latest_version endpoint is not supported in WebGL (CORS)
            return await invidiousInstance.GetVideoUrlWebGl(videoId, itag, cancellationToken);
#else
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
#endif
        }

        static async Task<string> GetVideoUrlWebGl(this InvidiousInstance invidiousInstance, string videoId, string itag = null, CancellationToken cancellationToken = default)
        {
            var instanceUrl = await invidiousInstance.GetInstanceUrl(cancellationToken);
            var videoInfo = await InvidiousApi.GetVideoInfo(instanceUrl, videoId, cancellationToken);

            if (string.IsNullOrEmpty(itag))
            {
                // 720p, the highest quality available with the video and audio combined
                itag = "22";
            }

            var format = videoInfo.FormatStreams.Find(f => f.Itag == itag);
            if (format == null)
            {
                // Maybe we're looking for an adaptive format?
                format = videoInfo.AdaptiveFormats.Find(f => f.Itag == itag);
            }

            if (format == null)
            {
                // On older videos, itag 22 may not be available
                // Get any format at this point
                format = videoInfo.FormatStreams.LastOrDefault();
            }

            if (format == null)
            {
                throw new InvalidOperationException("No video format found");
            }

            var uri = new Uri(format.Url);
            var instanceUri = new Uri(instanceUrl);
            var builder = new UriBuilder(uri)
            {
                Host = instanceUri.Host,
                Scheme = instanceUri.Scheme,
                Port = instanceUri.Port
            };

            return builder.Uri.ToString();
        }

        public static async Task<VideoInfo> GetVideoInfo(this InvidiousInstance invidiousInstance, string videoId, CancellationToken cancellationToken = default)
        {
            var instanceUrl = await invidiousInstance.GetInstanceUrl(cancellationToken);
            return await InvidiousApi.GetVideoInfo(instanceUrl, videoId, cancellationToken);
        }

        public static string GetVideoThumbnailUrl(this InvidiousInstance invidiousInstance, string videoId)
        {
            var instanceUrl = invidiousInstance.InstanceUrl;
            if (string.IsNullOrEmpty(instanceUrl))
            {
                throw new InvalidOperationException("InstanceUrl is null or empty. Call InvidiousInstance.GetInstanceUrl() first.");
            }

            return $"{instanceUrl}/vi/{videoId}/mqdefault.jpg";
        }
    }
}
