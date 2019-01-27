// MIT License - Copyright (c) 2016 Can Güney Aksakalli

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace iBicha
{
    class YoutubeServer
    {
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        public YoutubeServer(int port)
        {
            this.Initialize(port);
        }

        public YoutubeServer()
        {
            //get an empty port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint) l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(port);
        }

        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private async void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception)
                {
                }
            }
        }

        private void Process(HttpListenerContext context)
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
                    Debug.Log(videoId);

                    var client = new YoutubeClient();

                    var streamInfoSet = client.GetVideoMediaStreamInfosAsync(videoId).Result;
                    var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
                    client.DownloadMediaStreamAsync(streamInfo, context.Response.OutputStream).RunSynchronously();

                    context.Response.StatusCode = (int) HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                }
            }

            context.Response.OutputStream.Close();
        }

        private void Initialize(int port)
        {
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }
    }
}