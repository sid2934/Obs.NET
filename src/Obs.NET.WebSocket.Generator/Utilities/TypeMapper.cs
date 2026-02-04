namespace Obs.Generator.Utilities;

/// <summary>
///     Maps OBS WebSocket types to C# CLR types
/// </summary>
public static class TypeMapper
{
    /// <summary>
    ///     Mapping of well-known field names to their strongly-typed model classes.
    ///     These models include [JsonExtensionData] for forward compatibility.
    /// </summary>
    private static readonly Dictionary<string, string> WellKnownArrayTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "scenes", "Obs.NET.Models.ObsScene" },
        { "inputs", "Obs.NET.Models.ObsInput" },
        { "sceneItems", "Obs.NET.Models.ObsSceneItem" },
        { "filters", "Obs.NET.Models.ObsFilter" },
        { "outputs", "Obs.NET.Models.ObsOutput" },
        { "monitors", "Obs.NET.Models.ObsMonitor" },
        { "transitions", "Obs.NET.Models.ObsTransition" }
    };

    /// <summary>
    ///     Mapping of well-known field names to their strongly-typed model classes for Object types.
    /// </summary>
    private static readonly Dictionary<string, string> WellKnownObjectTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "sceneItemTransform", "Obs.NET.Models.ObsSceneItemTransform" }
    };

    /// <summary>
    ///     Converts OBS value type to CLR type with optional handling
    /// </summary>
    /// <param name="obsType">The OBS type (String, Number, Boolean, Object, Array, Array&lt;String&gt;, etc.)</param>
    /// <param name="isOptional">Whether the field is optional</param>
    /// <param name="fieldName">Optional field name to enable well-known type mapping</param>
    /// <returns>The C# type string</returns>
    public static string MapObsTypeToCSharp(string obsType, bool isOptional, string? fieldName = null)
    {
        var baseType = MapObsTypeToBaseType(obsType, fieldName);

        // For optional types, make them all nullable (including strings)
        // This allows assigning null from extension methods without warnings
        if (isOptional)
        {
            if (baseType == "string") return "string?";

            return $"{baseType}?";
        }

        return baseType;
    }

    /// <summary>
    ///     Maps OBS type string to base C# type
    /// </summary>
    private static string MapObsTypeToBaseType(string obsType, string? fieldName = null)
    {
        // Handle generic array types like Array<String>, Array<Object>, etc.
        if (obsType.StartsWith("Array<") && obsType.EndsWith(">"))
        {
            var innerType = obsType.Substring(6, obsType.Length - 7); // Extract type between Array< and >

            // Check if this is an Array<Object> with a well-known field name
            if (innerType == "Object" && fieldName != null && WellKnownArrayTypes.TryGetValue(fieldName, out var modelType))
            {
                return $"IEnumerable<{modelType}>";
            }

            var mappedInnerType = MapObsTypeToBaseType(innerType, null);
            return $"IEnumerable<{mappedInnerType}>";
        }

        // Check if this is an Object with a well-known field name
        if (obsType == "Object" && fieldName != null && WellKnownObjectTypes.TryGetValue(fieldName, out var objectModelType))
        {
            return objectModelType;
        }

        return obsType switch
        {
            "String" => "string",
            "Number" => "double",
            "Boolean" => "bool",
            "Object" => "System.Text.Json.JsonElement",
            "Array" => "System.Text.Json.JsonElement", // Untyped array falls back to JsonElement
            "Any" => "System.Text.Json.JsonElement",   // Any type uses JsonElement for flexibility
            _ => "System.Text.Json.JsonElement"        // Unknown types default to JsonElement for safety
        };
    }

    /// <summary>
    ///     Converts OBS value type to CLR type for extension method parameters
    ///     Makes all optional types explicitly nullable (including strings)
    /// </summary>
    /// <param name="obsType">The OBS type (String, Number, Boolean, Object, Array, Array&lt;String&gt;, etc.)</param>
    /// <param name="isOptional">Whether the field is optional</param>
    /// <param name="fieldName">Optional field name to enable well-known type mapping</param>
    /// <returns>The C# type string with explicit nullable annotation for optional types</returns>
    public static string MapObsTypeToCSharpForParameter(string obsType, bool isOptional, string? fieldName = null)
    {
        var baseType = MapObsTypeToBaseType(obsType, fieldName);

        // For extension method parameters, make ALL optional types explicitly nullable
        // This includes strings, so we can use = null safely
        if (isOptional)
        {
            // String needs ? annotation when optional in parameters
            if (baseType == "string") return "string?";

            // Value types and other types get ?
            return $"{baseType}?";
        }

        return baseType;
    }
}