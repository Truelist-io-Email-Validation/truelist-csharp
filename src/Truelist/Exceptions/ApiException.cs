namespace Truelist.Exceptions;

/// <summary>
/// Thrown when the API returns an unexpected error response.
/// </summary>
public class ApiException : TruelistException
{
    /// <summary>
    /// The response body from the API, if available.
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Creates a new ApiException.
    /// </summary>
    public ApiException(string message, int statusCode)
        : base(message, statusCode) { }

    /// <summary>
    /// Creates a new ApiException with the response body.
    /// </summary>
    public ApiException(string message, int statusCode, string? responseBody)
        : base(message, statusCode)
    {
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Creates a new ApiException with an inner exception.
    /// </summary>
    public ApiException(string message, int statusCode, Exception innerException)
        : base(message, statusCode, innerException) { }
}
