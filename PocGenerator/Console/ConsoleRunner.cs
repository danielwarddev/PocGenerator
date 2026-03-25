using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;
using PocGenerator.Generation;
using PocGenerator.Planning;
using PocGenerator.Verification;
using System.Diagnostics;


namespace PocGenerator.Console;

public class ConsoleRunner : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<ConsoleRunner> _logger;
    private readonly ICopilotService _copilotService;
    private readonly IPlanningPhaseHandler _planningPhaseHandler;
    private readonly IGenerationPhaseHandler _generationPhaseHandler;
    private readonly IVerificationPhaseHandler _verificationPhaseHandler;

    public ConsoleRunner(
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<ConsoleRunner> logger,
        ICopilotService copilotService,
        IPlanningPhaseHandler planningPhaseHandler,
        IGenerationPhaseHandler generationPhaseHandler,
        IVerificationPhaseHandler verificationPhaseHandler)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _copilotService = copilotService;
        _planningPhaseHandler = planningPhaseHandler;
        _generationPhaseHandler = generationPhaseHandler;
        _verificationPhaseHandler = verificationPhaseHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting PocGenerator...");
            await _copilotService.Initialize(stoppingToken);

            var plan = await _planningPhaseHandler.CreatePlan(stoppingToken);
            await _generationPhaseHandler.GenerateMvp(plan.OutputDirectory, plan.SpecFiles, stoppingToken);
            await _verificationPhaseHandler.VerifyMvp(plan.OutputDirectory, stoppingToken);
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
