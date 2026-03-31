using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using PocGenerator.Planning;

namespace PocGenerator.Console;

public enum RetryStartPhase
{
    Generation,
    Verification,
    Completed
}

public record RetryExecutionPlan(
    RetryStartPhase StartPhase,
    string OutputDirectory,
    ProjectPlan? ProjectPlan,
    int NextSpecIndex);

public interface IRetryExecutionPlanner
{
    Task<RetryExecutionPlan> Create(string retryDirectory, CancellationToken cancellationToken = default);
}

public class RetryExecutionPlanner : IRetryExecutionPlanner
{
    private readonly IGitService _gitService;
    private readonly IProjectPlanReconstructor _projectPlanReconstructor;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<RetryExecutionPlanner> _logger;

    public RetryExecutionPlanner(
        IGitService gitService,
        IProjectPlanReconstructor projectPlanReconstructor,
        IFileSystem fileSystem,
        ILogger<RetryExecutionPlanner> logger)
    {
        _gitService = gitService;
        _projectPlanReconstructor = projectPlanReconstructor;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<RetryExecutionPlan> Create(string retryDirectory, CancellationToken cancellationToken = default)
    {
        var outputDirectory = _fileSystem.Path.GetFullPath(retryDirectory);
        var lastCommitMessage = await _gitService.GetLog(outputDirectory, cancellationToken);

        if (string.IsNullOrWhiteSpace(lastCommitMessage))
        {
            throw new InvalidOperationException(
                $"Cannot retry '{outputDirectory}' because no checkpoint commits were found. Start a new run instead.");
        }

        if (lastCommitMessage == "verification")
        {
            _logger.LogInformation(
                "Retry requested for {OutputDirectory}, but the most recent checkpoint is verification, which means the process completed successfully",
                outputDirectory);

            return new RetryExecutionPlan(
                RetryStartPhase.Completed,
                outputDirectory,
                ProjectPlan: null,
                NextSpecIndex: 0);
        }

        await _gitService.CleanAndRestore(outputDirectory, cancellationToken);

        var projectPlan = _projectPlanReconstructor.Reconstruct(outputDirectory);

        if (lastCommitMessage == "planning")
        {
            return new RetryExecutionPlan(
                RetryStartPhase.Generation,
                outputDirectory,
                projectPlan,
                NextSpecIndex: 0);
        }

        var completedSpecCount = int.Parse(lastCommitMessage["spec ".Length..]);
        var startPhase = completedSpecCount >= projectPlan.SpecFiles.Count
            ? RetryStartPhase.Verification
            : RetryStartPhase.Generation;

        return new RetryExecutionPlan(
            startPhase,
            outputDirectory,
            projectPlan,
            NextSpecIndex: completedSpecCount);
    }
}