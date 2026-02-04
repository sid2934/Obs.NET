#region

using System.Text.Json.Serialization;
using Obs.Generator.Converters;

#endregion

namespace Obs.Generator.Models;

// Custom converter handles resolving enumValue expressions (numeric, shift, bitwise OR, identifier references)
[JsonConverter(typeof(ObsWsEnumIdentifierConverter))]
public class ObsWsEnumIdentifier
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("enumIdentifier")]
    public string EnumIdentifier { get; set; } = string.Empty;

    [JsonPropertyName("rpcVersion")]
    public string RpcVersion { get; set; } = string.Empty;

    [JsonPropertyName("deprecated")]
    public bool Deprecated { get; set; }

    [JsonPropertyName("initialVersion")]
    public string InitialVersion { get; set; } = string.Empty;

    /// <summary>
    ///     Resolved integer value for this enum identifier. The JSON source may contain:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Integer literal (e.g. <c>5</c>)</description>
    ///         </item>
    ///         <item>
    ///             <description>Shift expression like <c>1 &lt;&lt; 4</c></description>
    ///         </item>
    ///         <item>
    ///             <description>Bitwise OR like <c>FlagA | FlagB</c></description>
    ///         </item>
    ///         <item>
    ///             <description>Combination like <c>1 &lt;&lt; 3 | FlagA</c></description>
    ///         </item>
    ///     </list>
    ///     Identifiers referenced must have been defined earlier in the same JSON array (forward references not supported).
    /// </summary>
    [JsonPropertyName("enumValue")]
    public int EnumValue { get; set; }
}