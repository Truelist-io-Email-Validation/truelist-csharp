using System.Text.Json.Serialization;

namespace Truelist;

/// <summary>
/// Represents the result of an email validation request.
/// </summary>
public record ValidationResult
{
    /// <summary>
    /// The validation state: valid, invalid, risky, or unknown.
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// The validation sub-state providing additional detail.
    /// </summary>
    [JsonPropertyName("sub_state")]
    public string SubState { get; init; } = string.Empty;

    /// <summary>
    /// A suggested correction for the email address, if any.
    /// </summary>
    [JsonPropertyName("suggestion")]
    public string? Suggestion { get; init; }

    /// <summary>
    /// Whether the email address belongs to a free email provider.
    /// </summary>
    [JsonPropertyName("free_email")]
    public bool FreeEmail { get; init; }

    /// <summary>
    /// Whether the email address is a role-based address (e.g., info@, support@).
    /// </summary>
    [JsonPropertyName("role")]
    public bool Role { get; init; }

    /// <summary>
    /// Whether the email address is from a disposable email provider.
    /// </summary>
    [JsonPropertyName("disposable")]
    public bool Disposable { get; init; }

    /// <summary>
    /// Returns true if the email is valid (state == "valid").
    /// </summary>
    [JsonIgnore]
    public bool IsValid => string.Equals(State, "valid", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the email is invalid (state == "invalid").
    /// </summary>
    [JsonIgnore]
    public bool IsInvalid => string.Equals(State, "invalid", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the email is risky (state == "risky").
    /// </summary>
    [JsonIgnore]
    public bool IsRisky => string.Equals(State, "risky", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the email state is unknown (state == "unknown").
    /// </summary>
    [JsonIgnore]
    public bool IsUnknown => string.Equals(State, "unknown", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the email belongs to a free email provider.
    /// </summary>
    [JsonIgnore]
    public bool IsFreeEmail => FreeEmail;

    /// <summary>
    /// Returns true if the email is a role-based address.
    /// </summary>
    [JsonIgnore]
    public bool IsRole => Role;

    /// <summary>
    /// Returns true if the email is from a disposable email provider.
    /// </summary>
    [JsonIgnore]
    public bool IsDisposable => Disposable;

    /// <summary>
    /// Checks if the email is valid, optionally treating risky emails as valid.
    /// </summary>
    /// <param name="allowRisky">If true, both "valid" and "risky" states are considered valid.</param>
    /// <returns>True if the email passes validation per the given criteria.</returns>
    public bool IsValidEmail(bool allowRisky = false)
    {
        if (IsValid) return true;
        if (allowRisky && IsRisky) return true;
        return false;
    }
}
