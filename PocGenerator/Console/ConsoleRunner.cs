using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;
using System.Diagnostics;


namespace PocGenerator.Console;

public class ConsoleRunner : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<ConsoleRunner> _logger;
    private readonly CliArgs _cliArgs;
    private readonly ICopilotService _copilotService;
    private readonly INormalFlowRunner _normalFlowRunner;
    private readonly IRetryFlowRunner _retryFlowRunner;

    public ConsoleRunner(
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<ConsoleRunner> logger,
        CliArgs cliArgs,
        ICopilotService copilotService,
        INormalFlowRunner normalFlowRunner,
        IRetryFlowRunner retryFlowRunner)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _cliArgs = cliArgs;
        _copilotService = copilotService;
        _normalFlowRunner = normalFlowRunner;
        _retryFlowRunner = retryFlowRunner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting PocGenerator...");
            await _copilotService.Initialize(stoppingToken);

            if (_cliArgs.RetryPath is null)
            {
                await _normalFlowRunner.Run(stoppingToken);
            }
            else
            {
                await _retryFlowRunner.Run(_cliArgs.RetryPath, stoppingToken);
            }
        }
        catch (Exception ex)
        when (HandleWithoutLosingLoggingScope(() => _logger.LogCritical(ex, "An unexpected fatal error has occurred.")))
        {
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("PocGenerator completed in {ElapsedTime}", stopwatch.Elapsed);
            _hostApplicationLifetime.StopApplication();
        }
    }

    private bool HandleWithoutLosingLoggingScope(Action action)
    {
        action();
        return false;
    }
}
