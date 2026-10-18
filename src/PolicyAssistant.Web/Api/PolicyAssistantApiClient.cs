using PolicyAssistant.Shared;

namespace PolicyAssistant.Web.Api;

/// <summary>Typed client for the PolicyAssistant API.</summary>
public sealed class PolicyAssistantApiClient(HttpClient httpClient)
{
    /// <summary>Sends a question to <c>POST /chat</c>. Throws on network errors, timeouts and non-success status codes.</summary>
    public async Task<ChatResponse> AskAsync(string question, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("chat", new ChatRequest(question), cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty response.");
    }
}
