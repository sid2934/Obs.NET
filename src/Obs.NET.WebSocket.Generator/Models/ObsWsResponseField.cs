#region

using System.Text.Json.Serialization;

#endregion

namespace Obs.Generator.Models;

public class ObsWsResponseField
{
    [JsonPropertyName("valueName")]
    public string ValueName { get; set; } = string.Empty;

    [JsonPropertyName("valueType")]
    public string ValueType { get; set; } = string.Empty;

    [JsonPropertyName("valueDescription")]
    public string ValueDescription { get; set; } = string.Empty;
}