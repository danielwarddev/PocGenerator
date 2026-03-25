using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;

namespace PocGenerator.Planning;

public interface IPlanningPhaseHandler
{
    Task<ProjectPlan> CreatePlan(CancellationToken cancellationToken = default);
}

public class PlanningPhaseHandler : IPlanningPhaseHandler
{
    private readonly ICopilotService _copilotService;
    private readonly ISystemPromptProvider _systemPromptProvider;
    private readonly IIdeaFileLocator _ideaFileLocator;
    private readonly ISlugGenerator _slugGenerator;
    private readonly IProjectPlanner _projectPlanner;
    private readonly ISpecSplitter _specSplitter;
    private readonly ILogger<PlanningPhaseHandler> _logger;

    public PlanningPhaseHandler(
        ICopilotService copilotService,
        ISystemPromptProvider systemPromptProvider,
        IIdeaFileLocator ideaFileLocator,
        ISlugGenerator slugGenerator,
        IProjectPlanner projectPlanner,
        ISpecSplitter specSplitter,
        ILogger<PlanningPhaseHandler> logger)
    {
        _copilotService = copilotService;
        _systemPromptProvider = systemPromptProvider;
        _ideaFileLocator = ideaFileLocator;
        _slugGenerator = slugGenerator;
        _projectPlanner = projectPlanner;
        _specSplitter = specSplitter;
        _logger = logger;
    }

    public async Task<ProjectPlan> CreatePlan(CancellationToken cancellationToken)
    {
        var planningPrompt = _systemPromptProvider.GetPlanningPrompt();
        await using var session = await _copilotService.CreateSession(new CreateSessionConfig(planningPrompt), cancellationToken);

        var ideaFiles = _ideaFileLocator.GetIdeaFiles();
        _logger.LogInformation("Found idea file: {IdeaFilePath}", ideaFiles.IdeaFilePath);

        if (ideaFiles.ContextFilePaths.Count > 0)
            _logger.LogInformation("Found {ContextFileCount} context file(s) to attach for planning", ideaFiles.ContextFilePaths.Count);

        var slug = await _slugGenerator.Generate(session, ideaFiles.IdeaFilePath, cancellationToken);
        _logger.LogInformation("Project slug: {Slug}", slug);

        var plan = await _projectPlanner.GeneratePlan(session, slug, ideaFiles, cancellationToken);
        _logger.LogInformation("Planning complete. Plan written to: {PlanFilePath}", plan.PlanFilePath);

        var specFiles = await _specSplitter.SplitPlan(plan, cancellationToken);
        _logger.LogInformation("Spec splitting complete. {SpecCount} specs generated", specFiles.Count);

        return plan with { SpecFiles = specFiles };
    }
}
