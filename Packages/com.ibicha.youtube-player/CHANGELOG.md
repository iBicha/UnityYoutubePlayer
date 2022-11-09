# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.1] - 2022-11-08
### Fixed
- error message missing from exception
- Fixed 360 rending by specifying the `--extractor-args "youtube:player-client=web"` option

### Changed
- yt-dlp is now the default extractor, since youtube-dl did not receive a release for a long time

## [2.0.0] - 2022-07-20
### Fixed
- Added exception for when yt-dlp is missing locally

### Changed
- **BREAKING CHANGE:** Changed type `YoutubeVideoFormat.Fps` from `int` to `float`. 
  - This is needed since youtube-dl started returning decimal numbers for frame rate, causing deserialization errors.

## [1.7.0] - 2022-07-12
### Added
- Added support for yt-dlp fork - see https://github.com/yt-dlp/yt-dlp
  - yt-dlp is now the default tool to use in YoutubePlayer.cs, as it is able to bypass the download speed limit when using a local instance of the tool.
  - youtube-dl-server also supports yt-dlp, but might still hit download speed limits, as it has a different IP address than the machine running Unity.

### Changed
- Updated Json.NET - Newtonsoft dependency 

## [1.6.0] - 2021-10-10
### Added
- Example scene to load a playlist

## [1.5.1] - 2021-10-10
### Added
- YoutubeDl will now try to use a local instance of youtube-dl instead of pinging a server. 
  - This is turned on by default. Use `YoutubeDl.UseLocalInstance` to change the behaviour
  - Playing the video will automatically download youtube-dl and check for updates.
  - This feature is only available for Desktop platforms.

## [1.4.1] - 2021-07-02
### Changed
- Changed YoutubeDl Api to avoid ambiguous calls

## [1.4.0] - 2021-07-02
### Added
- A CHANGELOG.md, README.md and LICENSE files.
- Schema to allow downloading only needed fields
  - For playing a video, only the `url` field is requested.
  - For downloading a video, only `title`, `_filename` , `ext` and `url` fields are requeted.

### Changed
- Changed Unity minimum version to 2019.4

### Fixed 
- `JsonSerializationException` on IL2CPP by adding a default .ctor to model files
