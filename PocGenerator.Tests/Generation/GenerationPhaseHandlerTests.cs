using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator.Copilot;
using PocGenerator.Generation;

namespace PocGenerator.Tests.Generation;

public class GenerationPhaseHandlerTests
{
    private readonly ISystemPromptProvider _systemPromptProvider = Substitute.For<ISystemPromptProvider>();
    private readonly ICodeGenerator _codeGenerator = Substitute.For<ICodeGenerator>();
    private readonly ILogger<GenerationPhaseHandler> _logger = Substitute.For<ILogger<GenerationPhaseHandler>>();
    private readonly GenerationPhaseHandler _sut;

    public GenerationPhaseHandlerTests()
    {
        _systemPromptProvider.GetDevelopingPrompt().Returns("test-prompt");
        _sut = new GenerationPhaseHandler(_systemPromptProvider, _codeGenerator, _logger);

        _codeGenerator
            .Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var specs = callInfo.ArgAt<IReadOnlyList<string>>(2);
                var results = specs.Select(s => new SpecResult(s, Success: true)).ToList();
                return new GenerationResult(results);
            });
    }

    [Fact]
    public async Task When_Spec_Files_Provided_Then_They_Are_Passed_To_CodeGenerator()
    {
        var specFiles = CreateSpecFileList("spec-01-auth.md", "spec-02-dashboard.md");

        await _sut.GenerateMvp("/output", specFiles, TestContext.Current.CancellationToken);

        await _codeGenerator.Received(1).Generate(
            "test-prompt",
            "/output",
            specFiles,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_CodeGenerator_Returns_Result_Then_It_Is_Forwarded()
    {
        var specFiles = CreateSpecFileList("spec-01-auth.md");
        var expected = new GenerationResult(
            [new SpecResult("/output/Specs/spec-01-auth.md", Success: true)]);

        _codeGenerator
            .Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.GenerateMvp("/output", specFiles, TestContext.Current.CancellationToken);

        result.Should().BeSameAs(expected);
    }

    private static IReadOnlyList<string> CreateSpecFileList(params string[] fileNames)
    {
        return fileNames.Select(n => $"/output/Specs/{n}").ToList();
    }
}
