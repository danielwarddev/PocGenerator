using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator.Copilot;
using PocGenerator.Generation;

namespace PocGenerator.Tests.Generation;

public class CodeGeneratorTests
{
    private readonly ICopilotService _copilotService = Substitute.For<ICopilotService>();
    private readonly MockFileSystem _fileSystem = new();
    private readonly ILogger<CodeGenerator> _logger = Substitute.For<ILogger<CodeGenerator>>();
    private readonly CodeGenerator _sut;

    public CodeGeneratorTests()
    {
        _copilotService
            .CreateSession(Arg.Any<CreateSessionConfig>(), Arg.Any<CancellationToken>())
            .Returns((CopilotSession)null!);

        _sut = new CodeGenerator(_copilotService, _fileSystem, _logger);
    }

    [Fact]
    public async Task When_Spec_Provided_Then_One_Message_Is_Sent()
    {
        var specFile = SetupSpecFile("spec-01-auth.md");
        SetupOutputDirectory();

        await _sut.Generate("test-prompt", "/output", specFile, TestContext.Current.CancellationToken);

        await _copilotService.Received(1).SendMessage(
            Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Spec_Provided_Then_Result_Reports_Success()
    {
        var specFile = SetupSpecFile("spec-01-auth.md");
        SetupOutputDirectory();

        var result = await _sut.Generate("test-prompt", "/output", specFile, TestContext.Current.CancellationToken);

        result.Should().Be(new SpecResult(specFile, Success: true));
    }

    [Fact]
    public async Task When_Prompt_Sent_Then_It_Contains_Output_Directory()
    {
        var specFile = SetupSpecFile("spec-01-auth.md");
        SetupOutputDirectory();

        await _sut.Generate("test-prompt", "/output", specFile, TestContext.Current.CancellationToken);

        await _copilotService.Received().SendMessage(
            Arg.Is<SendMessageConfig>(c => c.Prompt.Contains("/output")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Spec_File_Attached_Then_Only_Spec_File_Is_Attached()
    {
        var specFile = SetupSpecFile("spec-01-auth.md");
        _fileSystem.AddDirectory("/output");

        await _sut.Generate("test-prompt", "/output", specFile, TestContext.Current.CancellationToken);

        await _copilotService.Received().SendMessage(
            Arg.Is<SendMessageConfig>(c =>
                c.AttachmentPaths != null &&
                c.AttachmentPaths.Count == 1 &&
                c.AttachmentPaths[0].Contains("spec-01-auth.md")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Hard_Cap_Set_To_Zero_Then_No_Spec_Is_Processed()
    {
        var config = new CodeGenerator.HardCapConfig(MaxRequests: 0);
        var sut = new CodeGenerator(_copilotService, _fileSystem, _logger, config);
        var specFile = SetupSpecFile("spec-01-auth.md");
        SetupOutputDirectory();

        var result = await sut.Generate("test-prompt", "/output", specFile, TestContext.Current.CancellationToken);

        result.Should().BeNull();
        await _copilotService.DidNotReceive().SendMessage(
            Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void When_Prompt_Template_Accessed_Then_It_Contains_Key_Instructions()
    {
        CodeGenerator.PromptTemplate.Should().Contain("dotnet test");
        CodeGenerator.PromptTemplate.Should().Contain("{OUTPUT_DIRECTORY}");
        CodeGenerator.PromptTemplate.Should().Contain("fix");
        CodeGenerator.PromptTemplate.Should().Contain("fake");
    }

    [Fact]
    public void When_HardCapConfig_Not_Provided_Then_Default_Is_50()
    {
        var generator = new CodeGenerator(_copilotService, _fileSystem, _logger);
        generator.Should().NotBeNull();
    }

    [Fact]
    public async Task When_Hard_Cap_Reached_Then_Later_Specs_Are_Skipped_Across_Calls()
    {
        var config = new CodeGenerator.HardCapConfig(MaxRequests: 1);
        var sut = new CodeGenerator(_copilotService, _fileSystem, _logger, config);
        var firstSpec = SetupSpecFile("spec-01-auth.md");
        var secondSpec = SetupSpecFile("spec-02-dashboard.md");
        SetupOutputDirectory();

        var firstResult = await sut.Generate("test-prompt", "/output", firstSpec, TestContext.Current.CancellationToken);
        var secondResult = await sut.Generate("test-prompt", "/output", secondSpec, TestContext.Current.CancellationToken);

        firstResult.Should().Be(new SpecResult(firstSpec, Success: true));
        secondResult.Should().BeNull();
        await _copilotService.Received(1).SendMessage(
            Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>());
    }

    private string SetupSpecFile(string specFileName)
    {
        var path = $"/output/Specs/{specFileName}";
        _fileSystem.AddFile(path, new MockFileData($"# {specFileName}\n\nSpec content."));
        return path;
    }

    private void SetupOutputDirectory()
    {
        _fileSystem.AddDirectory("/output");
    }
}
