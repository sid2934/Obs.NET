namespace Obs.NET;

/// <summary>
///     Exception thrown when an OBS WebSocket request fails
/// </summary>
public class ObsRequestException : Exception
{
    public ObsRequestException(string message) : base(message)
    {
    }

    public ObsRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}