namespace PolicyAssistant.Shared;

/// <summary>A question sent from the UI to the API.</summary>
/// <param name="Question">The user's question.</param>
/// <param name="History">Recent conversation turns, oldest first. Unused until follow-up support is added.</param>
public sealed record ChatRequest(string Question, IReadOnlyList<ChatTurn>? History = null);
