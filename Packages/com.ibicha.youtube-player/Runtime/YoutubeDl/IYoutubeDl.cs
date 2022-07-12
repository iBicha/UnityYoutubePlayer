using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubePlayer
{
    interface IYoutubeDl
    {
        Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            IEnumerable<string> schema, YoutubeDlCli cli, CancellationToken cancellationToken = default);
    }
}
