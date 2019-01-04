using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class SeekInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public YoutubeTest youtubePlayer;

    private VideoPlayer videoPlayer;
    private Image playbackProgress;

    private void Awake()
    {
        videoPlayer = youtubePlayer.GetComponent<VideoPlayer>();
        playbackProgress = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Seek(Input.mousePosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Seek(Input.mousePosition);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        videoPlayer.Pause();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        videoPlayer.Play();
    }

    private void Seek(Vector2 cursorPosition)
    {
        var progress = cursorPosition.x / Screen.width;
        if (youtubePlayer.Seek(progress))
        {
            playbackProgress.fillAmount = progress;
        }
    }
}