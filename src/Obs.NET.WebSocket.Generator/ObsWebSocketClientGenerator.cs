#region

using System.Text.Json;
using Obs.Generator.Converters;
using Obs.Generator.Generators;
using Obs.Generator.Models;

#endregion

namespace Obs.Generator;

internal class ObsWebSocketClientGenerator
{
    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Register custom converters for enums
        Converters = { new ObsWsEnumIdentifierConverter(), new ObsWsEnumConverter() }
    };

    private static void Main(string[] args)
    {
        Console.WriteLine("=== OBS WebSocket Client Generator ===\n");

        // Get resources directory from command line or use default
        var resourcesDirectory = args.Length > 0
            ? args[0]
            // Default: use the resources directory in the build output (copied by .csproj)
            : Path.Combine(AppContext.BaseDirectory, "resources");

        if (!Directory.Exists(resourcesDirectory))
        {
            Console.WriteLine($"Error: Resources directory not found at {resourcesDirectory}");
            Console.WriteLine("\nUsage: dotnet run [resources-directory]");
            Console.WriteLine("Example: dotnet run /path/to/resources");
            return;
        }

        Console.WriteLine($"Resources directory: {resourcesDirectory}");

        // 1. Load the protocol.json file
        var docFilePath = Path.Combine(resourcesDirectory, "protocol.json");

        if (!File.Exists(docFilePath))
        {
            Console.WriteLine($"Error: protocol.json not found at {docFilePath}");
            return;
        }

        Console.WriteLine($"Loading API definition from: {docFilePath}");
        var docFileString = File.ReadAllText(docFilePath);

        var apiDefinition = JsonSerializer.Deserialize<ObsWsApiDefinition>(docFileString, JsonOptions);

        if (apiDefinition == null)
        {
            Console.WriteLine("Error: Failed to parse protocol.json. Validate the schema matches the models.");
            return;
        }

        Console.WriteLine(
            $"Loaded: {apiDefinition.Enums.Length} enums, {apiDefinition.Requests.Length} requests, {apiDefinition.Events.Length} events\n");

        // 2. Setup output directories
        var outputBasePath = GetOutputBasePath();
        var enumsOutputPath = Path.Combine(outputBasePath, "Enums");
        var requestsOutputPath = Path.Combine(outputBasePath, "Requests");
        var eventsOutputPath = Path.Combine(outputBasePath, "Events");

        // 3. Generate Enums
        Console.WriteLine("Generating enums...");
        var enumTemplatePath = Path.Combine(resourcesDirectory, "ObsWsEnumTemplate.txt");
        EnumGenerator enumGenerator = new(enumTemplatePath);
        enumGenerator.GenerateAll(apiDefinition, enumsOutputPath);
        Console.WriteLine($"Generated {apiDefinition.Enums.Length} enum files to {enumsOutputPath}\n");

        // 4. Generate Requests
        Console.WriteLine("Generating requests...");
        var requestTemplatePath = Path.Combine(resourcesDirectory, "ObsWsRequestTemplate.txt");
        RequestGenerator requestGenerator = new(requestTemplatePath);
        requestGenerator.GenerateAll(apiDefinition, requestsOutputPath);
        Console.WriteLine($"Generated {apiDefinition.Requests.Length} request files to {requestsOutputPath}\n");

        // 5. Generate Events
        Console.WriteLine("Generating events...");
        var eventTemplatePath = Path.Combine(resourcesDirectory, "ObsWsEventTemplate.txt");
        EventGenerator eventGenerator = new(eventTemplatePath);
        eventGenerator.GenerateAll(apiDefinition, eventsOutputPath);
        Console.WriteLine($"Generated {apiDefinition.Events.Length} event files to {eventsOutputPath}\n");

        // 6. Generate Extension Methods
        Console.WriteLine("Generating extension methods...");
        var extensionTemplatePath = Path.Combine(resourcesDirectory, "ObsWsExtensionMethodsTemplate.txt");
        ExtensionMethodsGenerator extensionGenerator = new(extensionTemplatePath);
        var extensionOutputPath = Path.Combine(outputBasePath, "ObsWsClientExtensions.cs");
        extensionGenerator.GenerateExtensionMethods(apiDefinition, extensionOutputPath);
        Console.WriteLine($"Generated extension methods file: {extensionOutputPath}");

        // 7. Generate Event Handlers
        Console.WriteLine("Generating event handlers...");
        var eventHandlerTemplatePath = Path.Combine(resourcesDirectory, "ObsWsEventHandlersTemplate.txt");
        EventHandlerGenerator eventHandlerGenerator = new(eventHandlerTemplatePath);
        var eventHandlerOutputPath = Path.Combine(outputBasePath, "ObsWsClient.Events.cs");
        eventHandlerGenerator.GenerateEventHandlers(apiDefinition, eventHandlerOutputPath);
        Console.WriteLine($"Generated event handlers file: {eventHandlerOutputPath}\n");

        Console.WriteLine("=== Generation Complete ===");
    }

    private static string GetOutputBasePath()
    {
        // Navigate from current directory to find the solution root (contains src/ and artifacts/)
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current != null && !Directory.Exists(Path.Combine(current.FullName, "src"))) current = current.Parent;

        if (current == null)
            throw new InvalidOperationException(
                $"Could not find solution root from build output path: {AppContext.BaseDirectory}");

        var outputPath = Path.Combine(current.FullName, "src", "Obs.NET", "Generated");

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
            Console.WriteLine($"Created output directory: {outputPath}");
        }

        return outputPath;
    }
}