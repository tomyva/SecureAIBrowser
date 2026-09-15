namespace SecureAIBrowser.AI;

/// <summary>Contract for a future, replaceable AI provider. No provider is connected in Phase 1.</summary>
public interface IAIProvider
{
    Task<string> AskAsync(string prompt, string context);
}
