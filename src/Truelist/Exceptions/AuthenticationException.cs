namespace Truelist.Exceptions;

/// <summary>
/// Thrown when the API returns a 401 Unauthorized response.
/// This exception is never retried.
/// </summary>
public class AuthenticationException : TruelistException
{
    /// <summary>
    /// Creates a new AuthenticationException.
    /// </summary>
    public AuthenticationException()
        : base("Authentication failed. Check your API key.", 401) { }

    /// <summary>
    /// Creates a new AuthenticationException with a custom message.
    /// </summary>
    public AuthenticationException(string message)
        : base(message, 401) { }
}
