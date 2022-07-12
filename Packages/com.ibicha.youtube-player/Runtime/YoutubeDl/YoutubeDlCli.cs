namespace YoutubePlayer
{
    public class YoutubeDlCli
    {
        public YoutubeDlCli(string value) { Value = value; }
        public string Value { get; }

        /// <summary>
        /// https://github.com/ytdl-org/youtube-dl
        /// </summary>
        public static YoutubeDlCli YoutubeDl => new YoutubeDlCli("youtube-dl");

        /// <summary>
        /// https://github.com/yt-dlp/yt-dlp
        /// </summary>
        public static YoutubeDlCli YtDlp => new YoutubeDlCli("yt-dlp");
    }
}
