using AwesomeAssertions;
using SpanishLearning.Core;
using SpanishLearning.Core.Services;

namespace SpanishLearning.Core.UnitTests;

public class QuizServiceTests
{
    private readonly QuizService _service = new();

    [Fact]
    public void When_GetQuestions_Is_Called_Then_Count_Should_Match_SeedData()
    {
        var result = _service.GetQuestions();

        result.Should().HaveCount(SeedData.QuizQuestions.Count);
    }

    [Fact]
    public void When_GetShuffled_Is_Called_Then_Same_Items_Are_Returned()
    {
        var shuffled = _service.GetShuffled();

        shuffled.Should().HaveCount(SeedData.QuizQuestions.Count);
        shuffled.Select(q => q.Id).Should().BeEquivalentTo(SeedData.QuizQuestions.Select(q => q.Id));
    }

    [Fact]
    public void When_GetShuffled_Is_Called_Multiple_Times_Then_Order_Is_Likely_Different()
    {
        var run1 = _service.GetShuffled().Select(q => q.Id).ToList();
        var run2 = _service.GetShuffled().Select(q => q.Id).ToList();

        // With 10 items there are 10! = 3,628,800 permutations so identical order is extremely unlikely
        // We check that at least once in several tries the order differs (probabilistic, not deterministic)
        var attemptsWithDifferentOrder = Enumerable.Range(0, 10)
            .Select(_ => _service.GetShuffled().Select(q => q.Id).ToList())
            .Any(order => !order.SequenceEqual(run1));

        attemptsWithDifferentOrder.Should().BeTrue();
    }
}
