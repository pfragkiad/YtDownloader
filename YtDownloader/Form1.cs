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
            string url = textBox1.Text;
            if (string.IsNullOrWhiteSpace(url)) return;

            await _downloader.Download(url,chkIsPlaylist.Checked, txtTarget.Text);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBoxOutput.Clear();
        }
    }
}
