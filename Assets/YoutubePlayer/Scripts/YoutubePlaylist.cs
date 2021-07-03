using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

namespace YoutubePlayer
{
    public class YoutubePlaylist : MonoBehaviour
    {
        static readonly string[] k_PlaylistFields = { "title", "entries" };

        public string playlistUrl;

        public Text PlaylistTitle;
        public RectTransform PlaylistContent;
        public GameObject PlaylistScrollView;
        public GameObject PlaylistItemPrefab;
        public GameObject PlaylistItemPlayerPrefab;

        async void Start()
        {
            var playList = await YoutubeDl.GetVideoMetaDataAsync<YoutubePlaylistFlatMetadata>(playlistUrl,
                YoutubeDlOptions.FlatPlaylist, k_PlaylistFields);

            PlaylistTitle.text = playList.Title;

            foreach (var entry in playList.Entries)
            {
                var button = Instantiate(PlaylistItemPrefab, PlaylistContent);
                button.GetComponentInChildren<Text>().text = entry.Title;
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    PlaylistTitle.text = entry.Title;
                    PlaylistScrollView.SetActive(false);
                    var playerObject = Instantiate(PlaylistItemPlayerPrefab);
                    playerObject.GetComponent<VideoPlayer>().targetCamera = Camera.main;
                    playerObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        Destroy(playerObject);
                        PlaylistScrollView.SetActive(true);
                        PlaylistTitle.text = playList.Title;
                    });
                    var player = playerObject.GetComponent<YoutubePlayer>();
                    player.PlayVideoAsync(entry.Url);
                });
            }
        }
    }
}
