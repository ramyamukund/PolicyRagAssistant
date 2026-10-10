using System.ComponentModel.DataAnnotations;

namespace PolicyAssistant.Api.Options;

/// <summary>Settings for the chat model deployed in Foundry.</summary>
public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    /// <summary>The OpenAI-compatible v1 endpoint, e.g. https://&lt;resource&gt;.openai.azure.com/openai/v1.</summary>
    [Required, Url]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>The deployment name, sent as the model name on the v1 endpoint.</summary>
    [Required]
    public string ChatDeployment { get; set; } = string.Empty;

    [Range(0.0, 2.0)]
    public float Temperature { get; set; } = 0.1f;

    [Range(1, 16000)]
    public int MaxOutputTokens { get; set; } = 800;
}
