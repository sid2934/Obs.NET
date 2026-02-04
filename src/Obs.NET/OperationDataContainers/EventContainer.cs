namespace Obs.NET.OperationDataContainers;

/// <summary>
/// </summary>
public class EventContainer : IObsWsOpDataContainer
{
    /// <summary>
    /// </summary>
    public required string EventType { get; set; }

    /// <summary>
    /// </summary>
    public required int EventIntent { get; set; }

    /// <summary>
    /// </summary>
    // ToDo: This needs to be similar to Op6 where we have a generic type for each EventData
    public object? EventData { get; set; }

    /// <inheritdoc />
    public static int Op => 5;
}