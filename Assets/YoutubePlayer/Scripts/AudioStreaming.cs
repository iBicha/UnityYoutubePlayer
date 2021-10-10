using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace YoutubePlayer
{
    public class AudioStreaming : MonoBehaviour
    {
        public string youtubeUrl;

        public AudioType audioType;

        public AudioSource audioSource;

        void Start()
        {
            Debug.Log($"Loading {audioType} from {youtubeUrl}");

            // Server requires an ffmpeg installation
            // https://github.com/iBicha/youtube-dl-server/tree/feature/audio-stream

            var requestUrl = BuildRequestUrl();
            Debug.Log($"Request url: {requestUrl}");
            var downloadHandler = new DownloadHandlerAudioClip(requestUrl, audioType)
            {
                streamAudio = true,
            };
            var request = new UnityWebRequest(requestUrl, "GET", downloadHandler, null);
            // TODO: Can't create audio clip until request is finished - how do we stream on the go?
            request.SendWebRequest().completed += operation =>
            {
                if (!string.IsNullOrEmpty(request.error))
                {
                    throw new Exception(request.error);
                }
                audioSource.clip = downloadHandler.audioClip;
                audioSource.Play();
            };
        }

        string BuildRequestUrl()
        {
            var baseUrl = "http://localhost:3000/v1/stream/audio";
            // youtube-dl format
            var inputFormat = "bestaudio";
            // ffmpeg output format
            var outputFormat = GetOutputFormat();
            var options = new Dictionary<string, string>
            {
                ["input"] = inputFormat,
                ["output"] = outputFormat,
                ["url"] = UnityWebRequest.EscapeURL(youtubeUrl),
            };
            return $"{baseUrl}?{string.Join("&", options.Select(pair => $"{pair.Key}={pair.Value}"))}";
        }

        string GetOutputFormat()
        {
            switch (audioType)
            {
                case AudioType.MPEG:
                    return "mp3";
                case AudioType.OGGVORBIS:
                    return "ogg";

                // Does not work in Unity, but works in browser for some reason
                // case AudioType.WAV:
                //     return "wav";

                default:
                    throw new NotSupportedException($"{audioType} not supported. Only mp3 and ogg.");
            }
        }
    }
}
