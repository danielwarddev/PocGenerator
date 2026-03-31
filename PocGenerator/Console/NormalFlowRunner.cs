using PocGenerator.Generation;
using PocGenerator.Planning;
using PocGenerator.Verification;

namespace PocGenerator.Console;

public interface INormalFlowRunner
{
    Task Run(CancellationToken cancellationToken = default);
}

public class NormalFlowRunner : INormalFlowRunner
{
    private readonly IPlanningPhaseHandler _planningPhaseHandler;
    private readonly IGenerationPhaseHandler _generationPhaseHandler;
    private readonly IVerificationPhaseHandler _verificationPhaseHandler;

    public NormalFlowRunner(
        IPlanningPhaseHandler planningPhaseHandler,
        IGenerationPhaseHandler generationPhaseHandler,
        IVerificationPhaseHandler verificationPhaseHandler)
    {
        _planningPhaseHandler = planningPhaseHandler;
        _generationPhaseHandler = generationPhaseHandler;
        _verificationPhaseHandler = verificationPhaseHandler;
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        var plan = await _planningPhaseHandler.CreatePlan(cancellationToken);
        await _generationPhaseHandler.GenerateMvp(plan.OutputDirectory, plan.SpecFiles, cancellationToken);
        await _verificationPhaseHandler.VerifyMvp(plan.OutputDirectory, plan.PlanFilePath, cancellationToken);
    }
}