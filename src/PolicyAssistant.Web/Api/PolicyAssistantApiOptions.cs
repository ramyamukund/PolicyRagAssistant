using System.ComponentModel.DataAnnotations;

namespace PolicyAssistant.Web.Api;

/// <summary>Where the Web app finds the PolicyAssistant API.</summary>
public sealed class PolicyAssistantApiOptions
{
    public const string SectionName = "PolicyAssistantApi";

    /// <summary>The API's base URL, e.g. http://localhost:5271.</summary>
    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Range(1, 600)]
    public int TimeoutSeconds { get; set; } = 60;
}
