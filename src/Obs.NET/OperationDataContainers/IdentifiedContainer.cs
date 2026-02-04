namespace Obs.NET.OperationDataContainers;

/// <summary>
///     The identify request was received and validated, and the connection is now ready for normal operation.
/// </summary>
public class IdentifiedContainer : IObsWsOpDataContainer
{
    /// <summary>
    ///     The RPC version that was negotiated during the identification process.
    /// </summary>
    public required int NegotiatedRpcVersion { get; set; }

    /// <inheritdoc />
    public static int Op => 2;
}