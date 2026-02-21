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
    /// The name of the user.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; init; } = string.Empty;

    /// <summary>
    /// The user's time zone.
    /// </summary>
    [JsonPropertyName("time_zone")]
    public string TimeZone { get; init; } = string.Empty;

    /// <summary>
    /// Whether the user has an admin role.
    /// </summary>
    [JsonPropertyName("is_admin_role")]
    public bool IsAdminRole { get; init; }

    /// <summary>
    /// The nested account details.
    /// </summary>
    [JsonPropertyName("account")]
    public AccountDetails? Account { get; init; }
}

/// <summary>
/// Nested account details with plan information.
/// </summary>
public record AccountDetails
{
    /// <summary>
    /// The account name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The payment plan.
    /// </summary>
    [JsonPropertyName("payment_plan")]
    public string PaymentPlan { get; init; } = string.Empty;
}
