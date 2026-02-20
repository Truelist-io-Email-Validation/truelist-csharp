namespace Truelist.Exceptions;

/// <summary>
/// Base exception for all Truelist SDK errors.
/// </summary>
public class TruelistException : Exception
{
    /// <summary>
    /// The HTTP status code returned by the API, if available.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Creates a new TruelistException.
    /// </summary>
    public TruelistException(string message) : base(message) { }

    /// <summary>
    /// Creates a new TruelistException with an HTTP status code.
    /// </summary>
    public TruelistException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Creates a new TruelistException with an inner exception.
    /// </summary>
    public TruelistException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Creates a new TruelistException with an HTTP status code and inner exception.
    /// </summary>
    public TruelistException(string message, int statusCode, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
