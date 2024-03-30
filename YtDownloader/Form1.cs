using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace YtDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //TODO: yt-dlp in settings ? or internal exe
            //TODO: format of exported file 

            var settings = YtSettings.Default;
            string lastFolder = settings.Folder;
            if (!string.IsNullOrWhiteSpace(lastFolder) && Directory.Exists(lastFolder))
                txtTarget.Text = lastFolder;
        }

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

        static CultureInfo EN = CultureInfo.GetCultureInfo("en-US");

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            // Path to the executable
            string exePath = Program.Configuration!["yt-dlp"]!; // "C:\\tools\\yt-dlp.exe";



            string url = textBox1.Text;
            if (string.IsNullOrWhiteSpace(url)) return;

            if (chkIsPlaylist.Checked)
            {
                if (url.Contains("youtube"))
                {
                    var m = Regex.Match(url, "list=(?<id>[a-zA-Z0-9_-]{11})");
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

            await Download(exePath, arguments);
        }

        private async Task Download(string exePath, string arguments)
        {
            // Create a new process start info
            ProcessStartInfo startInfo = new()
            {
                FileName = exePath,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = txtTarget.Text

            };

            Cursor.Current = Cursors.WaitCursor;
            Application.UseWaitCursor = true;

            // Create a new process
            using (Process process = new())
            {
                process.StartInfo = startInfo;

                // Event handler for capturing standard output
                process.OutputDataReceived += (s, args) =>
                {
                    //TODO: Track completion from process output?
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        // Append the output to the TextBox
                        BeginInvoke(() =>
                        {
                            //[download] 100%
                            //[download] 100.0%
                            var m = Regex.Match(args.Data, @"\[download\]\s+(?<perc>\d+(\.\d+)?)%");
                            if (m.Success)
                                progressBar1.Value = (int)float.Parse(m.Groups["perc"].Value, EN);
                            else
                                textBoxOutput.AppendText(args.Data + Environment.NewLine);
                            //   Application.DoEvents();
                        });
                    }
                };
                progressBar1.Value = 0;
                progressBar1.Visible = true;


                // Start the process
                process.Start();

                // Begin asynchronous reading of the standard output stream
                process.BeginOutputReadLine();

                // Wait for the process to exit
                await process.WaitForExitAsync();

                // Display the exit code
                textBoxOutput.AppendText("Process exited with code: " + process.ExitCode + Environment.NewLine);

                progressBar1.Visible = false;
            }

            Application.UseWaitCursor = false;
            Cursor.Current = Cursors.Default;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBoxOutput.Clear();
        }
    }
}
