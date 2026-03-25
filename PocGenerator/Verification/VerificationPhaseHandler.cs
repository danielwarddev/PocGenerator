using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;
using PocGenerator.Planning;

namespace PocGenerator.Verification;

public interface IVerificationPhaseHandler
{
    Task VerifyMvp(string outputDirectory, CancellationToken cancellationToken = default);
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

    public async Task VerifyMvp(string outputDirectory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MVP verification phase for {OutputDirectory}", outputDirectory);

        var systemPrompt = _systemPromptProvider.GetVerificationPrompt();
        var ideaFilePath = _ideaFileLocator.GetIdeaFiles().IdeaFilePath;

        await using (var session = await _copilotService.CreateSession(new CreateSessionConfig(systemPrompt, OutputDirectory: outputDirectory), cancellationToken))
        {
            var verificationPrompt = BuildVerificationPrompt(outputDirectory);
            await _copilotService.SendMessage(new SendMessageConfig(verificationPrompt, session, [ideaFilePath]), cancellationToken);
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
        You are performing a final verification pass on a fully generated MVP project. The idea/spec file is attached — read it to understand what the application does.

        WORKING DIRECTORY: {OUTPUT_DIRECTORY}

        INSTRUCTIONS:
        1. Read the attached file to understand the application's purpose and features.
        2. Run `dotnet build` in the working directory to ensure the solution compiles.
        3. Run `dotnet test` in the working directory to ensure all unit tests pass.
        4. If the project includes a web application:
           a. Start it using `dotnet run`
           b. Use the Playwright MCP browser to test all core user flows from the spec
           c. Fix any issues that arise
           d. Re-run `dotnet test` after each fix
           e. Stop the application when done
        5. If the project is a console application, run it and verify output matches the spec.
        6. Do NOT stop until every core user flow works correctly end-to-end and all tests pass.
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
