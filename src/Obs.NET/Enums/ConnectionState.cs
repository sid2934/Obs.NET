namespace Obs.NET.Enums;

/// <summary>
///     Connection state of the OBS WebSocket client
/// </summary>
public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Authenticated
}