using System.IO.Abstractions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;

namespace PocGenerator.Generation;

public record SpecResult(string SpecFile, bool Success);

public record GenerationResult(IReadOnlyList<SpecResult> SpecResults)
{
    public int SucceededCount => SpecResults.Count(r => r.Success);
}

public interface ICodeGenerator
{
    Task<SpecResult?> Generate(
        string systemPrompt,
        string outputDirectory,
        string specFile,
        CancellationToken cancellationToken = default);
}

public class CodeGenerator : ICodeGenerator
{
    public record HardCapConfig(int MaxRequests = 50);

    private readonly ICopilotService _copilotService;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<CodeGenerator> _logger;
    private readonly int _hardCap;
    private int _requestCount;

    public CodeGenerator(ICopilotService copilotService, IFileSystem fileSystem, ILogger<CodeGenerator> logger, HardCapConfig? config = null)
    {
        _copilotService = copilotService;
        _fileSystem = fileSystem;
        _logger = logger;
        _hardCap = config?.MaxRequests ?? 50;
    }

    public async Task<SpecResult?> Generate(
        string systemPrompt,
        string outputDirectory,
        string specFile,
        CancellationToken cancellationToken = default)
    {
        if (_requestCount >= _hardCap)
        {
            _logger.LogWarning(
                "Hard cap of {HardCap} requests reached before spec {SpecFile}. Stopping generation",
                _hardCap,
                specFile);
            return null;
        }

        var specName = _fileSystem.Path.GetFileNameWithoutExtension(specFile);
        _logger.LogInformation("Implementing spec: {SpecName}", specName);

        await using var session = await _copilotService.CreateSession(new CreateSessionConfig(systemPrompt, OutputDirectory: outputDirectory), cancellationToken);
        var prompt = BuildPrompt(outputDirectory);
        await _copilotService.SendMessage(new SendMessageConfig(prompt, session, [specFile]), cancellationToken);
        _requestCount++;

        _logger.LogInformation("Spec {SpecName} completed successfully", specName);
        return new SpecResult(specFile, Success: true);
    }

    private static string BuildPrompt(string outputDirectory)
    {
        return PromptTemplate.Replace("{OUTPUT_DIRECTORY}", outputDirectory);
    }

    public const string PromptTemplate = """
        You are implementing a spec for an MVP project. The spec file is attached — read it and implement ALL code it describes.

        WORKING DIRECTORY: {OUTPUT_DIRECTORY}

        INSTRUCTIONS:
        1. Read the attached spec file carefully. Implement every file and feature it describes.
        2. Write all code files to the correct paths within the project structure in the working directory above.
        3. After writing the code, run `dotnet test` in the working directory to build and test the solution.
        4. If `dotnet test` fails (build errors or test failures):
           a. Read the error output carefully
           b. Fix the code to resolve the errors
           c. Run `dotnet test` again
           d. Repeat until all tests pass
        5. Do NOT stop until `dotnet test` passes successfully, or you have exhausted all reasonable fix attempts.

        RULES:
        - All data must be fake/mock — NEVER call real external services, APIs, or databases (except local ones the project creates).
        - All code must compile and all tests must pass.
        - Follow the project's existing patterns and conventions.
        - Write idiomatic C# with nullable reference types enabled.
        - If a spec mentions a project you haven't seen yet, check if it exists. If not, use the appropriate `create_*_project()` tool before writing code.
        - Whenever you encounter an issue that required a fix (build error, test failure, logic bug, etc.), record it in `fixes.md` at the solution root. Use a short description of the problem and the fix applied.
        """;
}
