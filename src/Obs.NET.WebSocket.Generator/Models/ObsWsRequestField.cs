#region

using System.Text.Json.Serialization;

#endregion

namespace Obs.Generator.Models;

public class ObsWsRequestField
{
    [JsonPropertyName("valueName")]
    public string ValueName { get; set; } = string.Empty;

    [JsonPropertyName("valueType")]
    public string ValueType { get; set; } = string.Empty;

    [JsonPropertyName("valueDescription")]
    public string ValueDescription { get; set; } = string.Empty;

    [JsonPropertyName("valueRestrictions")]
    public string ValueRestrictions { get; set; } = string.Empty;

    [JsonPropertyName("valueOptional")]
    public bool ValueOptional { get; set; }

    [JsonPropertyName("valueOptionalBehavior")]
    public string ValueOptionalBehavior { get; set; } = string.Empty;
}