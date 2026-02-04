#region

using Obs.Generator.Models;
using Obs.Generator.Utilities;
using Scriban;

#endregion

namespace Obs.Generator.Generators;

/// <summary>
///     Generates request and response C# classes from OBS WebSocket API definitions
/// </summary>
public class RequestGenerator
{
    private readonly Template _template;

    public RequestGenerator(string templatePath)
    {
        var templateContent = File.ReadAllText(templatePath);
        _template = Template.Parse(templateContent);

        if (_template.HasErrors)
            throw new InvalidOperationException(
                $"Template parsing failed: {string.Join(", ", _template.Messages)}");
    }

    /// <summary>
    ///     Generates C# code for a request
    /// </summary>
    public string Generate(ObsWsRequest request)
    {
        // Prepare the model with all necessary data
        var model = new
        {
            description = SanitizeXmlComment(request.Description),
            request_type = request.RequestType,
            request_fields = request.RequestFields.Select(f => new
            {
                value_name = f.ValueName,
                value_description = SanitizeXmlComment(f.ValueDescription),
                value_optional = f.ValueOptional,
                value_optional_behavior = SanitizeCSharpStringLiteral(f.ValueOptionalBehavior),
                clr_type = TypeMapper.MapObsTypeToCSharp(f.ValueType, f.ValueOptional, f.ValueName),
                property_name = ResolvePropertyName(f.ValueName)
            }).ToArray(),
            response_fields = request.ResponseFields.Select(f => new
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
    ///     Resolves property name, handling conflicts with static RequestType property
    /// </summary>
    private static string ResolvePropertyName(string valueName)
    {
        var propertyName = NamingHelper.SanitizeIdentifier(
            NamingHelper.ToPascalCase(valueName));

        // If property name conflicts with the static RequestType, rename to SubRequestType
        if (propertyName == "RequestType") return "SubRequestType";

        return propertyName;
    }

    /// <summary>
    ///     Generates all request files from the API definition, organized by category subdirectories
    /// </summary>
    public void GenerateAll(ObsWsApiDefinition apiDefinition, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        foreach (var request in apiDefinition.Requests)
        {
            var code = Generate(request);
            var fileName = $"{request.RequestType}Request.cs";

            // Create subdirectory based on category
            var categoryDir = SanitizeCategoryName(request.Category);
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
            return string.Join("\n/// ", lines);

        return text;
    }

    /// <summary>
    ///     Sanitizes text for C# string literals (escapes quotes, backslashes, and removes backticks)
    /// </summary>
    private static string SanitizeCSharpStringLiteral(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // Remove or replace backticks (markdown code formatting) since they're invalid in C#
        text = text.Replace("`", "");

        // Escape backslashes and quotes for C# string literals
        text = text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

        return text;
    }
}