namespace PolicyAssistant.Shared;

/// <summary>A source document chunk cited in an answer.</summary>
/// <param name="Number">The marker number used inline in the answer, e.g. 1 for [1].</param>
/// <param name="Title">The source document title.</param>
/// <param name="Snippet">A short excerpt of the cited chunk.</param>
/// <param name="ChunkId">The search index key of the chunk.</param>
public sealed record Citation(int Number, string Title, string Snippet, string ChunkId);
