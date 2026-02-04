#region

using System.Text.Json.Serialization;

#endregion

namespace Obs.Generator.Models;

public class ObsWsRequest
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = string.Empty;

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

    [JsonPropertyName("requestFields")]
    public ObsWsRequestField[] RequestFields { get; set; } = [];

    [JsonPropertyName("responseFields")]
    public ObsWsResponseField[] ResponseFields { get; set; } = [];
}