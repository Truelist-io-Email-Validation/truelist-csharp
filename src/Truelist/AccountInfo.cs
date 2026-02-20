using System.Text.Json.Serialization;

namespace Truelist;

/// <summary>
/// Represents Truelist account information.
/// </summary>
public record AccountInfo
{
    /// <summary>
    /// The email address associated with the account.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The current plan name.
    /// </summary>
    [JsonPropertyName("plan")]
    public string Plan { get; init; } = string.Empty;

    /// <summary>
    /// The number of validation credits remaining.
    /// </summary>
    [JsonPropertyName("credits")]
    public int Credits { get; init; }
}
