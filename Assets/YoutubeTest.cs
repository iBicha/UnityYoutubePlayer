using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using YoutubeExplode;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

[RequireComponent(typeof(VideoPlayer))]
public class YoutubeTest : MonoBehaviour, IProgress<double>
{
    public float bufferTime = 10f;

    public string targetVideo;

    public TMP_Text captions;

    public Image bufferProgress;
    public Image playbackProgress;

    private List<ClosedCaption> captionList;

    private int captionStartIndex;
    private int captionEndIndex;

    private StringBuilder currentCaption = new StringBuilder();
    private VideoPlayer videoPlayer;
    private double downloadProgress;
    private string fileNameUrl;
    private string fileName;
    private float requiredProgress = 1f;
    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.started += source => captionStartIndex = captionEndIndex = -1;
        var texture = Texture2D.whiteTexture;
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100);
        bufferProgress.sprite = playbackProgress.sprite = sprite;
    }

    // Start is called before the first frame update
    void Start()
    {
        //await this task for full download
        var downloadTask = DownloadYoutubeVideo();
        StartCoroutine(PlayVideoAfterProgress());
    }

    IEnumerator PlayVideoAfterProgress()
    {
        //We wait until the video buffered <bufferTime> seconds before starting
        yield return new WaitUntil(() => downloadProgress >= requiredProgress);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = fileNameUrl;
        videoPlayer.Play();
    }

    private static string NormalizeVideoId(string input)
    {
        return YoutubeClient.TryParseVideoId(input, out var videoId)
            ? videoId
            : input;
    }

    /// <summary>
    /// Turns file size in bytes into human-readable string.
    /// </summary>
    private static string NormalizeFileSize(long fileSize)
    {
        string[] units = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
        double size = fileSize;
        var unit = 0;

        while (size >= 1024)
        {
            size /= 1024;
            ++unit;
        }

        return $"{size:0.#} {units[unit]}";
    }

    private async Task DownloadYoutubeVideo()
    {
        // Client
        var client = new YoutubeClient();

        // Get the video ID
        var videoId = targetVideo;
        videoId = NormalizeVideoId(videoId);
        Debug.Log($"Video ID: {videoId}");

        // Get the video info
        Debug.Log("Obtaining general video info... ");
        var video = await client.GetVideoAsync(videoId);
        Debug.Log('✓');
        Debug.Log($"> {video.Title} by {video.Author}");

        requiredProgress = Mathf.Min(bufferTime / (float) video.Duration.TotalSeconds, 1f);

        // Get media stream info set
        Debug.Log("Obtaining media stream info set... ");
        var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);
        Debug.Log('✓');
        Debug.Log("> " +
                  $"{streamInfoSet.Muxed.Count} muxed streams, " +
                  $"{streamInfoSet.Video.Count} video-only streams, " +
                  $"{streamInfoSet.Audio.Count} audio-only streams");

        // Get the best muxed stream
        var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
        Debug.Log("Selected muxed stream with highest video quality:");
        Debug.Log("> " +
                  $"{streamInfo.VideoQualityLabel} video quality | " +
                  $"{streamInfo.Container} format | " +
                  $"{NormalizeFileSize(streamInfo.Size)}");

        // Compose file name, based on metadata
        var fileExtension = streamInfo.Container.GetFileExtension();

        fileName = $"{video.Title}.{fileExtension}";

        // Replace illegal characters in file name
        foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidFileNameChar, '_');
        }

        fileName = Path.Combine(Application.temporaryCachePath, fileName);

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        fileNameUrl = $"file://{fileName}";
        // Download video
        Debug.Log("Downloading... ");

        cancellationTokenSource = new CancellationTokenSource();
        using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
        {
            await client.DownloadMediaStreamAsync(streamInfo, stream, this, cancellationTokenSource.Token);
        }

        Debug.Log($"Video saved to '{fileName}'");

        Debug.Log($"Downloading captions...");
        
        var trackInfos = await client.GetVideoClosedCaptionTrackInfosAsync(videoId);

        var trackInfo = trackInfos.First(t => t.Language.Code == "en");
        var captionTrack = await client.GetClosedCaptionTrackAsync(trackInfo);
        captionList = new List<ClosedCaption>(captionTrack.Captions);
        
        Debug.Log($"Captions downloaded.");

    }

    public void Report(double value)
    {
        downloadProgress = value;
    }

    public bool Seek(float progress)
    {
        if(!(videoPlayer.length > 0))
            return false;

        if(!videoPlayer.canSetTime)
            return false;
        
        progress = Mathf.Clamp01(progress);
        if (downloadProgress < 1f)
        {
            //You can't seek too close if it's still buffering, say 2 seconds
            var safetyRange = 2f / videoPlayer.length;
            if (progress > downloadProgress + safetyRange)
            {
                return false;
            }
        }

        videoPlayer.time = videoPlayer.length * progress;
        return true;
    }
    
    private void Update()
    {
        bufferProgress.fillAmount = (float) downloadProgress;
        if (videoPlayer.isPlaying)
        {
            playbackProgress.fillAmount = (float) (videoPlayer.length > 0 ? videoPlayer.time / videoPlayer.length : 0);
        }
        UpdateCaption();
    }

    private void UpdateCaption()
    {
        if (captionList?.Count > 0)
        {
            var firstCaption = captionList.FirstOrDefault(c => videoPlayer.time >= c.Offset.TotalSeconds
                                                    && videoPlayer.time <= (c.Offset + c.Duration).TotalSeconds);


            var lastCaption = captionList.LastOrDefault(c => videoPlayer.time >= c.Offset.TotalSeconds
                                                    && videoPlayer.time <= (c.Offset + c.Duration).TotalSeconds);

            var currentCaptionStartIndex = captionList.IndexOf(firstCaption);
            var currentCaptionEndIndex = captionList.IndexOf(lastCaption);;

            //New captions pushed/popped
            if (currentCaptionStartIndex != captionStartIndex || currentCaptionEndIndex != captionEndIndex)
            {
                captionStartIndex = currentCaptionStartIndex;
                captionEndIndex = currentCaptionEndIndex;

                if (captionStartIndex == -1)
                {
                    captions.text = "";
                    return;
                }
                
                currentCaption.Clear();

                for (int i = captionStartIndex; i <= captionEndIndex; i++)
                {
                    var c = captionList[i];
                    currentCaption.AppendLine(c.Text);
                }

                captions.text = currentCaption.ToString();
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label($"Download progress: {downloadProgress}");
        GUILayout.Label($"Play progress: {(videoPlayer.length > 0 ? videoPlayer.time / videoPlayer.length : 0)}");
    }

    private void OnDestroy()
    {
        videoPlayer.Stop();
        cancellationTokenSource?.Cancel();
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }
}