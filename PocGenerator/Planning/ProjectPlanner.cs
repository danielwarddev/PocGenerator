using System.IO.Abstractions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;

namespace PocGenerator.Planning;

public record ProjectPlan(CopilotSession Session, string OutputDirectory, string PlanFilePath, IReadOnlyList<string> SpecFiles);

public interface IProjectPlanner
{
    Task<ProjectPlan> GeneratePlan(CopilotSession session, string slug, IdeaFiles ideaFiles, CancellationToken cancellationToken = default);
}

public class ProjectPlanner : IProjectPlanner
{
    private readonly ICopilotService _copilotService;
    private readonly IOutputDirectoryService _outputDirectoryService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ProjectPlanner> _logger;

    public ProjectPlanner(
        ICopilotService copilotService,
        IOutputDirectoryService outputDirectoryService,
        IFileSystem fileSystem,
        ILogger<ProjectPlanner> logger)
    {
        _copilotService = copilotService;
        _outputDirectoryService = outputDirectoryService;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<ProjectPlan> GeneratePlan(CopilotSession session, string slug, IdeaFiles ideaFiles, CancellationToken cancellationToken)
    {
        var outputDirectory = await _outputDirectoryService.CreateOutputFolder(slug, cancellationToken);
        _outputDirectoryService.CopyProjectScripts(outputDirectory);
        _outputDirectoryService.CopyDirectoryBuildProps(outputDirectory);

        var mvpDefinitionSource = _fileSystem.Path.GetDirectoryName(ideaFiles.IdeaFilePath)!;
        _outputDirectoryService.CopyMvpDefinition(outputDirectory, mvpDefinitionSource);

        var response = await _copilotService.SendMessage(new SendMessageConfig(PlanningPrompt, session, ideaFiles.AllFilePaths()), cancellationToken);

        var planFilePath = WritePlanDocument(outputDirectory, response);
        _logger.LogInformation("Implementation plan written to: {PlanPath}", planFilePath);

        return new ProjectPlan(session, outputDirectory, planFilePath, []);
    }

    private string WritePlanDocument(string outputDirectory, string planContent)
    {
        var planPath = _fileSystem.Path.Combine(outputDirectory, "implementation-plan.md");
        _fileSystem.File.WriteAllText(planPath, planContent);
        return planPath;
    }

    private const string PlanningPrompt = """
        Give me a comprehensive implementation plan for an MVP project in markdown format. To do this, follow the instructions below.
            
        Analyze the attached mvp.md file describing an MVP idea. The document should include the following sections:

        ## Project Goal

        A clear summary of what the MVP does and who it's for.

        ## Requirements

        A list of the key functional requirements for the MVP. The project should NEVER reach out to real databases (except local ones it creates), HTTP services, APIs, etc. It can make these services that would do these things, but it should always use fake data in place of calling these things to simulate them for the MVP.

        ## Definition of Done

        A list of clear criteria of functionality that define when the project can be considered complete.

        ## Architecture Overview

        Describe the high-level architecture: what components exist, how they interact, and any key design decisions. In general, this should use vertical slicing.

        ## Solution Structure

        List every project in the .NET solution. For each project, include:

        - The project name (PascalCase)
        - Its purpose
        - Which PowerShell scaffolding script to run to create it. You MUST use these scripts to create projects. Do NOT call the dotnet CLI yourself to do it.

        Present this as a markdown table with columns: Project Name, Script, Purpose.

        Available PowerShell scaffolding scripts (you MUST only use these):
        - `create-console-project.ps1` — Console application
        - `create-library-project.ps1` — Class library
        - `create-blazor-project.ps1` — Blazor web app
        - `create-db-project.ps1` — EF Core database project
        - `create-test-project.ps1` — xUnit test project

        For each source project, also include a corresponding xUnit test project using `create-test-project.ps1`, unless testing doesn't apply (e.g. database projects).

        **IMPORTANT**: Do NOT create a pseudo-folder hierarchy using project names in the Solution Structure (e.g., do not use "src/", "tests/", or similar prefixes in project names). The PowerShell scripts handle folder creation automatically. Just list the project name as-is, and let the scripts determine the directory structure.

        ## Project References

        Describe which projects reference which (e.g. test projects reference their source project, the console app references the core library, etc.).

        ## Implementation Notes

        Any additional guidance for code generation: key patterns to follow, important libraries to use, data modeling notes, API design, etc.

        Return ONLY the markdown content. Do not wrap it in code fences or create a file.
    """;
}
