namespace Truelist.Exceptions;

/// <summary>
/// Thrown when the API returns a 429 Too Many Requests response.
/// </summary>
public class RateLimitException : TruelistException
{
    /// <summary>
    /// The number of seconds to wait before retrying, if provided by the API.
    /// </summary>
    public int? RetryAfterSeconds { get; }

    /// <summary>
    /// Creates a new RateLimitException.
    /// </summary>
    public RateLimitException()
        : base("Rate limit exceeded.", 429) { }

    /// <summary>
    /// Creates a new RateLimitException with a custom message.
    /// </summary>
    public RateLimitException(string message)
        : base(message, 429) { }

    /// <summary>
    /// Creates a new RateLimitException with a retry-after value.
    /// </summary>
    public RateLimitException(string message, int retryAfterSeconds)
        : base(message, 429)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
