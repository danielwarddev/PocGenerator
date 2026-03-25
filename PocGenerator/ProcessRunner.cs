using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PocGenerator;

public record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Success => ExitCode == 0;
}

public interface IProcessRunner
{
    Task Run(string fileName, string arguments, string workingDirectory, CancellationToken cancellationToken = default);
    Task<ProcessResult> RunWithOutput(string fileName, string arguments, string workingDirectory, CancellationToken cancellationToken = default);
    Task<string> RunProjectScript(string scriptName, string arguments, string workingDirectory, CancellationToken cancellationToken = default);
}

public class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;

    public ProcessRunner(ILogger<ProcessRunner> logger)
    {
        _logger = logger;
    }

    public async Task Run(string fileName, string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await RunWithOutput(fileName, arguments, workingDirectory, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"Process '{fileName} {arguments}' failed with exit code {result.ExitCode}: {result.StandardError}");
        }
    }

    public async Task<ProcessResult> RunWithOutput(string fileName, string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    public async Task<string> RunProjectScript(string scriptName, string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        var scriptPath = Path.Combine(workingDirectory, "ProjectScripts", scriptName);
        _logger.LogTrace("Running script: pwsh -File {ScriptPath} {Arguments} in {WorkingDirectory}",
            scriptPath, arguments, workingDirectory);

        var result = await RunWithOutput(
            "pwsh", $"-File \"{scriptPath}\" {arguments}", workingDirectory, cancellationToken);

        var output = result.StandardOutput;
        if (!string.IsNullOrWhiteSpace(result.StandardError))
            output += "\n\nSTDERR:\n" + result.StandardError;

        if (!result.Success)
        {
            _logger.LogWarning("Script {ScriptName} failed with exit code {ExitCode}", scriptName, result.ExitCode);
            output = $"SCRIPT FAILED (exit code {result.ExitCode}):\n{output}";
        }

        return output;
    }
}
