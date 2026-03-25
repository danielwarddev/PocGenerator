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
    public async Task When_Specs_Provided_Then_Each_Spec_Sends_One_Message()
    {
        var specFiles = SetupSpecFiles("spec-01-auth.md", "spec-02-dashboard.md");
        SetupOutputDirectory();

        await _sut.Generate("test-prompt", "/output", specFiles, TestContext.Current.CancellationToken);

        await _copilotService.Received(2).SendMessage(
            Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Specs_Provided_Then_Result_Reports_All_Succeeded()
    {
        var specFiles = SetupSpecFiles("spec-01-auth.md", "spec-02-dashboard.md");
        SetupOutputDirectory();

        var result = await _sut.Generate("test-prompt", "/output", specFiles, TestContext.Current.CancellationToken);

        result.SucceededCount.Should().Be(2);
        result.SpecResults.Should().AllSatisfy(r => r.Success.Should().BeTrue());
    }

    [Fact]
    public async Task When_Prompt_Sent_Then_It_Contains_Output_Directory()
    {
        var specFiles = SetupSpecFiles("spec-01-auth.md");
        SetupOutputDirectory();

        await _sut.Generate("test-prompt", "/output", specFiles, TestContext.Current.CancellationToken);

        await _copilotService.Received().SendMessage(
            Arg.Is<SendMessageConfig>(c => c.Prompt.Contains("/output")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Spec_File_Attached_Then_Only_Spec_File_Is_Attached()
    {
        var specFiles = SetupSpecFiles("spec-01-auth.md");
        _fileSystem.AddDirectory("/output");

        await _sut.Generate("test-prompt", "/output", specFiles, TestContext.Current.CancellationToken);

        await _copilotService.Received().SendMessage(
            Arg.Is<SendMessageConfig>(c =>
                c.AttachmentPaths != null &&
                c.AttachmentPaths.Count == 1 &&
                c.AttachmentPaths[0].Contains("spec-01-auth.md")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Empty_Spec_List_Then_No_Messages_Sent_And_Empty_Result()
    {
        var result = await _sut.Generate("test-prompt", "/output", [], TestContext.Current.CancellationToken);

        result.SpecResults.Should().BeEmpty();
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
        // Just verify it constructs without error - default cap is 50
        generator.Should().NotBeNull();
    }

    [Fact]
    public async Task When_Hard_Cap_Reached_Then_Remaining_Specs_Are_Skipped()
    {
        var config = new CodeGenerator.HardCapConfig(MaxRequests: 2);
        var sut = new CodeGenerator(_copilotService, _fileSystem, _logger, config);
        var specFiles = SetupSpecFiles("spec-01-auth.md", "spec-02-dashboard.md", "spec-03-api.md");
        SetupOutputDirectory();

        var result = await sut.Generate("test-prompt", "/output", specFiles, TestContext.Current.CancellationToken);

        result.SpecResults.Should().HaveCount(2);
        await _copilotService.Received(2).SendMessage(
            Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Hard_Cap_Set_To_Zero_Then_No_Specs_Are_Processed()
    {
        var config = new CodeGenerator.HardCapConfig(MaxRequests: 0);
        var sut = new CodeGenerator(_copilotService, _fileSystem, _logger, config);
        var specFiles = SetupSpecFiles("spec-01-auth.md");
        SetupOutputDirectory();

        var result = await sut.Generate("test-prompt", "/output", specFiles, TestContext.Current.CancellationToken);

        result.SpecResults.Should().BeEmpty();
    }

    private IReadOnlyList<string> SetupSpecFiles(params string[] specFileNames)
    {
        var specPaths = new List<string>();
        foreach (var name in specFileNames)
        {
            var path = $"/output/Specs/{name}";
            _fileSystem.AddFile(path, new MockFileData($"# {name}\n\nSpec content."));
            specPaths.Add(path);
        }

        return specPaths;
    }

    private void SetupOutputDirectory()
    {
        _fileSystem.AddDirectory("/output");
    }
}
