using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

public class AsyncHttpServer
{
    private readonly HttpListener _listener;

    public AsyncHttpServer(int portNumber)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(string.Format("http://+:{0}/", portNumber));
    }

    public async Task Start(CancellationToken cancellationToken = default(CancellationToken))
    {
        _listener.Start();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ctx = await _listener.GetContextAsync();
            cancellationToken.ThrowIfCancellationRequested();
            await Process(ctx, cancellationToken);
        }
    }

    private async Task Process(HttpListenerContext context, CancellationToken cancellationToken = default(CancellationToken))
    {
        string fullUrl = "https://www.youtube.com" + context.Request.RawUrl;
        string videoId;
        if (!YoutubeClient.TryParseVideoId(fullUrl, out videoId))
        {
            context.Response.StatusCode = (int) HttpStatusCode.NotFound;
        }
        else
        {
            try
            {
//                context.Response.StatusCode = (int) HttpStatusCode.OK;
                
                Debug.Log(videoId);

                var client = new YoutubeClient();

                var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);
                var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
                cancellationToken.ThrowIfCancellationRequested();
              
                context.Response.ContentLength64 = streamInfo.Size;
                context.Response.ContentType = "video/mp4";

                using (var ms = new MemoryStream())
                {
                    Debug.Log("DownloadMediaStreamAsync");
                    await client.DownloadMediaStreamAsync(streamInfo, ms, null, cancellationToken);
                    Debug.Log("CopyToAsync");
                    ms.Seek(0, SeekOrigin.Begin);
                    await ms.CopyToAsync(context.Response.OutputStream, 81920, cancellationToken);
                    
                    Debug.Log("Done");
                }
                
                await context.Response.OutputStream.FlushAsync();
                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                try
                {
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                }
                catch (Exception)
                {
                }
            }
        }

        context.Response.OutputStream.Close();
    }
    
    
    public async Task Stop()
    {
        if (_listener.IsListening)
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}