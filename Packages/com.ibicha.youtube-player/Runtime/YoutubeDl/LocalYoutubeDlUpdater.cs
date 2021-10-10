using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define UNITY_WINDOWS
#endif

namespace YoutubePlayer
{
    class LocalYoutubeDlUpdater
    {
#if UNITY_WINDOWS
        const string k_BinaryFile = "youtube-dl.exe";
        const string k_BinaryUrl = "https://yt-dl.org/downloads/latest/youtube-dl.exe";
#else
        const string k_BinaryFile = "youtube-dl";
        const string k_BinaryUrl = "https://yt-dl.org/downloads/latest/youtube-dl";
#endif
        const string k_LocalVersionKey = "youtube-dl-version";
        const string k_LastUpdateCheckKey = "youtube-dl-last-update-check";

        // If we checked for updates less than 1 day ago, no need to check again.
        static readonly TimeSpan k_UpdateCheckInterval = new TimeSpan(1, 0 , 0, 0);

        public string BinaryLocation { get; }

        public bool DidUpdate { get; private set; }

        Task m_CurrentUpdateTask;

        DateTime LastUpdateCheckUtc
        {
            get
            {
                // DateTime.MinValue.Ticks is 0
                var ticksString = PlayerPrefs.GetString(k_LastUpdateCheckKey, "0");

                if (!long.TryParse(ticksString, out var ticks))
                {
                    return DateTime.MinValue;
                }

                return new DateTime(ticks, DateTimeKind.Utc);
            }
            set => PlayerPrefs.SetString(k_LastUpdateCheckKey, value.Ticks.ToString());
        }

        string LocalVersion
        {
            get => PlayerPrefs.GetString(k_LocalVersionKey, null);
            set => PlayerPrefs.SetString(k_LocalVersionKey, value);
        }

        public LocalYoutubeDlUpdater()
        {
            BinaryLocation = Path.Combine(Application.persistentDataPath, k_BinaryFile);
        }

        public async Task UpdateAsync(CancellationToken cancellationToken = default)
        {
            if (m_CurrentUpdateTask != null)
            {
                await m_CurrentUpdateTask;
                return;
            }

            var updateCompletionSource = new TaskCompletionSource<object>();
            m_CurrentUpdateTask = updateCompletionSource.Task;

            try
            {
                var latestVersion = await CheckForUpdatesAsync(cancellationToken);
                if (string.IsNullOrEmpty(latestVersion))
                {
                    return;
                }

                await DownloadFile(k_BinaryUrl, BinaryLocation);
                await MakeBinaryExecutableAsync(BinaryLocation);
                LocalVersion = latestVersion;
                DidUpdate = true;
                updateCompletionSource.TrySetResult(null);
            }
            catch (Exception exception)
            {
                updateCompletionSource.TrySetException(exception);
            }
            finally
            {
                m_CurrentUpdateTask = null;
            }
        }

        async Task<string> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var localBinaryExists = File.Exists(BinaryLocation);
                if (localBinaryExists)
                {
                    if (LastUpdateCheckUtc + k_UpdateCheckInterval > DateTime.UtcNow)
                    {
                        return null;
                    }
                }

                var latestVersion = await GetLatestVersionAsync(cancellationToken);
                if (string.IsNullOrEmpty(latestVersion))
                {
                    return null;
                }

                LastUpdateCheckUtc = DateTime.UtcNow;

                if (!localBinaryExists)
                {
                    return latestVersion;
                }

                return LocalVersion != latestVersion ? latestVersion : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        Task<string> GetLatestVersionAsync(CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            var request = new UnityWebRequest(k_BinaryUrl, "HEAD");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.redirectLimit = 1;
            request.SendWebRequest().completed += operation =>
            {
                var location = request.GetResponseHeader("Location");
                taskCompletionSource.TrySetResult(location);
            };

            cancellationToken.Register(obj =>
            {
                ((UnityWebRequest)obj).Abort();
                taskCompletionSource.TrySetCanceled(cancellationToken);
            }, request);

            return taskCompletionSource.Task;
        }

        Task DownloadFile(string url, string destination)
        {
            var requestTaskSource = new TaskCompletionSource<object>();
            var request = new UnityWebRequest(url, "GET");
            request.downloadHandler = new DownloadHandlerFile(destination);
            request.SendWebRequest().completed += operation =>
            {
                if (!string.IsNullOrEmpty(request.error))
                {
                    requestTaskSource.TrySetException(new Exception(request.error));
                    return;
                }

                requestTaskSource.TrySetResult(null);
            };

            return requestTaskSource.Task;
        }

        Task MakeBinaryExecutableAsync(string filename)
        {
#if UNITY_WINDOWS
            return Task.CompletedTask;
#else
            var taskCompletionSource = new TaskCompletionSource<object>();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{filename}\""
                }
            };

            process.EnableRaisingEvents = true;

            process.Exited += (sender, args) =>
            {
                if (process.ExitCode == 0)
                {
                    taskCompletionSource.TrySetResult(null);
                }
                else
                {
                    taskCompletionSource.TrySetException(new Exception($"chmod failed with exit code: {process.ExitCode}"));
                }
            };

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }

            return taskCompletionSource.Task;
#endif
        }
    }
}
