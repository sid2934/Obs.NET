#region

using Obs.Generator.Models;
using Obs.Generator.Utilities;
using Scriban;

#endregion

namespace Obs.Generator.Generators;

/// <summary>
///     Generates enum C# classes from OBS WebSocket API definitions
/// </summary>
public class EnumGenerator
{
    private readonly Template _template;

    public EnumGenerator(string templatePath)
    {
        var templateContent = File.ReadAllText(templatePath);
        _template = Template.Parse(templateContent);

        if (_template.HasErrors)
            throw new InvalidOperationException(
                $"Template parsing failed: {string.Join(", ", _template.Messages)}");
    }

    /// <summary>
    ///     Generates C# code for an enum
    /// </summary>
    public string Generate(ObsWsEnum obsEnum)
    {
        // Check if this is a flags enum by looking at the enum name or if values are powers of 2
        var isFlags = obsEnum.EnumType.Contains("Flag") ||
                      obsEnum.EnumType.Contains("Subscription") ||
                      IsFlagsEnum(obsEnum.EnumIdentifiers);

        var model = new
        {
            description = $"OBS WebSocket enum: {obsEnum.EnumType}",
            enum_type = NamingHelper.SanitizeIdentifier(obsEnum.EnumType),
            is_flags = isFlags,
            identifiers = obsEnum.EnumIdentifiers.Select(i => new
            {
                name = NamingHelper.SanitizeIdentifier(i.EnumIdentifier),
                description = SanitizeXmlComment(i.Description),
                value = i.EnumValue
            }).ToArray()
        };

        return _template.Render(model);
    }

    /// <summary>
    ///     Determines if an enum should be a flags enum based on its values
    /// </summary>
    private static bool IsFlagsEnum(ObsWsEnumIdentifier[] identifiers)
    {
        // Check if most values are powers of 2 (indicating flags)
        var nonZeroValues = identifiers.Where(i => i.EnumValue != 0).ToArray();
        if (nonZeroValues.Length < 2) return false;

        var powerOfTwoCount = nonZeroValues.Count(i =>
            i.EnumValue > 0 && (i.EnumValue & (i.EnumValue - 1)) == 0);

        return powerOfTwoCount >= nonZeroValues.Length * 0.7; // 70% threshold
    }

    /// <summary>
    ///     Generates all enum files from the API definition
    /// </summary>
    public void GenerateAll(ObsWsApiDefinition apiDefinition, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        foreach (var obsEnum in apiDefinition.Enums)
        {
            var code = Generate(obsEnum);
            var fileName = $"{obsEnum.EnumType}.cs";
            var filePath = Path.Combine(outputDirectory, fileName);

            File.WriteAllText(filePath, code);
            Console.WriteLine($"Generated: {fileName}");
        }
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