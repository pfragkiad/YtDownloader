using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    public event EventHandler? DownloadStarted;
    public event EventHandler? DownloadFinished;
    public event ProgressChangedEventHandler? DownloadProgressChanged;
    public event EventHandler<StatusChangedEventArgs>? StatusChanged;

    static CultureInfo EN = CultureInfo.GetCultureInfo("en-US");

    public async Task Download(
        string exePath,
        string arguments,
        string workingDirectory)
    {
        // Create a new process start info
        ProcessStartInfo startInfo = new()
        {
            FileName = exePath,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            WorkingDirectory = workingDirectory

        };

        //Cursor.Current = Cursors.WaitCursor;
        //Application.UseWaitCursor = true;

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
                            DownloadProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage,null));
                        }
                        else
                        {
                            // textBoxOutput.AppendText(args.Data + Environment.NewLine);
                            StatusChanged?.Invoke(this, new StatusChangedEventArgs($"{status}"));
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

            // Wait for the process to exit
            await process.WaitForExitAsync();

            //// Display the exit code
            DownloadFinished?.Invoke(this, EventArgs.Empty);
            //textBoxOutput.AppendText("Process exited with code: " + process.ExitCode + Environment.NewLine);
            //progressBar1.Visible = false;
        }

        Application.UseWaitCursor = false;
        Cursor.Current = Cursors.Default;
    }

}
