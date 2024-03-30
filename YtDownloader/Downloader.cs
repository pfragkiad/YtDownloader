using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    public event EventHandler? DownloadCancelled;
    public event EventHandler? Finished;
    public event ProgressChangedEventHandler? DownloadProgressChanged;
    public event EventHandler<StatusChangedEventArgs>? StatusChanged;


    public async Task Download(
        string urlOrId,
        bool isPlaylist,
        string targetDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        int count = 1;

        if (urlOrId.Contains("/"))
        {
            urlOrId = GetId(urlOrId, isPlaylist);
        }
        else
        {
            //force playlist validation if the plain url is given
            int? returnedCount = await GetPlaylistCount(urlOrId);
            isPlaylist = (returnedCount ?? 0) > 0;
            if (isPlaylist) count = returnedCount!.Value;
        }

        //string? title = await GetTitle(urlOrId);
        //Debugger.Break();


        string arguments = !isPlaylist ?
            $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"%(title)s.%(ext)s\" -- {urlOrId}" :
            $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"%(playlist_index)02d %(title)s.%(ext)s\" -- {urlOrId}";


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

        int currentIndex = 1;

        // Create a new process
        using Process process = new();

        process.StartInfo = startInfo;

        // Event handler for capturing standard output
        process.OutputDataReceived += (s, args) =>
        {
            string? status = args.Data;
            if (string.IsNullOrEmpty(status)) return;


            //for single file
            /*
[download] Downloading item 3 of 9
[youtube] Extracting URL: https://www.youtube.com/watch?v=eREL7WJm2Po
[youtube] eREL7WJm2Po: Downloading webpage
[youtube] eREL7WJm2Po: Downloading ios player API JSON
[youtube] eREL7WJm2Po: Downloading android player API JSON
[youtube] eREL7WJm2Po: Downloading m3u8 information
[info] eREL7WJm2Po: Downloading 1 format(s): 251
[download] 02 Pare Karotsa Ki Ela.mp3 has already been downloaded
or
[download] Destination: 03 Sitiakes Kondilies.webm  
[download]   0.0% of    4.04MiB at  353.77KiB/s ETA 00:11
[download]   0.1% of    4.04MiB at    1.04MiB/s ETA 00:03
[download]   0.2% of    4.04MiB at    1.79MiB/s ETA 00:02
[download]   0.4% of    4.04MiB at    3.83MiB/s ETA 00:01
[download]   0.7% of    4.04MiB at    2.77MiB/s ETA 00:01
[download]   1.5% of    4.04MiB at    3.85MiB/s ETA 00:01
[download]   3.1% of    4.04MiB at    4.99MiB/s ETA 00:00
[download]   6.2% of    4.04MiB at    6.38MiB/s ETA 00:00
[download]  12.3% of    4.04MiB at    7.90MiB/s ETA 00:00
[download]  24.7% of    4.04MiB at    8.37MiB/s ETA 00:00
[download]  49.4% of    4.04MiB at    9.07MiB/s ETA 00:00
[download]  98.9% of    4.04MiB at    9.27MiB/s ETA 00:00
[download] 100.0% of    4.04MiB at    9.23MiB/s ETA 00:00
[download] 100% of    4.04MiB in 00:00:00 at 7.44MiB/s   
[ExtractAudio] Destination: 03 Sitiakes Kondilies.mp3
             */
            //[download] 100%
            //[download] 100.0%
            var m = Regex.Match(status, @"\[download\]\s+(?<perc>\d+(\.\d+)?)%");
            if (m.Success)
            {
                float partialPercentage = float.Parse(m.Groups["perc"].Value, EN) / 100.0f;
                int percentage = (int)(100.0f * ((currentIndex - 1) + partialPercentage) / count);
                DownloadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage, null));
            }
            else
            {
                //[download] Downloading item 1 of 9
                var m2 = Regex.Match(status, @"\[download\] Downloading item\s+(?<i>\d+)");
                if (m2.Success)
                {
                    currentIndex = int.Parse(m2.Groups["i"].Value);
                    int percentage = (int)(100.0f * (currentIndex - 1) / count);
                    DownloadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage, null));

                }
                else
                    StatusChanged?.Invoke(this, new StatusChangedEventArgs(status));
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
            //// Display the exit code
            if (process.ExitCode == 0)
                Downloaded?.Invoke(this, EventArgs.Empty);
            else
                DownloadFailed?.Invoke(this, EventArgs.Empty);
        }
        catch (TaskCanceledException)
        {
            DownloadCancelled?.Invoke(this, EventArgs.Empty);
            Finished?.Invoke(this, EventArgs.Empty);
            return;
        }
        finally
        {
            Finished?.Invoke(this, EventArgs.Empty);

        }


    }

    private static string GetId(string url, bool isPlaylist)
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

    public async Task<int?> GetPlaylistCount(string playlistId)
    {
        string arguments = $"{playlistId} -I0 -O playlist:playlist_count";

        ProcessStarter starter = Program.ServiceProvider!.GetRequiredService<ProcessStarter>();
        var output = await starter.RunAndReturnOutput(_exePath, arguments);

        //error if not playlist 
        /*
yt-dlp : WARNING: [youtube] Skipping player responses from android clients (got player responses for video "aQvGIIdgFDM
" instead of "wAo6lUU6Zxk")
         */

        if (string.IsNullOrWhiteSpace(output.StandardOutput)) return 0;

        //a plain number should be returned on success (or else the return is undefined)
        bool parsed = int.TryParse(output.StandardOutput.Trim(), out int count);
        return parsed ? count : null;
    }

    public async Task<string?> GetFilename(string videoId)
    {
        string arguments = $" --get-filename -- {videoId}";

        ProcessStarter starter = Program.ServiceProvider!.GetRequiredService<ProcessStarter>();
        var output = await starter.RunAndReturnOutput(_exePath, arguments);
        return string.IsNullOrWhiteSpace(output.StandardOutput) ? null : output.StandardOutput.Trim();
    }
    public async Task<string?> GetTitle(string videoId)
    {
        string arguments = $" --get-title -- {videoId}";

        ProcessStarter starter = Program.ServiceProvider!.GetRequiredService<ProcessStarter>();
        var output = await starter.RunAndReturnOutput(_exePath, arguments);
        return string.IsNullOrWhiteSpace(output.StandardOutput) ? null : output.StandardOutput.Trim();
    }
}
