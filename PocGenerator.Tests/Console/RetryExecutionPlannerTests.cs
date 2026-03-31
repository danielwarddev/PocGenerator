using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator.Planning;
using AppConsole = PocGenerator.Console;

namespace PocGenerator.Tests.Console;

public class RetryExecutionPlannerTests
{
    private const string RetryDirectory = "/runs/2026-03-31-MyApp";

    private readonly IGitService _gitService = Substitute.For<IGitService>();
    private readonly IProjectPlanReconstructor _projectPlanReconstructor = Substitute.For<IProjectPlanReconstructor>();
    private readonly MockFileSystem _fileSystem = new();
    private readonly ILogger<AppConsole.RetryExecutionPlanner> _logger = Substitute.For<ILogger<AppConsole.RetryExecutionPlanner>>();
    private readonly AppConsole.RetryExecutionPlanner _sut;

    public RetryExecutionPlannerTests()
    {
        _sut = new AppConsole.RetryExecutionPlanner(_gitService, _projectPlanReconstructor, _fileSystem, _logger);
        SetUpReconstructor(specCount: 3);
    }

    [Fact]
    public async Task When_Git_Log_Is_Empty_Then_Create_Should_Throw_With_Fresh_Run_Message()
    {
        _gitService.GetLog(_fileSystem.Path.GetFullPath(RetryDirectory), Arg.Any<CancellationToken>()).Returns(string.Empty);

        var act = () => _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no checkpoint commits were found*Start a new run instead*");
        await _gitService.DidNotReceive().CleanAndRestore(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Last_Commit_Is_Planning_Then_Create_Should_Resume_From_First_Spec()
    {
        _gitService.GetLog(_fileSystem.Path.GetFullPath(RetryDirectory), Arg.Any<CancellationToken>()).Returns("planning");

        var result = await _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        result.StartPhase.Should().Be(AppConsole.RetryStartPhase.Generation);
        result.NextSpecIndex.Should().Be(0);
        result.ProjectPlan.Should().NotBeNull();
    }

    [Fact]
    public async Task When_Last_Commit_Is_Spec_Checkpoint_Then_Create_Should_Resume_From_Next_Spec()
    {
        _gitService.GetLog(_fileSystem.Path.GetFullPath(RetryDirectory), Arg.Any<CancellationToken>()).Returns("spec 2");

        var result = await _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        result.StartPhase.Should().Be(AppConsole.RetryStartPhase.Generation);
        result.NextSpecIndex.Should().Be(2);
    }

    [Fact]
    public async Task When_Last_Commit_Is_Final_Spec_Then_Create_Should_Resume_From_Verification()
    {
        SetUpReconstructor(specCount: 2);
        _gitService.GetLog(_fileSystem.Path.GetFullPath(RetryDirectory), Arg.Any<CancellationToken>()).Returns("spec 2");

        var result = await _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        result.StartPhase.Should().Be(AppConsole.RetryStartPhase.Verification);
    }

    [Fact]
    public async Task When_Last_Commit_Is_Verification_Then_Create_Should_Return_Completed_Without_Cleaning()
    {
        _gitService.GetLog(_fileSystem.Path.GetFullPath(RetryDirectory), Arg.Any<CancellationToken>()).Returns("verification");

        var result = await _sut.Create(RetryDirectory, TestContext.Current.CancellationToken);

        result.StartPhase.Should().Be(AppConsole.RetryStartPhase.Completed);
        await _gitService.DidNotReceive().CleanAndRestore(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void SetUpReconstructor(int specCount)
    {
        var fullRetryDirectory = _fileSystem.Path.GetFullPath(RetryDirectory);
        var specFiles = Enumerable.Range(1, specCount)
            .Select(index => $"{fullRetryDirectory}/Specs/spec-{index:D2}.md")
            .ToList();

        _projectPlanReconstructor.Reconstruct(fullRetryDirectory)
            .Returns(new ProjectPlan(fullRetryDirectory, $"{fullRetryDirectory}/implementation-plan.md", specFiles));
    }
}