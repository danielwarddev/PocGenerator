using Microsoft.Extensions.Logging;
using PocGenerator.Generation;
using PocGenerator.Verification;

namespace PocGenerator.Console;

public interface IRetryFlowRunner
{
    Task Run(string retryPath, CancellationToken cancellationToken = default);
}

public class RetryFlowRunner : IRetryFlowRunner
{
    private readonly IRetryExecutionPlanner _retryExecutionPlanner;
    private readonly IGenerationPhaseHandler _generationPhaseHandler;
    private readonly IVerificationPhaseHandler _verificationPhaseHandler;
    private readonly ILogger<RetryFlowRunner> _logger;

    public RetryFlowRunner(
        IRetryExecutionPlanner retryExecutionPlanner,
        IGenerationPhaseHandler generationPhaseHandler,
        IVerificationPhaseHandler verificationPhaseHandler,
        ILogger<RetryFlowRunner> logger)
    {
        _retryExecutionPlanner = retryExecutionPlanner;
        _generationPhaseHandler = generationPhaseHandler;
        _verificationPhaseHandler = verificationPhaseHandler;
        _logger = logger;
    }

    public async Task Run(string retryPath, CancellationToken cancellationToken = default)
    {
        var retryPlan = await _retryExecutionPlanner.Create(retryPath, cancellationToken);

        _logger.LogInformation("Retrying run in {OutputDirectory}", retryPlan.OutputDirectory);

        if (retryPlan.StartPhase == RetryStartPhase.Completed)
        {
            _logger.LogInformation(
                "The run in {OutputDirectory} already completed successfully. Nothing to retry.",
                retryPlan.OutputDirectory);
            return;
        }

        var plan = retryPlan.ProjectPlan!;

        if (retryPlan.StartPhase == RetryStartPhase.Generation)
        {
            var remainingSpecFiles = plan.SpecFiles.Skip(retryPlan.NextSpecIndex).ToList();

            await _generationPhaseHandler.GenerateMvp(plan.OutputDirectory, remainingSpecFiles, cancellationToken);
        }

        await _verificationPhaseHandler.VerifyMvp(plan.OutputDirectory, plan.PlanFilePath, cancellationToken);
    }
}