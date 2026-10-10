namespace PolicyAssistant.Api.Retrieval;

/// <summary>Finds the policy chunks most relevant to a question.</summary>
public interface IRetriever
{
    /// <returns>Relevant chunks, best first. Empty when nothing is relevant enough.</returns>
    Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(string question, CancellationToken cancellationToken);
}

/// <summary>A chunk of a policy document returned by search.</summary>
public sealed record RetrievedChunk(string ChunkId, string Title, string Content, double? RerankerScore);
