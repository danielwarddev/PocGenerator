using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PocGenerator;
using Serilog;
using Serilog.Events;
using System.IO.Abstractions;
using PocGenerator.Console;
using PocGenerator.Copilot;
using PocGenerator.Generation;
using PocGenerator.Planning;
using PocGenerator.Verification;
using SlugGenerator = PocGenerator.Planning.SlugGenerator;
using SpecSplitter = PocGenerator.Planning.SpecSplitter;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var cliArgs = Parser.Default
            .ParseArguments<CliArgs>(args)
            .MapResult(
                parsed => parsed,
                _ => new CliArgs());

        services.AddHostedService<ConsoleRunner>();
        services.AddSingleton<ICopilotClient, CopilotClientImpl>();
        services.AddSingleton<ICopilotService, CopilotService>();
        services.AddSingleton<ISystemPromptProvider, SystemPromptProvider>();
        services.AddSingleton<ISlugGenerator, SlugGenerator>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IIdeaFileLocator, IdeaFileLocator>();
        services.AddSingleton<IOutputDirectoryService, OutputDirectoryService>();
        services.AddSingleton<IProjectPlanner, ProjectPlanner>();
        services.AddSingleton<ISpecSplitter, SpecSplitter>();
        services.AddSingleton<IPlanningPhaseHandler, PlanningPhaseHandler>();
        services.AddSingleton<ICodeGenerator, CodeGenerator>();
        services.AddSingleton(new CodeGenerator.HardCapConfig());
        services.AddSingleton<IGenerationPhaseHandler, GenerationPhaseHandler>();
        services.AddSingleton<IVerificationPhaseHandler, VerificationPhaseHandler>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton(new OutputDirectoryService.ProjectScriptsConfig(
            Path.Combine(AppContext.BaseDirectory, "ProjectScripts")));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(cliArgs);
    })
    .UseConsoleLifetime()
    .UseSerilog((context, configuration) =>
    {
        var runTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        var infoLogPath = Path.Combine(logsDir, $"pocgenerator-info-{runTimestamp}.log");
        var traceLogPath = Path.Combine(logsDir, $"pocgenerator-trace-{runTimestamp}.log");
        var copilotLogPath = Path.Combine(logsDir, $"pocgenerator-copilot-{runTimestamp}.log");

        configuration
            .MinimumLevel.Verbose()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(
                infoLogPath,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File(traceLogPath);

        var copilotLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("CopilotEvent"))
            .WriteTo.File(copilotLogPath)
            .CreateLogger();

        configuration.WriteTo.Logger(copilotLogger);
    })
    .Build()
    .RunAsync();