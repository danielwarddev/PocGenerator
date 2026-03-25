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
    private readonly ILogger<GenerationPhaseHandler> _logger;

    public GenerationPhaseHandler(
        ISystemPromptProvider systemPromptProvider,
        ICodeGenerator codeGenerator,
        ILogger<GenerationPhaseHandler> logger)
    {
        _systemPromptProvider = systemPromptProvider;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    public async Task<GenerationResult> GenerateMvp(string outputDirectory, IReadOnlyList<string> specFiles, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting code generation for {Count} specs", specFiles.Count);

        var systemPrompt = _systemPromptProvider.GetDevelopingPrompt();
        return await _codeGenerator.Generate(systemPrompt, outputDirectory, specFiles, cancellationToken);
    }
}
