# Obs.NET

A full-featured OBS Studio WebSocket v5 client library for .NET.

[![NuGet](https://img.shields.io/nuget/v/Obs.NET.svg)](https://www.nuget.org/packages/Obs.NET)
[![GitHub](https://img.shields.io/github/stars/sid2934/Obs.NET?style=social)](https://github.com/sid2934/Obs.NET)
[![CI](https://github.com/sid2934/Obs.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/sid2934/Obs.NET/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

## Features

- Modern async/await API for controlling OBS Studio
- 146 request types covering scenes, sources, filters, recording, streaming, and more
- 57 strongly-typed events for all OBS state changes
- Automatic reconnection with configurable retry logic
- SHA256 authentication support
- .NET 8.0, .NET 9.0, and .NET 10.0 support

## Installation

```bash
dotnet add package Obs.NET
```

Or via the NuGet Package Manager:

```powershell
Install-Package Obs.NET
```

## Quick Start

```csharp
using Obs.NET;

// Create client with default options (localhost:4455)
await using var client = new ObsWsClient();

// Or configure options
await using var client = new ObsWsClient(new ObsWsClientOptions
{
    Host = "localhost",
    Port = 4455,
    Password = "your-password"
});

// Connect to OBS
await client.ConnectAsync();

// Get current scene
var sceneList = await client.GetSceneListAsync();
Console.WriteLine($"Current scene: {sceneList?.CurrentProgramSceneName}");

// Switch scenes
await client.SetCurrentProgramSceneAsync("Scene 2");

// Start recording
await client.StartRecordAsync();

// Stop recording
var result = await client.StopRecordAsync();
Console.WriteLine($"Recording saved to: {result?.OutputPath}");
```

## Examples

The `Obs.NET.Example` project contains runnable examples demonstrating various library features.

### Running Examples

```bash
# List all available examples
dotnet run --project src/Obs.NET.Example -- --list-examples

# Run the simple example (beginner-friendly, minimal dependencies)
dotnet run --project src/Obs.NET.Example -- --example simple

# Run with connection options
dotnet run --project src/Obs.NET.Example -- --example simple --host 192.168.1.100 --password mypassword
```

### Available Examples

| Example | Description |
|---------|-------------|
| `simple` | Minimal beginner-friendly example: creates a scene, adds text, records a 2-second clip, and cleans up. Uses only `Console.WriteLine`. |
| `connection` | Comprehensive example with rich terminal output using Spectre.Console. Demonstrates events, recording, source creation, and more. |

See [src/Obs.NET.Example/Examples.md](src/Obs.NET.Example/Examples.md) for detailed documentation on running and creating examples.

## Configuration Options

```csharp
var options = new ObsWsClientOptions
{
    // Connection settings
    Host = "localhost",           // OBS WebSocket host
    Port = 4455,                  // OBS WebSocket port (default: 4455)
    Password = "your-password",   // Optional password for authentication

    // Timeout and retry settings
    RequestTimeout = TimeSpan.FromSeconds(30),   // Request timeout
    ReconnectDelay = TimeSpan.FromSeconds(5),    // Delay between reconnection attempts
    MaxReconnectAttempts = 5,                    // Max reconnection attempts

    // Event subscriptions (default: All)
    EventSubscriptions = EventSubscription.All
};
```

## Event Handling

Subscribe to strongly-typed events:

```csharp
// Scene changes
client.CurrentProgramSceneChangedEventReceived += async (e) =>
{
    Console.WriteLine($"Scene changed to: {e.SceneName}");
};

// Recording state
client.RecordStateChangedEventReceived += async (e) =>
{
    Console.WriteLine($"Recording state: {e.OutputState}");
};

// Stream state
client.StreamStateChangedEventReceived += async (e) =>
{
    Console.WriteLine($"Stream state: {e.OutputState}");
};

// Connection state
client.ConnectionStateChanged += async (state) =>
{
    Console.WriteLine($"Connection state: {state}");
};
```

## Available Request Categories

| Category         | Description                  | Example Methods                                                        |
|------------------|------------------------------|------------------------------------------------------------------------|
| **General**      | Version info, stats, hotkeys | `GetVersionAsync()`, `GetStatsAsync()`                                 |
| **Config**       | Profiles, scene collections  | `GetProfileListAsync()`, `SetCurrentProfileAsync()`                    |
| **Scenes**       | Scene management             | `GetSceneListAsync()`, `SetCurrentProgramSceneAsync()`                 |
| **Inputs**       | Sources and inputs           | `GetInputListAsync()`, `SetInputMuteAsync()`                           |
| **Scene Items**  | Items within scenes          | `GetSceneItemListAsync()`, `SetSceneItemEnabledAsync()`                |
| **Filters**      | Source filters               | `GetSourceFilterListAsync()`, `SetSourceFilterEnabledAsync()`          |
| **Transitions**  | Scene transitions            | `GetCurrentSceneTransitionAsync()`, `SetCurrentSceneTransitionAsync()` |
| **Outputs**      | Virtual cam, replay buffer   | `StartVirtualCamAsync()`, `SaveReplayBufferAsync()`                    |
| **Stream**       | Streaming control            | `StartStreamAsync()`, `StopStreamAsync()`, `GetStreamStatusAsync()`    |
| **Record**       | Recording control            | `StartRecordAsync()`, `StopRecordAsync()`, `PauseRecordAsync()`        |
| **Media Inputs** | Media playback               | `GetMediaInputStatusAsync()`, `TriggerMediaInputActionAsync()`         |
| **UI**           | Studio mode, projectors      | `GetStudioModeEnabledAsync()`, `OpenVideoMixProjectorAsync()`          |

## Event Subscriptions

Control which events you receive to optimize performance:

```csharp
var options = new ObsWsClientOptions
{
    // Subscribe to specific event categories
    EventSubscriptions = EventSubscription.Scenes | EventSubscription.Inputs
};

// Available subscriptions:
// - EventSubscription.None
// - EventSubscription.General
// - EventSubscription.Config
// - EventSubscription.Scenes
// - EventSubscription.Inputs
// - EventSubscription.Transitions
// - EventSubscription.Filters
// - EventSubscription.Outputs
// - EventSubscription.SceneItems
// - EventSubscription.MediaInputs
// - EventSubscription.Ui
// - EventSubscription.All (default)
// - EventSubscription.InputVolumeMeters (high volume)
// - EventSubscription.InputActiveStateChanged (high volume)
// - EventSubscription.InputShowStateChanged (high volume)
```

## Connection States

```csharp
public enum ConnectionState
{
    Disconnected,    // Not connected
    Connecting,      // Connection in progress
    Connected,       // WebSocket connected, not yet authenticated
    Authenticated    // Fully connected and authenticated
}
```

## Requirements

- .NET 8.0, .NET 9.0, or .NET 10.0
- OBS Studio 28.0+ with [obs-websocket](https://github.com/obsproject/obs-websocket) 5.0+

## OBS WebSocket Setup

1. Open OBS Studio
2. Go to **Tools** > **WebSocket Server Settings**
3. Enable the WebSocket server
4. (Optional) Set a password for authentication
5. Note the port number (default: 4455)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for attribution of third-party materials.

## Acknowledgments

- [OBS Project](https://obsproject.com/) for OBS Studio
- [obs-websocket](https://github.com/obsproject/obs-websocket) for the WebSocket API
