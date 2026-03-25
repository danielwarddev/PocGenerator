using SpanishLearning.Core.Models;

namespace SpanishLearning.Core.Services;

public class LessonService
{
    public IReadOnlyList<Lesson> GetAll() => SeedData.Lessons;

    public Lesson GetById(int id) =>
        SeedData.Lessons.FirstOrDefault(l => l.Id == id)
        ?? throw new KeyNotFoundException($"Lesson with id {id} was not found.");
}
