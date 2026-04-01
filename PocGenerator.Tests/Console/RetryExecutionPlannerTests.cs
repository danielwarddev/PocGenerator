using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator.Planning;
using AppConsole = PocGenerator.Console;

namespace PocGenerator.Tests.Console;

public class RetryExecutionPlannerTests
{
    private const string RetryDirectory = "/runs/2026-03-31-MyApp";
    private const string RetryDirectoryName = "2026-03-31-MyApp";

    private readonly IGitService _gitService = Substitute.For<IGitService>();
    private readonly IProjectPlanReconstructor _projectPlanReconstructor = Substitute.For<IProjectPlanReconstructor>();
    private readonly IOutputDirectoryService _outputDirectoryService = Substitute.For<IOutputDirectoryService>();
    private readonly ILogger<AppConsole.RetryExecutionPlanner> _logger = Substitute.For<ILogger<AppConsole.RetryExecutionPlanner>>();
    private readonly AppConsole.RetryExecutionPlanner _sut;

    public RetryExecutionPlannerTests()
    {
        _sut = new AppConsole.RetryExecutionPlanner(_gitService, _projectPlanReconstructor, _outputDirectoryService, _logger);
        _outputDirectoryService.ResolveOutputDirectory(RetryDirectory).Returns(RetryDirectory);
        SetUpReconstructor(specCount: 3);
    }

    [Fact]
    public async Task When_Git_Log_Is_Empty_Then_Create_Should_Throw_With_Fresh_Run_Message()
    {
        _gitService.GetLog(RetryDirectory, Arg.Any<CancellationToken>()).Returns(string.Empty);

        var act = () => _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no checkpoint commits were found*Start a new run instead*");
        await _gitService.DidNotReceive().CleanAndRestore(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Last_Commit_Is_Planning_Then_Create_Should_Resume_From_First_Spec()
    {
        _gitService.GetLog(RetryDirectory, Arg.Any<CancellationToken>()).Returns("planning");

        var result = await _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        result.StartPhase.Should().Be(AppConsole.RetryStartPhase.Generation);
        result.NextSpecIndex.Should().Be(0);
        result.ProjectPlan.Should().NotBeNull();
    }

    [Fact]
    public async Task When_Last_Commit_Is_Spec_Checkpoint_Then_Create_Should_Resume_From_Next_Spec()
    {
        _gitService.GetLog(RetryDirectory, Arg.Any<CancellationToken>()).Returns("spec 2");

        var result = await _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        result.StartPhase.Should().Be(AppConsole.RetryStartPhase.Generation);
        result.NextSpecIndex.Should().Be(2);
    }

    [Fact]
    public async Task When_Last_Commit_Is_Final_Spec_Then_Create_Should_Resume_From_Verification()
    {
        SetUpReconstructor(specCount: 2);
        _gitService.GetLog(RetryDirectory, Arg.Any<CancellationToken>()).Returns("spec 2");

        var result = await _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        result.StartPhase.Should().Be(AppConsole.RetryStartPhase.Verification);
    }

    [Fact]
    public async Task When_Last_Commit_Is_Verification_Then_Create_Should_Return_Completed_Without_Cleaning()
    {
        _gitService.GetLog(RetryDirectory, Arg.Any<CancellationToken>()).Returns("verification");

        var result = await _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        result.StartPhase.Should().Be(AppConsole.RetryStartPhase.Completed);
        await _gitService.DidNotReceive().CleanAndRestore(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Retry_Path_Is_A_Bare_Folder_Name_Then_Create_Should_Use_OutputDirectoryService_To_Resolve_It()
    {
        var expectedRetryDirectory = RetryDirectory;
        _outputDirectoryService.ResolveOutputDirectory(RetryDirectoryName).Returns(expectedRetryDirectory);
        _gitService.GetLog(expectedRetryDirectory, Arg.Any<CancellationToken>()).Returns("planning");

        var result = await _sut.Create(RetryDirectoryName, TestContext.Current.CancellationToken);

        result.OutputDirectory.Should().Be(expectedRetryDirectory);
        _outputDirectoryService.Received(1).ResolveOutputDirectory(RetryDirectoryName);
        await _gitService.Received(1).GetLog(expectedRetryDirectory, Arg.Any<CancellationToken>());
    }

    private void SetUpReconstructor(int specCount)
    {
        var specFiles = Enumerable.Range(1, specCount)
            .Select(index => $"{RetryDirectory}/Specs/spec-{index:D2}.md")
            .ToList();

        _projectPlanReconstructor.Reconstruct(RetryDirectory)
            .Returns(new ProjectPlan(RetryDirectory, $"{RetryDirectory}/implementation-plan.md", specFiles));
    }
}