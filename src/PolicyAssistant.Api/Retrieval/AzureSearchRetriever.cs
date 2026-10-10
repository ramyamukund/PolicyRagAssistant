using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using PolicyAssistant.Api.Options;

namespace PolicyAssistant.Api.Retrieval;

/// <summary>
/// Runs one hybrid query (full-text + server-side vectorized query) with semantic reranking,
/// then drops results below the configured reranker score.
/// </summary>
public sealed class AzureSearchRetriever(
    SearchClient searchClient,
    IOptions<AzureSearchOptions> options,
    ILogger<AzureSearchRetriever> logger) : IRetriever
{
    private readonly AzureSearchOptions _options = options.Value;

    public async Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(string question, CancellationToken cancellationToken)
    {
        var searchOptions = new SearchOptions
        {
            Size = _options.Top,
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = _options.SemanticConfigurationName,
            },
            VectorSearch = new VectorSearchOptions
            {
                Queries =
                {
                    new VectorizableTextQuery(question)
                    {
                        KNearestNeighborsCount = _options.Top,
                        Fields = { _options.VectorField },
                    },
                },
            },
        };
        searchOptions.Select.Add(_options.KeyField);
        searchOptions.Select.Add(_options.TitleField);
        searchOptions.Select.Add(_options.ContentField);

        var response = await searchClient.SearchAsync<SearchDocument>(question, searchOptions, cancellationToken);

        var all = new List<RetrievedChunk>();
        await foreach (var result in response.Value.GetResultsAsync().WithCancellation(cancellationToken))
        {
            all.Add(new RetrievedChunk(
                ChunkId: GetString(result.Document, _options.KeyField),
                Title: GetString(result.Document, _options.TitleField),
                Content: GetString(result.Document, _options.ContentField),
                RerankerScore: result.SemanticSearch?.RerankerScore));
        }

        var kept = all.Where(c => c.RerankerScore >= _options.MinRerankerScore).ToList();

        logger.LogInformation(
            "Search returned {ResultCount} results, kept {KeptCount} at reranker score >= {Threshold}. Top scores: {TopScores}",
            all.Count,
            kept.Count,
            _options.MinRerankerScore,
            string.Join(", ", all.Select(c => c.RerankerScore?.ToString("0.00") ?? "n/a")));

        return kept;
    }

    private static string GetString(SearchDocument document, string field) =>
        document.TryGetValue(field, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
}
