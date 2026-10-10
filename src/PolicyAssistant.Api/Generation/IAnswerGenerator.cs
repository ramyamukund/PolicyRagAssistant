using PolicyAssistant.Api.Retrieval;

namespace PolicyAssistant.Api.Generation;

/// <summary>Writes an answer to a question from a set of numbered sources.</summary>
public interface IAnswerGenerator
{
    /// <returns>The answer text, citing sources inline as [n] where n is the 1-based source position.</returns>
    Task<string> GenerateAsync(string question, IReadOnlyList<RetrievedChunk> sources, CancellationToken cancellationToken);
}
