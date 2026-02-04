#region

using Obs.NET.Models;
using Obs.NET.Requests;

#endregion

namespace Obs.NET.OperationDataContainers;

/// <summary>
///     An <see cref="IObsWsOpDataContainer" /> that represents a response to a request sent to the OBS WebSocket server.
/// </summary>
/// <typeparam name="TResponseData">The specific type that maps the response payload</typeparam>
public class RequestResponseContainer<TResponseData> : IObsWsOpDataContainer
{
    /// <inheritdoc cref="IObsWsRequestData" />
    public required string RequestType { get; set; }

    /// <summary>
    ///     A string that uniquely identifies the request. This ID is copied from the corresponding request.
    /// </summary>
    public required string RequestId { get; set; }

    /// <summary>
    ///     A <seealso cref="RequestStatus" /> object that contains status information about the request.
    /// </summary>
    public required RequestStatus RequestStatus { get; set; }

    /// <summary>
    ///     An object that contains the response data for the request.
    ///     The structure of this property varies depending on the request type.
    /// </summary>
    public TResponseData? ResponseData { get; set; }

    /// <inheritdoc />
    public static int Op => 7;
}