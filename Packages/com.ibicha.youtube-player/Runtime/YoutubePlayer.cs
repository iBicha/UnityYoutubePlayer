using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

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

        VideoPlayer m_VideoPlayer;

        void Awake()
        {
            m_VideoPlayer = GetComponent<VideoPlayer>();
        }

        async void OnEnable()
        {
            if (m_VideoPlayer.playOnAwake)
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

                var metaData = await YoutubeDl.GetVideoMetaDataAsync(videoUrl);
                
                m_VideoPlayer.source = VideoSource.Url;

                //Resetting the same url restarts the video...
                if (m_VideoPlayer.url != metaData.Url)
                    m_VideoPlayer.url = metaData.Url;

                m_VideoPlayer.Play();
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
        public async Task<string> DownloadVideoAsync(string destinationFolder = null, string videoUrl = null, CancellationToken cancellationToken = default)
        {
            try
            {
                videoUrl = videoUrl ?? youtubeUrl;

                var video = await YoutubeDl.GetVideoMetaDataAsync(videoUrl);

                cancellationToken.ThrowIfCancellationRequested();
                
                var fileName = $"{video.Title}.{video.Extension}";

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

                await YoutubeDownloader.DownloadAsync(video, filePath, cancellationToken);
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }
    }
}