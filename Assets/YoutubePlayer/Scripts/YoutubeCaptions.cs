using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using YoutubeExplode.Models.ClosedCaptions;

namespace YoutubePlayer
{
    /// <summary>
    /// Downloads and plays Youtube captions on a TextMesh Pro component.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class YoutubeCaptions : MonoBehaviour
    {
        /// <summary>
        /// A YoutubePlayer component to synchronize captions with
        /// </summary>
        public YoutubePlayer youtubePlayer;
        
        private VideoPlayer videoPlayer;

        private TMP_Text captionsText;
        private StringBuilder currentCaption = new StringBuilder();

        private string currentVideoUrl;
        private List<ClosedCaption> captionList;
        private int captionStartIndex;
        private int captionEndIndex;

        private void Awake()
        {
            videoPlayer = youtubePlayer.GetComponent<VideoPlayer>();
            captionsText = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (youtubePlayer == null) return;
            youtubePlayer.YoutubeVideoStarting += OnYoutubeVideoStarting;
        }

        private void OnDisable()
        {
            if (youtubePlayer == null) return;
            youtubePlayer.YoutubeVideoStarting -= OnYoutubeVideoStarting;
        }

        private async void OnYoutubeVideoStarting(string url)
        {
            if (url == currentVideoUrl) return;

            captionList = await DownloadCaptionsAsync();
            if (captionList != null)
                currentVideoUrl = url;
        }

        /// <summary>
        /// Download closed captions for the current Youtube Player
        /// </summary>
        /// <returns>A ClosedCaptionTrack object.</returns>
        public async Task<List<ClosedCaption>> DownloadCaptionsAsync()
        {
            var track = await youtubePlayer.DownloadClosedCaptions();
            return (track?.Captions?.Count ?? 0) == 0 ? null : new List<ClosedCaption>(track.Captions);
        }

        private void Update()
        {
            UpdateCaption();
        }

        private void UpdateCaption()
        {
            if (!(captionList?.Count > 0)) return;

            var firstCaption = captionList.FirstOrDefault(c => videoPlayer.time >= c.Offset.TotalSeconds
                                                               && videoPlayer.time <=
                                                               (c.Offset + c.Duration).TotalSeconds);

            var lastCaption = captionList.LastOrDefault(c => videoPlayer.time >= c.Offset.TotalSeconds
                                                             && videoPlayer.time <=
                                                             (c.Offset + c.Duration).TotalSeconds);

            var currentCaptionStartIndex = captionList.IndexOf(firstCaption);
            var currentCaptionEndIndex = captionList.IndexOf(lastCaption);

            //New captions pushed/popped
            if (currentCaptionStartIndex != captionStartIndex || currentCaptionEndIndex != captionEndIndex)
            {
                captionStartIndex = currentCaptionStartIndex;
                captionEndIndex = currentCaptionEndIndex;

                if (captionStartIndex == -1)
                {
                    captionsText.text = "";
                    return;
                }

                currentCaption.Clear();

                for (int i = captionStartIndex; i <= captionEndIndex; i++)
                {
                    var c = captionList[i];
                    currentCaption.AppendLine(c.Text);
                }

                captionsText.text = currentCaption.ToString();
            }
        }
    }
}