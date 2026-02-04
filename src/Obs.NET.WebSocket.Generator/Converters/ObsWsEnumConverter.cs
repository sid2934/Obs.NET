#region

using System.Text.Json;
using System.Text.Json.Serialization;
using Obs.Generator.Models;

#endregion

namespace Obs.Generator.Converters;

internal sealed class ObsWsEnumConverter : JsonConverter<ObsWsEnum>
{
    public override ObsWsEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialize the entire object into a JsonElement and let JsonSerializer handle nested deserialization.
        var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
        if (element.ValueKind != JsonValueKind.Object) throw new JsonException("Expected JSON object for ObsWsEnum");

        if (!element.TryGetProperty("enumType", out var enumTypeElem) ||
            enumTypeElem.ValueKind != JsonValueKind.String)
            throw new JsonException("enumType is required and must be a string for ObsWsEnum");

        var enumType = enumTypeElem.GetString()!;
        var identifiers = Array.Empty<ObsWsEnumIdentifier>();

        if (element.TryGetProperty("enumIdentifiers", out var idsElem) &&
            idsElem.ValueKind == JsonValueKind.Array)
        {
            // Begin enum scope so nested ObsWsEnumIdentifierConverter can resolve identifiers in this enumType
            ObsWsEnumIdentifierConverter.BeginEnumScope(enumType);
            try
            {
                // Deserialize the array using JsonSerializer so the ObsWsEnumIdentifierConverter is used
                identifiers = idsElem.Deserialize<ObsWsEnumIdentifier[]>(options) ?? Array.Empty<ObsWsEnumIdentifier>();
            }
            finally
            {
                ObsWsEnumIdentifierConverter.EndEnumScope();
            }
        }

        return new ObsWsEnum { EnumType = enumType, EnumIdentifiers = identifiers };
    }

    public override void Write(Utf8JsonWriter writer, ObsWsEnum value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("enumType", value.EnumType);
        writer.WritePropertyName("enumIdentifiers");
        JsonSerializer.Serialize(writer, value.EnumIdentifiers, options);
        writer.WriteEndObject();
    }
}