using AwesomeAssertions;
using SpanishLearning.Core;
using SpanishLearning.Core.Models;

namespace SpanishLearning.Core.UnitTests;

public class SeedDataTests
{
    [Fact]
    public void When_Flashcards_Are_Accessed_Then_Collection_Should_Have_At_Least_20_Entries()
    {
        SeedData.Flashcards.Count.Should().BeGreaterThanOrEqualTo(20);
    }

    [Fact]
    public void When_Flashcards_Are_Accessed_Then_Greetings_Category_Should_Exist()
    {
        SeedData.Flashcards.Should().Contain(f => f.Category == "greetings");
    }

    [Fact]
    public void When_Flashcards_Are_Accessed_Then_Numbers_Category_Should_Exist()
    {
        SeedData.Flashcards.Should().Contain(f => f.Category == "numbers");
    }

    [Fact]
    public void When_Flashcards_Are_Accessed_Then_Food_Category_Should_Exist()
    {
        SeedData.Flashcards.Should().Contain(f => f.Category == "food");
    }

    [Fact]
    public void When_Flashcards_Are_Accessed_Then_At_Least_3_Categories_Should_Be_Present()
    {
        var distinctCategories = SeedData.Flashcards.Select(f => f.Category).Distinct().Count();
        distinctCategories.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void When_QuizQuestions_Are_Accessed_Then_Collection_Should_Have_At_Least_10_Entries()
    {
        SeedData.QuizQuestions.Count.Should().BeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void When_QuizQuestions_Are_Accessed_Then_All_Questions_Should_Have_Valid_CorrectIndex()
    {
        foreach (var question in SeedData.QuizQuestions)
        {
            question.CorrectIndex.Should().BeGreaterThanOrEqualTo(0);
            question.CorrectIndex.Should().BeLessThan(question.Options.Length);
        }
    }

    [Fact]
    public void When_Stories_Are_Accessed_Then_Collection_Should_Have_Exactly_2_Entries()
    {
        SeedData.Stories.Count.Should().Be(2);
    }

    [Fact]
    public void When_Stories_Are_Accessed_Then_Each_Story_Should_Have_At_Least_3_Paragraphs()
    {
        foreach (var story in SeedData.Stories)
        {
            var normalizedText = story.SpanishText.Replace("\r\n", "\n").Trim();
            var paragraphCount = normalizedText.Split("\n\n", StringSplitOptions.RemoveEmptyEntries).Length;
            paragraphCount.Should().BeGreaterThanOrEqualTo(3, because: $"story '{story.Title}' must have at least 3 paragraphs");
        }
    }

    [Fact]
    public void When_Lessons_Are_Accessed_Then_Collection_Should_Have_Exactly_3_Entries()
    {
        SeedData.Lessons.Count.Should().Be(3);
    }

    [Fact]
    public void When_Lessons_Are_Accessed_Then_Basic_Greetings_Lesson_Should_Exist()
    {
        SeedData.Lessons.Should().Contain(l => l.Title == "Basic Greetings");
    }

    [Fact]
    public void When_Lessons_Are_Accessed_Then_Numbers_Lesson_Should_Exist()
    {
        SeedData.Lessons.Should().Contain(l => l.Title == "Numbers 1–20");
    }

    [Fact]
    public void When_Lessons_Are_Accessed_Then_Present_Tense_Lesson_Should_Exist()
    {
        SeedData.Lessons.Should().Contain(l => l.Title == "Present Tense Verbs");
    }

    [Fact]
    public void When_Resources_Are_Accessed_Then_Collection_Should_Have_At_Least_5_Entries()
    {
        SeedData.Resources.Count.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void When_Resources_Are_Accessed_Then_All_Resources_Should_Have_NonEmpty_Fields()
    {
        foreach (var resource in SeedData.Resources)
        {
            resource.Title.Should().NotBeNullOrWhiteSpace();
            resource.Url.Should().NotBeNullOrWhiteSpace();
            resource.Description.Should().NotBeNullOrWhiteSpace();
        }
    }
}
