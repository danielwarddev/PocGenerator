using SpanishLearning.Core.Models;

namespace SpanishLearning.Core.Services;

public class QuizService
{
    public IReadOnlyList<QuizQuestion> GetQuestions() => SeedData.QuizQuestions;

    public IReadOnlyList<QuizQuestion> GetShuffled()
    {
        var list = SeedData.QuizQuestions.ToList();
        System.Random.Shared.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list));
        return list;
    }
}
