using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Globalization;
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

    private readonly string _exePath;
    private readonly IServiceProvider _provider;
    private readonly ConsoleRunner _process;

    public Downloader(IServiceProvider provider, IConfiguration configuration, ConsoleRunner process)
    {
        _exePath = configuration!["yt-dlp"]!;
        _provider = provider;
        _process = process;
    }

    public event EventHandler? DownloadStarted;
    public event EventHandler? Downloaded;
    public event EventHandler? DownloadFailed;
    public event EventHandler? DownloadCancelled;
    public event EventHandler? Finished;
    public event ProgressChangedEventHandler? DownloadProgressChanged;
    public event EventHandler<StatusChangedEventArgs>? LogStatus;
    public event EventHandler<StatusChangedEventArgs>? StatusChanged;

    /*
    [download] Downloading item 3 of 9
    [youtube] Extracting URL: https://www.youtube.com/watch?v=C2tN_YDZsdI
    [youtube] C2tN_YDZsdI: Downloading webpage
    [youtube] C2tN_YDZsdI: Downloading ios player API JSON
    [youtube] C2tN_YDZsdI: Downloading android player API JSON
    [youtube] C2tN_YDZsdI: Downloading m3u8 information
    [info] C2tN_YDZsdI: Downloading 1 format(s): 251
    [download] Destination: 09 Irakliotikos Pidihtos.webm
    ...
    [ExtractAudio] Destination: 09 Irakliotikos Pidihtos.mp3
    Deleting original file 09 Irakliotikos Pidihtos.webm (pass -k to keep)
     */

    /* (playlist specific)
    [youtube:playlist] Extracting URL: OLAK5uy_k3vbo8oJfA9SWuKNTcE6ftF-1HCNUM-Hk
    [youtube:tab] Extracting URL: https://www.youtube.com/playlist?list=OLAK5uy_k3vbo8oJfA9SWuKNTcE6ftF-1HCNUM-Hk
    [youtube:tab] OLAK5uy_k3vbo8oJfA9SWuKNTcE6ftF-1HCNUM-Hk: Downloading webpage
    [youtube:tab] OLAK5uy_k3vbo8oJfA9SWuKNTcE6ftF-1HCNUM-Hk: Redownloading playlist API JSON with unavailable videos
    [download] Downloading playlist: Hori Ke Tragoudia Tis Kritis
    [youtube:tab] Playlist Hori Ke Tragoudia Tis Kritis: Downloading 9 items of 9
     */

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

    or
    [ExtractAudio] Not converting audio 01 Irakliotikes Kondilies.mp3; file is already in target format mp3
     */


    List<string> IgnoredLabels = ["[youtube]", "[info]", "Deleting original", "[ExtractAudio] Not converting", "[youtube:tab]", "[youtube:playlist]", "[FixupM4a]"];

    public async Task Download(
        string urlOrId,
        bool isPlaylist,
        string targetDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        int count = 1;
        int currentIndex = 1;

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

        string arguments = !isPlaylist ?
            $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"%(title)s.%(ext)s\" -- {urlOrId}" :
            $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"%(playlist_index)02d %(title)s.%(ext)s\" -- {urlOrId}";


        var process = _provider.GetRequiredService<ConsoleRunner>();

        process.OutputReceived += (s, args) =>
        {
            string? status = args.Data;
            if (string.IsNullOrEmpty(status)) return;
            
            foreach (string label in IgnoredLabels) if (status.StartsWith(label)) return;

            /*
            [download] 02 Pare Karotsa Ki Ela.mp3 has already been downloaded
            or
            [download] Destination: 03 Sitiakes Kondilies.webm  
            [download]   0.0% of    4.04MiB at  353.77KiB/s ETA 00:11
            [download]   0.1% of    4.04MiB at    1.04MiB/s ETA 00:03
                        ..
            [download]  98.9% of    4.04MiB at    9.27MiB/s ETA 00:00
            [download] 100.0% of    4.04MiB at    9.23MiB/s ETA 00:00
            [download] 100% of    4.04MiB in 00:00:00 at 7.44MiB/s   
            [ExtractAudio] Destination: 03 Sitiakes Kondilies.mp3
             */

            var matchPartialDownload = Regex.Match(status, @"\[download\]\s+(?<perc>\d+(\.\d+)?)%");
            if (matchPartialDownload.Success)
            {
                float partialPercentage = float.Parse(matchPartialDownload.Groups["perc"].Value, EN) / 100.0f;
                int percentage = (int)(100.0f * ((currentIndex - 1) + partialPercentage) / count);
                DownloadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage, null));
                return;
            }

            //[download] Downloading item 1 of 9
            var matchItemDownload = Regex.Match(status, @"\[download\] Downloading item\s+(?<i>\d+)");
            if (matchItemDownload.Success)
            {
                currentIndex = int.Parse(matchItemDownload.Groups["i"].Value);
                int percentage = (int)(100.0f * (currentIndex - 1) / count);
                DownloadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage, null));

                StatusChanged?.Invoke(this, new StatusChangedEventArgs($"Downloading {currentIndex} of {count}..."));
                return;
            }

            if (status.StartsWith("[ExtractAudio] Destination"))
            {
                //[ExtractAudio] Destination: 09 Irakliotikos Pidihtos.mp3
                StatusChanged?.Invoke(this, new StatusChangedEventArgs($"Converting {currentIndex} of {count}..."));
                return;
            }

            LogStatus?.Invoke(this, new StatusChangedEventArgs(status));
        };

        try
        {
            DownloadStarted?.Invoke(this, EventArgs.Empty);

            int exitCode = await process.RunWithEvents(_exePath, arguments, targetDirectory, cancellationToken);
            if (exitCode == 0)
                Downloaded?.Invoke(this, EventArgs.Empty);
            else
                DownloadFailed?.Invoke(this, EventArgs.Empty);
        }
        catch (TaskCanceledException)
        {
            DownloadCancelled?.Invoke(this, EventArgs.Empty);
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


    #region Video/playlist properties

    public async Task<int?> GetPlaylistCount(string playlistId)
    {
        string? output = await _process.Run(_exePath, $"{playlistId} -I0 -O playlist:playlist_count");
        if (output is null) return null;

        bool parsed = int.TryParse(output, out int count);
        return parsed ? count : null;
    }

    public async Task<string?> GetFilename(string videoId) => await _process.Run(_exePath, $" --get-filename -- {videoId}");
    
    public async Task<string?> GetTitle(string videoId) =>  await _process.Run(_exePath, $" --get-title -- {videoId}");

    #endregion
}
