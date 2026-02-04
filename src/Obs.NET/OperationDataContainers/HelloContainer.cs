#region

using System.Text.Json.Serialization;
using Obs.NET.Models;

#endregion

namespace Obs.NET.OperationDataContainers;

public class HelloContainer : IObsWsOpDataContainer
{
    /// <summary>
    ///     The version of OBS Studio the server is running.
    /// </summary>
    [JsonPropertyName("obsStudioVersion")]
    public required string ObsStudioVersion { get; set; }

    /// <summary>
    ///     May be used as a soft feature level hint. For example,
    ///     a new WebSocket request may only be available in a specific obs-websocket
    ///     version or newer, but the rpcVersion will not be increased, as no breaking
    ///     changes have occured. Be aware, that no guarantees will be made on these assumptions,
    ///     and you should still verify that the requests you desire to use are available in
    ///     obs-websocket via the GetVersion request.
    /// </summary>
    [JsonPropertyName("obsWebSocketVersion")]
    public required string ObsWebSocketVersion { get; set; }

    /// <summary>
    ///     A version number which gets incremented on each breaking change to the obs-websocket protocol.
    ///     Its usage in this context is to provide the current rpc version that the server would like to use.
    /// </summary>
    [JsonPropertyName("rpcVersion")]
    public required int RpcVersion { get; set; }

    /// <summary>
    ///     Authentication information if authentication is required
    /// </summary>
    [JsonPropertyName("authentication")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HelloAuthenticationBody? Authentication { get; set; }

    /// <inheritdoc />
    /// >
    public static int Op => 0;
}