using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YtDownloader
{
    public partial class MainForm : Form
    {
        static CultureInfo EN = CultureInfo.GetCultureInfo("en-US");

        private readonly Downloader _downloader;

        public MainForm(Downloader downloader)
        {
            InitializeComponent();
            //TODO: format of exported file 

            var settings = YtSettings.Default;
            string lastFolder = settings.Folder;
            if (!string.IsNullOrWhiteSpace(lastFolder) && Directory.Exists(lastFolder))
                txtTarget.Text = lastFolder;

            _downloader = downloader;
            _downloader.DownloadStarted += Downloader_DownloadStarted;
            _downloader.Downloaded += Downloader_Downloaded;
            _downloader.Finished += Downloader_Finished;
            _downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
            _downloader.StatusChanged += Downloader_StatusChanged;
            _downloader.DownloadFailed += Downloader_DownloadFailed;
        }

        #region Downloader events
        private void Downloader_StatusChanged(object? sender, StatusChangedEventArgs e)
        {
            BeginInvoke(() =>
            {
                textBoxOutput.AppendText($"{e.Status}\r\n");
            });
        }

        private void Downloader_DownloadProgressChanged(object? sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            BeginInvoke(() =>
            {
                progressBar1.Value = e.ProgressPercentage;
            });
        }

        private void Downloader_Finished(object? sender, EventArgs e)
        {
            progressBar1.Visible = false;
            Application.UseWaitCursor = false;
            Cursor.Current = Cursors.Default;
        }

        private void Downloader_DownloadStarted(object? sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Application.UseWaitCursor = true;

            tstStatus.Text = "Downloading...";

            progressBar1.Value = 0;
            progressBar1.Visible = true;
        }

        private void Downloader_Downloaded(object? sender, EventArgs e)
        {
            tstStatus.Text = "Successfully downloaded";
        }
        private void Downloader_DownloadFailed(object? sender, EventArgs e)
        {
            tstStatus.Text = "Download failed.";
        }
        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var settings = YtSettings.Default;
            string lastFolder = txtTarget.Text;
            if (!string.IsNullOrWhiteSpace(lastFolder) && Directory.Exists(lastFolder))
            {
                settings.Folder = lastFolder;
                settings.Save();
            }

            base.OnFormClosing(e);
        }


        private async void btnDownload_Click(object sender, EventArgs e)
        {
            // Path to the executable
            string exePath = Program.Configuration!["yt-dlp"]!; // "C:\\tools\\yt-dlp.exe";



            string url = textBox1.Text;
            if (string.IsNullOrWhiteSpace(url)) return;

            if (chkIsPlaylist.Checked)
            {
                if (url.Contains("list=") )
                {
                    var m = Regex.Match(url, "list=(?<id>[a-zA-Z0-9_-]+)");
                    if (m.Success)
                        url = m.Groups["id"].Value;
                }
                else url = $"https://www.youtube.com/watch?list={url}";
            }
            else
            {
                if (url.Contains("youtu.be"))
                {
                    //https://youtu.be/YX7Atj6-IIA?t=17
                    var m = Regex.Match(url, "https://youtu.be/(?<id>[a-zA-Z0-9_-]{11})");
                    if (m.Success)
                        url = m.Groups["id"].Value;

                }
                //https://www.youtube.com/watch?v=wAo6lUU6Zxk&pp=ygUSaGVscGxlc3MgZmlyZWhvdXNl
                else if (url.Contains("youtube"))
                {
                    var m = Regex.Match(url, "v=(?<id>[a-zA-Z0-9_-]{11})");
                    if (m.Success)
                        url = m.Groups["id"].Value;
                }
                else url = $"https://www.youtube.com/watch?v={url}";
            }

            // Arguments to pass to the executable
            string arguments = !chkIsPlaylist.Checked ?
                $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"%(title)s.%(ext)s\" -- {url}" :
                $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 -o \"%(playlist_index)02d %(title)s.%(ext)s\" -- {url}";

            if (!Directory.Exists(txtTarget.Text)) Directory.CreateDirectory(txtTarget.Text);

            //await Download(exePath, arguments);
            await _downloader.Download(arguments, txtTarget.Text);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBoxOutput.Clear();
        }
    }
}
