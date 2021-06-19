using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using YoutubeExplode;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

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
                var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);
                var streamInfo = streamManifest.WithHighestVideoQualitySupported();
                if (streamInfo == null)
                    throw new NotSupportedException($"No supported streams in youtube video '{videoUrl}'");

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
                var video = await youtubeClient.Videos.GetAsync(videoUrl);
                var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);
                
                cancellationToken.ThrowIfCancellationRequested();
                var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();
                if (streamInfo == null)
                    throw new NotSupportedException($"No supported streams in youtube video '{videoUrl}'");

                var fileExtension = streamInfo.Container;
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

                await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filePath, progress, cancellationToken);
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
        public async Task<ClosedCaptionTrack> DownloadClosedCaptions(string videoUrl = null, string language = "en")
        {
            videoUrl = videoUrl ?? youtubeUrl;
            var trackManifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoUrl);

            if (trackManifest.Tracks.Count == 0)
                return null;
            
            var trackInfo = trackManifest.TryGetByLanguage(language) ?? trackManifest.Tracks.First();
            return await youtubeClient.Videos.ClosedCaptions.GetAsync(trackInfo);
        }
    }
}