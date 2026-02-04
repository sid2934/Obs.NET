using System.Text.Json;
using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Represents the transform properties of an OBS scene item.
/// </summary>
public class ObsSceneItemTransform
{
    /// <summary>
    /// X position of the scene item
    /// </summary>
    [JsonPropertyName("positionX")]
    public double PositionX { get; set; }

    /// <summary>
    /// Y position of the scene item
    /// </summary>
    [JsonPropertyName("positionY")]
    public double PositionY { get; set; }

    /// <summary>
    /// Rotation of the scene item in degrees
    /// </summary>
    [JsonPropertyName("rotation")]
    public double Rotation { get; set; }

    /// <summary>
    /// X scale factor of the scene item
    /// </summary>
    [JsonPropertyName("scaleX")]
    public double ScaleX { get; set; }

    /// <summary>
    /// Y scale factor of the scene item
    /// </summary>
    [JsonPropertyName("scaleY")]
    public double ScaleY { get; set; }

    /// <summary>
    /// Base width of the source in pixels
    /// </summary>
    [JsonPropertyName("sourceWidth")]
    public int SourceWidth { get; set; }

    /// <summary>
    /// Base height of the source in pixels
    /// </summary>
    [JsonPropertyName("sourceHeight")]
    public int SourceHeight { get; set; }

    /// <summary>
    /// Rendered width of the scene item in pixels
    /// </summary>
    [JsonPropertyName("width")]
    public double Width { get; set; }

    /// <summary>
    /// Rendered height of the scene item in pixels
    /// </summary>
    [JsonPropertyName("height")]
    public double Height { get; set; }

    /// <summary>
    /// Alignment of the scene item
    /// </summary>
    [JsonPropertyName("alignment")]
    public int Alignment { get; set; }

    /// <summary>
    /// Type of bounds (e.g., "OBS_BOUNDS_NONE", "OBS_BOUNDS_STRETCH")
    /// </summary>
    [JsonPropertyName("boundsType")]
    public string? BoundsType { get; set; }

    /// <summary>
    /// Alignment of the bounds
    /// </summary>
    [JsonPropertyName("boundsAlignment")]
    public int BoundsAlignment { get; set; }

    /// <summary>
    /// Width of the bounds
    /// </summary>
    [JsonPropertyName("boundsWidth")]
    public double BoundsWidth { get; set; }

    /// <summary>
    /// Height of the bounds
    /// </summary>
    [JsonPropertyName("boundsHeight")]
    public double BoundsHeight { get; set; }

    /// <summary>
    /// Number of pixels cropped from the left
    /// </summary>
    [JsonPropertyName("cropLeft")]
    public int CropLeft { get; set; }

    /// <summary>
    /// Number of pixels cropped from the right
    /// </summary>
    [JsonPropertyName("cropRight")]
    public int CropRight { get; set; }

    /// <summary>
    /// Number of pixels cropped from the top
    /// </summary>
    [JsonPropertyName("cropTop")]
    public int CropTop { get; set; }

    /// <summary>
    /// Number of pixels cropped from the bottom
    /// </summary>
    [JsonPropertyName("cropBottom")]
    public int CropBottom { get; set; }

    /// <summary>
    /// Whether to crop to the bounds
    /// </summary>
    [JsonPropertyName("cropToBounds")]
    public bool CropToBounds { get; set; }

    /// <summary>
    /// Additional properties not explicitly defined in this model.
    /// Provides forward compatibility with future OBS versions.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
