using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubePlayer
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the muxed stream with highest video quality and Mp4 container.
        /// Returns null if sequence is empty.
        /// </summary>
        public static MuxedStreamInfo WithHighestVideoQualitySupported(this IEnumerable<MuxedStreamInfo> streamInfos)
        {
            if(streamInfos == null)
                throw new ArgumentNullException(nameof(streamInfos));
            return streamInfos
                .Where(info => info.Container == Container.Mp4)
                .Select(info => info).OrderByDescending(s => s.VideoQuality).FirstOrDefault();
        }
    
        /// <summary>
        /// Gets the muxed stream with highest video quality and Mp4 container.
        /// Returns null if sequence is empty.
        /// </summary>
        public static MuxedStreamInfo WithHighestVideoQualitySupported(this MediaStreamInfoSet streamInfoSet)
        {
            if(streamInfoSet == null)
                throw new ArgumentNullException(nameof(streamInfoSet));
            return streamInfoSet.Muxed.WithHighestVideoQualitySupported();
        }

        /// <summary>
        /// Convert closed captions to SRT format.
        /// </summary>
        /// <param name="closedCaptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">When closedCaptions is null</exception>
        public static string ToSRT(this IEnumerable<ClosedCaption> closedCaptions)
        {
            if(closedCaptions == null)
                throw new ArgumentNullException(nameof(closedCaptions));

            var buffer = new StringBuilder();
            var lineNumber = 1;
            
            foreach (var caption in closedCaptions)
            {
                // Line number
                buffer.AppendLine((lineNumber).ToString());

                // Time start --> time end
                buffer.Append(caption.Offset.ToString(@"hh\:mm\:ss\,fff"));
                buffer.Append(" --> ");
                buffer.Append((caption.Offset + caption.Duration).ToString(@"hh\:mm\:ss\,fff"));
                buffer.AppendLine();

                // Actual text
                buffer.AppendLine(caption.Text);
                buffer.AppendLine();
                
                lineNumber++;
            }

            return buffer.ToString();
        }
     
        /// <summary>
        /// Convert closed captions to SRT format.
        /// </summary>
        /// <param name="closedCaptionTrack"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">When closedCaptionTrack is null</exception>
        public static string ToSRT(this ClosedCaptionTrack closedCaptionTrack)
        {
            if(closedCaptionTrack == null)
                throw new ArgumentNullException(nameof(closedCaptionTrack));

            return closedCaptionTrack.Captions.ToSRT();
        }
    }
}
