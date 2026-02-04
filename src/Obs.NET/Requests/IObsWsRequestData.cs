#region

using System.Text.Json.Serialization;

#endregion

namespace Obs.NET.Requests;

public interface IObsWsRequestData
{
    /// <summary>
    ///     A string that uniquely identifies the request. This is copied from the corresponding request.
    /// </summary>
    [JsonIgnore]
    static virtual string RequestType => throw new NotImplementedException();
}