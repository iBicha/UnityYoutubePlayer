using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

[RequireComponent(typeof(VideoPlayer))]
public class YoutubeTest : MonoBehaviour, IProgress<double>
{
    public float bufferTime = 10f;
    
    public string targetVideo;

    public Image bufferProgress;
    public Image playbackProgress;
    
    private VideoPlayer videoPlayer;
    private double downloadProgress;
    private string fileNameUrl;
    private string fileName;
    private float requiredProgress = 1f;
    private CancellationTokenSource cancellationTokenSource;
    
    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        var texture = Texture2D.whiteTexture;
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100);
        bufferProgress.sprite = playbackProgress.sprite = sprite;
    }

    // Start is called before the first frame update
    async void Start()
    {
        MainAsync();
        StartCoroutine(PlayVideoAfterProgress());
    }

    IEnumerator PlayVideoAfterProgress()
    {
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

    private async Task MainAsync()
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
        Debug.Log("");
        
        requiredProgress = bufferTime / (float)video.Duration.TotalSeconds;
        
        // Get media stream info set
        Debug.Log("Obtaining media stream info set... ");
        var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);
        Debug.Log('✓');
        Debug.Log("> " +
                  $"{streamInfoSet.Muxed.Count} muxed streams, " +
                  $"{streamInfoSet.Video.Count} video-only streams, " +
                  $"{streamInfoSet.Audio.Count} audio-only streams");
        Debug.Log("");

        // Get the best muxed stream
        var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
        Debug.Log("Selected muxed stream with highest video quality:");
        Debug.Log("> " +
                  $"{streamInfo.VideoQualityLabel} video quality | " +
                  $"{streamInfo.Container} format | " +
                  $"{NormalizeFileSize(streamInfo.Size)}");
        Debug.Log("");

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
        
        Debug.Log("");

        Debug.Log($"Video saved to '{fileName}'");
    }

    public void Report(double value)
    {
        downloadProgress = value;
        Debug.Log($"Progress: {value}");
    }

    private void Update()
    {
        bufferProgress.fillAmount = (float) downloadProgress;
        playbackProgress.fillAmount = (float) (videoPlayer.length > 0 ? videoPlayer.time / videoPlayer.length : 0);
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