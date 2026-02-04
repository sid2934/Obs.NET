#region

using Obs.NET.Enums;

#endregion

namespace Obs.NET.Tests.Core;

/// <summary>
/// Tests for the ObsWsClientOptions class.
///
/// ObsWsClientOptions configures the OBS WebSocket client connection. These options control:
/// 1. Connection settings (host, port, password)
/// 2. Timeout behavior (request timeout)
/// 3. Reconnection behavior (delay, max attempts)
/// 4. Event subscriptions (which OBS events to receive)
///
/// Correct default values are critical because they define the out-of-box experience.
/// Users should be able to create an ObsWsClientOptions with minimal configuration
/// and have it work with a standard OBS WebSocket setup.
/// </summary>
public class ObsWsClientOptionsTests
{
    #region Default Values Tests

    /// <summary>
    /// Tests that the default host is "localhost".
    ///
    /// Most users run OBS on the same machine as their application.
    /// "localhost" is the standard default that works out of the box.
    /// </summary>
    [Test]
    public async Task DefaultHost_IsLocalhost()
    {
        // Act
        var options = new ObsWsClientOptions();

        // Assert
        await Assert.That(options.Host).IsEqualTo("localhost");
    }

    /// <summary>
    /// Tests that the default port is 4455.
    ///
    /// Port 4455 is the default port for OBS WebSocket v5. This differs from
    /// the v4 default of 4444. Using the correct default ensures compatibility
    /// with standard OBS installations.
    /// </summary>
    [Test]
    public async Task DefaultPort_Is4455()
    {
        // Act
        var options = new ObsWsClientOptions();

        // Assert
        await Assert.That(options.Port).IsEqualTo(4455);
    }

    /// <summary>
    /// Tests that the default password is null.
    ///
    /// OBS WebSocket can run with or without authentication. A null password
    /// indicates that no authentication will be attempted, which works with
    /// OBS installations that have authentication disabled.
    /// </summary>
    [Test]
    public async Task DefaultPassword_IsNull()
    {
        // Act
        var options = new ObsWsClientOptions();

        // Assert
        await Assert.That(options.Password).IsNull();
    }

    /// <summary>
    /// Tests that the default request timeout is 30 seconds.
    ///
    /// Request timeout determines how long to wait for OBS to respond to a request.
    /// 30 seconds is a reasonable default that accommodates slow operations
    /// (like starting a recording) while not waiting indefinitely on failures.
    /// </summary>
    [Test]
    public async Task DefaultRequestTimeout_Is30Seconds()
    {
        // Act
        var options = new ObsWsClientOptions();

        // Assert
        await Assert.That(options.RequestTimeout).IsEqualTo(TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Tests that the default reconnect delay is 5 seconds.
    ///
    /// When the connection is lost, the client waits before attempting to reconnect.
    /// 5 seconds provides a balance between quick recovery and avoiding rapid
    /// reconnection attempts that could overwhelm the server.
    /// </summary>
    [Test]
    public async Task DefaultReconnectDelay_Is5Seconds()
    {
        // Act
        var options = new ObsWsClientOptions();

        // Assert
        await Assert.That(options.ReconnectDelay).IsEqualTo(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Tests that the default max reconnect attempts is 5.
    ///
    /// After 5 failed reconnection attempts, the client gives up. This prevents
    /// infinite reconnection loops when OBS is truly unavailable, while still
    /// allowing recovery from temporary network issues.
    /// </summary>
    [Test]
    public async Task DefaultMaxReconnectAttempts_Is5()
    {
        // Act
        var options = new ObsWsClientOptions();

        // Assert
        await Assert.That(options.MaxReconnectAttempts).IsEqualTo(5);
    }

    /// <summary>
    /// Tests that the default event subscriptions is EventSubscription.All.
    ///
    /// By default, clients receive all non-high-volume events. This provides
    /// full functionality out of the box. Users can restrict subscriptions
    /// to reduce network traffic if needed.
    /// </summary>
    [Test]
    public async Task DefaultEventSubscriptions_IsAll()
    {
        // Act
        var options = new ObsWsClientOptions();

        // Assert
        await Assert.That(options.EventSubscriptions).IsEqualTo(EventSubscription.All);
    }

    #endregion

    #region Custom Values Tests

    /// <summary>
    /// Tests that custom host can be set.
    ///
    /// Users connecting to OBS on a remote machine need to specify
    /// the host IP address or hostname.
    /// </summary>
    [Test]
    public async Task Host_CanBeSetToCustomValue()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            Host = "192.168.1.100"
        };

        // Assert
        await Assert.That(options.Host).IsEqualTo("192.168.1.100");
    }

    /// <summary>
    /// Tests that custom port can be set.
    ///
    /// Users may configure OBS WebSocket to use a non-default port
    /// to avoid conflicts or for security reasons.
    /// </summary>
    [Test]
    public async Task Port_CanBeSetToCustomValue()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            Port = 5000
        };

        // Assert
        await Assert.That(options.Port).IsEqualTo(5000);
    }

    /// <summary>
    /// Tests that password can be set for authenticated connections.
    ///
    /// When OBS WebSocket authentication is enabled, the password must be
    /// provided to successfully connect and authenticate.
    /// </summary>
    [Test]
    public async Task Password_CanBeSetForAuthentication()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            Password = "my-secret-password"
        };

        // Assert
        await Assert.That(options.Password).IsEqualTo("my-secret-password");
    }

    /// <summary>
    /// Tests that request timeout can be customized.
    ///
    /// Some operations may need longer timeouts (e.g., starting a complex scene),
    /// while others may need shorter timeouts for faster failure detection.
    /// </summary>
    [Test]
    public async Task RequestTimeout_CanBeCustomized()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            RequestTimeout = TimeSpan.FromMinutes(2)
        };

        // Assert
        await Assert.That(options.RequestTimeout).IsEqualTo(TimeSpan.FromMinutes(2));
    }

    /// <summary>
    /// Tests that reconnect delay can be customized.
    ///
    /// Users may want faster reconnection for critical applications
    /// or slower reconnection to reduce server load.
    /// </summary>
    [Test]
    public async Task ReconnectDelay_CanBeCustomized()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            ReconnectDelay = TimeSpan.FromSeconds(10)
        };

        // Assert
        await Assert.That(options.ReconnectDelay).IsEqualTo(TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Tests that max reconnect attempts can be customized.
    ///
    /// Critical applications might want more attempts, while scripts
    /// might want fewer or zero (fail fast).
    /// </summary>
    [Test]
    public async Task MaxReconnectAttempts_CanBeCustomized()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            MaxReconnectAttempts = 10
        };

        // Assert
        await Assert.That(options.MaxReconnectAttempts).IsEqualTo(10);
    }

    /// <summary>
    /// Tests that max reconnect attempts can be set to zero.
    ///
    /// Setting to zero disables automatic reconnection, useful for
    /// one-shot scripts or when manual reconnection is preferred.
    /// </summary>
    [Test]
    public async Task MaxReconnectAttempts_CanBeSetToZero()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            MaxReconnectAttempts = 0
        };

        // Assert
        await Assert.That(options.MaxReconnectAttempts).IsEqualTo(0);
    }

    #endregion

    #region Event Subscription Tests

    /// <summary>
    /// Tests that specific event categories can be subscribed to.
    ///
    /// EventSubscription is a flags enum, allowing users to subscribe
    /// to specific categories only. This test verifies that individual
    /// categories can be selected.
    /// </summary>
    [Test]
    public async Task EventSubscriptions_CanBeSetToSpecificCategories()
    {
        // Arrange & Act - Subscribe only to Scenes and Outputs events
        var options = new ObsWsClientOptions
        {
            EventSubscriptions = EventSubscription.Scenes | EventSubscription.Outputs
        };

        // Assert
        await Assert.That(options.EventSubscriptions.HasFlag(EventSubscription.Scenes)).IsTrue();
        await Assert.That(options.EventSubscriptions.HasFlag(EventSubscription.Outputs)).IsTrue();
        await Assert.That(options.EventSubscriptions.HasFlag(EventSubscription.Inputs)).IsFalse();
    }

    /// <summary>
    /// Tests that all events can be disabled.
    ///
    /// Setting EventSubscription.None disables all events. This is useful
    /// when the client only needs to send requests and doesn't care about
    /// OBS state changes, reducing network traffic.
    /// </summary>
    [Test]
    public async Task EventSubscriptions_CanBeSetToNone()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            EventSubscriptions = EventSubscription.None
        };

        // Assert
        await Assert.That(options.EventSubscriptions).IsEqualTo(EventSubscription.None);
    }

    /// <summary>
    /// Tests that high-volume events can be included.
    ///
    /// High-volume events (like InputVolumeMeters) are not included in
    /// EventSubscription.All by default. They must be explicitly added
    /// by combining flags.
    /// </summary>
    [Test]
    public async Task EventSubscriptions_CanIncludeHighVolumeEvents()
    {
        // Arrange & Act - Include all events plus volume meters
        var options = new ObsWsClientOptions
        {
            EventSubscriptions = EventSubscription.All | EventSubscription.InputVolumeMeters
        };

        // Assert
        await Assert.That(options.EventSubscriptions.HasFlag(EventSubscription.All)).IsTrue();
        await Assert.That(options.EventSubscriptions.HasFlag(EventSubscription.InputVolumeMeters)).IsTrue();
    }

    /// <summary>
    /// Tests that EventSubscription.All does not include high-volume events.
    ///
    /// High-volume events can generate a lot of traffic. They are intentionally
    /// excluded from the All value to prevent accidental performance issues.
    /// </summary>
    [Test]
    public async Task EventSubscriptions_All_DoesNotIncludeHighVolumeEvents()
    {
        // Arrange
        var allEvents = EventSubscription.All;

        // Assert - High-volume events should not be included
        await Assert.That(allEvents.HasFlag(EventSubscription.InputVolumeMeters)).IsFalse();
        await Assert.That(allEvents.HasFlag(EventSubscription.InputActiveStateChanged)).IsFalse();
        await Assert.That(allEvents.HasFlag(EventSubscription.InputShowStateChanged)).IsFalse();
        await Assert.That(allEvents.HasFlag(EventSubscription.SceneItemTransformChanged)).IsFalse();
    }

    #endregion

    #region Object Initializer Tests

    /// <summary>
    /// Tests that ObsWsClientOptions supports object initializer syntax.
    ///
    /// C# object initializers provide a clean way to configure options.
    /// This test verifies that all properties can be set in a single initializer.
    /// </summary>
    [Test]
    public async Task ObsWsClientOptions_SupportsObjectInitializer()
    {
        // Arrange & Act
        var options = new ObsWsClientOptions
        {
            Host = "obs-server.local",
            Port = 4455,
            Password = "secret",
            RequestTimeout = TimeSpan.FromSeconds(60),
            ReconnectDelay = TimeSpan.FromSeconds(3),
            MaxReconnectAttempts = 10,
            EventSubscriptions = EventSubscription.Scenes | EventSubscription.Outputs
        };

        // Assert
        await Assert.That(options.Host).IsEqualTo("obs-server.local");
        await Assert.That(options.Port).IsEqualTo(4455);
        await Assert.That(options.Password).IsEqualTo("secret");
        await Assert.That(options.RequestTimeout).IsEqualTo(TimeSpan.FromSeconds(60));
        await Assert.That(options.ReconnectDelay).IsEqualTo(TimeSpan.FromSeconds(3));
        await Assert.That(options.MaxReconnectAttempts).IsEqualTo(10);
        await Assert.That(options.EventSubscriptions).IsEqualTo(EventSubscription.Scenes | EventSubscription.Outputs);
    }

    /// <summary>
    /// Tests typical remote connection configuration.
    ///
    /// This represents a common real-world scenario: connecting to OBS
    /// running on another machine on the local network.
    /// </summary>
    [Test]
    public async Task ObsWsClientOptions_RemoteConnectionConfiguration()
    {
        // Arrange & Act - Typical remote connection setup
        var options = new ObsWsClientOptions
        {
            Host = "192.168.1.50",
            Port = 4455,
            Password = "streaming-room-password",
            MaxReconnectAttempts = 10  // More attempts for remote connection
        };

        // Assert
        await Assert.That(options.Host).IsEqualTo("192.168.1.50");
        await Assert.That(options.Password).IsEqualTo("streaming-room-password");
        await Assert.That(options.MaxReconnectAttempts).IsEqualTo(10);
    }

    /// <summary>
    /// Tests minimal script configuration with no reconnection.
    ///
    /// This represents a common scenario for one-shot scripts that
    /// should fail fast rather than retry indefinitely.
    /// </summary>
    [Test]
    public async Task ObsWsClientOptions_MinimalScriptConfiguration()
    {
        // Arrange & Act - Quick script that should fail fast
        var options = new ObsWsClientOptions
        {
            Password = "script-password",
            MaxReconnectAttempts = 0,
            RequestTimeout = TimeSpan.FromSeconds(10)
        };

        // Assert
        await Assert.That(options.Host).IsEqualTo("localhost");  // Default
        await Assert.That(options.Port).IsEqualTo(4455);  // Default
        await Assert.That(options.MaxReconnectAttempts).IsEqualTo(0);  // No retries
        await Assert.That(options.RequestTimeout).IsEqualTo(TimeSpan.FromSeconds(10));
    }

    #endregion
}
