namespace YoutubePlayer
{
    public class YoutubeDlOptions
    {
        public static readonly YoutubeDlOptions Default = new YoutubeDlOptions("best", null, "--no-warnings");
        public static readonly YoutubeDlOptions Three60 = new YoutubeDlOptions("bestvideo[height<=?1080]", "", "--no-warnings");
        public static readonly YoutubeDlOptions FlatPlaylist = new YoutubeDlOptions("best", null, "--flat-playlist --no-warnings");

        public YoutubeDlOptions(string format, string userAgent = null, string custom = null)
        {
            Format = format;
            UserAgent = userAgent;
            Custom = custom;
        }

        public string Format;
        public string UserAgent;
        public string Custom;
    }
}
