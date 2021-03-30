# UnityYoutubePlayer
Play and download youtube videos in Unity using [youtube-dl](https://github.com/ytdl-org/youtube-dl) and Unity's VideoPlayer, and uses [youtube-dl-server](https://github.com/iBicha/youtube-dl-server) to fetch metadata from youtube-dl.

## Preview
<img src="screenshot.png" width="400" />

## How To Install

This package uses the [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html) feature to import dependent
packages. Please add the following sections to the package manifest file
(`Packages/manifest.json`).

To the `scopedRegistries` section:

```
{
  "name": "iBicha",
  "url": "https://registry.npmjs.com",
  "scopes": [ "com.ibicha" ]
}
```

To the `dependencies` section:

```
"com.ibicha.youtube-player": "1.3.3"
```

After changes, the manifest file should look like below:

```
{
  "scopedRegistries": [
    {
      "name": "iBicha",
      "url": "https://registry.npmjs.com",
      "scopes": [ "com.ibicha" ]
    }
  ],
  "dependencies": {
    "com.ibicha.youtube-player": "1.3.3",
    ...
```

## Usage
- A minimalistic example:
```
public class SimpleYoutubeVideo : MonoBehaviour
{
    void Start()
    {
        GetComponent<VideoPlayer>().PlayYoutubeVideoAsync("https://www.youtube.com/watch?v=1PuGuqpHQGo");
    }
}
```
Or,

- Add a `YoutubePlayer` component on a `GameObject` with a `VideoPlayer`. Set the url in the inspector.
The `YoutubePlayer` will follow the `Play On Awake` setting of the video player. You can also call `YoutubePlayer.PlayVideoAsync`.

- In addition, you can call `YoutubePlayer.DownloadVideoAsync` to download the video to a file instead. See the `DownloadYoutubeVideo` as an example.

- `VideoPlayerProgress` allows to display the progress of the video, as well as seeking.

- See `Assets\YoutubePlayer\Scenes` for more examples.

## Dependencies
UnityYoutubePlayer uses [youtube-dl](https://github.com/ytdl-org/youtube-dl) for parsing webpages and getting a raw video url that Unity's VideoPlayer can play.
To allow maximum platform compatibilty (e.g. mobile, desktop) and to be able to update the library without rebuilding the game, we're using [youtube-dl-server](https://github.com/iBicha/youtube-dl-server) web API.

The package uses a free instance of the server hosted on heroku (shared between everyone). 
For better reliability and performance, it is recommanded to host this on your own.

### How do I host my own youtube-dl server?
 - Go to [youtube-dl-server](https://github.com/iBicha/youtube-dl-server)
 - Click the `Deploy to Heroku` purple button
 - Finish the setup on Heroku.
 - Make `YoutubePlayer` use your newly hosted instance:
To make `YoutubePlayer` use your instance of the server, make sure to set the `ServerUrl` before making calls to youtube APIs, e.g.
```
YoutubeDl.ServerUrl = "http://your-self-hosted-server.com";
```
For reference, the default instance is 
```
YoutubeDl.ServerUrl = "https://unity-youtube-dl-server.herokuapp.com";
```
And you can test it in the browser: https://unity-youtube-dl-server.herokuapp.com/v1/video?url=https://www.youtube.com/watch?v=1PuGuqpHQGo
## Older version
For the version used with [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode), see the [legacy/youtube-explode](https://github.com/iBicha/UnityYoutubePlayer/tree/legacy/youtube-explode) branch. Please note that it has a different license than the current version.
