using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YoutubePlayer
{
    class LocalYoutubeDl : IYoutubeDl
    {
        LocalYoutubeDlUpdater m_LocalYoutubeDlUpdater = new LocalYoutubeDlUpdater("youtube-dl", "https://github.com/ytdl-org/youtube-dl/releases/latest/download/youtube-dl");
        LocalYoutubeDlUpdater m_LocalYtDlpUpdater = new LocalYoutubeDlUpdater("yt-dlp", "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp");

        public async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options,
            IEnumerable<string> schema, YoutubeDlCli cli, CancellationToken cancellationToken = default)
        {
            LocalYoutubeDlUpdater updater = null;
            switch (cli.Value)
            {
                case "youtube-dl":
                    updater = m_LocalYoutubeDlUpdater;
                    break;
                case "yt-dlp":
                    updater = m_LocalYtDlpUpdater;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cli), cli, null);
            }

            if (updater.NeedsUpdate || updater.IsUpdating)
            {
                // Update method does not get passed the cancellation token, since it is a global operation
                // Where multiple video requests might be waiting on.
                // TODO: A better design. Maybe expose updating to the public API?
                await updater.UpdateAsync();
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(updater.BinaryLocation))
            {
                throw new FileNotFoundException($"{updater.BinaryFile} binary not found.", "youtube-dl");
            }

            var arguments = BuildArguments(youtubeUrl, options);
            var stdout = await ReadProcessOutputAsync(updater.BinaryLocation, arguments, cancellationToken);

            return JsonConvert.DeserializeObject<T>(stdout);
        }

        string BuildArguments(string youtubeUrl, YoutubeDlOptions options)
        {
            var arguments = "--dump-single-json";
            if (!string.IsNullOrWhiteSpace(options.Format))
            {
                arguments += $" -f \"{options.Format}\"";
            }
            if (options.UserAgent != null)
            {
                arguments += $" --user-agent \"{options.UserAgent}\"";
            }
            if (options.Custom != null)
            {
                arguments += $" {options.Custom}";
            }

            arguments += $" \"{youtubeUrl}\"";

            return arguments;
        }

        Task<string> ReadProcessOutputAsync(string filename, string arguments, CancellationToken cancellationToken = default)
        {
            var execName = Path.GetFileName(filename);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true,
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    stdout.AppendLine(args.Data);
                }
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    stderr.AppendLine(args.Data);
                }
            };

            var taskCompletionSource = new TaskCompletionSource<string>();

            process.Exited += (sender, args) =>
            {
                // It's possible that Exited fires before ErrorDataReceived event.
                // WaitForExit would for us to wait until we receive the EOF on the output stream.
                process.WaitForExit();
                var output = stdout + "\n" + stderr;
                if (process.ExitCode != 0)
                {
                    taskCompletionSource.TrySetException(new Exception($"{execName} existed with code {process.ExitCode}. \n{output}"));
                    return;
                }

                taskCompletionSource.TrySetResult(output);
            };

            try
            {
                process.Start();

                cancellationToken.Register(obj =>
                {
                    ((Process)obj).Kill();
                }, process);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }

            return taskCompletionSource.Task;
        }
    }
}
