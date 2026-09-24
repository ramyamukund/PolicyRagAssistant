namespace PolicyAssistant.Shared;

/// <summary>A single prior message in the conversation.</summary>
/// <param name="Role">Either "user" or "assistant".</param>
/// <param name="Content">The message text.</param>
public sealed record ChatTurn(string Role, string Content)
{
    public const string UserRole = "user";
    public const string AssistantRole = "assistant";
}
