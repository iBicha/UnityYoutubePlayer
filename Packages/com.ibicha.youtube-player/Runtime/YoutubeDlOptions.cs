namespace YoutubePlayer
{
    public class YoutubeDlOptions
    {
        public static readonly YoutubeDlOptions Default = new YoutubeDlOptions("best");
        public static readonly YoutubeDlOptions Three60 = new YoutubeDlOptions("bestvideo[height<=?1080]", "");

        public YoutubeDlOptions(string format, string userAgent = null)
        {
            Format = format;
            UserAgent = userAgent;
        }

        public string Format;
        public string UserAgent;
    }

}