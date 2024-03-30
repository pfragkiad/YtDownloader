using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace YtDownloader;


public struct ProcessOutput
{
    public string StandardOutput { get; init; }
    public string StandardError { get; init; }
}

public class ProcessStarter
{

    public async Task<ProcessOutput> RunAndReturnOutput(
        string executablePath,
        string arguments,
        string workingDirectory)
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

        //process.ErrorDataReceived += (s, args) =>
        //{
        //    if (args is null) return;

        //};

        //process.OutputDataReceived += (s, args) =>
        //{

        //};
        process.Start();

        await process.WaitForExitAsync();

        return new ProcessOutput
        {
            StandardOutput = await process.StandardOutput.ReadToEndAsync(),
            StandardError = await process.StandardError.ReadToEndAsync()
        };
    }


}
