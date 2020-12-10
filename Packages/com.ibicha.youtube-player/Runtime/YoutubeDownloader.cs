using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YoutubePlayer;

public class YoutubeDownloader
{
    public static Task DownloadAsync(YoutubeVideoMetaData video, string filePath)
    {
        var tcs = new TaskCompletionSource<bool>();
        var request = UnityWebRequest.Get(video.Url);
        request.downloadHandler = new DownloadHandlerFile(filePath);
        request.SendWebRequest().completed += operation => {               
            if (request.isHttpError || request.isNetworkError)
            {
                tcs.TrySetException(new Exception(request.error));
                return;
            }
            tcs.TrySetResult(true);
        };
        return tcs.Task;
    }
}
