#region

using Obs.NET.Enums;

#endregion

namespace Obs.NET.OperationDataContainers;

/// <summary>
///     Response to <see cref="HelloContainer" /> message, should contain authentication string if authentication is
///     required, along with PubSub subscriptions and other session parameters.
/// </summary>
public class IdentifyContainer : IObsWsOpDataContainer
{
    /// <summary>
    ///     The authentication string to be used to establish a connection
    /// </summary>
    public string? Authentication { get; set; }

    /// <summary>
    ///     A bitmask of EventSubscriptions items to subscribe to events and event categories at will.
    ///     By default, all event categories are subscribed, except for events marked as high volume.
    ///     High volume events must be explicitly subscribed to.
    /// </summary>
    public required EventSubscription EventSubscriptions { get; set; }

    /// <summary>
    ///     The version number that the client would like the obs-websocket server to use.
    /// </summary>
    public required int RpcVersion { get; set; }

    /// <inheritdoc />
    public static int Op => 1;
}