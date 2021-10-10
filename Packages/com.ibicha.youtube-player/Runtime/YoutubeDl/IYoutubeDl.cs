using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubePlayer
{
    interface IYoutubeDl
    {
        Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            IEnumerable<string> schema, CancellationToken cancellationToken = default);
    }
}
