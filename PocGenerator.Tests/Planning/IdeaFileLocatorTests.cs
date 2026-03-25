using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using PocGenerator.Planning;

namespace PocGenerator.Tests.Planning;

public class IdeaFileLocatorTests
{
    private static string FolderPath => Path.Combine(AppContext.BaseDirectory, "mvp-definition");
    private static string MvpPath => Path.Combine(FolderPath, "mvp.md");

    [Fact]
    public void When_MvpDefinitionFolder_And_MvpMd_Exist_Then_GetIdeaFiles_Returns_Idea_Path()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(MvpPath, new MockFileData("# My SaaS Idea"));
        var sut = new IdeaFileLocator(fileSystem);

        var result = sut.GetIdeaFiles();

        result.IdeaFilePath.Should().Be(MvpPath);
    }

    [Fact]
    public void When_Context_Files_Exist_Then_GetIdeaFiles_Returns_Them_Excluding_MvpMd()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(MvpPath, new MockFileData("# My SaaS Idea"));
        var contextPath = Path.Combine(FolderPath, "schema.sql");
        fileSystem.AddFile(contextPath, new MockFileData("CREATE TABLE ..."));
        var sut = new IdeaFileLocator(fileSystem);

        var result = sut.GetIdeaFiles();

        result.ContextFilePaths.Should().ContainSingle()
            .Which.Should().Be(contextPath);
    }

    [Fact]
    public void When_MvpDefinitionFolder_Not_Found_Then_GetIdeaFiles_Throws_DirectoryNotFoundException()
    {
        var fileSystem = new MockFileSystem();
        var sut = new IdeaFileLocator(fileSystem);

        var act = () => sut.GetIdeaFiles();

        act.Should().Throw<DirectoryNotFoundException>()
            .WithMessage("*mvp-definition*");
    }

    [Fact]
    public void When_MvpMd_Not_Found_In_Folder_Then_GetIdeaFiles_Throws_FileNotFoundException()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(FolderPath);
        var sut = new IdeaFileLocator(fileSystem);

        var act = () => sut.GetIdeaFiles();

        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*mvp.md*");
    }
}
