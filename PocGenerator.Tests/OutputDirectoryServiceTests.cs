using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using PocGenerator.Console;

namespace PocGenerator.Tests;

public class OutputDirectoryServiceTests
{
    private const string ScriptsSourceDirectory = "/app/ProjectScripts";

    private readonly MockFileSystem _fileSystem = new();
    private readonly CliArgs _cliArgs = new() { OutputPath = "/output" };
    private readonly ILogger<OutputDirectoryService> _logger = Substitute.For<ILogger<OutputDirectoryService>>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 2, 23, 10, 0, 0, TimeSpan.Zero));
    private readonly IProcessRunner _processRunner = Substitute.For<IProcessRunner>();
    private readonly OutputDirectoryService _sut;

    public OutputDirectoryServiceTests()
    {
        var config = new OutputDirectoryService.ProjectScriptsConfig(ScriptsSourceDirectory);
        SetUpDefaultScriptFiles();
        _sut = new OutputDirectoryService(_fileSystem, _cliArgs, _logger, _timeProvider, _processRunner, config);
    }

    [Fact]
    public async Task When_Output_Directory_Does_Not_Exist_Then_CreateOutputFolder_Creates_And_Returns_Path()
    {
        var result = await _sut.CreateOutputFolder("MyApp", TestContext.Current.CancellationToken);
        var expectedPath = _fileSystem.Path.GetFullPath("/output/2026-02-23-MyApp");

        result.Should().Be(expectedPath);
        await _processRunner.Received(1).Run(
            "dotnet",
            "new sln --name MyApp --output 2026-02-23-MyApp",
            _fileSystem.Path.GetFullPath("/output"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task When_Output_Directory_Already_Exists_Then_CreateOutputFolder_Throws()
    {
        _fileSystem.AddDirectory("/output/2026-02-23-MyApp");

        var act = () => _sut.CreateOutputFolder("MyApp", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public void When_CopyProjectScripts_Called_Then_Ps1_Scripts_Are_Copied()
    {
        var outputDir = _fileSystem.Path.GetFullPath("/output/test-dir");
        _fileSystem.AddDirectory(outputDir);

        _sut.CopyProjectScripts(outputDir);

        var targetDir = _fileSystem.Path.Combine(outputDir, "ProjectScripts");
        _fileSystem.File.Exists(_fileSystem.Path.Combine(targetDir, "create-console-project.ps1")).Should().BeTrue();
        _fileSystem.File.Exists(_fileSystem.Path.Combine(targetDir, "create-library-project.ps1")).Should().BeTrue();
    }

    [Fact]
    public void When_CopyProjectScripts_Called_Then_ConsoleFiles_Are_Copied()
    {
        var outputDir = _fileSystem.Path.GetFullPath("/output/test-dir");
        _fileSystem.AddDirectory(outputDir);

        _sut.CopyProjectScripts(outputDir);

        var targetDir = _fileSystem.Path.Combine(outputDir, "ProjectScripts", "ConsoleFiles");
        _fileSystem.File.Exists(_fileSystem.Path.Combine(targetDir, "Program.cs.template")).Should().BeTrue();
    }

    [Fact]
    public void When_CopyDirectoryBuildProps_Called_Then_DirectoryBuildProps_Is_Copied()
    {
        var outputDir = _fileSystem.Path.GetFullPath("/output/test-dir");
        _fileSystem.AddDirectory(outputDir);

        _sut.CopyDirectoryBuildProps(outputDir);

        var targetPath = _fileSystem.Path.Combine(outputDir, "Directory.Build.props");
        _fileSystem.File.Exists(targetPath).Should().BeTrue();
    }

    [Fact]
    public void When_CopyMvpDefinition_Called_Then_Files_Are_Copied_To_Output()
    {
        var outputDir = _fileSystem.Path.GetFullPath("/output/test-dir");
        _fileSystem.AddDirectory(outputDir);
        _fileSystem.AddFile("/source/mvp-definition/mvp.md", "# My Idea");
        _fileSystem.AddFile("/source/mvp-definition/context.md", "# Context");

        _sut.CopyMvpDefinition(outputDir, "/source/mvp-definition");

        var targetDir = _fileSystem.Path.Combine(outputDir, "mvp-definition");
        _fileSystem.File.Exists(_fileSystem.Path.Combine(targetDir, "mvp.md")).Should().BeTrue();
        _fileSystem.File.Exists(_fileSystem.Path.Combine(targetDir, "context.md")).Should().BeTrue();
    }

    [Fact]
    public void When_Source_MvpDefinition_Does_Not_Exist_Then_CopyMvpDefinition_Throws()
    {
        var outputDir = _fileSystem.Path.GetFullPath("/output/test-dir");
        _fileSystem.AddDirectory(outputDir);

        var act = () => _sut.CopyMvpDefinition(outputDir, "/nonexistent/mvp-definition");

        act.Should().Throw<DirectoryNotFoundException>();
    }

    [Fact]
    public void When_ConsoleFiles_Directory_Does_Not_Exist_Then_CopyProjectScripts_Still_Succeeds()
    {
        var fileSystem = new MockFileSystem();
        var outputDir = fileSystem.Path.GetFullPath("/output/test-dir");
        fileSystem.AddDirectory(outputDir);
        fileSystem.AddFile(
            fileSystem.Path.Combine(ScriptsSourceDirectory, "create-console-project.ps1"), "# script");
        fileSystem.AddFile(
            fileSystem.Path.Combine(ScriptsSourceDirectory, "Directory.build.props.template"),
            "<Project></Project>");

        var config = new OutputDirectoryService.ProjectScriptsConfig(ScriptsSourceDirectory);
        var sut = new OutputDirectoryService(fileSystem, _cliArgs, _logger, _timeProvider, _processRunner, config);

        sut.CopyProjectScripts(outputDir);

        var consoleFilesTarget = fileSystem.Path.Combine(outputDir, "ProjectScripts", "ConsoleFiles");
        fileSystem.Directory.Exists(consoleFilesTarget).Should().BeFalse();
    }

    private void SetUpDefaultScriptFiles()
    {
        _fileSystem.AddFile(
            _fileSystem.Path.Combine(ScriptsSourceDirectory, "create-console-project.ps1"),
            "# console script");
        _fileSystem.AddFile(
            _fileSystem.Path.Combine(ScriptsSourceDirectory, "create-library-project.ps1"),
            "# library script");
        _fileSystem.AddFile(
            _fileSystem.Path.Combine(ScriptsSourceDirectory, "Directory.build.props.template"),
            "<Project><PropertyGroup><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup></Project>");
        _fileSystem.AddFile(
            _fileSystem.Path.Combine(ScriptsSourceDirectory, "ConsoleFiles", "Program.cs.template"),
            "// program template");
    }
}
