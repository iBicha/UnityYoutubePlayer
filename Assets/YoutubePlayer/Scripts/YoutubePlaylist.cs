using System;
using UnityEngine;
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
                button.name = entry.Url;
                button.GetComponentInChildren<Text>().text = entry.Title;
                button.GetComponent<Button>().onClick.AddListener(OnItemClicked);

                async void OnItemClicked()
                {
                    PlaylistTitle.text = entry.Title;
                    PlaylistScrollView.SetActive(false);
                    var playerObject = Instantiate(PlaylistItemPlayerPrefab);
                    playerObject.GetComponent<VideoPlayer>().targetCamera = Camera.main;
                    playerObject.GetComponentInChildren<Button>().onClick.AddListener(OnCloseButtonClicked);
                    var player = playerObject.GetComponent<YoutubePlayer>();
                    await player.PlayVideoAsync(GetFullUrl(entry.Url));

                    void OnCloseButtonClicked()
                    {
                        Destroy(playerObject);
                        PlaylistScrollView.SetActive(true);
                        PlaylistTitle.text = playList.Title;
                    }
                }
            }
        }

        // Because Urls in the playlist are only the video ID, they can start with a
        // "-" (hyphen) character which causes an issue with youtube-dl
        // A workaround is to provide the full youtube url.
        static string GetFullUrl(string Url)
        {
            if (Url.StartsWith("http", StringComparison.Ordinal))
            {
                return Url;
            }
            return $"https://www.youtube.com/watch?v={Url}";
        }
    }
}
