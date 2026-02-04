using System.Globalization;
using System.Text.Json;
using Obs.NET.Enums;
using Spectre.Console;

namespace Obs.NET.Example.Examples;

/// <summary>
/// Demonstrates connecting to OBS, recording, creating text sources, and event handling
/// </summary>
public class ConnectionAndSetupDemo : IExample
{
    public string Name => "connection";
    public string Description => "Connection test with recording, text source creation, and event handling";

    public async Task<int> RunAsync(ExampleOptions options, CancellationToken cancellationToken)
    {
        // Display header
        Panel panel = new(new Markup("[bold blue]OBS WebSocket Connection Test[/]"))
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0)
        };
        AnsiConsole.Write(panel);

        AnsiConsole.MarkupLine("[cyan]Connection Settings:[/]");
        AnsiConsole.MarkupLine($"  - Host: [yellow]{options.Host}[/]");
        AnsiConsole.MarkupLine($"  - Port: [yellow]{options.Port}[/]");
        AnsiConsole.MarkupLine(
            $"  - Authentication: [yellow]{(!string.IsNullOrEmpty(options.Password) ? "Enabled" : "Disabled")}[/]");
        AnsiConsole.MarkupLine($"  - Verbose: [yellow]{options.Verbose}[/]");
        AnsiConsole.WriteLine();

        // Configure the OBS WebSocket client
        ObsWsClientOptions clientOptions = new()
        {
            Host = options.Host,
            Port = options.Port,
            Password = options.Password,
            RequestTimeout = TimeSpan.FromSeconds(10),
            EventSubscriptions = EventSubscription.General | EventSubscription.Scenes | EventSubscription.Outputs | EventSubscription.Inputs
        };

        await using ObsWsClient client = new(clientOptions);
        // Set up event monitoring
        List<string> eventMessages = [];

        // Subscribe to recording state changes
        client.RecordStateChangedEventReceived += async recordEvent =>
        {
            var message = "[red]Recording state changed, but the state was not able to be parsed[/]";
            if (Enum.TryParse<ObsOutputState>(recordEvent.OutputState, out var outputState))
            {
                message = outputState switch
                {
                    ObsOutputState.OBS_WEBSOCKET_OUTPUT_STARTING => "[yellow]Recording starting...[/]",
                    ObsOutputState.OBS_WEBSOCKET_OUTPUT_STARTED =>
                        $"[yellow]Recording started - File: {recordEvent.OutputPath}[/]",
                    ObsOutputState.OBS_WEBSOCKET_OUTPUT_STOPPING => "[yellow]Recording stopping...[/]",
                    ObsOutputState.OBS_WEBSOCKET_OUTPUT_STOPPED =>
                        $"[yellow]Recording stopped - File: {recordEvent.OutputPath}[/]",
                    ObsOutputState.OBS_WEBSOCKET_OUTPUT_PAUSED => "[yellow]Recording paused[/]",
                    ObsOutputState.OBS_WEBSOCKET_OUTPUT_RESUMED => "[yellow]Recording resumed[/]",
                    _ => $"[blue]Recording state changed - State: {recordEvent.OutputState}[/]"
                };
            }

            eventMessages.Add(message);
            AnsiConsole.MarkupLine($"[dim]Event: {message}[/]");
            await Task.CompletedTask;
        };

        // Subscribe to scene changes
        client.CurrentProgramSceneChangedEventReceived += async sceneEvent =>
        {
            var message = $"[cyan]Scene changed to: {sceneEvent.SceneName}[/]";
            eventMessages.Add(message);
            AnsiConsole.MarkupLine($"[dim]Event: {message}[/]");
            await Task.CompletedTask;
        };

        // Subscribe to input creation/removal
        client.InputCreatedEventReceived += async inputEvent =>
        {
            var message = $"[blue]Input created: {inputEvent.InputName} (Kind: {inputEvent.InputKind})[/]";
            eventMessages.Add(message);
            AnsiConsole.MarkupLine($"[dim]Event: {message}[/]");
            await Task.CompletedTask;
        };

        client.InputRemovedEventReceived += async inputEvent =>
        {
            var message = $"[yellow]Input removed: {inputEvent.InputName}[/]";
            eventMessages.Add(message);
            AnsiConsole.MarkupLine($"[dim]Event: {message}[/]");
            await Task.CompletedTask;
        };

        // Connection state monitoring if verbose
        if (options.Verbose)
        {
            client.ConnectionStateChanged += async state =>
            {
                AnsiConsole.MarkupLine($"[dim]Connection state changed: [yellow]{state}[/][/]");
                await Task.CompletedTask;
            };
        }

        // Connect to OBS
        await AnsiConsole.Status()
            .StartAsync("Connecting to OBS WebSocket server...", async ctx =>
            {
                await client.ConnectAsync(cancellationToken);
                ctx.Status("Connected! Verifying connection...");
                await Task.Delay(500, cancellationToken);
            });

        AnsiConsole.MarkupLine("[green]Successfully connected to OBS WebSocket server[/]");
        AnsiConsole.WriteLine();

        // Test 1: Get Version Information
        await AnsiConsole.Status()
            .StartAsync("Getting OBS version information...", async ctx =>
            {
                var versionResponse = await client.GetVersionAsync(cancellationToken);

                ctx.Status("Version information retrieved");
                await Task.Delay(300, cancellationToken);

                AnsiConsole.MarkupLine(
                    $"[green]OBS Version: [yellow]{versionResponse?.ObsVersion ?? "Unknown"}[/][/]");
                AnsiConsole.MarkupLine(
                    $"[green]WebSocket Version: [yellow]{versionResponse?.ObsWebSocketVersion ?? "Unknown"}[/][/]");
                AnsiConsole.MarkupLine(
                    $"[green]RPC Version: [yellow]{versionResponse?.RpcVersion.ToString(CultureInfo.InvariantCulture) ?? "Unknown"}[/][/]");
            });
        AnsiConsole.WriteLine();

        // Test 2: Get and Set Record Directory
        var originalRecordDirectory = string.Empty;
        var testRecordDirectory = Path.Combine(Path.GetTempPath(), "obs-test-recordings",
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

        await AnsiConsole.Status()
            .StartAsync("Configuring record directory...", async ctx =>
            {
                // Get current record directory
                var getRecordDirResponse = await client.GetRecordDirectoryAsync(cancellationToken);
                originalRecordDirectory = getRecordDirResponse?.RecordDirectory ?? string.Empty;

                ctx.Status($"Current directory: {originalRecordDirectory}");
                await Task.Delay(300, cancellationToken);

                // Create test directory
                Directory.CreateDirectory(testRecordDirectory);

                // Set new record directory
                ctx.Status($"Setting test directory: {testRecordDirectory}");
                await client.SetRecordDirectoryAsync(testRecordDirectory, cancellationToken);

                await Task.Delay(300, cancellationToken);
            });

        AnsiConsole.MarkupLine($"[green]Original record directory: [yellow]{originalRecordDirectory}[/][/]");
        AnsiConsole.MarkupLine($"[green]Test record directory set to: [yellow]{testRecordDirectory}[/][/]");
        AnsiConsole.WriteLine();

        // Test 3: Start Recording
        await AnsiConsole.Status()
            .StartAsync("Starting recording...", async ctx =>
            {
                await client.StartRecordAsync(cancellationToken);

                ctx.Status("Recording started");
                await Task.Delay(500, cancellationToken);
            });

        AnsiConsole.MarkupLine("[green]Recording started successfully[/]");

        // Test 4: Get current scene and add text source
        string? currentSceneName = null;
        var textSourceName = $"TestText_{DateTime.UtcNow:HHmmss}";
        var textSourceCreated = false;
        Exception? textSourceException = null;

        // Get current program scene first
        await AnsiConsole.Status()
            .StartAsync("Getting current scene...", async ctx =>
            {
                var sceneResponse = await client.GetCurrentProgramSceneAsync(cancellationToken);
                currentSceneName = sceneResponse?.SceneName;
                ctx.Status($"Current scene: {currentSceneName}");
                await Task.Delay(300, cancellationToken);
            });

        AnsiConsole.MarkupLine($"[green]Current scene: [yellow]{currentSceneName ?? "Unknown"}[/][/]");

        // Query available input kinds to find the correct text source type
        string? inputKind = null;
        await AnsiConsole.Status()
            .StartAsync("Finding available text input kinds...", async _ =>
            {
                var kindsResponse = await client.GetInputKindListAsync(false, cancellationToken);
                if (kindsResponse?.InputKinds != null)
                {
                    var kinds = kindsResponse.InputKinds;
                    AnsiConsole.MarkupLine($"[dim]Available input kinds: {string.Join(", ", kinds.Take(20))}...[/]");

                    // Find a text input kind - try various common names
                    string[] textKindPreferences = OperatingSystem.IsWindows()
                        ? ["text_gdiplus_v3", "text_gdiplus_v2", "text_gdiplus"]
                        : ["text_ft2_source_v3", "text_ft2_source_v2", "text_ft2_source"];

                    foreach (var preferredKind in textKindPreferences)
                    {
                        if (kinds.Contains(preferredKind))
                        {
                            inputKind = preferredKind;
                            break;
                        }
                    }

                    // If not found, try to find any text-related kind
                    if (inputKind == null)
                    {
                        inputKind = kinds.FirstOrDefault(k => k.Contains("text"));
                    }

                    AnsiConsole.MarkupLine($"[dim]Selected text input kind: {inputKind ?? "none found"}[/]");
                }
            });

        if (string.IsNullOrEmpty(inputKind))
        {
            AnsiConsole.MarkupLine("[yellow]Could not find a text input kind - skipping text source creation[/]");
        }
        else
        {
            // Create text source with current UTC time
            var currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");

            // Configure text settings based on the input kind
            Dictionary<string, object> textSettings = new()
            {
                ["text"] = $"Hello {currentTime}"
            };

            // Add font settings for GDI+ sources
            if (inputKind.Contains("gdiplus"))
            {
                textSettings["font"] = new Dictionary<string, object>
                {
                    ["face"] = "Arial",
                    ["size"] = 72,
                    ["flags"] = 0
                };
            }

            AnsiConsole.MarkupLine($"[dim]Creating text source: {textSourceName} (kind: {inputKind})[/]");

            // Log the request details
            AnsiConsole.MarkupLine($"[dim]  Scene: {Markup.Escape(currentSceneName ?? "null")}[/]");
            AnsiConsole.MarkupLine($"[dim]  Input Kind: {inputKind}[/]");

            var inputSettingsJson = JsonSerializer.SerializeToElement(textSettings);
            AnsiConsole.MarkupLine($"[dim]  Settings: {inputSettingsJson}[/]");

            await AnsiConsole.Status()
                .StartAsync($"Creating text source: {textSourceName}...", async ctx =>
                {
                    try
                    {
                        var createResponse = await client.CreateInputAsync(
                            textSourceName,
                            inputKind,
                            currentSceneName,
                            null,
                            inputSettingsJson,
                            true,
                            cancellationToken);

                        if (createResponse != null)
                        {
                            AnsiConsole.MarkupLine($"[dim]  Response: InputUuid={createResponse.InputUuid}, SceneItemId={createResponse.SceneItemId}[/]");

                            textSourceCreated = true;
                            ctx.Status("Text source created! Moving to top...");

                            // Move the text source to the top of the scene (index 0) so it's visible
                            await client.SetSceneItemIndexAsync(
                                createResponse.SceneItemId,
                                0, // Index 0 = top of the list (rendered last, appears on top)
                                currentSceneName,
                                null,
                                cancellationToken);

                            AnsiConsole.MarkupLine($"[dim]  Moved scene item {createResponse.SceneItemId} to index 0[/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]  CreateInputAsync returned null response[/]");
                        }

                        await Task.Delay(300, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        textSourceException = ex;
                    }
                });

            if (textSourceCreated)
            {
                AnsiConsole.MarkupLine($"[green]Added text source: [yellow]{textSourceName}[/] (moved to top)[/]");
                AnsiConsole.MarkupLine(
                    $"[green]Text displays: [yellow]Hello {currentTime}[/][/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Failed to create text source (continuing without it)[/]");
                if (textSourceException != null)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(textSourceException.Message)}[/]");
                    // Always show the exception for debugging
                    AnsiConsole.WriteException(textSourceException);
                }
            }
        }

        // Record for 10 seconds with text visible the entire time
        await AnsiConsole.Status()
            .StartAsync("Recording with text visible...", async ctx =>
            {
                for (var i = 10; i > 0; i--)
                {
                    ctx.Status($"Recording{(textSourceCreated ? " with text visible" : "")}... {i} seconds remaining");
                    await Task.Delay(1000, cancellationToken);
                }
            });

        AnsiConsole.MarkupLine("[green]Recording completed (10 seconds total)[/]");

        // Test 5: Check Recording Status
        await AnsiConsole.Status()
            .StartAsync("Checking recording status...", async _ =>
            {
                var statusResponse = await client.GetRecordStatusAsync(cancellationToken);

                AnsiConsole.MarkupLine(
                    $"[green]Recording active: [yellow]{statusResponse?.OutputActive.ToString() ?? "Unknown"}[/][/]");
                AnsiConsole.MarkupLine(
                    $"[green]Recording paused: [yellow]{statusResponse?.OutputPaused.ToString() ?? "Unknown"}[/][/]");
                AnsiConsole.MarkupLine(
                    $"[green]Timecode: [yellow]{statusResponse?.OutputTimecode ?? "Unknown"}[/][/]");
                AnsiConsole.MarkupLine(
                    $"[green]Duration: [yellow]{statusResponse?.OutputDuration.ToString(CultureInfo.InvariantCulture) ?? "Unknown"}ms[/][/]");

                await Task.Delay(300, cancellationToken);
            });
        AnsiConsole.WriteLine();

        // Remove text source just before stopping
        if (textSourceCreated)
        {
            await AnsiConsole.Status()
                .StartAsync("Removing text source...", async _ =>
                {
                    try
                    {
                        await client.RemoveInputAsync(textSourceName, null, cancellationToken);
                        await Task.Delay(300, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        if (options.Verbose)
                        {
                            AnsiConsole.MarkupLine($"[yellow]Failed to remove text source: {ex.Message}[/]");
                        }
                    }
                });

            AnsiConsole.MarkupLine("[green]Text source removed[/]");
        }

        // Test 6: Stop Recording
        string? recordedFilePath = null;
        await AnsiConsole.Status()
            .StartAsync("Stopping recording...", async ctx =>
            {
                var stopResponse = await client.StopRecordAsync(cancellationToken);

                recordedFilePath = stopResponse?.OutputPath;

                ctx.Status("Recording stopped");
                await Task.Delay(500, cancellationToken);
            });

        AnsiConsole.MarkupLine("[green]Recording stopped successfully[/]");
        if (!string.IsNullOrEmpty(recordedFilePath))
        {
            AnsiConsole.MarkupLine($"[green]Recorded file saved to: [yellow]{recordedFilePath}[/][/]");

            if (File.Exists(recordedFilePath))
            {
                FileInfo fileInfo = new(recordedFilePath);
                AnsiConsole.MarkupLine($"[green]File size: [yellow]{fileInfo.Length:N0} bytes[/][/]");
            }
        }

        AnsiConsole.WriteLine();

        // Test 7: Restore Original Record Directory
        try
        {
            await AnsiConsole.Status()
                .StartAsync("Restoring original record directory...", async _ =>
                {
                    await client.SetRecordDirectoryAsync(originalRecordDirectory, cancellationToken);
                    await Task.Delay(300, cancellationToken);
                });

            AnsiConsole.MarkupLine($"[green]Record directory restored to: [yellow]{originalRecordDirectory}[/][/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Could not restore original record directory: {Markup.Escape(ex.Message)}[/]");
        }

        // Test 8: Get Stats
        await AnsiConsole.Status()
            .StartAsync("Getting OBS statistics...", async _ =>
            {
                var statsResponse = await client.GetStatsAsync(cancellationToken);

                AnsiConsole.MarkupLine(
                    $"[green]CPU Usage: [yellow]{statsResponse?.CpuUsage.ToString("F2") ?? "Unknown"}%[/][/]");
                AnsiConsole.MarkupLine(
                    $"[green]Memory Usage: [yellow]{statsResponse?.MemoryUsage.ToString("F2") ?? "Unknown"} MB[/][/]");
                AnsiConsole.MarkupLine(
                    $"[green]Active FPS: [yellow]{statsResponse?.ActiveFps.ToString("F2") ?? "Unknown"}[/][/]");
                AnsiConsole.MarkupLine(
                    $"[green]Render Total Frames: [yellow]{statsResponse?.RenderTotalFrames.ToString(CultureInfo.InvariantCulture) ?? "Unknown"}[/][/]");

                await Task.Delay(300, cancellationToken);
            });
        AnsiConsole.WriteLine();

        // Test 9: Event Summary
        if (eventMessages.Count > 0)
        {
            AnsiConsole.WriteLine();
            var eventTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn(new TableColumn("[yellow]Events Received During Test[/]").Centered());

            AnsiConsole.MarkupLine($"[cyan]Total events received: [yellow]{eventMessages.Count}[/][/]");
            AnsiConsole.WriteLine();

            foreach (var eventMessage in eventMessages)
            {
                eventTable.AddRow(eventMessage);
            }

            AnsiConsole.Write(eventTable);
        }

        // Summary
        AnsiConsole.WriteLine();
        Panel successPanel =
            new(new Markup(
                "[bold green]Connection and setup demo completed successfully![/]\n\n[dim]The OBS WebSocket client library is working correctly and can communicate with OBS Studio.[/]\n[dim]Event handlers are properly receiving and processing OBS events.[/]"))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green"),
                Padding = new Padding(1, 0)
            };
        AnsiConsole.Write(successPanel);

        return 0;
    }
}
