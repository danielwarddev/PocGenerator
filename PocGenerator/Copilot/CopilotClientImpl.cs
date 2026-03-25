using GitHub.Copilot.SDK;
using GitHub.Copilot.SDK.Rpc;

namespace PocGenerator.Copilot;

public class CopilotClientImpl : ICopilotClient
{
    private readonly CopilotClient _client;

    /// <summary>
    /// Initializes a new instance wrapping a freshly created <see cref="CopilotClient"/>.
    /// </summary>
    /// <param name="options">Optional client options forwarded to the underlying <see cref="CopilotClient"/>.</param>
    public CopilotClientImpl(CopilotClientOptions? options = null)
    {
        _client = new CopilotClient(options);
    }

    /// <inheritdoc />
    public ServerRpc Rpc => _client.Rpc;

    /// <inheritdoc />
    public ConnectionState State => _client.State;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
        => _client.StartAsync(cancellationToken);

    /// <inheritdoc />
    public Task StopAsync()
        => _client.StopAsync();

    /// <inheritdoc />
    public Task ForceStopAsync()
        => _client.ForceStopAsync();

    /// <inheritdoc />
    public Task<CopilotSession> CreateSessionAsync(SessionConfig config, CancellationToken cancellationToken = default)
        => _client.CreateSessionAsync(config, cancellationToken);

    /// <inheritdoc />
    public Task<CopilotSession> ResumeSessionAsync(string sessionId, ResumeSessionConfig config, CancellationToken cancellationToken = default)
        => _client.ResumeSessionAsync(sessionId, config, cancellationToken);

    /// <inheritdoc />
    public Task<PingResponse> PingAsync(string? message = null, CancellationToken cancellationToken = default)
        => _client.PingAsync(message, cancellationToken);

    /// <inheritdoc />
    public Task<GetStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
        => _client.GetStatusAsync(cancellationToken);

    /// <inheritdoc />
    public Task<GetAuthStatusResponse> GetAuthStatusAsync(CancellationToken cancellationToken = default)
        => _client.GetAuthStatusAsync(cancellationToken);

    /// <inheritdoc />
    public Task<List<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
        => _client.ListModelsAsync(cancellationToken);

    /// <inheritdoc />
    public Task<string?> GetLastSessionIdAsync(CancellationToken cancellationToken = default)
        => _client.GetLastSessionIdAsync(cancellationToken);

    /// <inheritdoc />
    public Task DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        => _client.DeleteSessionAsync(sessionId, cancellationToken);

    /// <inheritdoc />
    public Task<List<SessionMetadata>> ListSessionsAsync(SessionListFilter? filter = null, CancellationToken cancellationToken = default)
        => _client.ListSessionsAsync(filter, cancellationToken);

    /// <inheritdoc />
    public Task<string?> GetForegroundSessionIdAsync(CancellationToken cancellationToken = default)
        => _client.GetForegroundSessionIdAsync(cancellationToken);

    /// <inheritdoc />
    public Task SetForegroundSessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
        => _client.SetForegroundSessionIdAsync(sessionId, cancellationToken);

    /// <inheritdoc />
    public IDisposable On(Action<SessionLifecycleEvent> handler)
        => _client.On(handler);

    /// <inheritdoc />
    public IDisposable On(string eventType, Action<SessionLifecycleEvent> handler)
        => _client.On(eventType, handler);

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _client.DisposeAsync();
    }
}
