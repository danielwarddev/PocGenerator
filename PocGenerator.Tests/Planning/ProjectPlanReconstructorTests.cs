using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using PocGenerator.Planning;

namespace PocGenerator.Tests.Planning;

public class ProjectPlanReconstructorTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly ProjectPlanReconstructor _sut;

    public ProjectPlanReconstructorTests()
    {
        _sut = new ProjectPlanReconstructor(_fileSystem);
    }

    [Fact]
    public void When_No_Spec_Files_Are_Found_Then_Reconstruct_Should_Throw()
    {
        _fileSystem.AddFile("/output/2026-03-31-MyApp/implementation-plan.md", "# Plan");
        _fileSystem.AddDirectory("/output/2026-03-31-MyApp/Specs");

        var act = () => _sut.Reconstruct("/output/2026-03-31-MyApp");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No spec files were found*");
    }

}