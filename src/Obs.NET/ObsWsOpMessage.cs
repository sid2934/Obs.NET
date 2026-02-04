#region

using System.Text.Json.Serialization;
using Obs.NET.OperationDataContainers;

#endregion

namespace Obs.NET;

/// <summary>
///     A generic message structure that enables the serialization and deserialization
///     of operation messages with varying data container types.
///     Each operation type has its own specific data container that implements
///     the <see cref="IObsWsOpDataContainer" /> interface. This generic class
///     allows for strong typing of the data container while maintaining a consistent
///     message structure across different operation types.
/// </summary>
/// <typeparam name="TOpDataContainer">The specific data container class that matches this messages structure.</typeparam>
public class ObsWsOpMessage<TOpDataContainer>
    where TOpDataContainer : IObsWsOpDataContainer
{
    /// <summary>
    ///     The unique numeric code that designates which operation this message is associated with.
    /// </summary>
    [JsonPropertyName("op")]
    public int Op => TOpDataContainer.Op;

    /// <summary>
    ///     The operation specific data container that holds the payload for this message.
    /// </summary>
    [JsonPropertyName("d")]
    public required TOpDataContainer D { get; init; }
}