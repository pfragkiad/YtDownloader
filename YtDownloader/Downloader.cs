using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Configuration.Internal;
using System.Diagnostics;
using System.Globalization;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace YtDownloader;

public class StatusChangedEventArgs : EventArgs
{
    public StatusChangedEventArgs(string status)
    {
        Status = status;
    }

    public string Status { get; }
}

public class Downloader
{
#pragma warning disable IDE1006 // Naming Styles
    readonly static CultureInfo EN = CultureInfo.GetCultureInfo("en-US");
#pragma warning restore IDE1006 // Naming Styles

    readonly string _exePath;

    public Downloader(IConfiguration configuration)
    {
        _exePath = configuration!["yt-dlp"]!;
    }

    public event EventHandler? DownloadStarted;
    public event EventHandler? Downloaded;
    public event EventHandler? DownloadFailed;
    public event EventHandler DownloadCancelled;
    public event EventHandler? Finished;
    public event ProgressChangedEventHandler? DownloadProgressChanged;
    public event EventHandler<StatusChangedEventArgs>? StatusChanged;


    public async Task Download(
        string url,
        bool isPlaylist,
        string targetDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        url = GetCleanUrlOrId(url, isPlaylist);

        string arguments = !isPlaylist ?
            $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"%(title)s.%(ext)s\" -- {url}" :
            $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"%(playlist_index)02d %(title)s.%(ext)s\" -- {url}";


        // Create a new process start info
        ProcessStartInfo startInfo = new()
        {
            FileName = _exePath,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            WorkingDirectory = targetDirectory

        };

        //Cursor.Current = Cursors.WaitCursor;
        //Application.UseWaitCursor = true;

        /* example output (without download percentage)
         [youtube] Extracting URL: oRSgKylpbEo
        [youtube] oRSgKylpbEo: Downloading webpage
        [youtube] oRSgKylpbEo: Downloading ios player API JSON
        [youtube] oRSgKylpbEo: Downloading android player API JSON
        [youtube] oRSgKylpbEo: Downloading m3u8 information
        [info] oRSgKylpbEo: Downloading 1 format(s): 258
        [download] Destination: Tenacious D - Beelzeboss (The Final Showdown) - 4K - 5.1 Surround.m4a
        [FixupM4a] Correcting container of "Tenacious D - Beelzeboss (The Final Showdown) - 4K - 5.1 Surround.m4a"
        [ExtractAudio] Destination: Tenacious D - Beelzeboss (The Final Showdown) - 4K - 5.1 Surround.mp3
        Deleting original file Tenacious D - Beelzeboss (The Final Showdown) - 4K - 5.1 Surround.m4a (pass -k to keep)
         */


        // Create a new process
        using (Process process = new())
        {
            process.StartInfo = startInfo;

            // Event handler for capturing standard output
            process.OutputDataReceived += (s, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    //[download] 100%
                    //[download] 100.0%
                    var m = Regex.Match(args.Data, @"\[download\]\s+(?<perc>\d+(\.\d+)?)%");
                    string? status = args.Data;
                    if (status is null) return;
                    // Append the output to the TextBox
                    //BeginInvoke(() =>
                    //{
                    if (m.Success)
                    {
                        int percentage = (int)float.Parse(m.Groups["perc"].Value, EN);
                        //progressBar1.Value = (int)float.Parse(m.Groups["perc"].Value, EN);
                        DownloadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage, null));
                    }
                    else
                    {
                        // textBoxOutput.AppendText(args.Data + Environment.NewLine);
                        StatusChanged?.Invoke(this, new StatusChangedEventArgs(status));
                    }
                    //   Application.DoEvents();
                    //});
                }
            };
            //progressBar1.Value = 0;
            //progressBar1.Visible = true;

            DownloadStarted?.Invoke(this, EventArgs.Empty);

            // Start the process
            process.Start();

            // Begin asynchronous reading of the standard output stream
            process.BeginOutputReadLine();

            try
            {
                // Wait for the process to exit
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                DownloadCancelled?.Invoke(this, EventArgs.Empty);
                Finished?.Invoke(this, EventArgs.Empty);
                return;
            }

            //// Display the exit code
            if (process.ExitCode == 0)
                Downloaded?.Invoke(this, EventArgs.Empty);
            else
                DownloadFailed?.Invoke(this, EventArgs.Empty);

            //textBoxOutput.AppendText("Process exited with code: " + process.ExitCode + Environment.NewLine);
            //progressBar1.Visible = false;
        }

        //Application.UseWaitCursor = false;
        //Cursor.Current = Cursors.Default;

        Finished?.Invoke(this, EventArgs.Empty);
    }

    private static string GetCleanUrlOrId(string url, bool isPlaylist)
    {
        if (isPlaylist)
        {
            if (url.Contains("list="))
            {
                var m = Regex.Match(url, "list=(?<id>[a-zA-Z0-9_-]+)");
                if (m.Success)
                    url = m.Groups["id"].Value;
            }
            //leave the text as list
            //else url = $"https://www.youtube.com/watch?list={url}";
        }
        else
        {
            if (url.Contains("youtu.be"))
            {
                //https://youtu.be/YX7Atj6-IIA?t=17
                var m = Regex.Match(url, "https://youtu.be/(?<id>[a-zA-Z0-9_-]+)");
                if (m.Success)
                    url = m.Groups["id"].Value;

            }
            //https://www.youtube.com/watch?v=wAo6lUU6Zxk&pp=ygUSaGVscGxlc3MgZmlyZWhvdXNl
            else if (url.Contains("youtube"))
            {
                var m = Regex.Match(url, "v=(?<id>[a-zA-Z0-9_-]+)");
                if (m.Success)
                    url = m.Groups["id"].Value;
            }
            //else url = $"https://www.youtube.com/watch?v={url}";
        }

        return url;
    }
}
