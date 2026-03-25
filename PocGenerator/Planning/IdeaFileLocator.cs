using System.IO.Abstractions;

namespace PocGenerator.Planning;

public record IdeaFiles(string IdeaFilePath, IReadOnlyList<string> ContextFilePaths)
{
    public List<string> AllFilePaths() => [IdeaFilePath, .. ContextFilePaths];
}

public interface IIdeaFileLocator
{
    IdeaFiles GetIdeaFiles();
}

public class IdeaFileLocator : IIdeaFileLocator
{
    private const string IdeaFolderName = "mvp-definition";
    private const string IdeaFileName = "mvp.md";

    private readonly IFileSystem _fileSystem;

    public IdeaFileLocator(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public IdeaFiles GetIdeaFiles()
    {
        var ideaFolderPath = _fileSystem.Path.Combine(AppContext.BaseDirectory, IdeaFolderName);

        if (!_fileSystem.Directory.Exists(ideaFolderPath))
        {
            throw new DirectoryNotFoundException(
                $"No {IdeaFolderName} folder found in the current directory. " +
                $"Please create a {IdeaFolderName} folder containing your {IdeaFileName} file.");
        }

        var ideaFilePath = _fileSystem.Path.Combine(ideaFolderPath, IdeaFileName);

        if (!_fileSystem.File.Exists(ideaFilePath))
        {
            throw new FileNotFoundException(
                $"No {IdeaFileName} file found in the {IdeaFolderName} folder. " +
                $"Please create a {IdeaFileName} file describing your SaaS idea.");
        }

        var contextFilePaths = _fileSystem.Directory.GetFiles(ideaFolderPath)
            .Where(f => !string.Equals(_fileSystem.Path.GetFileName(f), IdeaFileName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new IdeaFiles(ideaFilePath, contextFilePaths);
    }
}
