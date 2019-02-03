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
    [RequireComponent(typeof(TMP_Text))]
    public class YoutubeCaptions : MonoBehaviour
    {
        public YoutubePlayer youtubePlayer;
        private VideoPlayer videoPlayer;

        private TMP_Text captionsText;
        private StringBuilder currentCaption = new StringBuilder();

        private List<ClosedCaption> captionList;
        private int captionStartIndex;
        private int captionEndIndex;

        private async void Start()
        {
            videoPlayer = youtubePlayer.GetComponent<VideoPlayer>();
            captionsText = GetComponent<TMP_Text>();

            captionList = await DownloadCaptionsAsync();
            if (captionList == null)
                enabled = false;
        }

        public async Task<List<ClosedCaption>> DownloadCaptionsAsync()
        {
            var track = await youtubePlayer.DownloadClosedCaptions();
            return (track?.Captions?.Count ?? 0) == 0 ? null : new List<ClosedCaption>(track.Captions);
        }

        void Update()
        {
            UpdateCaption();
        }


        private void UpdateCaption()
        {
            if (captionList?.Count > 0)
            {
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
}