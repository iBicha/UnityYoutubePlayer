using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace YoutubePlayer
{
    public class DownloadYoutubeVideo : MonoBehaviour, IProgress<double>
    {
        public YoutubePlayer youtubePlayer;
        public Environment.SpecialFolder destination;
        private Image downloadProgress;
        private float progress;

        private void Start()
        {
            downloadProgress = GetComponentsInChildren<Image>().First(image => image.gameObject != gameObject);
            if (downloadProgress.sprite == null)
            {
                var texture = Texture2D.whiteTexture;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100);
                downloadProgress.sprite = sprite;
            }
        }

        public async void Download()
        {
            Debug.Log("Downloading, please wait...");
            
            var videoDownloadTask = youtubePlayer.DownloadVideoAsync(Environment.GetFolderPath(destination), null, this);
            var captionsDownloadTask = youtubePlayer.DownloadClosedCaptions();

            var filePath = await videoDownloadTask;
            var captionTrack = await captionsDownloadTask;
            
            var srtPath = Path.ChangeExtension(filePath, ".srt");
            File.WriteAllText(srtPath, captionTrack.ToSRT());
            
            Debug.Log($"Video saved to {Path.GetFullPath(filePath)}");
        }

        public void Report(double value)
        {
            progress = (float) value;
        }

        private void Update()
        {
            downloadProgress.fillAmount = progress;
        }
    }
}