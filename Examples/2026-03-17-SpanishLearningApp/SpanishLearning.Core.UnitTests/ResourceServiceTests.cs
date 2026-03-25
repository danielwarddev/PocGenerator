using AwesomeAssertions;
using SpanishLearning.Core.Services;

namespace SpanishLearning.Core.UnitTests;

public class ResourceServiceTests
{
    private readonly ResourceService _service = new();

    [Fact]
    public void When_GetAll_Is_Called_Then_Count_Should_Be_At_Least_Five()
    {
        var result = _service.GetAll();

        result.Count.Should().BeGreaterThanOrEqualTo(5);
    }
}
