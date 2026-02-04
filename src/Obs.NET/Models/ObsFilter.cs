using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Represents an OBS filter with known properties and extensibility for additional data.
/// </summary>
public class ObsFilter
{
    /// <summary>
    /// Name of the filter
    /// </summary>
    [JsonPropertyName("filterName")]
    public string FilterName { get; set; } = string.Empty;

    /// <summary>
    /// Kind of filter (e.g., "color_filter", "mask_filter")
    /// </summary>
    [JsonPropertyName("filterKind")]
    public string FilterKind { get; set; } = string.Empty;

    /// <summary>
    /// Index position of the filter in the filter list
    /// </summary>
    [JsonPropertyName("filterIndex")]
    public int FilterIndex { get; set; }

    /// <summary>
    /// Whether the filter is enabled
    /// </summary>
    [JsonPropertyName("filterEnabled")]
    public bool FilterEnabled { get; set; }

    /// <summary>
    /// Settings of the filter (dynamic based on filter kind)
    /// </summary>
    [JsonPropertyName("filterSettings")]
    public JsonElement? FilterSettings { get; set; }

    /// <summary>
    /// Additional properties not explicitly defined in this model.
    /// Provides forward compatibility with future OBS versions.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
