using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace PocGenerator.Copilot;

public record SendMessageConfig(
    string Prompt,
    CopilotSession Session,
    IReadOnlyList<string>? AttachmentPaths = null,
    TimeSpan? Timeout = null
);

public record CreateSessionConfig(
    string SystemPrompt,
    string? OutputDirectory = null);

public interface ICopilotService
{
    Task Initialize(CancellationToken cancellationToken = default);
    Task<CopilotSession> CreateSession(CreateSessionConfig config, CancellationToken cancellationToken = default);
    Task<string> SendMessage(SendMessageConfig config, CancellationToken cancellationToken = default);
}

public class CopilotService : ICopilotService, IAsyncDisposable, IDisposable
{
    private bool _disposed;
    private readonly ICopilotClient _copilotClient;
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<CopilotService> _logger;

    public CopilotService(ICopilotClient copilotClient, IProcessRunner processRunner, ILogger<CopilotService> logger)
    {
        _copilotClient = copilotClient;
        _processRunner = processRunner;
        _logger = logger;
    }

    public async Task Initialize(CancellationToken cancellationToken)
    {
        await _copilotClient.StartAsync(cancellationToken);
    }

    public async Task<CopilotSession> CreateSession(CreateSessionConfig config, CancellationToken cancellationToken = default)
    {
        var tools = new List<AIFunction>();
        if (config.OutputDirectory is not null)
        {
            var projectTools = new ProjectTools(_processRunner, config.OutputDirectory);
            tools.AddRange(
            [
                AIFunctionFactory.Create(projectTools.CreateConsoleProject),
                AIFunctionFactory.Create(projectTools.CreateBlazorProject),
                AIFunctionFactory.Create(projectTools.CreateLibraryProject),
                AIFunctionFactory.Create(projectTools.CreateDatabaseProject),
                AIFunctionFactory.Create(projectTools.CreateTestProject),
            ]);
        }

        var mcpServers = new Dictionary<string, object>
        {
            ["playwright"] = new McpLocalServerConfig
            {
                Command = "npx",
                Args = ["@playwright/mcp@latest", "--headless", "--isolated"],
                Tools = ["*"]
            }
        };

        var session = await _copilotClient.CreateSessionAsync(new SessionConfig
        {
            // 0x:
            // gpt-5-mini
            // 1x:
            // claude-sonnet-4.6
            // gpt-5.3-codex
            // 3x:
            // claude-opus-4.6
            Model = "claude-sonnet-4.6",
            SessionId = $"poc-generator-{Guid.CreateVersion7()}",
            OnPermissionRequest = (request, invocation) =>
            {
                _logger.LogPermissionRequest(request, invocation);
                return Task.FromResult(new PermissionRequestResult { Kind = PermissionRequestResultKind.Approved });
            },
            ReasoningEffort = "xhigh",
            SystemMessage = new SystemMessageConfig
            {
                Mode = SystemMessageMode.Append,
                Content = config.SystemPrompt
            },
            Tools = tools,
            McpServers = mcpServers,
            Hooks = new SessionHooks
            {
                OnPreToolUse = async (input, invocation) =>
                {
                    _logger.LogCopilotHook(input, invocation);
                    return await Task.FromResult(new PreToolUseHookOutput
                    {
                        PermissionDecision = "allow"
                    });
                },
                OnPostToolUse = async (input, invocation) =>
                {
                    _logger.LogCopilotHook(input, invocation);
                    return null;
                },
                OnErrorOccurred = async (input, invocation) =>
                {
                    _logger.LogCopilotHook(input, invocation);
                    return null;
                }
            }
        },
        cancellationToken);

        session.On(sessionEvent =>
        {
            _logger.LogCopilotEvent(sessionEvent);
        });

        return session;
    }

    public async Task<string> SendMessage(SendMessageConfig config, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.Prompt))
        {
            throw new ArgumentException("Prompt cannot be null, empty, or whitespace.", nameof(config));
        }

        var attachmentPaths = config.AttachmentPaths ?? [];
        var timeout = config.Timeout ?? TimeSpan.FromMinutes(60);

        var messageOptions = new MessageOptions
        {
            Prompt = config.Prompt
        };

        if (attachmentPaths.Count > 0)
        {
            messageOptions.Attachments = attachmentPaths
                .Select(path => new UserMessageDataAttachmentsItemFile
                {
                    DisplayName = Path.GetFileName(path),
                    Path = path
                } as UserMessageDataAttachmentsItem)
                .ToList();
        }

        var response = await config.Session.SendAndWaitAsync(messageOptions, timeout, cancellationToken);

        return response?.Data?.Content ?? string.Empty;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _copilotClient.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await _copilotClient.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private sealed class ProjectTools(IProcessRunner processRunner, string outputDirectory)
    {
        [DisplayName("create_console_project")]
        [Description("Create a new .NET console application project and add it to the solution. The project will include Microsoft.Extensions.Hosting, Serilog, and CommandLineParser packages, plus templated Program.cs, ConsoleRunner, and CliArgs files.")]
        public Task<string> CreateConsoleProject(
            [Description("The name of the console project to create, e.g. 'MyApp'")] string projectName)
            => processRunner.RunProjectScript("create-console-project.ps1", $"-ProjectName {projectName}", outputDirectory);

        [DisplayName("create_blazor_project")]
        [Description("Create a new Blazor web application project and add it to the solution. The project will include Radzen.Blazor with theme and component setup.")]
        public Task<string> CreateBlazorProject(
            [Description("The name of the Blazor project to create, e.g. 'MyApp.Web'")] string projectName)
            => processRunner.RunProjectScript("create-blazor-project.ps1", $"-ProjectName {projectName}", outputDirectory);

        [DisplayName("create_library_project")]
        [Description("Create a new .NET class library project and add it to the solution. Optionally includes the GitHub.Copilot.SDK package and CopilotService/ICopilotClient scaffolding when the project needs to call AI.")]
        public Task<string> CreateLibraryProject(
            [Description("The name of the class library project to create, e.g. 'MyApp.Core'")] string projectName,
            [Description("When true, adds the GitHub.Copilot.SDK NuGet package and copies CopilotService and ICopilotClient template files into the project.")] bool withCopilot = false)
            => processRunner.RunProjectScript("create-library-project.ps1", $"-ProjectName {projectName}{(withCopilot ? " -WithCopilot" : "")}", outputDirectory);

        [DisplayName("create_database_project")]
        [Description("Create a new .NET class library project configured for Entity Framework Core (with Npgsql provider) and add it to the solution.")]
        public Task<string> CreateDatabaseProject(
            [Description("The name of the database project to create, e.g. 'MyApp.Database'")] string projectName)
            => processRunner.RunProjectScript("create-db-project.ps1", $"-ProjectName {projectName}", outputDirectory);

        [DisplayName("create_test_project")]
        [Description("Create a new xUnit test project for an existing source project and add it to the solution. The test project will include AwesomeAssertions, AutoFixture, and NSubstitute packages, plus a project reference to the source project. The test project name will be '{SourceProjectName}.UnitTests'.")]
        public Task<string> CreateTestProject(
            [Description("The name of the source project to create tests for, e.g. 'MyApp.Core'. The test project will be named '{SourceProjectName}.UnitTests'.")] string sourceProjectName)
            => processRunner.RunProjectScript("create-test-project.ps1", $"-SourceProjectName {sourceProjectName}", outputDirectory);
    }
}