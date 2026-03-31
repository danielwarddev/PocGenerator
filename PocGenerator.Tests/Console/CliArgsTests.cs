using AwesomeAssertions;
using AppConsole = PocGenerator.Console;

namespace PocGenerator.Tests.Console;

public class CliArgsTests
{
    [Fact]
    public void When_Retry_Value_Is_Missing_Then_Parse_Should_Throw()
    {
        var act = () => AppConsole.CliArgs.Parse(["--retry"]);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*--retry*requires a value*");
    }

    [Fact]
    public void When_Retry_Value_Is_Empty_Then_Parse_Should_Throw()
    {
        var act = () => AppConsole.CliArgs.Parse(["--retry", ""]);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*--retry*requires a non-empty value*");
    }

    [Fact]
    public void When_Retry_Value_Is_Provided_Then_Parse_Should_Return_It()
    {
        var result = AppConsole.CliArgs.Parse(["--retry", "/output/failed-run"]);

        result.RetryPath.Should().Be("/output/failed-run");
    }
}