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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObsRequestException" /> class with the obs-websocket
    ///     RequestStatus code and comment that caused the failure.
    /// </summary>
    /// <param name="message">The formatted error message.</param>
    /// <param name="code">The obs-websocket RequestStatus code returned by OBS.</param>
    /// <param name="comment">The obs-websocket RequestStatus comment describing the failure, if any.</param>
    public ObsRequestException(string message, int code, string? comment) : base(message)
    {
        Code = code;
        Comment = comment;
    }

    /// <summary>
    ///     Gets the obs-websocket RequestStatus code returned by OBS, or <c>null</c> when the exception
    ///     was created without status details.
    /// </summary>
    public int? Code { get; }

    /// <summary>
    ///     Gets the obs-websocket RequestStatus comment describing the failure, or <c>null</c> when no
    ///     comment was provided or the exception was created without status details.
    /// </summary>
    public string? Comment { get; }
}
