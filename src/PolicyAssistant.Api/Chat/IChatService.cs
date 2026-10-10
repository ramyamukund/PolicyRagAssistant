using PolicyAssistant.Shared;

namespace PolicyAssistant.Api.Chat;

/// <summary>Answers a policy question end to end: retrieve, generate, cite.</summary>
public interface IChatService
{
    Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken);
}
