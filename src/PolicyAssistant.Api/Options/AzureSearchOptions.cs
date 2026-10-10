using System.ComponentModel.DataAnnotations;

namespace PolicyAssistant.Api.Options;

/// <summary>Settings for the Azure AI Search index the policies were imported into.</summary>
public sealed class AzureSearchOptions
{
    public const string SectionName = "AzureSearch";

    [Required, Url]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string IndexName { get; set; } = string.Empty;

    [Required]
    public string SemanticConfigurationName { get; set; } = string.Empty;

    [Required]
    public string ContentField { get; set; } = "chunk";

    [Required]
    public string TitleField { get; set; } = "title";

    [Required]
    public string VectorField { get; set; } = "text_vector";

    [Required]
    public string KeyField { get; set; } = "chunk_id";

    [Range(1, 50)]
    public int Top { get; set; } = 5;

    /// <summary>Results with a semantic reranker score (0–4) below this are dropped.</summary>
    [Range(0.0, 4.0)]
    public double MinRerankerScore { get; set; } = 1.5;
}
