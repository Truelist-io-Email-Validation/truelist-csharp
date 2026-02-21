namespace Truelist;

/// <summary>
/// Configuration options for the Truelist client.
/// </summary>
public class TruelistOptions
{
    /// <summary>
    /// Base URL for the Truelist API. Defaults to https://api.truelist.io.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.truelist.io";

    /// <summary>
    /// Request timeout. Defaults to 10 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Maximum number of retries for failed requests. Defaults to 2.
    /// Auth errors (401) are never retried.
    /// </summary>
    public int MaxRetries { get; set; } = 2;
}
