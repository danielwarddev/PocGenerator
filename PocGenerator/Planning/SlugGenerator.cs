using System.Text.RegularExpressions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using PocGenerator.Copilot;

namespace PocGenerator.Planning;

public interface ISlugGenerator
{
    Task<string> Generate(CopilotSession session, string ideaFilePath, CancellationToken cancellationToken = default);
}

public partial class SlugGenerator : ISlugGenerator
{
    private const int MaxSlugLength = 50;

    private const string SlugPrompt =
        """
        Based on the attached mvp.md file describing an MVP idea, generate a short slug suitable for a directory name. It should be short, memorable, and descriptive of the idea. Follow these naming rules:

        Rules:
        - Alphabetical characters only
        - Name must be in PascalCase (e.g. "MyGreatApp")
        - Maximum 5 words / 50 characters
        - NO SPECIAL CHARACTERS, such as hyphens, spaces, backticks, tildes, underscores, etc.
        - Return ONLY the slug, nothing else
        """;

    private readonly ICopilotService _copilotService;
    private readonly ILogger<SlugGenerator> _logger;

    public SlugGenerator(ICopilotService copilotService, ILogger<SlugGenerator> logger)
    {
        _copilotService = copilotService;
        _logger = logger;
    }

    public async Task<string> Generate(CopilotSession session, string ideaFilePath, CancellationToken cancellationToken)
    {
        var response = await _copilotService.SendMessage(new SendMessageConfig(SlugPrompt, session, [ideaFilePath]), cancellationToken);
        var slug = Sanitize(response);
        _logger.LogInformation("Generated slug: {Slug}", slug);
        return slug;
    }

    private static string Sanitize(string slug)
    {
        slug = NonAlphanumericRegex().Replace(slug, "");

        if (slug.Length > MaxSlugLength)
            slug = slug[..MaxSlugLength];

        return slug;
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphanumericRegex();
}
