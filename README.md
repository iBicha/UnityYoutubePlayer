# UnityYoutubePlayer
Play and download youtube videos in Unity using YoutubeExplode and Unity's VideoPlayer.

Uses for handling video and caption downloading.


## Preview
![](screenshot.png)

## Usage
- Add a `YoutubePlayer` component on a `GameObject` with a `VideoPlayer`. Set the url in the inspector.
The `YoutubePlayer` will follow the `Play On Awake` setting of the video player. You can also call `YoutubePlayer.PlayVideoAsync`.

- In addition, you can call `YoutubePlayer.DownloadVideoAsync` to download the video to a file instead. See the `DownloadYoutubeVideo` as an example.

- `VideoPlayerProgress` allows to display the progress of the video, as well as seeking.

- Captions can be downloaded and displayed on a TextMesh Pro component with the `YoutubeCaptions` script.

- See `Scenes\SampleScene` for a complete example.

## Dependencies
UnityYoutubePlayer relies heavily on the work done in [Tyrrrz/YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) which is responsible for parsing and downloading videos and captions. This library has the following dependencies:

- [AngleSharp (Github)](https://github.com/AngleSharp/AngleSharp)
- [Newtonsoft.Json (Asset Store)](https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347)

In addition, we use `TextMesh Pro` to display captions (but this can be easily swapped to use another kind of text UI)
