using SpanishLearning.Core.Models;

namespace SpanishLearning.Core.Services;

public class StoryService
{
    public IReadOnlyList<Story> GetAll() => SeedData.Stories;

    public Story GetById(int id) =>
        SeedData.Stories.FirstOrDefault(s => s.Id == id)
        ?? throw new KeyNotFoundException($"Story with id {id} was not found.");
}
