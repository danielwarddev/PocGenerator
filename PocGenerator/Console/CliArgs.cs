using CommandLine;

namespace PocGenerator.Console;

public class CliArgs
{
    public const string DefaultOutputPath = "./mvp-outputs";

    [Option("output", Default = DefaultOutputPath)]
    public string OutputPath { get; init; } = DefaultOutputPath;
}