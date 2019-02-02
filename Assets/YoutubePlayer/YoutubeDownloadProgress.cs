using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace YoutubePlayer
{
    [RequireComponent(typeof(Image))]
    public class YoutubeDownloadProgress : MonoBehaviour
    {
        public YoutubePlayer youtubePlayer;

        private Image bufferProgress;
        private string videoId;
        private void Start()
        {
            bufferProgress = GetComponent<Image>();
            if (bufferProgress.sprite == null)
            {
                var texture = Texture2D.whiteTexture;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100);
                bufferProgress.sprite = sprite;
            }

            //A little hacky, but enough for the sake of the example.
            var url = youtubePlayer.GetComponent<VideoPlayer>().url;
            videoId = url.Substring(url.LastIndexOf("v=", StringComparison.InvariantCultureIgnoreCase) + 2);
        }
    
        // Update is called once per frame
        void Update()
        {
            bufferProgress.fillAmount = youtubePlayer.GetDownloadProgressForVideo(videoId);
        }
    }
}
