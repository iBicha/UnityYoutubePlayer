# Decision Record for YouTube Backend Choice

## Context

In order to play videos in a Unity game, we must access the video file that can be fed to Unity's VideoPlayer. Attempting to play `https://www.youtube.com/watch?v=VIDEO_ID` does not work because it returns a web page, which in turn downloads different JavaScript scripts, CSS files, and other assets. The video file is not accessible through a simple API. Instead, other tools (that mostly rely on reverse engineering how YouTube works) can be used to "extract" the actual link to the video file that can be played.

### First Attempt: YouTubeExplode

[YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) is a library for accessing metadata of videos, channels, and playlists. Although it was more suitable for .NET projects and NuGet, adding the right packages using NuGetForUnity was enough to get the functionality running in Unity.

Since YouTube makes continuous changes to their APIs and formats, having the logic built into the Unity game meant game builds frequently broke and needed an update. This is why another approach was needed.

### Second Attempt: youtube-dl-server

A small server, [youtube-dl-server](https://github.com/iBicha/youtube-dl-server), was created for this purpose: send a request with the video ID needed, and the response would be returned in JSON, containing video metadata and data streams. While the server was simple, a small wrapper around [youtube-dl](https://github.com/ytdl-org/youtube-dl) and [yt-dlp](https://github.com/yt-dlp/yt-dlp), there were a few issues with this server:

- Lack of caching: Video metadata was fetched every time it was requested, which was inefficient.
- Poor performance: Each request would spin a separate process to run Python running youtube-dl/yt-dlp.
- It was not secure.

But the approach stood its ground for a little while. Since the server was only returning the metadata and not actually streaming the video, it was not possible to run this on WebGL due to CORS issues when trying to reach video files from YouTube servers.

### Third Attempt: youtube-dl-server + local yt-dlp

An exploration was made to execute the yt-dlp CLI locally when possible (typically on desktop platforms and Mono), which would reduce the load on servers and also reduce the amount of blocked requests. Using a server to fetch the metadata but accessing the video file from a different machine (the client) could result in blocked requests (403).

A while after this, I decided to stop maintaining the demo server. First because it performed poorly, and also because I did not budget to finance this server, regardless of how people were using it.

This resulted in many confused developers asking, "Why does it work in the editor, but when I make an Android/iOS build, it doesn't work?"

The structure of the package was obviously flawed and needed a rework:

- Works in the editor, but it was not clear to devs that they need to set up a server to even test on an Android device.
- Still have the same downsides: no WebGL support, and an inefficient server.

## Decision

### Fourth (and Current) Attempt: Invidious

The package has been reworked to use [Invidious](https://github.com/iv-org/invidious) APIs to fetch video metadata.

## Consequences

Using Invidious achieves multiple goals:

- Reduce the complexity of the previous approach: by unifying across platforms, they work the same way by making a web request to the server.
- Invidious is designed to be a server for YouTube files and contains some caching mechanisms. This is useful for a Unity project where users are viewing mostly the same videos (e.g., cutscenes in the game). Invidious would return the same metadata of a video if it is less than 10 minutes old (at the time of writing), although streams are known to expire after 6 hours.
- Invidious is self-hosted and is a good candidate for devs to be in control.
- Volunteers generously provided a public instances to use, which can be very handy in trying to integrate playing YouTube videos in a Unity game before having to set up a server. This way, public instances are suitable for testing and demo purposes, but once game devs are planning to scale, they have the freedom to host instances as needed.
- WebGL support is now possible since Invidious is capable of not only fetching video metadata and streaming data but can also stream the video itself without CORS restrictions.
