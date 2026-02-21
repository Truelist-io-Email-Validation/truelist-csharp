using System.Text.Json;
using Truelist;

namespace Truelist.Tests;

public class ValidationResultTests
{
    #region State Property Tests

    [Fact]
    public void IsValid_WhenStateIsOk_ReturnsTrue()
    {
        var result = new ValidationResult { State = "ok" };
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsValid_WhenStateIsEmailInvalid_ReturnsFalse()
    {
        var result = new ValidationResult { State = "email_invalid" };
        Assert.False(result.IsValid);
    }

    [Fact]
    public void IsValid_CaseInsensitive()
    {
        var result = new ValidationResult { State = "Ok" };
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsInvalid_WhenStateIsEmailInvalid_ReturnsTrue()
    {
        var result = new ValidationResult { State = "email_invalid" };
        Assert.True(result.IsInvalid);
    }

    [Fact]
    public void IsInvalid_WhenStateIsOk_ReturnsFalse()
    {
        var result = new ValidationResult { State = "ok" };
        Assert.False(result.IsInvalid);
    }

    [Fact]
    public void IsAcceptAll_WhenStateIsAcceptAll_ReturnsTrue()
    {
        var result = new ValidationResult { State = "accept_all" };
        Assert.True(result.IsAcceptAll);
    }

    [Fact]
    public void IsAcceptAll_WhenStateIsOk_ReturnsFalse()
    {
        var result = new ValidationResult { State = "ok" };
        Assert.False(result.IsAcceptAll);
    }

    [Fact]
    public void IsUnknown_WhenStateIsUnknown_ReturnsTrue()
    {
        var result = new ValidationResult { State = "unknown" };
        Assert.True(result.IsUnknown);
    }

    [Fact]
    public void IsUnknown_WhenStateIsOk_ReturnsFalse()
    {
        var result = new ValidationResult { State = "ok" };
        Assert.False(result.IsUnknown);
    }

    #endregion

    #region SubState Convenience Property Tests

    [Fact]
    public void IsDisposable_WhenSubStateIsDisposable_ReturnsTrue()
    {
        var result = new ValidationResult { SubState = "is_disposable" };
        Assert.True(result.IsDisposable);
    }

    [Fact]
    public void IsDisposable_WhenSubStateIsEmailOk_ReturnsFalse()
    {
        var result = new ValidationResult { SubState = "email_ok" };
        Assert.False(result.IsDisposable);
    }

    [Fact]
    public void IsRole_WhenSubStateIsRole_ReturnsTrue()
    {
        var result = new ValidationResult { SubState = "is_role" };
        Assert.True(result.IsRole);
    }

    [Fact]
    public void IsRole_WhenSubStateIsEmailOk_ReturnsFalse()
    {
        var result = new ValidationResult { SubState = "email_ok" };
        Assert.False(result.IsRole);
    }

    #endregion

    #region JSON Deserialization Tests

    [Fact]
    public void Deserialize_FullResponse_MapsAllFields()
    {
        var json = @"{
            ""address"": ""user@example.com"",
            ""domain"": ""example.com"",
            ""canonical"": ""user"",
            ""mx_record"": ""mx.example.com"",
            ""first_name"": ""John"",
            ""last_name"": ""Doe"",
            ""email_state"": ""ok"",
            ""email_sub_state"": ""email_ok"",
            ""verified_at"": ""2026-02-21T10:00:00.000Z"",
            ""did_you_mean"": ""user@gmail.com""
        }";

        var result = JsonSerializer.Deserialize<ValidationResult>(json);

        Assert.NotNull(result);
        Assert.Equal("user@example.com", result!.Email);
        Assert.Equal("example.com", result.Domain);
        Assert.Equal("user", result.Canonical);
        Assert.Equal("mx.example.com", result.MxRecord);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("ok", result.State);
        Assert.Equal("email_ok", result.SubState);
        Assert.Equal("2026-02-21T10:00:00.000Z", result.VerifiedAt);
        Assert.Equal("user@gmail.com", result.Suggestion);
    }

    [Fact]
    public void Deserialize_NullFields_MapsCorrectly()
    {
        var json = @"{
            ""address"": ""user@example.com"",
            ""domain"": ""example.com"",
            ""canonical"": ""user"",
            ""mx_record"": null,
            ""first_name"": null,
            ""last_name"": null,
            ""email_state"": ""ok"",
            ""email_sub_state"": ""email_ok"",
            ""verified_at"": null,
            ""did_you_mean"": null
        }";

        var result = JsonSerializer.Deserialize<ValidationResult>(json);

        Assert.NotNull(result);
        Assert.Null(result!.MxRecord);
        Assert.Null(result.FirstName);
        Assert.Null(result.LastName);
        Assert.Null(result.VerifiedAt);
        Assert.Null(result.Suggestion);
    }

    [Fact]
    public void Deserialize_MissingSuggestion_DefaultsToNull()
    {
        var json = @"{
            ""address"": ""user@example.com"",
            ""domain"": ""example.com"",
            ""email_state"": ""ok"",
            ""email_sub_state"": ""email_ok""
        }";

        var result = JsonSerializer.Deserialize<ValidationResult>(json);

        Assert.NotNull(result);
        Assert.Null(result!.Suggestion);
    }

    #endregion
}
