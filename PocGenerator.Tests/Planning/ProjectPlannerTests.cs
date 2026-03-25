using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator.Copilot;
using PocGenerator.Planning;

namespace PocGenerator.Tests;

public class ProjectPlannerTests
{
    private readonly ICopilotService _copilotService = Substitute.For<ICopilotService>();
    private readonly IOutputDirectoryService _outputDirectoryService = Substitute.For<IOutputDirectoryService>();
    private readonly MockFileSystem _fileSystem = new();
    private readonly ILogger<ProjectPlanner> _logger = Substitute.For<ILogger<ProjectPlanner>>();
    private readonly CopilotSession _session = null!;
    private readonly ProjectPlanner _sut;

    public ProjectPlannerTests()
    {
        _sut = new ProjectPlanner(_copilotService, _outputDirectoryService, _fileSystem, _logger);
    }

    [Fact]
    public async Task When_GeneratePlan_Succeeds_Then_Plan_File_Contains_Copilot_Response()
    {
        var planContent = "## Project Goal\nA task tracker MVP.\n\n## Solution Structure\n| Project Name | Script | Purpose |\n|---|---|---|\n| TaskTracker | create-console-project.ps1 | Main entry point |";
        _fileSystem.AddDirectory("/output/2026-02-23-TestSlug");
        _outputDirectoryService.CreateOutputFolder(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("/output/2026-02-23-TestSlug");
        _copilotService.SendMessage(Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>())
            .Returns(planContent);

        await _sut.GeneratePlan(_session, "TestSlug", new IdeaFiles("/idea/mvp.md", []), TestContext.Current.CancellationToken);

        var planPath = "/output/2026-02-23-TestSlug/implementation-plan.md";
        _fileSystem.File.Exists(planPath).Should().BeTrue();
        var content = _fileSystem.File.ReadAllText(planPath);
        content.Should().Contain("task tracker MVP");
        content.Should().Contain("TaskTracker");
        content.Should().Contain("create-console-project.ps1");
    }

    [Fact]
    public async Task When_GeneratePlan_Succeeds_Then_Returns_Plan_With_Session_And_OutputDir()
    {
        _fileSystem.AddDirectory("/output/2026-02-23-TestSlug");
        _outputDirectoryService.CreateOutputFolder(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("/output/2026-02-23-TestSlug");
        _copilotService.SendMessage(Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>())
            .Returns("# Plan");

        var result = await _sut.GeneratePlan(_session, "TestSlug", new IdeaFiles("/idea/mvp.md", []), TestContext.Current.CancellationToken);

        result.Session.Should().Be(_session);
        result.OutputDirectory.Should().Be("/output/2026-02-23-TestSlug");
        result.PlanFilePath.Should().Contain("2026-02-23-TestSlug").And.Contain("implementation-plan.md");
    }

    [Fact]
    public async Task When_Output_Service_Throws_Then_GeneratePlan_Throws()
    {
        _outputDirectoryService.CreateOutputFolder(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<string>(_ => throw new InvalidOperationException("Output directory already exists"));

        var act = async () => await _sut.GeneratePlan(_session, "MyApp", new IdeaFiles("/idea/mvp.md", []), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
