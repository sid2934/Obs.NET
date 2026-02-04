using System.Text.Json;
using Obs.NET.Enums;

namespace Obs.NET.Example.Examples;

/// <summary>
/// A minimal example demonstrating basic OBS WebSocket operations.
/// This example avoids external dependencies to focus on the Obs.NET library.
/// </summary>
public class  SimpleRecordingDemo : IExample
{
    public string Name => "simple";
    public string Description => "Minimal example: create scene, add text, record 2s clip, cleanup";

    public async Task<int> RunAsync(ExampleOptions options, CancellationToken cancellationToken)
    {
        Console.WriteLine("=== Obs.NET Simple Recording Demo ===");
        Console.WriteLine();

        // Step 1: Configure the client
        // ObsWsClientOptions holds connection settings and which events we want to receive.
        // For this simple demo, we only need General events (connection state).
        var clientOptions = new ObsWsClientOptions
        {
            Host = options.Host,
            Port = options.Port,
            Password = options.Password,
            EventSubscriptions = EventSubscription.General
        };

        // Step 2: Create the client
        // ObsWsClient implements IAsyncDisposable, so we use 'await using' to ensure
        // the WebSocket connection is properly closed when we're done.
        await using var client = new ObsWsClient(clientOptions);

        // Step 3: Connect to OBS
        // This establishes the WebSocket connection and handles authentication if needed.
        Console.WriteLine($"Connecting to OBS at {options.Host}:{options.Port}...");
        await client.ConnectAsync(cancellationToken);
        Console.WriteLine("Connected!");
        Console.WriteLine();

        // Step 4: Get OBS version to verify the connection works
        var version = await client.GetVersionAsync(cancellationToken);
        Console.WriteLine($"OBS Version: {version?.ObsVersion}");
        Console.WriteLine($"WebSocket Version: {version?.ObsWebSocketVersion}");
        Console.WriteLine();

        // We'll track the original scene so we can restore it at the end
        var originalScene = await client.GetCurrentProgramSceneAsync(cancellationToken);
        var originalSceneName = originalScene?.SceneName;
        Console.WriteLine($"Current scene: {originalSceneName}");

        // Step 5: Create a new scene
        // Scenes are containers that hold sources (video, audio, text, images, etc.)
        const string sceneName = "ObsExample";
        Console.WriteLine($"Creating scene: {sceneName}");
        await client.CreateSceneAsync(sceneName, cancellationToken);

        // Step 6: Switch to our new scene
        // SetCurrentProgramSceneAsync changes what's being shown/recorded
        Console.WriteLine($"Switching to scene: {sceneName}");
        await client.SetCurrentProgramSceneAsync(sceneName, null, cancellationToken);

        // Step 7: Find an available text input kind
        // Different platforms have different text sources:
        // - Windows: text_gdiplus_v2 or text_gdiplus_v3
        // - Linux/Mac: text_ft2_source_v2 or text_ft2_source_v3
        var kindsResponse = await client.GetInputKindListAsync(false, cancellationToken);
        var textKind = kindsResponse?.InputKinds?.FirstOrDefault(k => k.Contains("text"));

        if (textKind == null)
        {
            Console.WriteLine("Warning: No text input kind found, skipping text source creation");
        }
        else
        {
            // Step 8: Create a text source
            // Inputs are the actual content (video capture, text, images, etc.)
            // We add them to scenes to make them visible.
            const string textSourceName = "ExampleText";
            Console.WriteLine($"Creating text source: {textSourceName} (kind: {textKind})");

            // Configure the text settings - this varies by input kind
            var textSettings = new Dictionary<string, object>
            {
                ["text"] = "Hello from Obs.NET!"
            };

            // CreateInputAsync creates the input and adds it to the specified scene
            var inputSettings = JsonSerializer.SerializeToElement(textSettings);
            await client.CreateInputAsync(
                textSourceName,
                textKind,
                sceneName,      // Add to our new scene
                null,           // No scene UUID (we're using name)
                inputSettings,
                true,           // Enable the source immediately
                cancellationToken);

            Console.WriteLine("Text source created!");
        }

        // Step 9: Start recording
        // OBS will record whatever is currently shown in the program scene
        Console.WriteLine();
        Console.WriteLine("Starting recording...");
        await client.StartRecordAsync(cancellationToken);

        // Step 10: Wait for 2 seconds while recording
        Console.WriteLine("Recording for 2 seconds...");
        await Task.Delay(2000, cancellationToken);

        // Step 11: Stop recording
        // StopRecordAsync returns the path to the recorded file
        Console.WriteLine("Stopping recording...");
        var stopResponse = await client.StopRecordAsync(cancellationToken);
        Console.WriteLine($"Recording saved to: {stopResponse?.OutputPath}");

        // Step 12: Cleanup - restore original scene and delete our test scene
        Console.WriteLine();
        Console.WriteLine("Cleaning up...");

        // Switch back to the original scene before deleting ours
        if (!string.IsNullOrEmpty(originalSceneName))
        {
            await client.SetCurrentProgramSceneAsync(originalSceneName, null, cancellationToken);
            Console.WriteLine($"Restored scene: {originalSceneName}");
        }

        // RemoveSceneAsync deletes the scene and all its scene items
        // Note: This doesn't delete the inputs themselves, just removes them from the scene
        await client.RemoveSceneAsync(sceneName, null, cancellationToken);
        Console.WriteLine($"Deleted scene: {sceneName}");

        Console.WriteLine();
        Console.WriteLine("Done!");

        return 0;
    }
}
