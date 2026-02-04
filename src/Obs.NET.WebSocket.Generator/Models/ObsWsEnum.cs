#region

using System.Text.Json.Serialization;
using Obs.Generator.Converters;

#endregion

namespace Obs.Generator.Models;

[JsonConverter(typeof(ObsWsEnumConverter))]
public class ObsWsEnum
{
    [JsonPropertyName("enumType")]
    public required string EnumType { get; set; }

    [JsonPropertyName("enumIdentifiers")]
    public ObsWsEnumIdentifier[] EnumIdentifiers { get; set; } = [];
}