# Obs.NET Examples

This project contains example applications demonstrating how to use the Obs.NET library to interact with OBS Studio via WebSocket.

## Prerequisites

See the main [README.md](../../README.md) for prerequisites and setup instructions.

## Running Examples

```bash
# Run the default example (connection)
dotnet run --project src/Obs.NET.Example

# Run a specific example
dotnet run --project src/Obs.NET.Example -- --example simple

# List all available examples
dotnet run --project src/Obs.NET.Example -- --list-examples

# Show help
dotnet run --project src/Obs.NET.Example -- --help
```

## Command Line Options

| Option | Description |
|--------|-------------|
| `-h, --host <host>` | OBS WebSocket host (default: localhost) |
| `-p, --port <port>` | OBS WebSocket port (default: 4455) |
| `--password <password>` | OBS WebSocket password (if authentication is enabled) |
| `-e, --example <name>` | Example to run (default: connection) |
| `--list-examples` | List all available examples |
| `-v, --verbose` | Enable verbose output |
| `--help` | Show help message |

## Available Examples

### simple

A minimal, beginner-friendly example using only `Console.WriteLine` for output. Demonstrates the core workflow without external dependencies.

**What it does:**
1. Connects to OBS
2. Creates a new scene called "ObsExample"
3. Adds a text source displaying "Hello from Obs.NET!"
4. Records a 2-second clip
5. Cleans up by deleting the scene

**Run it:**
```bash
dotnet run --project src/Obs.NET.Example -- --example simple
```

### connection

A comprehensive example using Spectre.Console for rich terminal output. Demonstrates connection handling, event subscriptions, and various OBS operations.

**What it does:**
1. Connects to OBS with detailed status output
2. Retrieves OBS version information
3. Configures a temporary recording directory
4. Starts recording
5. Creates a text source with timestamp
6. Records for 10 seconds
7. Displays recording statistics
8. Stops recording and shows file details
9. Restores original settings
10. Displays a summary of received events

**Run it:**
```bash
dotnet run --project src/Obs.NET.Example -- --example connection
```

## Creating New Examples

To add a new example:

1. Create a new class in the `Examples/` directory implementing `IExample`:

```csharp
public class MyExample : IExample
{
    public string Name => "my-example";
    public string Description => "Brief description of what this example does";

    public async Task<int> RunAsync(ExampleOptions options, CancellationToken cancellationToken)
    {
        // Create and configure the client
        var clientOptions = new ObsWsClientOptions
        {
            Host = options.Host,
            Port = options.Port,
            Password = options.Password,
            EventSubscriptions = EventSubscription.General
        };

        await using var client = new ObsWsClient(clientOptions);
        await client.ConnectAsync(cancellationToken);

        // Your example code here...

        return 0; // Exit code
    }
}
```

2. Register it in `Program.cs`:

```csharp
Dictionary<string, IExample> examples = new(StringComparer.OrdinalIgnoreCase)
{
    ["connection"] = new ConnectionAndSetupDemo(),
    ["simple"] = new SimpleRecordingDemo(),
    ["my-example"] = new MyExample()  // Add your example here
};
```

3. Update this file to document your new example.
