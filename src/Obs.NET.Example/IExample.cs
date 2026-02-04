namespace Obs.NET.Example;

/// <summary>
/// Options passed to examples for configuration
/// </summary>
public record ExampleOptions
{
    /// <summary>
    /// OBS WebSocket host
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// OBS WebSocket port
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// OBS WebSocket password (null if no authentication)
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Whether to output verbose logging
    /// </summary>
    public bool Verbose { get; init; }
}

/// <summary>
/// Interface for example demonstrations of the OBS WebSocket client
/// </summary>
public interface IExample
{
    /// <summary>
    /// The name of the example (used with --example flag)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// A brief description of what the example demonstrates
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Run the example
    /// </summary>
    /// <param name="options">Configuration options for the example</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exit code (0 for success)</returns>
    Task<int> RunAsync(ExampleOptions options, CancellationToken cancellationToken);
}
