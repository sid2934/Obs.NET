namespace Obs.Generator.Utilities;

/// <summary>
///     Handles naming conversions and sanitization for C# identifiers
/// </summary>
public static class NamingHelper
{
    /// <summary>
    ///     Converts a camelCase, snake_case, dot-separated, or space-separated string to PascalCase
    /// </summary>
    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        // Handle dot-separated property names (e.g., "keyModifiers.shift" -> "KeyModifiersShift")
        if (input.Contains('.'))
        {
            var parts = input.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(p =>
                char.ToUpperInvariant(p[0]) + p.Substring(1)));
        }

        // Handle space-separated words
        if (input.Contains(' '))
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(p =>
                char.ToUpperInvariant(p[0]) + p.Substring(1).ToLowerInvariant()));
        }

        // Handle snake_case
        if (input.Contains('_'))
        {
            var parts = input.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Select(p =>
                char.ToUpperInvariant(p[0]) + p.Substring(1).ToLowerInvariant()));
        }

        // Handle camelCase
        return char.ToUpperInvariant(input[0]) + input.Substring(1);
    }

    /// <summary>
    ///     Converts a string to camelCase (first character lowercase, rest follows PascalCase rules)
    /// </summary>
    public static string ToCamelCase(string input)
    {
        var pascalCase = ToPascalCase(input);

        if (string.IsNullOrEmpty(pascalCase)) return pascalCase;

        return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
    }

    /// <summary>
    ///     Sanitizes identifiers to avoid C# keywords
    /// </summary>
    public static string SanitizeIdentifier(string input)
    {
        return input switch
        {
            "event" => "@event",
            "string" => "@string",
            "object" => "@object",
            "bool" => "@bool",
            "int" => "@int",
            "double" => "@double",
            "float" => "@float",
            "long" => "@long",
            "short" => "@short",
            "byte" => "@byte",
            "char" => "@char",
            "void" => "@void",
            "class" => "@class",
            "interface" => "@interface",
            "namespace" => "@namespace",
            "using" => "@using",
            "params" => "@params",
            _ => input
        };
    }
}