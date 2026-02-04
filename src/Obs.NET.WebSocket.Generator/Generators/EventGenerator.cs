#region

using Obs.Generator.Models;
using Obs.Generator.Utilities;
using Scriban;

#endregion

namespace Obs.Generator.Generators;

/// <summary>
///     Generates event C# classes from OBS WebSocket API definitions
/// </summary>
public class EventGenerator
{
    private readonly Template _template;

    public EventGenerator(string templatePath)
    {
        var templateContent = File.ReadAllText(templatePath);
        _template = Template.Parse(templateContent);

        if (_template.HasErrors)
            throw new InvalidOperationException(
                $"Template parsing failed: {string.Join(", ", _template.Messages)}");
    }

    /// <summary>
    ///     Generates C# code for an event
    /// </summary>
    public string Generate(ObsWsEvent obsEvent)
    {
        var model = new
        {
            description = SanitizeXmlComment(obsEvent.Description),
            event_type = obsEvent.EventType,
            event_subscription = obsEvent.EventSubscription,
            data_fields = obsEvent.DataFields.Select(f => new
            {
                value_name = f.ValueName,
                value_description = SanitizeXmlComment(f.ValueDescription),
                clr_type = TypeMapper.MapObsTypeToCSharp(f.ValueType, false, f.ValueName),
                property_name = ResolvePropertyName(f.ValueName)
            }).ToArray()
        };

        return _template.Render(model);
    }

    /// <summary>
    ///     Resolves property name, handling conflicts with static EventType property
    /// </summary>
    private static string ResolvePropertyName(string valueName)
    {
        var propertyName = NamingHelper.SanitizeIdentifier(
            NamingHelper.ToPascalCase(valueName));

        // If property name conflicts with the static EventType, rename to SubEventType
        if (propertyName == "EventType") return "SubEventType";

        return propertyName;
    }

    /// <summary>
    ///     Generates all event files from the API definition, organized by category subdirectories
    /// </summary>
    public void GenerateAll(ObsWsApiDefinition apiDefinition, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        foreach (var obsEvent in apiDefinition.Events)
        {
            var code = Generate(obsEvent);
            var fileName = $"{obsEvent.EventType}Event.cs";

            // Create subdirectory based on category
            var categoryDir = SanitizeCategoryName(obsEvent.Category);
            var categoryPath = Path.Combine(outputDirectory, categoryDir);

            if (!Directory.Exists(categoryPath)) Directory.CreateDirectory(categoryPath);

            var filePath = Path.Combine(categoryPath, fileName);

            File.WriteAllText(filePath, code);
            Console.WriteLine($"Generated: {categoryDir}/{fileName}");
        }
    }

    /// <summary>
    ///     Sanitizes category name to create a valid directory name in PascalCase
    /// </summary>
    private static string SanitizeCategoryName(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) return "General";

        // Convert to PascalCase and remove any invalid characters
        return NamingHelper.SanitizeIdentifier(
            NamingHelper.ToPascalCase(category));
    }

    /// <summary>
    ///     Sanitizes text for XML documentation comments and handles multi-line descriptions
    /// </summary>
    private static string SanitizeXmlComment(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // First, escape XML special characters
        text = text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");

        // Handle multi-line descriptions by adding proper line breaks with XML comment formatting
        // Split on various line break formats
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length > 1)
            // Join lines with proper XML comment line breaks
            return string.Join("\n    /// ", lines);

        return text;
    }
}