using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Represents an OBS scene item with known properties and extensibility for additional data.
/// </summary>
public class ObsSceneItem
{
    /// <summary>
    /// Numeric ID of the scene item
    /// </summary>
    [JsonPropertyName("sceneItemId")]
    public int SceneItemId { get; set; }

    /// <summary>
    /// Index position of the scene item in the scene
    /// </summary>
    [JsonPropertyName("sceneItemIndex")]
    public int SceneItemIndex { get; set; }

    /// <summary>
    /// Name of the source associated with the scene item
    /// </summary>
    [JsonPropertyName("sourceName")]
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// UUID of the source associated with the scene item
    /// </summary>
    [JsonPropertyName("sourceUuid")]
    public string SourceUuid { get; set; } = string.Empty;

    /// <summary>
    /// Type of the source (e.g., "OBS_SOURCE_TYPE_INPUT", "OBS_SOURCE_TYPE_SCENE")
    /// </summary>
    [JsonPropertyName("sourceType")]
    public string? SourceType { get; set; }

    /// <summary>
    /// Kind of input if the source is an input
    /// </summary>
    [JsonPropertyName("inputKind")]
    public string? InputKind { get; set; }

    /// <summary>
    /// Whether the scene item is a group
    /// </summary>
    [JsonPropertyName("isGroup")]
    public bool? IsGroup { get; set; }

    /// <summary>
    /// Whether the scene item is enabled/visible
    /// </summary>
    [JsonPropertyName("sceneItemEnabled")]
    public bool? SceneItemEnabled { get; set; }

    /// <summary>
    /// Whether the scene item is locked
    /// </summary>
    [JsonPropertyName("sceneItemLocked")]
    public bool? SceneItemLocked { get; set; }

    /// <summary>
    /// Blend mode of the scene item
    /// </summary>
    [JsonPropertyName("sceneItemBlendMode")]
    public string? SceneItemBlendMode { get; set; }

    /// <summary>
    /// Additional properties not explicitly defined in this model.
    /// Provides forward compatibility with future OBS versions.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
