using AwesomeAssertions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator.Copilot;
using PocGenerator.Planning;

namespace PocGenerator.Tests;

public class SlugGeneratorTests
{
    private readonly ICopilotService _copilotService = Substitute.For<ICopilotService>();
    private readonly ILogger<SlugGenerator> _logger = Substitute.For<ILogger<SlugGenerator>>();
    private readonly CopilotSession _session = null!;
    private const string IdeaFilePath = "/path/to/mvp.md";
    private readonly SlugGenerator _sut;

    public SlugGeneratorTests()
    {
        _sut = new SlugGenerator(_copilotService, _logger);
    }

    [Fact]
    public async Task When_Response_Is_Already_Valid_Then_Generate_Returns_It_Unchanged()
    {
        _copilotService.SendMessage(Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>())
            .Returns("CoolIdea");

        var result = await _sut.Generate(_session, IdeaFilePath, TestContext.Current.CancellationToken);

        result.Should().Be("CoolIdea");
    }

    [Fact]
    public async Task When_Response_Has_Non_Alphanumeric_Chars_Then_They_Are_Removed()
    {
        _copilotService.SendMessage(Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>())
            .Returns("invalid-slug_with spaces!");

        var result = await _sut.Generate(_session, IdeaFilePath, TestContext.Current.CancellationToken);

        result.Should().Be("invalidslugwithspaces");
    }

    [Fact]
    public async Task When_Response_Exceeds_Max_Length_Then_It_Is_Truncated_To_50()
    {
        var longSlug = new string('A', 60);
        _copilotService.SendMessage(Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>())
            .Returns(longSlug);

        var result = await _sut.Generate(_session, IdeaFilePath, TestContext.Current.CancellationToken);

        result.Should().HaveLength(50);
    }

    [Fact]
    public async Task When_Response_Has_Non_Alphanumeric_And_Is_Long_Then_Both_Are_Sanitized()
    {
        var dirtyLongSlug = "My-Super-Long-Project: " + new string('A', 50);
        _copilotService.SendMessage(Arg.Any<SendMessageConfig>(), Arg.Any<CancellationToken>())
            .Returns(dirtyLongSlug);

        var result = await _sut.Generate(_session, IdeaFilePath, TestContext.Current.CancellationToken);

        result.Should().MatchRegex("^[a-zA-Z0-9]+$");
        result.Length.Should().BeLessThanOrEqualTo(50);
    }
}
