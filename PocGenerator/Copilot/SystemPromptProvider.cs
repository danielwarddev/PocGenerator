using System.Reflection;

namespace PocGenerator.Copilot;

public interface ISystemPromptProvider
{
    string GetPlanningPrompt();
    string GetDevelopingPrompt();
    string GetVerificationPrompt();
}

public class SystemPromptProvider : ISystemPromptProvider
{
    private readonly Lazy<string> _planningPrompt = new(() => ReadEmbeddedResource("SystemPrompt_SpecCreation.md"));
    private readonly Lazy<string> _developingPrompt = new(() => ReadEmbeddedResource("SystemPrompt_Developing.md"));
    private readonly Lazy<string> _verificationPrompt = new(() => ReadEmbeddedResource("SystemPrompt_Verification.md"));

    public string GetPlanningPrompt() => _planningPrompt.Value;

    public string GetDevelopingPrompt() => _developingPrompt.Value;

    public string GetVerificationPrompt() => _verificationPrompt.Value;

    private static string ReadEmbeddedResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded resource '{fileName}' not found.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
