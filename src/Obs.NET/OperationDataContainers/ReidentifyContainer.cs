#region

using Obs.NET.Enums;

#endregion

namespace Obs.NET.OperationDataContainers;

/// <summary>
///     Sent at any time after initial identification to update the provided session parameters.
/// </summary>
public class ReidentifyContainer : IObsWsOpDataContainer
{
    /// <summary>
    ///     A bitmask of EventSubscriptions items to subscribe to events and event categories at will.
    ///     By default, all event categories are subscribed, except for events marked as high volume.
    ///     High volume events must be explicitly subscribed to.
    /// </summary>
    public required EventSubscription EventSubscriptions { get; set; }

    /// <inheritdoc />
    public static int Op => 3;
}