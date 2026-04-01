using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using PocGenerator.Console;

namespace PocGenerator;

public interface IOutputDirectoryService
{
    Task<string> CreateOutputFolder(string slug, CancellationToken cancellationToken = default);
    string ResolveOutputDirectory(string outputDirectoryOrName);
    void CopyProjectScripts(string outputDirectory);
    void CopyDirectoryBuildProps(string outputDirectory);
    void CopyGitignore(string outputDirectory);
    void CopyMvpDefinition(string outputDirectory, string sourceMvpDefinitionDirectory);
}

public class OutputDirectoryService : IOutputDirectoryService
{
    public record ProjectScriptsConfig(string ProjectScriptsSourceDirectory);

    private readonly IFileSystem _fileSystem;
    private readonly CliArgs _cliArgs;
    private readonly ILogger<OutputDirectoryService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IProcessRunner _processRunner;
    private readonly ProjectScriptsConfig _config;

    public OutputDirectoryService(
        IFileSystem fileSystem,
        CliArgs cliArgs,
        ILogger<OutputDirectoryService> logger,
        TimeProvider timeProvider,
        IProcessRunner processRunner,
        ProjectScriptsConfig config)
    {
        _fileSystem = fileSystem;
        _cliArgs = cliArgs;
        _logger = logger;
        _timeProvider = timeProvider;
        _processRunner = processRunner;
        _config = config;
    }

    public async Task<string> CreateOutputFolder(string slug, CancellationToken cancellationToken)
    {
        var date = _timeProvider.GetLocalNow().ToString("yyyy-MM-dd");
        var directoryName = $"{date}-{slug}";
        var outputDirectory = _fileSystem.Path.Combine(_cliArgs.OutputPath, directoryName);
        var fullPath = _fileSystem.Path.GetFullPath(outputDirectory);

        if (_fileSystem.Directory.Exists(fullPath))
        {
            _logger.LogWarning("Output directory already exists: {OutputDirectory}", fullPath);
            throw new InvalidOperationException($"Output directory already exists: {fullPath}");
        }

        var parentPath = _fileSystem.Path.GetFullPath(_cliArgs.OutputPath);
        _fileSystem.Directory.CreateDirectory(parentPath);

        await _processRunner.Run(
            "dotnet", $"new sln --name {slug} --output {directoryName}",
            parentPath, cancellationToken);

        _logger.LogTrace("Created solution at: {OutputDirectory}", fullPath);

        return fullPath;
    }

    public string ResolveOutputDirectory(string outputDirectoryOrName)
    {
        var isMoreThanOnlyFolderName = 
            _fileSystem.Path.IsPathRooted(outputDirectoryOrName) || outputDirectoryOrName.Contains(_fileSystem.Path.DirectorySeparatorChar) ||
            outputDirectoryOrName.Contains(_fileSystem.Path.AltDirectorySeparatorChar) ||
            outputDirectoryOrName.StartsWith(".", StringComparison.Ordinal);

        if (isMoreThanOnlyFolderName)
        {
            return _fileSystem.Path.GetFullPath(outputDirectoryOrName);
        }
        else
        {
            var outputDirectory = _fileSystem.Path.Combine(_cliArgs.OutputPath, outputDirectoryOrName);
            return _fileSystem.Path.GetFullPath(outputDirectory);
        }
    }

    public void CopyProjectScripts(string outputDirectory)
    {
        var targetScriptsDir = _fileSystem.Path.Combine(outputDirectory, "ProjectScripts");
        _fileSystem.Directory.CreateDirectory(targetScriptsDir);

        foreach (var scriptFile in _fileSystem.Directory.GetFiles(_config.ProjectScriptsSourceDirectory, "*.ps1"))
        {
            var fileName = _fileSystem.Path.GetFileName(scriptFile);
            var targetPath = _fileSystem.Path.Combine(targetScriptsDir, fileName);
            _fileSystem.File.Copy(scriptFile, targetPath, overwrite: true);
        }

        var consoleFilesSource = _fileSystem.Path.Combine(_config.ProjectScriptsSourceDirectory, "ConsoleFiles");
        if (!_fileSystem.Directory.Exists(consoleFilesSource)) return;

        var consoleFilesTarget = _fileSystem.Path.Combine(targetScriptsDir, "ConsoleFiles");
        _fileSystem.Directory.CreateDirectory(consoleFilesTarget);

        foreach (var templateFile in _fileSystem.Directory.GetFiles(consoleFilesSource))
        {
            var fileName = _fileSystem.Path.GetFileName(templateFile);
            var targetPath = _fileSystem.Path.Combine(consoleFilesTarget, fileName);
            _fileSystem.File.Copy(templateFile, targetPath, overwrite: true);
        }

        _logger.LogTrace("Copied project scripts to {TargetDir}", targetScriptsDir);
    }

    public void CopyDirectoryBuildProps(string outputDirectory)
    {
        var templatePath = _fileSystem.Path.Combine(_config.ProjectScriptsSourceDirectory, "Directory.build.props.template");
        if (!_fileSystem.File.Exists(templatePath)) return;

        var targetPath = _fileSystem.Path.Combine(outputDirectory, "Directory.Build.props");
        _fileSystem.File.Copy(templatePath, targetPath, overwrite: true);
    }

    public void CopyGitignore(string outputDirectory)
    {
        var sourcePath = _fileSystem.Path.Combine(_config.ProjectScriptsSourceDirectory, ".gitignore");
        if (!_fileSystem.File.Exists(sourcePath)) return;

        var targetPath = _fileSystem.Path.Combine(outputDirectory, ".gitignore");
        _fileSystem.File.Copy(sourcePath, targetPath, overwrite: true);
    }

    public void CopyMvpDefinition(string outputDirectory, string sourceMvpDefinitionDirectory)
    {
        var targetMvpDir = _fileSystem.Path.Combine(outputDirectory, "mvp-definition");
        _fileSystem.Directory.CreateDirectory(targetMvpDir);

        foreach (var file in _fileSystem.Directory.GetFiles(sourceMvpDefinitionDirectory))
        {
            var fileName = _fileSystem.Path.GetFileName(file);
            var targetPath = _fileSystem.Path.Combine(targetMvpDir, fileName);
            _fileSystem.File.Copy(file, targetPath, overwrite: true);
        }

        _logger.LogTrace("Copied mvp-definition to {TargetDir}", targetMvpDir);
    }
}