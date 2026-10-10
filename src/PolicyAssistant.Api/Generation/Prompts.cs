using System.Text;
using PolicyAssistant.Api.Retrieval;

namespace PolicyAssistant.Api.Generation;

/// <summary>All prompt text sent to the model lives here.</summary>
public static class Prompts
{
    public const string NotFoundAnswer = "I couldn't find that in the company policies.";

    public const string System =
        $"""
        You are a company policy assistant. Answer employee questions using only the numbered sources provided in the user message.

        Rules:
        - Use only information from the sources. Never invent or assume policy details.
        - Cite the sources that support each statement inline with their number in square brackets, e.g. [1] or [2][3].
        - If the sources do not contain the answer, reply with exactly: {NotFoundAnswer}
        - Be concise: a few sentences or a short list.
        """;

    /// <summary>Builds the user message: numbered sources followed by the question.</summary>
    public static string BuildUserMessage(string question, IReadOnlyList<RetrievedChunk> sources)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Sources:");
        for (var i = 0; i < sources.Count; i++)
        {
            builder.AppendLine($"[{i + 1}] Title: {sources[i].Title}");
            builder.AppendLine($"Content: {sources[i].Content}");
            builder.AppendLine();
        }

        builder.AppendLine($"Question: {question}");
        return builder.ToString();
    }
}
