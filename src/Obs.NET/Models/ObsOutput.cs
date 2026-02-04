using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Represents an OBS output with known properties and extensibility for additional data.
/// </summary>
public class ObsOutput
{
    /// <summary>
    /// Name of the output
    /// </summary>
    [JsonPropertyName("outputName")]
    public string OutputName { get; set; } = string.Empty;

    /// <summary>
    /// Kind of output
    /// </summary>
    [JsonPropertyName("outputKind")]
    public string OutputKind { get; set; } = string.Empty;

    /// <summary>
    /// Width of the output in pixels
    /// </summary>
    [JsonPropertyName("outputWidth")]
    public int OutputWidth { get; set; }

    /// <summary>
    /// Height of the output in pixels
    /// </summary>
    [JsonPropertyName("outputHeight")]
    public int OutputHeight { get; set; }

    /// <summary>
    /// Whether the output is currently active
    /// </summary>
    [JsonPropertyName("outputActive")]
    public bool OutputActive { get; set; }

    /// <summary>
    /// Output capability flags
    /// </summary>
    [JsonPropertyName("outputFlags")]
    public JsonElement? OutputFlags { get; set; }

    /// <summary>
    /// Additional properties not explicitly defined in this model.
    /// Provides forward compatibility with future OBS versions.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
