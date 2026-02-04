namespace Obs.NET.Models;

/// <summary>
///     A status object that contains properties indication whether the associated request was successful or not.
///     With additional information about errors if applicable.
/// </summary>
public class RequestStatus
{
    /// <summary>
    ///     <see langword="true" /> if the request was successful, <see langword="false" /> otherwise.
    /// </summary>
    public required bool Result { get; set; }

    /// <summary>
    ///     A numeric code representing the specific status of the request.
    /// </summary>
    public required int Code { get; set; }

    /// <summary>
    ///     Comment may be provided by the server on errors to offer further details on why a request failed.
    /// </summary>
    public string? Comment { get; set; }
}