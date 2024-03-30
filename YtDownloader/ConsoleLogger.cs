using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace YtDownloader;

public class ConsoleLogger
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _provider;

    public ConsoleLogger(ILogger logger, IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    private void Runner_OutputReceived(object? sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        _logger.LogInformation(e.Data);
    }

    private void Runner_ErrorReceived(object? sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        _logger.LogError(e.Data);
    }

    public async Task<int> RunWithEvents(
         string executablePath,
         string arguments,
         string workingDirectory = "",
         CancellationToken cancellationToken = default)
    {
        var runner = _provider.GetRequiredService<ConsoleRunner>();
        runner.ErrorReceived += Runner_ErrorReceived;
        runner.OutputReceived += Runner_OutputReceived;

        return await runner.RunWithEvents(executablePath,arguments,workingDirectory,cancellationToken);
    }
}
