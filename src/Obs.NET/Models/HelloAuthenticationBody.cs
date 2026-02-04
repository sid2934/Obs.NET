using System.Text.Json.Serialization;

namespace Obs.NET.Models;

/// <summary>
/// Authentication challenge data sent by OBS in the Hello message when authentication is required.
/// </summary>
public class HelloAuthenticationBody
{
    /// <summary>
    /// A random string used as part of the authentication hash.
    /// </summary>
    [JsonPropertyName("salt")]
    public required string Salt { get; set; }

    /// <summary>
    /// A random string that must be hashed with the password and salt.
    /// </summary>
    [JsonPropertyName("challenge")]
    public required string Challenge { get; set; }
}