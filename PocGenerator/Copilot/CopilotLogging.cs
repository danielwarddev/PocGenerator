using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

namespace PocGenerator.Copilot;

public static class CopilotLogging
{
    private const string CopilotLogTag = "COPILOT";
    private const string CopilotLogProperty = "CopilotEvent";

    public static void LogPermissionRequest(this ILogger logger, PermissionRequest request, PermissionInvocation invocation)
    {
        using (logger.BeginScope(new Dictionary<string, object> { { CopilotLogProperty, true } }))
        {
            logger.LogTrace("[{Tag}][Permission request] session={SessionId} kind={Kind} args={ToolArgs}",
                        CopilotLogTag, invocation.SessionId, request.Kind, Serialize(request.ExtensionData));
        }
    }

    public static void LogCopilotHook(this ILogger logger, PreToolUseHookInput input, HookInvocation invocation)
    {
        using (logger.BeginScope(new Dictionary<string, object> { { CopilotLogProperty, true } }))
        {
            logger.LogTrace("[{Tag}][Pre tool use hook] session={SessionId} tool={ToolName} args={ToolArgs}",
                        CopilotLogTag, invocation.SessionId, input.ToolName, input.ToolArgs);
        }
    }

    public static void LogCopilotHook(this ILogger logger, PostToolUseHookInput input, HookInvocation invocation)
    {
        using (logger.BeginScope(new Dictionary<string, object> { { CopilotLogProperty, true } }))
        {
            logger.LogTrace("[{Tag}][Post tool use hook] session={SessionId} tool={ToolName} args={ToolArgs}",
                        CopilotLogTag, invocation.SessionId, input.ToolName, input.ToolArgs);
        }
    }

    public static void LogCopilotHook(this ILogger logger, ErrorOccurredHookInput input, HookInvocation invocation)
    {
        using (logger.BeginScope(new Dictionary<string, object> { { CopilotLogProperty, true } }))
        {
            logger.LogError("[{Tag}][Post tool use hook] session={SessionId} error={Error} errorContext={ErrorContext}, recoverable={Recoverable}",
                        CopilotLogTag, invocation.SessionId, input.Error, input.ErrorContext, input.Recoverable);
        }
    }

    public static void LogCopilotEvent(this ILogger logger, SessionEvent sessionEvent)
    {
        using (logger.BeginScope(new Dictionary<string, object> { { CopilotLogProperty, true } }))
        {
            var data = sessionEvent.GetType().GetProperty("Data")?.GetValue(sessionEvent);

            logger.LogTrace("[{Tag}][{EventType}] id={EventId}, data={Data}",
                    CopilotLogTag, sessionEvent.Type, sessionEvent.Id, Serialize(data));
        }
    }

    private static string Serialize(object? data)
    {
        if (data is null)
            return "null";

        var node = JsonSerializer.SerializeToNode(data);
        if (node is JsonObject obj)
            obj.Remove("encryptedContent");
        return node?.ToJsonString() ?? "null";
    }
}
