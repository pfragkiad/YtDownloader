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
            //TODO: format of exported file ?

            var settings = YtSettings.Default;
            string[] lastFolders = settings.Folder.Split("\n");
            foreach (string lastFolder in lastFolders)
                if (Directory.Exists(lastFolder))
                    cboTarget.Items.Add(lastFolder);
            if (cboTarget.Items.Count > 0)
                cboTarget.SelectedIndex = 0;


            _downloader = downloader;
            _downloader.DownloadStarted += Downloader_DownloadStarted;
            _downloader.Downloaded += Downloader_Downloaded;
            _downloader.DownloadCancelled += Downloader_DownloadCancelled;
            _downloader.Finished += Downloader_Finished;
            _downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
            _downloader.LogStatus += Downloader_StatusChanged;
            _downloader.DownloadFailed += Downloader_DownloadFailed;
            _downloader.StatusChanged += Downloader_PlaylistItemDownloadStarted;
        }

        #region Downloader events

        private void Downloader_StatusChanged(object? sender, StatusChangedEventArgs e)
        {
            BeginInvoke(() =>
            {
                textBoxOutput.AppendText($"{e.Status}\r\n");
            });
        }
        private void Downloader_PlaylistItemDownloadStarted(object? sender, StatusChangedEventArgs e)
        {
            BeginInvoke(() =>
            {
                tstStatus.Text = e.Status;
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
            btnDownload.Text = "DOWNLOAD";
            Application.UseWaitCursor = false;
            Cursor.Current = Cursors.Default;
        }
        private void Downloader_DownloadCancelled(object? sender, EventArgs e)
        {
            tstStatus.Text = "Download cancelled.";
        }

        private void Downloader_DownloadStarted(object? sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Application.UseWaitCursor = true;

            tstStatus.Text = "Downloading...";

            btnDownload.Text = "CANCEL";
            progressBar1.Value = 0;
            progressBar1.Visible = true;
        }

        private void Downloader_Downloaded(object? sender, EventArgs e)
        {
            tstStatus.Text = "Successfully downloaded.";
        }
        private void Downloader_DownloadFailed(object? sender, EventArgs e)
        {
            tstStatus.Text = "Download failed.";
        }
        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var settings = YtSettings.Default;
            string lastFolders = string.Join("\n", cboTarget.Items.Cast<string>());
            settings.Folder = lastFolders;
            settings.Save();

            base.OnFormClosing(e);
        }

        CancellationTokenSource? source;
        bool alreadyDownloading = false;
        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (alreadyDownloading)
            {
                source!.Cancel();
                return;
            }

            string url = textBox1.Text;
            if (string.IsNullOrWhiteSpace(url)) return;

            alreadyDownloading = true;
            source = new();
            await _downloader.Download(url, chkIsPlaylist.Checked, cboTarget.Text, source.Token);
            source = null;

            if (cboTarget.Items.IndexOf(cboTarget.Text) == -1)
                cboTarget.Items.Insert(0, cboTarget.Text);

            alreadyDownloading = false;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBoxOutput.Clear();
        }
    }
}
