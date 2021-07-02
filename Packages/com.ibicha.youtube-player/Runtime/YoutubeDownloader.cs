using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YoutubePlayer;

public class YoutubeDownloader
{
    public static Task DownloadAsync(YoutubeVideoMetaData video, string filePath, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        var request = UnityWebRequest.Get(video.Url);
        cancellationToken.Register(o => request.Abort(), true);
        request.downloadHandler = new DownloadHandlerFile(filePath);
        request.SendWebRequest().completed += operation => {
#if UNITY_2020_2_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isHttpError || request.isNetworkError)
#endif
            {
                tcs.TrySetException(new Exception(request.error));
                return;
            }
            tcs.TrySetResult(true);
        };
        return tcs.Task;
    }
}
