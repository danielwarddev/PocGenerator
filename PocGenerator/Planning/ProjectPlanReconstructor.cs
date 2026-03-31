using System.IO.Abstractions;

namespace PocGenerator.Planning;

public interface IProjectPlanReconstructor
{
    ProjectPlan Reconstruct(string outputDirectory);
}

public class ProjectPlanReconstructor : IProjectPlanReconstructor
{
    private readonly IFileSystem _fileSystem;

    public ProjectPlanReconstructor(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ProjectPlan Reconstruct(string outputDirectory)
    {
        var planFilePath = _fileSystem.Path.Combine(outputDirectory, "implementation-plan.md");
        var specsDirectory = _fileSystem.Path.Combine(outputDirectory, "Specs");

        if (!_fileSystem.File.Exists(planFilePath))
        {
            throw new InvalidOperationException($"The implementation plan file does not exist: {planFilePath}");
        }

        var specFiles = _fileSystem.Directory
            .GetFiles(specsDirectory, "*.md")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (specFiles.Count == 0)
        {
            throw new InvalidOperationException($"No spec files were found in {specsDirectory}");
        }

        return new ProjectPlan(outputDirectory, planFilePath, specFiles);
    }
}