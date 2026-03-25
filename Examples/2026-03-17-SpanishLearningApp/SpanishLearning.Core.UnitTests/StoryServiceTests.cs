using AwesomeAssertions;
using SpanishLearning.Core;
using SpanishLearning.Core.Services;

namespace SpanishLearning.Core.UnitTests;

public class StoryServiceTests
{
    private readonly StoryService _service = new();

    [Fact]
    public void When_GetAll_Is_Called_Then_Count_Should_Match_SeedData()
    {
        var result = _service.GetAll();

        result.Should().HaveCount(SeedData.Stories.Count);
    }

    [Fact]
    public void When_GetById_Is_Called_With_Valid_Id_Then_Correct_Story_Is_Returned()
    {
        var result = _service.GetById(1);

        result.Id.Should().Be(1);
        result.Title.Should().Be(SeedData.Stories[0].Title);
    }

    [Fact]
    public void When_GetById_Is_Called_With_Unknown_Id_Then_KeyNotFoundException_Is_Thrown()
    {
        var act = () => _service.GetById(9999);

        act.Should().Throw<KeyNotFoundException>();
    }
}
