using SpanishLearning.Core.Models;

namespace SpanishLearning.Core.Services;

public class FlashcardService
{
    public IReadOnlyList<Flashcard> GetAll() => SeedData.Flashcards;

    public IReadOnlyList<Flashcard> GetByCategory(string category) =>
        SeedData.Flashcards
            .Where(f => f.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public IReadOnlyList<string> GetCategories() =>
        SeedData.Flashcards
            .Select(f => f.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToList();
}
