using System.Diagnostics;
namespace YtDownloader;

public readonly struct ProcessOutput
{
    public string StandardOutput { get; init; }
    public string StandardError { get; init; }

    public int ExitCode { get; init; }
}

public class ProcessStarter
{
    public async Task<ProcessOutput> RunAndReturnOutput(
        string executablePath,
        string arguments,
        string workingDirectory = "")
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = executablePath,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory
        };

        Process process = new()
        {
            StartInfo = startInfo
        };

        process.Start();

        await process.WaitForExitAsync();

        return new ProcessOutput
        {
            StandardOutput = await process.StandardOutput.ReadToEndAsync(),
            StandardError = await process.StandardError.ReadToEndAsync(),
            ExitCode = process.ExitCode
        };
    }


    public async Task<string?> Run(
        string executablePath,
        string arguments,
        string workingDirectory = "")
    {
        var output = await RunAndReturnOutput(executablePath, arguments, workingDirectory);
        return string.IsNullOrWhiteSpace(output.StandardOutput) ? null : output.StandardOutput.Trim();
    }


    public event EventHandler<DataReceivedEventArgs>? OutputReceived;
    public event EventHandler<DataReceivedEventArgs>? ErrorReceived;

    public async Task<int> RunWithEvents(
         string executablePath,
         string arguments,
         string workingDirectory = "",
         CancellationToken cancellationToken = default)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = executablePath,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory
        };

        Process process = new()
        {
            StartInfo = startInfo
        };

        process.OutputDataReceived += (s, args) => OutputReceived?.Invoke(this, args);
        process.ErrorDataReceived += (s, args) => ErrorReceived?.Invoke(this, args);

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        return process.ExitCode;
    }

}
