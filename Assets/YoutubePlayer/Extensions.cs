using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
