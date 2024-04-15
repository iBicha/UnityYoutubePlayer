using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YoutubePlayer.Models;

namespace YoutubePlayer.Extensions
{
    public static class VideoThumbnailInfoExtensions
    {
        public static async Task<Texture2D> ToTextureAsync(this VideoThumbnailInfo videoThumbnailInfo, CancellationToken cancellationToken = default)
        {
            var request = UnityWebRequestTexture.GetTexture(videoThumbnailInfo.Url);
            try
            {
                await request.SendWebRequestAsync(cancellationToken);
                return DownloadHandlerTexture.GetContent(request);
            }
            finally
            {
                request.Dispose();
            }
        }
    }
}
