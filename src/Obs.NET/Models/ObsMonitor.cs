using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Represents a system monitor with known properties and extensibility for additional data.
/// </summary>
public class ObsMonitor
{
    /// <summary>
    /// Index of the monitor
    /// </summary>
    [JsonPropertyName("monitorIndex")]
    public int MonitorIndex { get; set; }

    /// <summary>
    /// Name of the monitor
    /// </summary>
    [JsonPropertyName("monitorName")]
    public string MonitorName { get; set; } = string.Empty;

    /// <summary>
    /// Width of the monitor in pixels
    /// </summary>
    [JsonPropertyName("monitorWidth")]
    public int MonitorWidth { get; set; }

    /// <summary>
    /// Height of the monitor in pixels
    /// </summary>
    [JsonPropertyName("monitorHeight")]
    public int MonitorHeight { get; set; }

    /// <summary>
    /// X position of the monitor
    /// </summary>
    [JsonPropertyName("monitorPositionX")]
    public int MonitorPositionX { get; set; }

    /// <summary>
    /// Y position of the monitor
    /// </summary>
    [JsonPropertyName("monitorPositionY")]
    public int MonitorPositionY { get; set; }

    /// <summary>
    /// Additional properties not explicitly defined in this model.
    /// Provides forward compatibility with future OBS versions.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
