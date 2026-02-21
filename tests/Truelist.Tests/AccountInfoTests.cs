using System.Text.Json;
using Truelist;

namespace Truelist.Tests;

public class AccountInfoTests
{
    [Fact]
    public void Deserialize_FullResponse_MapsAllFields()
    {
        var json = @"{
            ""email"": ""team@company.com"",
            ""name"": ""Team Lead"",
            ""uuid"": ""a3828d19-1234-5678-9abc-def012345678"",
            ""time_zone"": ""America/New_York"",
            ""is_admin_role"": true,
            ""token"": ""test_token"",
            ""api_keys"": [],
            ""account"": {
                ""name"": ""Company Inc"",
                ""payment_plan"": ""pro"",
                ""users"": []
            }
        }";

        var result = JsonSerializer.Deserialize<AccountInfo>(json);

        Assert.NotNull(result);
        Assert.Equal("team@company.com", result!.Email);
        Assert.Equal("Team Lead", result.Name);
        Assert.Equal("a3828d19-1234-5678-9abc-def012345678", result.Uuid);
        Assert.Equal("America/New_York", result.TimeZone);
        Assert.True(result.IsAdminRole);
        Assert.NotNull(result.Account);
        Assert.Equal("Company Inc", result.Account!.Name);
        Assert.Equal("pro", result.Account.PaymentPlan);
    }

    [Fact]
    public void Deserialize_MinimalResponse_MapsCorrectly()
    {
        var json = @"{
            ""email"": ""user@example.com"",
            ""name"": ""User"",
            ""uuid"": ""abc-123"",
            ""time_zone"": ""UTC"",
            ""is_admin_role"": false
        }";

        var result = JsonSerializer.Deserialize<AccountInfo>(json);

        Assert.NotNull(result);
        Assert.Equal("user@example.com", result!.Email);
        Assert.False(result.IsAdminRole);
        Assert.Null(result.Account);
    }

    [Fact]
    public void Record_Equality_Works()
    {
        var account = new AccountDetails { Name = "Co", PaymentPlan = "pro" };
        var a = new AccountInfo { Email = "a@b.com", Name = "A", Uuid = "1", TimeZone = "UTC", IsAdminRole = false, Account = account };
        var b = new AccountInfo { Email = "a@b.com", Name = "A", Uuid = "1", TimeZone = "UTC", IsAdminRole = false, Account = account };

        Assert.Equal(a, b);
    }

    [Fact]
    public void Record_Inequality_Works()
    {
        var a = new AccountInfo { Email = "a@b.com", Name = "A", Uuid = "1" };
        var b = new AccountInfo { Email = "a@b.com", Name = "B", Uuid = "1" };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var info = new AccountInfo();

        Assert.Equal(string.Empty, info.Email);
        Assert.Equal(string.Empty, info.Name);
        Assert.Equal(string.Empty, info.Uuid);
        Assert.Equal(string.Empty, info.TimeZone);
        Assert.False(info.IsAdminRole);
        Assert.Null(info.Account);
    }
}
