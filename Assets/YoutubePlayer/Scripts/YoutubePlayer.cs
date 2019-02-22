using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using YoutubeExplode;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubePlayer
{
    /// <summary>
    /// Downloads and plays Youtube videos a VideoPlayer component
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class YoutubePlayer : MonoBehaviour
    {
        /// <summary>
        /// Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)
        /// </summary>
        public string youtubeUrl;

        /// <summary>
        /// VideoStartingDelegate 
        /// </summary>
        /// <param name="url">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        public delegate void VideoStartingDelegate(string url);

        /// <summary>
        /// Event fired when a youtube video is starting
        /// Useful to start downloading captions, etc.
        /// </summary>
        public event VideoStartingDelegate YoutubeVideoStarting;

        private VideoPlayer videoPlayer;
        private YoutubeClient youtubeClient;

        private void Awake()
        {
            youtubeClient = new YoutubeClient();
            videoPlayer = GetComponent<VideoPlayer>();
        }

        private async void OnEnable()
        {
            if (videoPlayer.playOnAwake)
                await PlayVideoAsync();
        }

        /// <summary>
        /// Play the youtube video in the attached Video Player component.
        /// </summary>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <returns>A Task to await</returns>
        /// <exception cref="NotSupportedException">When the youtube url doesn't contain any supported streams</exception>
        public async Task PlayVideoAsync(string videoUrl = null)
        {
            try
            {
                videoUrl = videoUrl ?? youtubeUrl;
                var videoId = GetVideoId(videoUrl);
                var streamInfoSet = await youtubeClient.GetVideoMediaStreamInfosAsync(videoId);
                var streamInfo = streamInfoSet.WithHighestVideoQualitySupported();
                if (streamInfo == null)
                    throw new NotSupportedException($"No supported streams in youtube video '{videoId}'");

                videoPlayer.source = VideoSource.Url;

                //Resetting the same url restarts the video...
                if (videoPlayer.url != streamInfo.Url)
                    videoPlayer.url = streamInfo.Url;

                videoPlayer.Play();
                youtubeUrl = videoUrl;
                YoutubeVideoStarting?.Invoke(youtubeUrl);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Download a youtube video to a destination folder
        /// </summary>
        /// <param name="destinationFolder">A folder to create the file in</param>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <param name="progress">An object implementing `IProgress` to get download progress, from 0 to 1</param>
        /// <param name="cancellationToken">A CancellationToken used to cancel the current async task</param>
        /// <returns>Returns the path to the file where the video was downloaded</returns>
        /// <exception cref="NotSupportedException">When the youtube url doesn't contain any supported streams</exception>
        public async Task<string> DownloadVideoAsync(string destinationFolder = null, string videoUrl = null,
            IProgress<double> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                videoUrl = videoUrl ?? youtubeUrl;
                var videoId = GetVideoId(videoUrl);
                var video = await youtubeClient.GetVideoAsync(videoId);
                var streamInfoSet = await youtubeClient.GetVideoMediaStreamInfosAsync(videoId);
                
                cancellationToken.ThrowIfCancellationRequested();
                var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
                if (streamInfo == null)
                    throw new NotSupportedException($"No supported streams in youtube video '{videoId}'");

                var fileExtension = streamInfo.Container.GetFileExtension();
                var fileName = $"{video.Title}.{fileExtension}";

                var invalidChars = Path.GetInvalidFileNameChars();
                foreach (var invalidChar in invalidChars)
                {
                    fileName = fileName.Replace(invalidChar.ToString(), "_");
                }

                var filePath = fileName;
                if (!string.IsNullOrEmpty(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                    filePath = Path.Combine(destinationFolder, fileName);
                }

                using (var output = File.Create(filePath))
                    await youtubeClient.DownloadMediaStreamAsync(streamInfo, output, progress, cancellationToken);

                return filePath;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Download closed captions for a youtube video
        /// </summary>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <returns>A ClosedCaptionTrack object.</returns>
        public async Task<ClosedCaptionTrack> DownloadClosedCaptions(string videoUrl = null)
        {
            videoUrl = videoUrl ?? youtubeUrl;
            var videoId = GetVideoId(videoUrl);
            var trackInfos = await youtubeClient.GetVideoClosedCaptionTrackInfosAsync(videoId);
            if (trackInfos?.Count == 0)
                return null;

            var trackInfo = trackInfos.FirstOrDefault(t => t.Language.Code == "en") ?? trackInfos.First();
            return await youtubeClient.GetClosedCaptionTrackAsync(trackInfo);
        }

        /// <summary>
        /// Try to parse a video ID from a video Url.
        /// If null is passed, it will use the current url of the Youtube Player instance.
        /// </summary>
        /// <param name="videoUrl">Youtube url (e.g. https://www.youtube.com/watch?v=VIDEO_ID)</param>
        /// <returns>The video ID</returns>
        /// <exception cref="ArgumentException">If the videoUrl is not a valid youtube url</exception>
        public string GetVideoId(string videoUrl = null)
        {
            if (!YoutubeClient.TryParseVideoId(videoUrl, out var videoId))
                throw new ArgumentException("Invalid youtube url", nameof(videoUrl));

            return videoId;
        }
    }
}