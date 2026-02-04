using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Represents an OBS input (source) with known properties and extensibility for additional data.
/// </summary>
public class ObsInput
{
    /// <summary>
    /// Name of the input
    /// </summary>
    [JsonPropertyName("inputName")]
    public string InputName { get; set; } = string.Empty;

    /// <summary>
    /// UUID of the input
    /// </summary>
    [JsonPropertyName("inputUuid")]
    public string InputUuid { get; set; } = string.Empty;

    /// <summary>
    /// The kind of input (e.g., "browser_source", "image_source")
    /// </summary>
    [JsonPropertyName("inputKind")]
    public string InputKind { get; set; } = string.Empty;

    /// <summary>
    /// The unversioned kind of input (without version suffix like _v2)
    /// </summary>
    [JsonPropertyName("unversionedInputKind")]
    public string? UnversionedInputKind { get; set; }

    /// <summary>
    /// Additional properties not explicitly defined in this model.
    /// Provides forward compatibility with future OBS versions.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
