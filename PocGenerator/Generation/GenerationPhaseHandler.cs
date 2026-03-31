using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;

namespace PocGenerator.Generation;

public interface IGenerationPhaseHandler
{
    Task<GenerationResult> GenerateMvp(string outputDirectory, IReadOnlyList<string> specFiles, CancellationToken cancellationToken = default);
}

public class GenerationPhaseHandler : IGenerationPhaseHandler
{
    private readonly ISystemPromptProvider _systemPromptProvider;
    private readonly ICodeGenerator _codeGenerator;
    private readonly IGitService _gitService;
    private readonly ILogger<GenerationPhaseHandler> _logger;

    public GenerationPhaseHandler(
        ISystemPromptProvider systemPromptProvider,
        ICodeGenerator codeGenerator,
        IGitService gitService,
        ILogger<GenerationPhaseHandler> logger)
    {
        _systemPromptProvider = systemPromptProvider;
        _codeGenerator = codeGenerator;
        _gitService = gitService;
        _logger = logger;
    }

    public async Task<GenerationResult> GenerateMvp(string outputDirectory, IReadOnlyList<string> specFiles, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting code generation for {Count} specs", specFiles.Count);

        var systemPrompt = _systemPromptProvider.GetDevelopingPrompt();
        var specResults = new List<SpecResult>();

        for (var i = 0; i < specFiles.Count; i++)
        {
            var result = await _codeGenerator.Generate(systemPrompt, outputDirectory, specFiles[i], cancellationToken);

            if (result is null)
            {
                _logger.LogWarning(
                    "Stopping generation before spec {SpecNumber} because no result was returned",
                    i + 1);
                break;
            }

            specResults.Add(result);

            if (result.Success)
            {
                await _gitService.AddAll(outputDirectory, cancellationToken);
                await _gitService.Commit($"spec {i + 1}", outputDirectory, cancellationToken);
                _logger.LogInformation("Generation checkpoint committed for spec {SpecNumber}", i + 1);
            }
        }

        return new GenerationResult(specResults);
    }
}
