using AwesomeAssertions;
using SpanishLearning.Core;
using SpanishLearning.Core.Services;

namespace SpanishLearning.Core.UnitTests;

public class FlashcardServiceTests
{
    private readonly FlashcardService _service = new();

    [Fact]
    public void When_GetAll_Is_Called_Then_Count_Should_Match_SeedData()
    {
        var result = _service.GetAll();

        result.Should().HaveCount(SeedData.Flashcards.Count);
    }

    [Fact]
    public void When_GetByCategory_Is_Called_With_Greetings_Then_Only_Greetings_Cards_Are_Returned()
    {
        var result = _service.GetByCategory("greetings");

        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(f => f.Category.Should().BeEquivalentTo("greetings"));
    }

    [Fact]
    public void When_GetByCategory_Is_Called_Case_Insensitively_Then_Results_Match()
    {
        var lower = _service.GetByCategory("food");
        var upper = _service.GetByCategory("FOOD");

        upper.Should().HaveCount(lower.Count);
    }

    [Fact]
    public void When_GetByCategory_Is_Called_With_Unknown_Category_Then_Empty_List_Is_Returned()
    {
        var result = _service.GetByCategory("nonexistent");

        result.Should().BeEmpty();
    }

    [Fact]
    public void When_GetCategories_Is_Called_Then_Distinct_Categories_Are_Returned()
    {
        var categories = _service.GetCategories();

        categories.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void When_GetCategories_Is_Called_Then_Known_Categories_Are_Present()
    {
        var categories = _service.GetCategories();

        categories.Should().Contain("greetings");
        categories.Should().Contain("numbers");
        categories.Should().Contain("food");
    }
}
