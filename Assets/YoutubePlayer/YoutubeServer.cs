using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using UnityEngine;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubePlayer
{
    public class YoutubeServer : IDisposable
    {
        private class StreamData : IProgress<double>
        {
            public MuxedStreamInfo StreamInfo; //Information about the video stream from youtube
            public string FileName; //File location where the video is cached
            public FileStream FileInputStream; //A file stream to download the data into the file
            public FileStream FileOutputStream; //A file stream to serve the downloaded data
            public float DownloadProgress;

            public void Report(double value)
            {
                DownloadProgress = (float) value;
            }
        }

        public int Port { get; }

        private readonly HttpListener httpListener;
        private YoutubeClient youtubeClient;
        private Dictionary<string, StreamData> videoCache;
        private string cachePath;

        private CancellationTokenSource cancellationTokenSource;

        public YoutubeServer(int port = -1)
        {
            youtubeClient = new YoutubeClient();

            if (port <= 0) port = GetAvailablePort();
            Port = port;

            videoCache = new Dictionary<string, StreamData>();
            cachePath = Path.Combine(Application.temporaryCachePath, "YoutubePlayer");
            Directory.CreateDirectory(cachePath);

            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://+:{Port}/");
            httpListener.Start();

            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            Task.Run(async () => await ServerLoop(cancellationToken));
        }

        public float GetDownloadProgressForVideo(string videoId)
        {
            if (videoCache.TryGetValue(videoId, out var streamData))
                return streamData.DownloadProgress;
            return 0;
        }

        public async Task<ClosedCaptionTrack> DownloadClosedCaptions(string videoId)
        {
            var trackInfos = await youtubeClient.GetVideoClosedCaptionTrackInfosAsync(videoId);
            var trackInfo = trackInfos.First(t => t.Language.Code == "en");
            return await youtubeClient.GetClosedCaptionTrackAsync(trackInfo);
        }

        private async Task ServerLoop(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var ctx = await httpListener.GetContextAsync();
                cancellationToken.ThrowIfCancellationRequested();
                await ProcessHttpListenerContext(ctx, cancellationToken);
            }
        }

        private async Task ProcessHttpListenerContext(HttpListenerContext context,
            CancellationToken cancellationToken = default(CancellationToken))
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
                    StreamData streamData;
                    if (!videoCache.TryGetValue(videoId, out streamData))
                    {
                        streamData = GetStreamDataForVideoId(videoId);
                        var streamInfoSet = await youtubeClient.GetVideoMediaStreamInfosAsync(videoId);
                        cancellationToken.ThrowIfCancellationRequested();

                        streamData.StreamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();

                        //TODO: download only requested range
                        //We start downloading the video without await
                        youtubeClient.DownloadMediaStreamAsync(streamData.StreamInfo, streamData.FileInputStream,
                            streamData,
                            cancellationToken);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    //TODO: better handling of range
                    int startByte = -1;
                    int endByte = -1;
                    if (context.Request.Headers["Range"] != null)
                    {
                        string rangeHeader = context.Request.Headers["Range"].Replace("bytes=", "");
                        string[] range = rangeHeader.Split('-');
                        startByte = int.Parse(range[0]);
                        if (range[1].Trim().Length > 0) int.TryParse(range[1], out endByte);
                        if (endByte == -1) endByte = (int) streamData.StreamInfo.Size - 1;
                    }
                    else
                    {
                        startByte = 0;
                        endByte = (int) streamData.StreamInfo.Size - 1;
                    }

                    byte[] buffer = new byte[endByte - startByte + 1];

                    while (streamData.FileOutputStream.Length < endByte + 1)
                    {
                        //The requested bytes are not downloaded yet, buffering...
                        await Task.Delay(5, cancellationToken);
                    }

                    streamData.FileOutputStream.Position = startByte;
                    var length = streamData.FileOutputStream.Read(buffer, 0, buffer.Length);

                    context.Response.StatusCode = (int) HttpStatusCode.PartialContent;
                    context.Response.ContentType = $"video/{streamData.StreamInfo.Container.GetFileExtension()}";
                    context.Response.AppendHeader("Accept-Ranges", "bytes");
                    context.Response.ContentLength64 = length;
                    context.Response.AppendHeader("Content-Range",
                        $"bytes {startByte}-{endByte}/{streamData.StreamInfo.Size}");

                    await context.Response.OutputStream.WriteAsync(buffer, 0, length, cancellationToken);
                    await context.Response.OutputStream.FlushAsync(cancellationToken);
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
                        // ignored
                    }
                }
            }

            context.Response.Close();
        }


        private StreamData GetStreamDataForVideoId(string videoId)
        {
            if (videoCache.TryGetValue(videoId, out var streamData))
                return streamData;

            streamData = new StreamData();
            streamData.FileName = Path.Combine(cachePath, videoId);
            streamData.FileInputStream =
                new FileStream(streamData.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            streamData.FileOutputStream =
                new FileStream(streamData.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            videoCache.Add(videoId, streamData);

            return streamData;
        }

        private static int GetAvailablePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Loopback, port: 0));
                return ((IPEndPoint) socket.LocalEndPoint).Port;
            }
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            if (httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
            }

            foreach (var streamData in videoCache.Values)
            {
                streamData.FileInputStream.Dispose();
                streamData.FileOutputStream.Dispose();
                if (File.Exists(streamData.FileName))
                    File.Delete(streamData.FileName);
            }
        }
    }
}