using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace YoutubePlayer
{
    public class DownloadYoutubeVideo : MonoBehaviour, IProgress<double>
    {
        public YoutubePlayer youtubePlayer;

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
           var filePath = await youtubePlayer.DownloadVideoAsync(null, null, this);
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
