using CommandLine;

namespace PocGenerator.Console;

public class CliArgs
{
    public const string DefaultOutputPath = "./mvp-outputs";

    [Option("output", Default = DefaultOutputPath)]
    public string OutputPath { get; init; } = DefaultOutputPath;

    [Option("retry")]
    public string? RetryPath { get; init; }

    public static CliArgs Parse(string[] args)
    {
        ValidateExplicitOptionValues(args);

        var parsedArgs = Parser.Default
            .ParseArguments<CliArgs>(args)
            .MapResult(
                parsed => parsed,
                _ => throw new InvalidOperationException("Failed to parse command line arguments."));

        if (parsedArgs.RetryPath is not null && string.IsNullOrWhiteSpace(parsedArgs.RetryPath))
        {
            throw new InvalidOperationException("The --retry option requires a non-empty path.");
        }

        return parsedArgs;
    }

    private static void ValidateExplicitOptionValues(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--retry=", StringComparison.Ordinal)
                || args[i].StartsWith("--output=", StringComparison.Ordinal))
            {
                ValidateInlineOptionValue(args[i]);
                continue;
            }

            if (args[i] is "--retry" or "--output")
            {
                ValidateOptionValue(args, i);
                i++;
            }
        }
    }

    private static void ValidateInlineOptionValue(string argument)
    {
        var separatorIndex = argument.IndexOf('=');
        var optionName = argument[..separatorIndex];
        var optionValue = argument[(separatorIndex + 1)..];

        if (string.IsNullOrWhiteSpace(optionValue))
        {
            throw new InvalidOperationException($"The {optionName} option requires a non-empty value.");
        }
    }

    private static void ValidateOptionValue(string[] args, int index)
    {
        var optionName = args[index];

        if (index == args.Length - 1 || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"The {optionName} option requires a value.");
        }

        if (string.IsNullOrWhiteSpace(args[index + 1]))
        {
            throw new InvalidOperationException($"The {optionName} option requires a non-empty value.");
        }
    }
}