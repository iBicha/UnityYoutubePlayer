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
        public Environment.SpecialFolder destination;
        Image m_DownloadProgress;
        float m_Progress;

        void Start()
        {
            m_DownloadProgress = GetComponentsInChildren<Image>().First(image => image.gameObject != gameObject);
            if (m_DownloadProgress.sprite == null)
            {
                var texture = Texture2D.whiteTexture;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100);
                m_DownloadProgress.sprite = sprite;
            }
        }

        public async void Download()
        {
            Debug.Log("Downloading, please wait...");
            
            var videoDownloadTask = youtubePlayer.DownloadVideoAsync(Environment.GetFolderPath(destination), null, this);
            var filePath = await videoDownloadTask;
            
            Debug.Log($"Video saved to {Path.GetFullPath(filePath)}");
        }

        public void Report(double value)
        {
            m_Progress = (float) value;
        }

        private void Update()
        {
            m_DownloadProgress.fillAmount = m_Progress;
        }
    }
}