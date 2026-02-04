#region

using System.Text.Json.Serialization;

#endregion

namespace Obs.Generator.Models;

public class ObsWsEvent
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("eventSubscription")]
    public string EventSubscription { get; set; } = string.Empty;

    [JsonPropertyName("complexity")]
    public int Complexity { get; set; }

    [JsonPropertyName("rpcVersion")]
    public string RpcVersion { get; set; } = string.Empty;

    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; set; }

    [JsonPropertyName("initialVersion")]
    public string InitialVersion { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("dataFields")]
    public ObsWsEventDataField[] DataFields { get; set; } = [];
}