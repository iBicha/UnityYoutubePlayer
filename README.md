# UnityYoutubePlayerTest
Play youtube videos in Unity using YoutubeExplode and Unity's VideoPlayer.

Uses [Tyrrrz/YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) for handling video and caption downloading.

Then it runs a http server to serve the video back into the player.
It also replaces the youtube origial url with the appropriate localhost url so that the video player can support youtube urls by default (see sample scene for usage)

![](screenshot.png)
