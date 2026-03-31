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
    private readonly IGitService _gitService = Substitute.For<IGitService>();
    private readonly ILogger<GenerationPhaseHandler> _logger = Substitute.For<ILogger<GenerationPhaseHandler>>();
    private readonly GenerationPhaseHandler _sut;

    public GenerationPhaseHandlerTests()
    {
        _systemPromptProvider.GetDevelopingPrompt().Returns("test-prompt");
        _sut = new GenerationPhaseHandler(_systemPromptProvider, _codeGenerator, _gitService, _logger);

        _codeGenerator
            .Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var specFile = callInfo.ArgAt<string>(2);
                return new SpecResult(specFile, Success: true);
            });
    }

    [Fact]
    public async Task When_Spec_Files_Provided_Then_Each_Spec_Is_Generated_And_Committed_In_Order()
    {
        var specFiles = CreateSpecFileList("spec-01-auth.md", "spec-02-dashboard.md");

        await _sut.GenerateMvp("/output", specFiles, TestContext.Current.CancellationToken);

        Received.InOrder(() =>
        {
            _codeGenerator.Generate(
                "test-prompt",
                "/output",
                "/output/Specs/spec-01-auth.md",
                Arg.Any<CancellationToken>());
            _gitService.AddAll("/output", Arg.Any<CancellationToken>());
            _gitService.Commit("spec 1", "/output", Arg.Any<CancellationToken>());
            _codeGenerator.Generate(
                "test-prompt",
                "/output",
                "/output/Specs/spec-02-dashboard.md",
                Arg.Any<CancellationToken>());
            _gitService.AddAll("/output", Arg.Any<CancellationToken>());
            _gitService.Commit("spec 2", "/output", Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task When_CodeGenerator_Returns_Result_Then_It_Is_Aggregated()
    {
        var specFiles = CreateSpecFileList("spec-01-auth.md", "spec-02-dashboard.md");

        var result = await _sut.GenerateMvp("/output", specFiles, TestContext.Current.CancellationToken);

        result.SpecResults.Should().BeEquivalentTo(
            [
                new SpecResult("/output/Specs/spec-01-auth.md", Success: true),
                new SpecResult("/output/Specs/spec-02-dashboard.md", Success: true)
            ]);
    }

    [Fact]
    public async Task When_CodeGenerator_Returns_No_Result_Then_Remaining_Specs_Are_Not_Processed()
    {
        var specFiles = CreateSpecFileList("spec-01-auth.md", "spec-02-dashboard.md", "spec-03-api.md");

        _codeGenerator
            .Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var spec = callInfo.ArgAt<string>(2);
                return spec.EndsWith("spec-02-dashboard.md", StringComparison.Ordinal)
                    ? null
                    : new SpecResult(spec, Success: true);
            });

        var result = await _sut.GenerateMvp("/output", specFiles, TestContext.Current.CancellationToken);

        result.SpecResults.Should().BeEquivalentTo(
            [new SpecResult("/output/Specs/spec-01-auth.md", Success: true)]);
        await _codeGenerator.DidNotReceive().Generate(
            Arg.Any<string>(),
            Arg.Any<string>(),
            "/output/Specs/spec-03-api.md",
            Arg.Any<CancellationToken>());
    }

    private static IReadOnlyList<string> CreateSpecFileList(params string[] fileNames)
    {
        return fileNames.Select(n => $"/output/Specs/{n}").ToList();
    }
}
