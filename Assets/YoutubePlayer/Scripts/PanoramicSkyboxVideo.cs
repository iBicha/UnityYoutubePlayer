using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace YoutubePlayer
{
    [RequireComponent(typeof(VideoPlayer))]
    public class PanoramicSkyboxVideo : MonoBehaviour
    {
        public Material skyboxMaterial;

        public string videoTextureName;

        private VideoPlayer videoPlayer;
        void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.prepareCompleted += VideoPlayerOnPrepareCompleted;
        }

        private void VideoPlayerOnPrepareCompleted(VideoPlayer source)
        {
            if (skyboxMaterial.HasProperty(videoTextureName))
            {
                skyboxMaterial.SetTexture(videoTextureName, videoPlayer.texture);
            }
            else
            {
                skyboxMaterial.mainTexture = videoPlayer.texture;
            }
        }

        private void OnDestroy()
        {
            videoPlayer.prepareCompleted -= VideoPlayerOnPrepareCompleted;
        }
    }
}