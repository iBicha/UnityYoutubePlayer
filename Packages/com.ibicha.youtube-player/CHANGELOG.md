# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
