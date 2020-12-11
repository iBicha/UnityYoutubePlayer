namespace YoutubePlayer
{
    public class YoutubeDlOptions
    {
        public static readonly YoutubeDlOptions Default = new YoutubeDlOptions("best");

        public YoutubeDlOptions(string format)
        {
            Format = format;
        }

        public string Format;
    }

}