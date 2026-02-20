using System.Text.Json;
using Truelist;

namespace Truelist.Tests;

public class AccountInfoTests
{
    [Fact]
    public void Deserialize_FullResponse_MapsAllFields()
    {
        var json = @"{
            ""email"": ""user@truelist.io"",
            ""plan"": ""pro"",
            ""credits"": 9542
        }";

        var result = JsonSerializer.Deserialize<AccountInfo>(json);

        Assert.NotNull(result);
        Assert.Equal("user@truelist.io", result!.Email);
        Assert.Equal("pro", result.Plan);
        Assert.Equal(9542, result.Credits);
    }

    [Fact]
    public void Deserialize_ZeroCredits_MapsCorrectly()
    {
        var json = @"{
            ""email"": ""user@truelist.io"",
            ""plan"": ""free"",
            ""credits"": 0
        }";

        var result = JsonSerializer.Deserialize<AccountInfo>(json);

        Assert.NotNull(result);
        Assert.Equal(0, result!.Credits);
    }

    [Fact]
    public void Deserialize_LargeCredits_MapsCorrectly()
    {
        var json = @"{
            ""email"": ""enterprise@truelist.io"",
            ""plan"": ""enterprise"",
            ""credits"": 1000000
        }";

        var result = JsonSerializer.Deserialize<AccountInfo>(json);

        Assert.NotNull(result);
        Assert.Equal(1000000, result!.Credits);
    }

    [Fact]
    public void Record_Equality_Works()
    {
        var a = new AccountInfo { Email = "a@b.com", Plan = "pro", Credits = 100 };
        var b = new AccountInfo { Email = "a@b.com", Plan = "pro", Credits = 100 };

        Assert.Equal(a, b);
    }

    [Fact]
    public void Record_Inequality_Works()
    {
        var a = new AccountInfo { Email = "a@b.com", Plan = "pro", Credits = 100 };
        var b = new AccountInfo { Email = "a@b.com", Plan = "pro", Credits = 200 };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var info = new AccountInfo();

        Assert.Equal(string.Empty, info.Email);
        Assert.Equal(string.Empty, info.Plan);
        Assert.Equal(0, info.Credits);
    }
}
