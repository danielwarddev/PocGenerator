using System.IO.Abstractions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;

namespace PocGenerator.Planning;

public interface ISpecSplitter
{
    Task<IReadOnlyList<string>> SplitPlan(CopilotSession session, ProjectPlan plan, CancellationToken cancellationToken = default);
}

public class SpecSplitter : ISpecSplitter
{
    public const int MaxSpecCount = 10;

    private readonly ICopilotService _copilotService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<SpecSplitter> _logger;

    public SpecSplitter(ICopilotService copilotService, IFileSystem fileSystem, ILogger<SpecSplitter> logger)
    {
        _copilotService = copilotService;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> SplitPlan(CopilotSession session, ProjectPlan plan, CancellationToken cancellationToken)
    {
        var specsDirectory = EnsureSpecsDirectory(plan.OutputDirectory);
        var prompt = BuildSplittingPrompt(specsDirectory);

        await _copilotService.SendMessage(
            new SendMessageConfig(prompt, session, [plan.PlanFilePath]), cancellationToken);

        var specFiles = _fileSystem.Directory
            .GetFiles(specsDirectory, "*.md")
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (specFiles.Count == 0)
        {
            throw new InvalidOperationException("Copilot did not create any spec files. Cannot proceed with code generation.");
        }
        else if (specFiles.Count > MaxSpecCount)
        {
            throw new InvalidOperationException($"Copilot created {specFiles.Count} specs, which exceeds the maximum of {MaxSpecCount}");
        }

        _logger.LogInformation("Generated {Count} spec files", specFiles.Count);
        return specFiles;
    }

    private string EnsureSpecsDirectory(string outputDirectory)
    {
        var specsDirectory = _fileSystem.Path.Combine(outputDirectory, "Specs");
        _fileSystem.Directory.CreateDirectory(specsDirectory);
        return specsDirectory;
    }

    private static string BuildSplittingPrompt(string specsDirectory)
    {
        return $"""
            You are given an implementation plan for an MVP project (attached). Your task is to split this plan into focused, independently implementable spec files — one per feature area.

            Create each spec as a separate file inside the following directory (use this EXACT absolute path):
            {specsDirectory}

            Name each file with a "spec-" prefix and a two-digit numeric index to preserve the order, followed by a kebab-case slug of the title. Examples:
            - spec-01-user-authentication.md
            - spec-02-dashboard-ui.md

            RULES:
            - Produce NO MORE THAN {MaxSpecCount} spec files. Keep it MVP — group related work together.
            - Each spec should represent a meaningful unit of work (e.g., "User Authentication", "Dashboard UI"), NOT a single file.
            - Group logically related items together (e.g., a model and its tests belong in one spec, not two).
            - Each spec MUST follow the exact structure defined in the system prompt, including ALL required sections in this order: Title & Status (use 📋 Not Started), User Story, Description, Acceptance Criteria (grouped under headings with checkboxes), Out of Scope, Technical Notes, and Definition of Done.
            - Additionally, each spec must include a "Files to Create or Modify" section listing the specific files for that spec.
            - Order specs by implementation dependency (foundational specs first).
            - You MUST create the files — do NOT just return the content as a response. Use the filesystem to write each file to the path above.
            - Use subagents to create each spec file. Each subagent should receive only the portion of the plan relevant to that spec and the spec template structure, and should write exactly one file.

            **CRITICAL FILE PATH RULES FOR "FILES TO CREATE OR MODIFY" SECTIONS**:
            - DO NOT use "src/" or "tests/" prefixes in file paths. The PowerShell scaffolding scripts create projects at the solution root (e.g., MyProject/, MyProject.Tests/) or will organize subdirectories automatically.
            - Reference file paths RELATIVE TO THE PROJECT ROOT. Examples:
              * For TCGCardBrowserCore: "Models/Card.cs", "Interfaces/IRepository.cs", etc.
              * For TCGCardBrowserCore.Tests: "CardModelTests.cs", "RepositoryTests.cs", etc.
            - Use project names as folder names directly (e.g., TCGCardBrowserCore/, TCGCardBrowserInfrastructure/), NOT nested under src/ or tests/.
            - If you need to suggest where a file belongs, use the project name as the container context (e.g., "In TCGCardBrowserCore.Tests: CardModelTests.cs").
            """;
    }
}
