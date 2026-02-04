namespace Obs.NET.Events;

/// <summary>
///     Marker interface for OBS WebSocket event data types
/// </summary>
public interface IObsWsEventData
{
    /// <summary>
    ///     The type identifier for this event
    /// </summary>
    static abstract string EventType { get; }
}