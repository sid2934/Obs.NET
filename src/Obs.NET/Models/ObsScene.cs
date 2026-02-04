using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Represents an OBS scene with known properties and extensibility for additional data.
/// </summary>
public class ObsScene
{
    /// <summary>
    /// Name of the scene
    /// </summary>
    [JsonPropertyName("sceneName")]
    public string SceneName { get; set; } = string.Empty;

    /// <summary>
    /// UUID of the scene
    /// </summary>
    [JsonPropertyName("sceneUuid")]
    public string SceneUuid { get; set; } = string.Empty;

    /// <summary>
    /// Index position of the scene in the scene list
    /// </summary>
    [JsonPropertyName("sceneIndex")]
    public int SceneIndex { get; set; }

    /// <summary>
    /// Additional properties not explicitly defined in this model.
    /// Provides forward compatibility with future OBS versions.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
