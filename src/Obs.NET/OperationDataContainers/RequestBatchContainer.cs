#region

using Obs.NET.Enums;

#endregion

namespace Obs.NET.OperationDataContainers;

/// <summary>
///     Client is making a batch of requests for obs-websocket.
///     Requests are processed serially (in order) by the server.
/// </summary>
public class RequestBatchContainer : IObsWsOpDataContainer
{
    /// <summary>
    ///     A string that uniquely identifies the request.
    ///     This ID is copied from the corresponding request.
    /// </summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     When <see langword="true" />, the processing of requests will be halted on first failure.
    ///     Returns only the processed requests in RequestBatchResponse.
    /// </summary>
    public bool? HaltOnFailure { get; set; }

    /// <summary>
    ///     The execution type of the batch request.
    /// </summary>
    public RequestBatchExecutionType ExecutionType { get; set; }

    /// <summary>
    ///     Requests in the requests array follow the same structure as the Request payload data format, however requestId is
    ///     an optional field.
    /// </summary>
    public List<object> Requests { get; set; } = new();

    /// <inheritdoc />
    public static int Op => 8;
}