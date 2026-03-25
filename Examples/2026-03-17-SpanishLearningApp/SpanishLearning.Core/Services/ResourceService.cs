using SpanishLearning.Core.Models;

namespace SpanishLearning.Core.Services;

public class ResourceService
{
    public IReadOnlyList<Resource> GetAll() => SeedData.Resources;
}
