#region

using Obs.NET.Enums;

#endregion

namespace Obs.NET;

/// <summary>
///     Configuration options for OBS WebSocket client
/// </summary>
public class ObsWsClientOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 4455;
    public string? Password { get; set; }
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxReconnectAttempts { get; set; } = 5;

    public EventSubscription EventSubscriptions { get; set; } = EventSubscription.All;
}