namespace Obs.NET.OperationDataContainers;

public class RequestBatchResponseContainer : IObsWsOpDataContainer
{
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    // ToDo: This needs to be similar to Op6 where we have a generic type
    public List<object> Results { get; set; } = new();

    /// <inheritdoc />
    public static int Op => 9;
}