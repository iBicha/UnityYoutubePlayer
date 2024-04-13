using UnityEngine;
using UnityEngine.Video;

namespace YoutubePlayer.Samples.Three60EquiRectangular
{
    [RequireComponent(typeof(VideoPlayer))]
    public class PanoramicSkyboxVideo : MonoBehaviour
    {
        public Material skyboxMaterial;

        public string videoTextureName;

        VideoPlayer m_VideoPlayer;
        void Awake()
        {
            m_VideoPlayer = GetComponent<VideoPlayer>();
            m_VideoPlayer.prepareCompleted += VideoPlayerOnPrepareCompleted;
        }

        void VideoPlayerOnPrepareCompleted(VideoPlayer source)
        {
            if (skyboxMaterial.HasProperty(videoTextureName))
            {
                skyboxMaterial.SetTexture(videoTextureName, m_VideoPlayer.texture);
            }
            else
            {
                skyboxMaterial.mainTexture = m_VideoPlayer.texture;
            }
        }

        void OnDestroy()
        {
            m_VideoPlayer.prepareCompleted -= VideoPlayerOnPrepareCompleted;
        }
    }
}
