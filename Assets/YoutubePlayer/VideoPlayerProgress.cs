using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace YoutubePlayer
{
    [RequireComponent(typeof(Image))]
    public class VideoPlayerProgress : MonoBehaviour
    {
        public VideoPlayer videoPlayer;

        private Image playbackProgress;
        private void Start()
        {
            playbackProgress = GetComponent<Image>();
            if (playbackProgress.sprite == null)
            {
                var texture = Texture2D.whiteTexture;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100);
                playbackProgress.sprite = sprite;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (videoPlayer.isPlaying)
            {
                playbackProgress.fillAmount =
                    (float) (videoPlayer.length > 0 ? videoPlayer.time / videoPlayer.length : 0);
            }
        }
    }
}
