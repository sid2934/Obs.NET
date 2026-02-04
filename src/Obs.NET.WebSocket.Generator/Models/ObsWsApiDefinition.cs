#region

using System.Text.Json.Serialization;

#endregion

namespace Obs.Generator.Models;

public class ObsWsApiDefinition
{
    [JsonPropertyName("enums")]
    public ObsWsEnum[] Enums { get; set; } = [];

    [JsonPropertyName("requests")]
    public ObsWsRequest[] Requests { get; set; } = [];

    [JsonPropertyName("events")]
    public ObsWsEvent[] Events { get; set; } = [];
}