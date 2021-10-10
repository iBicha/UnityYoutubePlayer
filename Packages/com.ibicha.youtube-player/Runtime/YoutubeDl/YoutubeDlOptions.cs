namespace YoutubePlayer
{
    public class YoutubeDlOptions
    {
        public static readonly YoutubeDlOptions Default = new YoutubeDlOptions("best");
        public static readonly YoutubeDlOptions Three60 = new YoutubeDlOptions("bestvideo[height<=?1080]", "");
        public static readonly YoutubeDlOptions FlatPlaylist = new YoutubeDlOptions("best", null, "--flat-playlist");

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
