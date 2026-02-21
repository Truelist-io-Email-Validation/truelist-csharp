using System.Text.Json.Serialization;

namespace Truelist;

/// <summary>
/// Represents the result of an email validation request.
/// </summary>
public record ValidationResult
{
    /// <summary>
    /// The email address that was validated.
    /// </summary>
    [JsonPropertyName("address")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The domain of the email address.
    /// </summary>
    [JsonPropertyName("domain")]
    public string Domain { get; init; } = string.Empty;

    /// <summary>
    /// The canonical (local) part of the email address.
    /// </summary>
    [JsonPropertyName("canonical")]
    public string? Canonical { get; init; }

    /// <summary>
    /// The MX record for the domain.
    /// </summary>
    [JsonPropertyName("mx_record")]
    public string? MxRecord { get; init; }

    /// <summary>
    /// The first name associated with the email, if available.
    /// </summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    /// <summary>
    /// The last name associated with the email, if available.
    /// </summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    /// <summary>
    /// The validation state: ok, email_invalid, accept_all, or unknown.
    /// </summary>
    [JsonPropertyName("email_state")]
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// The validation sub-state providing additional detail.
    /// </summary>
    [JsonPropertyName("email_sub_state")]
    public string SubState { get; init; } = string.Empty;

    /// <summary>
    /// The timestamp when the email was verified.
    /// </summary>
    [JsonPropertyName("verified_at")]
    public string? VerifiedAt { get; init; }

    /// <summary>
    /// A suggested correction for the email address, if any.
    /// </summary>
    [JsonPropertyName("did_you_mean")]
    public string? Suggestion { get; init; }

    /// <summary>
    /// Returns true if the email is valid (state == "ok").
    /// </summary>
    [JsonIgnore]
    public bool IsValid => string.Equals(State, "ok", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the email is invalid (state == "email_invalid").
    /// </summary>
    [JsonIgnore]
    public bool IsInvalid => string.Equals(State, "email_invalid", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the domain accepts all addresses (state == "accept_all").
    /// </summary>
    [JsonIgnore]
    public bool IsAcceptAll => string.Equals(State, "accept_all", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the email state is unknown (state == "unknown").
    /// </summary>
    [JsonIgnore]
    public bool IsUnknown => string.Equals(State, "unknown", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the email is from a disposable email provider (sub_state == "is_disposable").
    /// </summary>
    [JsonIgnore]
    public bool IsDisposable => string.Equals(SubState, "is_disposable", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the email is a role-based address (sub_state == "is_role").
    /// </summary>
    [JsonIgnore]
    public bool IsRole => string.Equals(SubState, "is_role", StringComparison.OrdinalIgnoreCase);
}
