using System.Text.RegularExpressions;
using PolicyAssistant.Api.Generation;
using PolicyAssistant.Api.Retrieval;
using PolicyAssistant.Shared;

namespace PolicyAssistant.Api.Chat;

public sealed partial class ChatService(
    IRetriever retriever,
    IAnswerGenerator generator,
    ILogger<ChatService> logger) : IChatService
{
    private const int SnippetLength = 200;

    private static readonly ChatResponse NotFound = new(Prompts.NotFoundAnswer, [], IsGrounded: false);

    public async Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var chunks = await retriever.RetrieveAsync(request.Question, cancellationToken);
        if (chunks.Count == 0)
        {
            logger.LogInformation("No relevant chunks retrieved; returning not-found answer without calling the model");
            return NotFound;
        }

        var answer = await generator.GenerateAsync(request.Question, chunks, cancellationToken);
        var citations = ExtractCitations(answer, chunks);

        logger.LogInformation("Answer cites {CitationCount} of {ChunkCount} sources", citations.Count, chunks.Count);

        return new ChatResponse(answer, citations, IsGrounded: citations.Count > 0);
    }

    /// <summary>Maps [n] markers (also [n, m]) in the answer to the sources they refer to, ignoring out-of-range numbers.</summary>
    internal static IReadOnlyList<Citation> ExtractCitations(string answer, IReadOnlyList<RetrievedChunk> chunks) =>
        CitationMarker().Matches(answer)
            .SelectMany(match => match.Groups[1].Value.Split(','))
            .Select(number => int.Parse(number.Trim()))
            .Where(number => number >= 1 && number <= chunks.Count)
            .Distinct()
            .Order()
            .Select(number => ToCitation(number, chunks[number - 1]))
            .ToList();

    private static Citation ToCitation(int number, RetrievedChunk chunk) =>
        new(number, chunk.Title, Snippet(chunk.Content), chunk.ChunkId);

    private static string Snippet(string content) =>
        content.Length <= SnippetLength ? content : content[..SnippetLength].TrimEnd() + "…";

    [GeneratedRegex(@"\[(\d{1,3}(?:\s*,\s*\d{1,3})*)\]")]
    private static partial Regex CitationMarker();
}
