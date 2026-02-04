#region

using Obs.Generator.Models;
using Obs.Generator.Utilities;
using Scriban;

#endregion

namespace Obs.Generator.Generators;

/// <summary>
///     Generates event handler declarations and dispatching logic for ObsWsClient
/// </summary>
public class EventHandlerGenerator
{
    private readonly Template _template;

    public EventHandlerGenerator(string templatePath)
    {
        var templateContent = File.ReadAllText(templatePath);
        _template = Template.Parse(templateContent);

        if (_template.HasErrors)
            throw new InvalidOperationException(
                $"Template parsing failed: {string.Join(", ", _template.Messages)}");
    }

    /// <summary>
    ///     Generates the event handlers partial class for ObsWsClient
    /// </summary>
    public void GenerateEventHandlers(ObsWsApiDefinition apiDefinition, string outputPath)
    {
        // Group events by category for organization
        var eventsByCategory = apiDefinition.Events
            .GroupBy(e => e.Category)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                category = g.Key,
                events = g.Select(e => new
                {
                    event_type = e.EventType,
                    event_name = NamingHelper.ToPascalCase(e.EventType),
                    description = SanitizeXmlComment(e.Description),
                    event_class = $"{e.EventType}Event"
                }).ToArray()
            }).ToArray();

        var model = new
        {
            categories = eventsByCategory,
            all_events = apiDefinition.Events.Select(e => new
            {
                event_type = e.EventType,
                event_name = NamingHelper.ToPascalCase(e.EventType),
                event_class = $"{e.EventType}Event"
            }).ToArray()
        };

        var generatedCode = _template.Render(model);
        File.WriteAllText(outputPath, generatedCode);
    }

    private static string SanitizeXmlComment(string comment)
    {
        return comment
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("\n", "\n    /// ");
    }
}