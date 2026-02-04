#region

using System.Text.Json.Serialization;

#endregion

namespace Obs.NET.OperationDataContainers;

/// <summary>
///     A common interface for all operation data containers.
/// </summary>
public interface IObsWsOpDataContainer
{
    /// <summary>
    ///     The numeric code that designates which operation a particular
    ///     data container is associated with.
    /// </summary>
    [JsonIgnore]
    static virtual int Op => throw new NotImplementedException();
}