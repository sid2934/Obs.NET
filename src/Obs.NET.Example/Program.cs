using Obs.NET.Example.Examples;
using Spectre.Console;

namespace Obs.NET.Example;

internal class Program
{
    // Register all available examples
    private static readonly Dictionary<string, IExample> Examples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["connection"] = new ConnectionAndSetupDemo(),
        ["simple"] = new SimpleRecordingDemo()
    };

    public static async Task<int> Main(string[] args)
    {
        // Parse command line arguments
        var host = GetArgument(args, "--host", "-h") ?? "localhost";
        var port = int.TryParse(GetArgument(args, "--port", "-p"), out var p) ? p : 4455;
        var password = GetArgument(args, "--password");
        var verbose = HasFlag(args, "--verbose", "-v");
        var exampleName = GetArgument(args, "--example", "-e") ?? "connection";

        if (HasFlag(args, "--help"))
        {
            PrintHelp();
            return 0;
        }

        if (HasFlag(args, "--list-examples"))
        {
            ListExamples();
            return 0;
        }

        // Find the requested example
        if (!Examples.TryGetValue(exampleName, out var example))
        {
            AnsiConsole.MarkupLine($"[red]Unknown example: {Markup.Escape(exampleName)}[/]");
            AnsiConsole.MarkupLine("[yellow]Available examples:[/]");
            foreach (var ex in Examples.Values)
            {
                AnsiConsole.MarkupLine($"  [cyan]{ex.Name}[/] - {ex.Description}");
            }
            return 1;
        }

        // Create options for the example
        var options = new ExampleOptions
        {
            Host = host,
            Port = port,
            Password = password,
            Verbose = verbose
        };

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            // Run the example
            return await example.RunAsync(options, cts.Token);
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Operation cancelled by user[/]");
            return 1;
        }
        catch (TimeoutException ex)
        {
            AnsiConsole.MarkupLine($"[red]Connection timeout: {ex.Message}[/]");
            AnsiConsole.MarkupLine("[yellow]Make sure OBS Studio is running and WebSocket server is enabled.[/]");
            AnsiConsole.MarkupLine("[dim]In OBS: Tools -> WebSocket Server Settings[/]");

            if (verbose)
            {
                AnsiConsole.WriteException(ex);
            }

            return 1;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("connect"))
        {
            AnsiConsole.MarkupLine($"[red]Connection failed: {ex.Message}[/]");
            AnsiConsole.MarkupLine("[yellow]Troubleshooting steps:[/]");
            AnsiConsole.MarkupLine("  1. Ensure OBS Studio is running");
            AnsiConsole.MarkupLine("  2. Enable WebSocket server in OBS (Tools -> WebSocket Server Settings)");
            AnsiConsole.MarkupLine($"  3. Verify host ({host}) and port ({port}) are correct");
            AnsiConsole.MarkupLine("  4. Check if authentication is required and password is correct");

            if (verbose)
            {
                AnsiConsole.WriteException(ex);
            }

            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Example failed: {ex.Message}[/]");

            if (verbose)
            {
                AnsiConsole.WriteException(ex);
            }

            return 1;
        }
    }

    private static void PrintHelp()
    {
        AnsiConsole.MarkupLine("[bold]Obs.NET Example - OBS WebSocket Client Demonstrations[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Usage:[/]");
        AnsiConsole.MarkupLine("  dotnet run -- [[options]]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Options:[/]");
        AnsiConsole.MarkupLine("  -h, --host [grey]<host>[/]       OBS host (default: localhost)");
        AnsiConsole.MarkupLine("  -p, --port [grey]<port>[/]       OBS port (default: 4455)");
        AnsiConsole.MarkupLine("  --password [grey]<password>[/]   OBS WebSocket password");
        AnsiConsole.MarkupLine("  -e, --example [grey]<name>[/]    Example to run (default: connection)");
        AnsiConsole.MarkupLine("  --list-examples         List all available examples");
        AnsiConsole.MarkupLine("  -v, --verbose           Enable verbose output");
        AnsiConsole.MarkupLine("  --help                  Show this help message");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Available Examples:[/]");
        foreach (var example in Examples.Values)
        {
            AnsiConsole.MarkupLine($"  [cyan]{example.Name}[/] - {example.Description}");
        }
    }

    private static void ListExamples()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[yellow]Name[/]").LeftAligned())
            .AddColumn(new TableColumn("[yellow]Description[/]").LeftAligned());

        foreach (var example in Examples.Values)
        {
            table.AddRow($"[cyan]{example.Name}[/]", example.Description);
        }

        AnsiConsole.Write(table);
    }

    private static string? GetArgument(string[] args, params string[] names)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (names.Contains(args[i]))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    private static bool HasFlag(string[] args, params string[] names)
    {
        return args.Any(a => names.Contains(a));
    }
}
