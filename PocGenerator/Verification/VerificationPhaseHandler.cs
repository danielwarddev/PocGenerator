using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;
using PocGenerator.Planning;

namespace PocGenerator.Verification;

public interface IVerificationPhaseHandler
{
    Task VerifyMvp(string outputDirectory, string implementationPlanPath, CancellationToken cancellationToken = default);
}

public class VerificationPhaseHandler : IVerificationPhaseHandler
{
    private readonly ICopilotService _copilotService;
    private readonly ISystemPromptProvider _systemPromptProvider;
    private readonly IIdeaFileLocator _ideaFileLocator;
    private readonly ILogger<VerificationPhaseHandler> _logger;

    public VerificationPhaseHandler(
        ICopilotService copilotService,
        ISystemPromptProvider systemPromptProvider,
        IIdeaFileLocator ideaFileLocator,
        ILogger<VerificationPhaseHandler> logger)
    {
        _copilotService = copilotService;
        _systemPromptProvider = systemPromptProvider;
        _ideaFileLocator = ideaFileLocator;
        _logger = logger;
    }

    public async Task VerifyMvp(string outputDirectory, string implementationPlanPath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MVP verification phase for {OutputDirectory}", outputDirectory);

        var systemPrompt = _systemPromptProvider.GetVerificationPrompt();
        var ideaFilePath = _ideaFileLocator.GetIdeaFiles().IdeaFilePath;

        await using (var session = await _copilotService.CreateSession(new CreateSessionConfig(systemPrompt, OutputDirectory: outputDirectory), cancellationToken))
        {
            var verificationPrompt = BuildVerificationPrompt(outputDirectory);
            await _copilotService.SendMessage(new SendMessageConfig(verificationPrompt, session, [implementationPlanPath]), cancellationToken);
        }

        _logger.LogInformation("MVP verification phase completed. Generating README");

        await using (var session = await _copilotService.CreateSession(new CreateSessionConfig(string.Empty), cancellationToken))
        {
            var readmePrompt = BuildReadmePrompt(outputDirectory);
            await _copilotService.SendMessage(new SendMessageConfig(readmePrompt, session, [ideaFilePath]), cancellationToken);
        }

        _logger.LogInformation("README generation completed");
    }

    private static string BuildVerificationPrompt(string outputDirectory)
    {
        return VerificationPromptTemplate.Replace("{OUTPUT_DIRECTORY}", outputDirectory);
    }

    private static string BuildReadmePrompt(string outputDirectory)
    {
        return ReadmePromptTemplate.Replace("{OUTPUT_DIRECTORY}", outputDirectory);
    }

    public const string VerificationPromptTemplate = """
        You are performing a final verification pass on a fully generated MVP project. The implementation plan is attached — read its **Definition of Done** section to identify all user flows that need e2e test coverage.

        WORKING DIRECTORY: {OUTPUT_DIRECTORY}

        INSTRUCTIONS:
        1. Read the attached implementation plan's Definition of Done to identify all core user flows.
        2. Run `dotnet build` in the working directory to ensure the solution compiles.
        3. Run `dotnet test` in the working directory to ensure all unit tests pass.
        4. Generate e2e test projects to cover all user flows:
           a. Create a **Playwright e2e test project** for web UI flows (page navigation, form submission, CRUD operations, interactive elements). Use Playwright when the flow involves browser rendering, JS interop, or visual layout.
           b. Create **HTTP integration tests** (using `WebApplicationFactory`) for API-level flows that don't require a browser. Use HTTP tests for pure API request/response verification.
           c. Implement each e2e test as a **separate subagent call** to avoid context bloat.
        5. After generating all tests, run the fix loop:
           a. Run `dotnet test` in the working directory
           b. If tests fail, diagnose the failures, fix the code or tests, and re-run `dotnet test`
           c. Repeat until all tests pass or attempts are exhausted
        6. Do NOT stop until all generated e2e tests pass along with all unit tests.
        """;

    public const string ReadmePromptTemplate = """
        You are generating a README.md file for a fully implemented MVP project. The idea/spec file is attached — read it to understand the application.

        WORKING DIRECTORY: {OUTPUT_DIRECTORY}

        INSTRUCTIONS:
        1. Explore the project structure in the working directory to understand the technical implementation.
        2. Write a `README.md` file to the root of the working directory covering:
           a. **Summary** — A concise paragraph (2–4 sentences) describing what the application does and who it is for.
           b. **Features** — A brief bulleted list of the core features (5–10 bullets).
           c. **Technical Architecture** — A Mermaid diagram (`classDiagram` or `flowchart TD`) showing the key components, their relationships, and dependencies. Include class names, interfaces, and important associations. Use `classDiagram` for object-oriented designs and `flowchart` for pipeline/service architectures.
           d. **Getting Started** — Minimal steps to build and run the project locally (prerequisites, build command, run command).
        3. Write the file to: `{OUTPUT_DIRECTORY}\README.md`
        4. The README should be clear, professional, and immediately useful to a developer picking up the project for the first time.

        RULES:
        - Use proper Markdown formatting with headings, code blocks, and lists.
        - The Mermaid diagram MUST use valid Mermaid syntax and include meaningful class/node names (not generic placeholders).
        - Do NOT include placeholder text — every section should contain real information derived from the actual project.
        """;
}
