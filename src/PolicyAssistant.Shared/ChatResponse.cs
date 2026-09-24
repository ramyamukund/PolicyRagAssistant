namespace PolicyAssistant.Shared;

/// <summary>The API's answer to a <see cref="ChatRequest"/>.</summary>
/// <param name="Answer">The generated answer, containing inline [n] citation markers.</param>
/// <param name="Citations">Only the sources actually cited in the answer.</param>
/// <param name="IsGrounded">True when the answer cites at least one source.</param>
public sealed record ChatResponse(string Answer, IReadOnlyList<Citation> Citations, bool IsGrounded);
