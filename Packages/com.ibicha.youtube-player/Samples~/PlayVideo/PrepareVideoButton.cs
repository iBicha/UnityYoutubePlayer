using UnityEngine;
using YoutubePlayer.Components;

namespace YoutubePlayer.Samples.PlayVideo
{
    public class PrepareVideoButton : MonoBehaviour
    {
        public InvidiousVideoPlayer invidiousVideoPlayer;

        public async void Prepare()
        {
            Debug.Log("Loading video...");
            await invidiousVideoPlayer.PrepareVideoAsync();
            Debug.Log("Video ready");
        }
    }
}
