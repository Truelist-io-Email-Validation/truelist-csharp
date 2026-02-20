using System.Text.Json;
using Truelist;

namespace Truelist.Tests;

public class ValidationResultTests
{
    #region State Property Tests

    [Fact]
    public void IsValid_WhenStateIsValid_ReturnsTrue()
    {
        var result = new ValidationResult { State = "valid" };
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsValid_WhenStateIsInvalid_ReturnsFalse()
    {
        var result = new ValidationResult { State = "invalid" };
        Assert.False(result.IsValid);
    }

    [Fact]
    public void IsValid_CaseInsensitive()
    {
        var result = new ValidationResult { State = "Valid" };
        Assert.True(result.IsValid);
    }

    [Fact]
    public void IsInvalid_WhenStateIsInvalid_ReturnsTrue()
    {
        var result = new ValidationResult { State = "invalid" };
        Assert.True(result.IsInvalid);
    }

    [Fact]
    public void IsInvalid_WhenStateIsValid_ReturnsFalse()
    {
        var result = new ValidationResult { State = "valid" };
        Assert.False(result.IsInvalid);
    }

    [Fact]
    public void IsRisky_WhenStateIsRisky_ReturnsTrue()
    {
        var result = new ValidationResult { State = "risky" };
        Assert.True(result.IsRisky);
    }

    [Fact]
    public void IsRisky_WhenStateIsValid_ReturnsFalse()
    {
        var result = new ValidationResult { State = "valid" };
        Assert.False(result.IsRisky);
    }

    [Fact]
    public void IsUnknown_WhenStateIsUnknown_ReturnsTrue()
    {
        var result = new ValidationResult { State = "unknown" };
        Assert.True(result.IsUnknown);
    }

    [Fact]
    public void IsUnknown_WhenStateIsValid_ReturnsFalse()
    {
        var result = new ValidationResult { State = "valid" };
        Assert.False(result.IsUnknown);
    }

    #endregion

    #region IsValidEmail Tests

    [Fact]
    public void IsValidEmail_ValidState_ReturnsTrue()
    {
        var result = new ValidationResult { State = "valid" };
        Assert.True(result.IsValidEmail());
    }

    [Fact]
    public void IsValidEmail_InvalidState_ReturnsFalse()
    {
        var result = new ValidationResult { State = "invalid" };
        Assert.False(result.IsValidEmail());
    }

    [Fact]
    public void IsValidEmail_RiskyState_WithoutAllowRisky_ReturnsFalse()
    {
        var result = new ValidationResult { State = "risky" };
        Assert.False(result.IsValidEmail());
    }

    [Fact]
    public void IsValidEmail_RiskyState_WithAllowRisky_ReturnsTrue()
    {
        var result = new ValidationResult { State = "risky" };
        Assert.True(result.IsValidEmail(allowRisky: true));
    }

    [Fact]
    public void IsValidEmail_ValidState_WithAllowRisky_ReturnsTrue()
    {
        var result = new ValidationResult { State = "valid" };
        Assert.True(result.IsValidEmail(allowRisky: true));
    }

    [Fact]
    public void IsValidEmail_InvalidState_WithAllowRisky_ReturnsFalse()
    {
        var result = new ValidationResult { State = "invalid" };
        Assert.False(result.IsValidEmail(allowRisky: true));
    }

    [Fact]
    public void IsValidEmail_UnknownState_WithAllowRisky_ReturnsFalse()
    {
        var result = new ValidationResult { State = "unknown" };
        Assert.False(result.IsValidEmail(allowRisky: true));
    }

    #endregion

    #region Boolean Property Tests

    [Fact]
    public void IsFreeEmail_WhenTrue_ReturnsTrue()
    {
        var result = new ValidationResult { FreeEmail = true };
        Assert.True(result.IsFreeEmail);
    }

    [Fact]
    public void IsFreeEmail_WhenFalse_ReturnsFalse()
    {
        var result = new ValidationResult { FreeEmail = false };
        Assert.False(result.IsFreeEmail);
    }

    [Fact]
    public void IsRole_WhenTrue_ReturnsTrue()
    {
        var result = new ValidationResult { Role = true };
        Assert.True(result.IsRole);
    }

    [Fact]
    public void IsRole_WhenFalse_ReturnsFalse()
    {
        var result = new ValidationResult { Role = false };
        Assert.False(result.IsRole);
    }

    [Fact]
    public void IsDisposable_WhenTrue_ReturnsTrue()
    {
        var result = new ValidationResult { Disposable = true };
        Assert.True(result.IsDisposable);
    }

    [Fact]
    public void IsDisposable_WhenFalse_ReturnsFalse()
    {
        var result = new ValidationResult { Disposable = false };
        Assert.False(result.IsDisposable);
    }

    #endregion

    #region JSON Deserialization Tests

    [Fact]
    public void Deserialize_FullResponse_MapsAllFields()
    {
        var json = @"{
            ""state"": ""valid"",
            ""sub_state"": ""ok"",
            ""suggestion"": ""user@gmail.com"",
            ""free_email"": true,
            ""role"": true,
            ""disposable"": true
        }";

        var result = JsonSerializer.Deserialize<ValidationResult>(json);

        Assert.NotNull(result);
        Assert.Equal("valid", result!.State);
        Assert.Equal("ok", result.SubState);
        Assert.Equal("user@gmail.com", result.Suggestion);
        Assert.True(result.FreeEmail);
        Assert.True(result.Role);
        Assert.True(result.Disposable);
    }

    [Fact]
    public void Deserialize_NullSuggestion_MapsCorrectly()
    {
        var json = @"{
            ""state"": ""valid"",
            ""sub_state"": ""ok"",
            ""suggestion"": null,
            ""free_email"": false,
            ""role"": false,
            ""disposable"": false
        }";

        var result = JsonSerializer.Deserialize<ValidationResult>(json);

        Assert.NotNull(result);
        Assert.Null(result!.Suggestion);
    }

    [Fact]
    public void Deserialize_MissingSuggestion_DefaultsToNull()
    {
        var json = @"{
            ""state"": ""valid"",
            ""sub_state"": ""ok"",
            ""free_email"": false,
            ""role"": false,
            ""disposable"": false
        }";

        var result = JsonSerializer.Deserialize<ValidationResult>(json);

        Assert.NotNull(result);
        Assert.Null(result!.Suggestion);
    }

    #endregion
}
