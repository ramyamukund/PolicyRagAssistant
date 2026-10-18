using PolicyAssistant.Shared;

namespace PolicyAssistant.Web.Chat;

/// <summary>A message shown in the chat: the user's question, an assistant answer, or an error.</summary>
public sealed class ChatMessage
{
    private ChatMessage(string role, string text)
    {
        Role = role;
        Text = text;
    }

    public string Role { get; }

    public string Text { get; }

    public IReadOnlyList<IReadOnlyList<AnswerSegment>> Paragraphs { get; private init; } = [];

    public IReadOnlyList<Citation> Citations { get; private init; } = [];

    public bool IsGrounded { get; private init; }

    public bool IsError { get; private init; }

    /// <summary>The citation whose details panel is open, if any.</summary>
    public Citation? ExpandedCitation { get; private set; }

    public bool IsUser => Role == ChatTurn.UserRole;

    public static ChatMessage FromUser(string question) => new(ChatTurn.UserRole, question);

    public static ChatMessage FromAnswer(ChatResponse response) => new(ChatTurn.AssistantRole, response.Answer)
    {
        Paragraphs = AnswerParser.Parse(response.Answer, response.Citations),
        Citations = response.Citations,
        IsGrounded = response.IsGrounded,
    };

    public static ChatMessage FromError(string message) => new(ChatTurn.AssistantRole, message)
    {
        Paragraphs = [[new TextSegment(message)]],
        IsError = true,
    };

    public void ToggleCitation(Citation citation) =>
        ExpandedCitation = ExpandedCitation == citation ? null : citation;
}
