using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace YoutubePlayer
{
    public class PlayVideo : MonoBehaviour
    {
        public VideoPlayer videoPlayer;

        Button m_Button;
        
        void Awake()
        {
            m_Button = GetComponent<Button>();
            m_Button.interactable = videoPlayer.isPrepared;
           videoPlayer.prepareCompleted += VideoPlayerOnPrepareCompleted;    
        }

        void VideoPlayerOnPrepareCompleted(VideoPlayer source)
        {
            m_Button.interactable = videoPlayer.isPrepared;
        }

        public void Play()
        {
            videoPlayer.Play();
        }

        void OnDestroy()
        {
            videoPlayer.prepareCompleted -= VideoPlayerOnPrepareCompleted;    
        }
    }
}