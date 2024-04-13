<!-- markdownlint-disable MD033 -->

# UnityYoutubePlayer

Play YouTube videos in Unity. Uses [Invidious](https://github.com/iv-org/invidious) to fetch the video metadata and Unity's VideoPlayer to play the video.

**Important:** The package is now at version 3 and contains breaking changes. If you want to know why this package evolved this way, please see [this document](./adr-backend.md)

## Preview

<img alt="Preview" src="screenshot.png" width="400" />

## How To Install

This package uses the [scoped registry](https://docs.unity3d.com/Manual/upm-scoped.html) feature to import dependent
packages. Please add the following sections to the package manifest file
(`Packages/manifest.json`).

To the `scopedRegistries` section:

```json
{
  "name": "iBicha",
  "url": "https://registry.npmjs.com",
  "scopes": [ "com.ibicha" ]
}
```

To the `dependencies` section:

```json
"com.ibicha.youtube-player": "3.2.0"
```

After changes, the manifest file should look like below:

```json
{
  "scopedRegistries": [
    {
      "name": "iBicha",
      "url": "https://registry.npmjs.com",
      "scopes": [ "com.ibicha" ]
    }
  ],
  "dependencies": {
    "com.ibicha.youtube-player": "3.2.0",
    ...
```

## Usage

See the package samples for more usage examples. They can be imported from the package manager.

## Invidious instances

Starting with UnityYoutubePlayer 3.0.0, we're using [https://github.com/iv-org/invidious](https://github.com/iv-org/invidious), a self-hosted alternative front-end to YouTube.

Invidious has a community of volenteers who provided public instances, which can be found at [https://api.invidious.io](https://api.invidious.io). These are great for getting familiar with Invidious, and for testing, prototyping, and demos. Once you're ready for scaling your game and anticipate a lot of traffic, please consider hosting your own instances to avoid abusing these resources.

## Older versions

For the version used with [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode), see the [legacy/youtube-explode](https://github.com/iBicha/UnityYoutubePlayer/tree/legacy/youtube-explode) branch. Please note that it has a different license than the current version.

For other versions, see the git tags.
Starting with 1.5.1, this package downloads and uses `youtube-dl` locally on Desktop platforms.
Starting with 1.7.0, this package downloads and uses `yt-dlp` locally on Desktop platforms, in addition to `youtube-dl`.
