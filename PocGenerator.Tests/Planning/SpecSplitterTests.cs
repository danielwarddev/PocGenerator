using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator.Copilot;
using PocGenerator.Planning;

namespace PocGenerator.Tests;

public class SpecSplitterTests
{
    private readonly ICopilotService _copilotService = Substitute.For<ICopilotService>();
    private readonly MockFileSystem _fileSystem = new();
    private readonly ILogger<SpecSplitter> _logger = Substitute.For<ILogger<SpecSplitter>>();
    private readonly CopilotSession _session = null!;
    private readonly SpecSplitter _sut;

    public SpecSplitterTests()
    {
        _sut = new SpecSplitter(_copilotService, _fileSystem, _logger);
    }

    [Fact]
    public async Task When_Copilot_Creates_Spec_Files_Then_They_Are_Discovered()
    {
        var plan = SetupPlan();
        SetupCopilotResponse();
        SimulateCopilotCreatedSpecs(
            ("/output/test/specs/spec-01-user-authentication.md", "# User Authentication\n\nHandle login."),
            ("/output/test/specs/spec-02-dashboard-ui.md", "# Dashboard UI\n\nCreate dashboard."));

        var result = await _sut.SplitPlan(_session, plan, TestContext.Current.CancellationToken);

        result.Should().Contain(f => f.Contains("spec-01-user-authentication.md"));
        result.Should().Contain(f => f.Contains("spec-02-dashboard-ui.md"));
    }

    [Fact]
    public async Task When_Copilot_Creates_More_Than_10_Specs_Then_Exception_Is_Thrown()
    {
        var plan = SetupPlan();
        SetupCopilotResponse();
        var specFiles = Enumerable.Range(1, 12)
            .Select(i => ($"/output/test/specs/spec-{i:D2}-feature-{i}.md", $"# Feature {i}\n\nDescription."))
            .ToArray();
        SimulateCopilotCreatedSpecs(specFiles);

        var act = () => _sut.SplitPlan(_session, plan, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*12*exceeds*10*");
    }

    [Fact]
    public async Task When_Copilot_Creates_No_Spec_Files_Then_Exception_Is_Thrown()
    {
        var plan = SetupPlan();
        SetupCopilotResponse();

        var act = () => _sut.SplitPlan(_session, plan, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*did not create any spec files*");
    }

    [Fact]
    public async Task When_Plan_Is_Split_Then_Plan_File_Is_Attached()
    {
        var plan = SetupPlan();
        SetupCopilotResponse();
        SimulateCopilotCreatedSpecs(("/output/test/specs/spec-01-feature.md", "# Feature\n\nContent."));

        await _sut.SplitPlan(_session, plan, TestContext.Current.CancellationToken);

        await _copilotService.Received(1).SendMessage(
            Arg.Is<SendMessageConfig>(c =>
                c.Session == _session &&
                c.AttachmentPaths != null &&
                c.AttachmentPaths.Count == 1 &&
                c.AttachmentPaths[0] == "/output/test/implementation-plan.md"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Plan_Is_Split_Then_Prompt_Contains_Specs_Directory_Path()
    {
        var plan = SetupPlan();
        SetupCopilotResponse();
        SimulateCopilotCreatedSpecs(("/output/test/specs/spec-01-feature.md", "# Feature\n\nContent."));

        await _sut.SplitPlan(_session, plan, TestContext.Current.CancellationToken);

        var expectedPath = _fileSystem.Path.Combine("/output/test", "Specs");
        await _copilotService.Received(1).SendMessage(
            Arg.Is<SendMessageConfig>(c =>
                c.Session == _session &&
                c.Prompt.Contains(expectedPath)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Plan_Is_Split_Then_Prompt_Instructs_File_Creation()
    {
        var plan = SetupPlan();
        SetupCopilotResponse();
        SimulateCopilotCreatedSpecs(("/output/test/specs/spec-01-feature.md", "# Feature\n\nContent."));

        await _sut.SplitPlan(_session, plan, TestContext.Current.CancellationToken);

        await _copilotService.Received(1).SendMessage(
            Arg.Is<SendMessageConfig>(c =>
                c.Session == _session &&
                c.Prompt.Contains("MUST create the files")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Plan_Is_Split_Then_Prompt_Mentions_Max_Spec_Count()
    {
        var plan = SetupPlan();
        SetupCopilotResponse();
        SimulateCopilotCreatedSpecs(("/output/test/specs/spec-01-feature.md", "# Feature\n\nContent."));

        await _sut.SplitPlan(_session, plan, TestContext.Current.CancellationToken);

        await _copilotService.Received(1).SendMessage(
            Arg.Is<SendMessageConfig>(c =>
                c.Session == _session &&
                c.Prompt.Contains("NO MORE THAN 10")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Copilot_Creates_Exactly_10_Specs_Then_All_Are_Kept()
    {
        var plan = SetupPlan();
        SetupCopilotResponse();
        var specFiles = Enumerable.Range(1, 10)
            .Select(i => ($"/output/test/specs/spec-{i:D2}-feature-{i}.md", $"# Feature {i}\n\nDescription."))
            .ToArray();
        SimulateCopilotCreatedSpecs(specFiles);

        var result = await _sut.SplitPlan(_session, plan, TestContext.Current.CancellationToken);

        result.Should().HaveCount(10);
    }

    [Fact]
    public void MaxSpecCount_Is_10()
    {
        SpecSplitter.MaxSpecCount.Should().Be(10);
    }

    private ProjectPlan SetupPlan()
    {
        _fileSystem.AddDirectory("/output/test");
        _fileSystem.AddFile("/output/test/implementation-plan.md", new MockFileData("# My Plan\nSome content"));
        return new ProjectPlan("/output/test", "/output/test/implementation-plan.md", []);
    }

    private void SetupCopilotResponse()
    {
        _copilotService.SendMessage(
                Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>())
            .Returns("Spec files created successfully.");
    }

    private void SimulateCopilotCreatedSpecs(params (string Path, string Content)[] specs)
    {
        _fileSystem.AddDirectory("/output/test/specs");
        foreach (var (path, content) in specs)
        {
            _fileSystem.AddFile(path, new MockFileData(content));
        }
    }
}
