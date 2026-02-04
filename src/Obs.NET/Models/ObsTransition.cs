using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Represents an OBS transition with known properties and extensibility for additional data.
/// </summary>
public class ObsTransition
{
    /// <summary>
    /// Name of the transition
    /// </summary>
    [JsonPropertyName("transitionName")]
    public string TransitionName { get; set; } = string.Empty;

    /// <summary>
    /// UUID of the transition
    /// </summary>
    [JsonPropertyName("transitionUuid")]
    public string TransitionUuid { get; set; } = string.Empty;

    /// <summary>
    /// Kind of transition
    /// </summary>
    [JsonPropertyName("transitionKind")]
    public string TransitionKind { get; set; } = string.Empty;

    /// <summary>
    /// Whether the transition has a fixed (unconfigurable) duration
    /// </summary>
    [JsonPropertyName("transitionFixed")]
    public bool TransitionFixed { get; set; }

    /// <summary>
    /// Whether the transition supports configuration
    /// </summary>
    [JsonPropertyName("transitionConfigurable")]
    public bool TransitionConfigurable { get; set; }

    /// <summary>
    /// Additional properties not explicitly defined in this model.
    /// Provides forward compatibility with future OBS versions.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
