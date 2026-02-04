#region

using Obs.Generator.Models;
using Obs.Generator.Utilities;
using Scriban;

#endregion

namespace Obs.Generator.Generators;

/// <summary>
///     Generates strongly-typed extension methods for ObsWsClient from OBS WebSocket API definitions
/// </summary>
public class ExtensionMethodsGenerator
{
    private readonly Template _template;

    public ExtensionMethodsGenerator(string templatePath)
    {
        var templateContent = File.ReadAllText(templatePath);
        _template = Template.Parse(templateContent);

        if (_template.HasErrors)
            throw new InvalidOperationException(
                $"Template parsing failed: {string.Join(", ", _template.Messages)}");
    }

    /// <summary>
    ///     Generates extension methods file from the API definition
    /// </summary>
    public void GenerateExtensionMethods(ObsWsApiDefinition apiDefinition, string outputFilePath)
    {
        // Group requests by category
        var categorizedRequests = apiDefinition.Requests
            .GroupBy(r => r.Category)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                name = SanitizeCategoryName(g.Key),
                requests = g.Select(r => new
                {
                    description = SanitizeXmlComment(r.Description),
                    request_type = $"{r.RequestType}Request",
                    response_type = $"{r.RequestType}Response",
                    method_name = r.RequestType,
                    response_description = $"Response data from {r.RequestType}",
                    has_request_fields = r.RequestFields.Length > 0,
                    request_fields = r.RequestFields
                        .OrderBy(f => f.ValueOptional) // Required first, optional last
                        .Select(f =>
                        {
                            var clrType = TypeMapper.MapObsTypeToCSharpForParameter(f.ValueType, f.ValueOptional, f.ValueName);
                            return new
                            {
                                parameter_name = NamingHelper.ToCamelCase(f.ValueName),
                                property_name = ResolvePropertyName(f.ValueName),
                                value_description = SanitizeXmlComment(f.ValueDescription),
                                value_optional = f.ValueOptional,
                                clr_type = clrType,
                                default_value = GetDefaultValue(clrType, f.ValueOptional)
                            };
                        }).ToArray()
                }).ToArray()
            }).ToArray();

        var model = new { categories = categorizedRequests };

        var code = _template.Render(model);

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        File.WriteAllText(outputFilePath, code);
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
    ///     Gets the appropriate default value for optional parameters based on CLR type
    /// </summary>
    private static string GetDefaultValue(string clrType, bool isOptional)
    {
        if (!isOptional) return string.Empty; // Not used for required parameters

        // Check if the type is nullable (ends with ? or is explicitly string?)
        // Nullable types: string?, Type?, etc.
        // Non-nullable types that are optional shouldn't have a default in extension methods

        if (clrType.EndsWith("?")) return " = null";

        // For non-nullable optional types (shouldn't happen with proper TypeMapper usage)
        // Don't provide a default value
        return string.Empty;
    }

    /// <summary>
    ///     Sanitizes category name to create a valid identifier in PascalCase
    /// </summary>
    private static string SanitizeCategoryName(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) return "General";

        return NamingHelper.SanitizeIdentifier(
            NamingHelper.ToPascalCase(category));
    }

    /// <summary>
    ///     Sanitizes text for XML documentation comments and handles multi-line descriptions
    /// </summary>
    private static string SanitizeXmlComment(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // Escape XML special characters
        text = text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");

        // Handle multi-line descriptions
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length > 1) return string.Join("\n    /// ", lines);

        return text;
    }
}