#region

using Obs.NET.Requests;

#endregion

namespace Obs.NET.OperationDataContainers;

public class RequestContainer<TRequestData> : IObsWsOpDataContainer
    where TRequestData : IObsWsRequestData
{
    /// <summary>
    ///     A string that uniquely identifies the request.
    ///     This ID is copied from the corresponding request.
    /// </summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <inheritdoc cref="IObsWsRequestData" />
    public string RequestType => TRequestData.RequestType;

    /// <summary>
    ///     An object that contains the request data for the request.
    ///     The structure of this property varies depending on the request type.
    /// </summary>
    public TRequestData? RequestData { get; set; }

    /// <inheritdoc />
    public static int Op => 6;
}