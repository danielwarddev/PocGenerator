namespace PocGenerator;

public interface IGitService
{
    Task Init(string workingDirectory, CancellationToken cancellationToken = default);
    Task AddAll(string workingDirectory, CancellationToken cancellationToken = default);
    Task Commit(string message, string workingDirectory, CancellationToken cancellationToken = default);
    Task<string> GetLog(string workingDirectory, CancellationToken cancellationToken = default);
    Task CleanAndRestore(string workingDirectory, CancellationToken cancellationToken = default);
}

public class GitService : IGitService
{
    private readonly IProcessRunner _processRunner;

    public GitService(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task Init(string workingDirectory, CancellationToken cancellationToken)
    {
        await _processRunner.Run("git", "init", workingDirectory, cancellationToken);
        await _processRunner.Run("git", "branch -M main", workingDirectory, cancellationToken);
    }

    public async Task AddAll(string workingDirectory, CancellationToken cancellationToken)
    {
        await _processRunner.Run("git", "add -A", workingDirectory, cancellationToken);
    }

    public async Task Commit(string message, string workingDirectory, CancellationToken cancellationToken)
    {
        await _processRunner.Run("git", $"commit -m \"{message}\"", workingDirectory, cancellationToken);
    }

    public async Task<string> GetLog(string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await _processRunner.RunWithOutput("git", "log --oneline", workingDirectory, cancellationToken);
        return result.StandardOutput;
    }

    public async Task CleanAndRestore(string workingDirectory, CancellationToken cancellationToken)
    {
        await _processRunner.Run("git", "clean -fd", workingDirectory, cancellationToken);
        await _processRunner.Run("git", "checkout .", workingDirectory, cancellationToken);
    }
}
