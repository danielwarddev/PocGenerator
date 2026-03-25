using AwesomeAssertions;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PocGenerator;
using PocGenerator.Copilot;

namespace PocGenerator.Tests.Copilot;

public class CopilotServiceTests
{
    private readonly ICopilotClient _client = Substitute.For<ICopilotClient>();
    private readonly IProcessRunner _processRunner = Substitute.For<IProcessRunner>();
    private readonly ILogger<CopilotService> _logger = Substitute.For<ILogger<CopilotService>>();
    private readonly CopilotSession _session = null!;
    private readonly CopilotService _sut;

    public CopilotServiceTests()
    {
        _sut = new CopilotService(_client, _processRunner, _logger);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public async Task When_Prompt_Is_Blank_Then_SendMessage_Should_Throw(string prompt)
    {
        var act = async () => await _sut.SendMessage(new SendMessageConfig(prompt, _session), TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}