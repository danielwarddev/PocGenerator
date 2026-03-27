using AwesomeAssertions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator.Copilot;
using PocGenerator.Planning;
using PocGenerator.Verification;

namespace PocGenerator.Tests.Verification;

public class VerificationPhaseHandlerTests
{
    private readonly ICopilotService _copilotService = Substitute.For<ICopilotService>();
    private readonly ISystemPromptProvider _systemPromptProvider = Substitute.For<ISystemPromptProvider>();
    private readonly IIdeaFileLocator _ideaFileLocator = Substitute.For<IIdeaFileLocator>();
    private readonly ILogger<VerificationPhaseHandler> _logger = Substitute.For<ILogger<VerificationPhaseHandler>>();
    private readonly VerificationPhaseHandler _sut;

    public VerificationPhaseHandlerTests()
    {
        _copilotService
            .CreateSession(Arg.Any<CreateSessionConfig>(), Arg.Any<CancellationToken>())
            .Returns((CopilotSession)null!);

        _systemPromptProvider.GetVerificationPrompt().Returns("test-prompt");
        _ideaFileLocator.GetIdeaFiles().Returns(new IdeaFiles("/ideas/mvp.md", []));

        _sut = new VerificationPhaseHandler(_copilotService, _systemPromptProvider, _ideaFileLocator, _logger);
    }

    [Fact]
    public async Task When_VerifyMvp_Called_Then_Verification_Session_Is_Created_With_Output_Directory()
    {
        await _sut.VerifyMvp("/output", "/output/implementation-plan.md", TestContext.Current.CancellationToken);

        await _copilotService.Received(1).CreateSession(
            Arg.Is<CreateSessionConfig>(c => 
                c.SystemPrompt == "test-prompt" && 
                c.OutputDirectory == "/output"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_VerifyMvp_Called_Then_Readme_Session_Is_Created_Without_Output_Directory()
    {
        await _sut.VerifyMvp("/output", "/output/implementation-plan.md", TestContext.Current.CancellationToken);

        await _copilotService.Received(1).CreateSession(
            Arg.Is<CreateSessionConfig>(c => 
                c.SystemPrompt == string.Empty && 
                c.OutputDirectory == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_VerifyMvp_Called_Then_Both_Messages_Contain_Output_Directory()
    {
        await _sut.VerifyMvp("/my/output/dir", "/my/output/dir/implementation-plan.md", TestContext.Current.CancellationToken);

        await _copilotService.Received(2).SendMessage(
            Arg.Is<SendMessageConfig>(c => c.Prompt.Contains("/my/output/dir")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_VerifyMvp_Called_Then_Implementation_Plan_Is_Attached_To_Verification_Session()
    {
        await _sut.VerifyMvp("/output", "/output/implementation-plan.md", TestContext.Current.CancellationToken);

        await _copilotService.Received(1).SendMessage(
            Arg.Is<SendMessageConfig>(c =>
                c.AttachmentPaths != null &&
                c.AttachmentPaths.Single() == "/output/implementation-plan.md"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_VerifyMvp_Called_Then_Idea_File_Is_Attached_To_Readme_Session()
    {
        _ideaFileLocator.GetIdeaFiles().Returns(new IdeaFiles("/ideas/my-idea.md", []));

        await _sut.VerifyMvp("/output", "/output/implementation-plan.md", TestContext.Current.CancellationToken);

        await _copilotService.Received(1).SendMessage(
            Arg.Is<SendMessageConfig>(c =>
                c.AttachmentPaths != null &&
                c.AttachmentPaths.Single() == "/ideas/my-idea.md"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void When_VerificationPromptTemplate_Built_Then_Output_Directory_Is_Substituted()
    {
        var result = VerificationPhaseHandler.VerificationPromptTemplate.Replace("{OUTPUT_DIRECTORY}", "/test/output");

        result.Should().Contain("/test/output");
        result.Should().NotContain("{OUTPUT_DIRECTORY}");
    }

    [Fact]
    public void When_ReadmePromptTemplate_Built_Then_Output_Directory_Is_Substituted()
    {
        var result = VerificationPhaseHandler.ReadmePromptTemplate.Replace("{OUTPUT_DIRECTORY}", "/test/output");

        result.Should().Contain("/test/output");
        result.Should().NotContain("{OUTPUT_DIRECTORY}");
    }
}
