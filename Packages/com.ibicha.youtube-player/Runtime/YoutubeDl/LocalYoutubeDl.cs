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
        LocalYoutubeDlUpdater m_LocalYoutubeDlUpdater = new LocalYoutubeDlUpdater();

        public async Task<T> GetVideoMetaDataAsync<T>(string youtubeUrl, YoutubeDlOptions options, IEnumerable<string> schema, CancellationToken cancellationToken = default)
        {
            if (m_LocalYoutubeDlUpdater.NeedsUpdate || m_LocalYoutubeDlUpdater.IsUpdating)
            {
                // Update method does not get passed the cancellation token, since it is a global operation
                // Where multiple video requests might be waiting on.
                // TODO: A better design. Maybe expose updating to the public API?
                await m_LocalYoutubeDlUpdater.UpdateAsync();
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(m_LocalYoutubeDlUpdater.BinaryLocation))
            {
                throw new FileNotFoundException("youtube-dl binary not found.", "youtube-dl");
            }

            var arguments = BuildArguments(youtubeUrl, options);
            var stdout = await ReadProcessOutputAsync(m_LocalYoutubeDlUpdater.BinaryLocation, arguments, cancellationToken);

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
                stdout.AppendLine(args.Data);
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                stderr.AppendLine(args.Data);
            };

            var taskCompletionSource = new TaskCompletionSource<string>();

            process.Exited += (sender, args) =>
            {
                var output = stdout + "\n" + stderr;
                if (process.ExitCode != 0)
                {
                    taskCompletionSource.TrySetException(new Exception($"youtube-dl existed with code {process.ExitCode}. \n{output}"));
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
