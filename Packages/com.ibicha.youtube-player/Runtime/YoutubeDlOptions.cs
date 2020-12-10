namespace YoutubePlayer
{
    public class YoutubeDlOptions
    {
        public static readonly YoutubeDlOptions Default = new YoutubeDlOptions("best", false);

        public YoutubeDlOptions(string format, bool subtitles)
        {
            Format = format;
            Subtitles = subtitles;
        }

        public string Format;
        public bool Subtitles;
    }

}