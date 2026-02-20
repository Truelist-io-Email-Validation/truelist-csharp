using System.Net;
using System.Text;
using System.Text.Json;
using Truelist;
using Truelist.Exceptions;

namespace Truelist.Tests;

public class TruelistClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _handler;
    private readonly HttpClient _httpClient;

    public TruelistClientTests()
    {
        _handler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri("https://api.truelist.io/")
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _handler.Dispose();
    }

    private TruelistClient CreateClient(TruelistOptions? options = null)
    {
        return new TruelistClient("test-api-key", options ?? new TruelistOptions(), _httpClient);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullApiKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TruelistClient(null!));
    }

    [Fact]
    public void Constructor_EmptyApiKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TruelistClient(""));
    }

    [Fact]
    public void Constructor_WhitespaceApiKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TruelistClient("  "));
    }

    #endregion

    #region ValidateAsync Tests

    [Fact]
    public async Task ValidateAsync_ValidEmail_ReturnsValidResult()
    {
        _handler.ResponseJson = @"{
            ""state"": ""valid"",
            ""sub_state"": ""ok"",
            ""suggestion"": null,
            ""free_email"": true,
            ""role"": false,
            ""disposable"": false
        }";

        using var client = CreateClient();
        var result = await client.ValidateAsync("user@example.com");

        Assert.Equal("valid", result.State);
        Assert.Equal("ok", result.SubState);
        Assert.True(result.IsValid);
        Assert.False(result.IsInvalid);
        Assert.True(result.IsFreeEmail);
        Assert.False(result.IsRole);
        Assert.False(result.IsDisposable);
    }

    [Fact]
    public async Task ValidateAsync_InvalidEmail_ReturnsInvalidResult()
    {
        _handler.ResponseJson = @"{
            ""state"": ""invalid"",
            ""sub_state"": ""failed_no_mailbox"",
            ""suggestion"": null,
            ""free_email"": false,
            ""role"": false,
            ""disposable"": false
        }";

        using var client = CreateClient();
        var result = await client.ValidateAsync("bad@invalid.com");

        Assert.Equal("invalid", result.State);
        Assert.Equal("failed_no_mailbox", result.SubState);
        Assert.True(result.IsInvalid);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_RiskyEmail_ReturnsRiskyResult()
    {
        _handler.ResponseJson = @"{
            ""state"": ""risky"",
            ""sub_state"": ""accept_all"",
            ""suggestion"": null,
            ""free_email"": false,
            ""role"": false,
            ""disposable"": false
        }";

        using var client = CreateClient();
        var result = await client.ValidateAsync("risky@example.com");

        Assert.Equal("risky", result.State);
        Assert.True(result.IsRisky);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_UnknownEmail_ReturnsUnknownResult()
    {
        _handler.ResponseJson = @"{
            ""state"": ""unknown"",
            ""sub_state"": ""unknown"",
            ""suggestion"": null,
            ""free_email"": false,
            ""role"": false,
            ""disposable"": false
        }";

        using var client = CreateClient();
        var result = await client.ValidateAsync("unknown@example.com");

        Assert.Equal("unknown", result.State);
        Assert.True(result.IsUnknown);
    }

    [Fact]
    public async Task ValidateAsync_NullEmail_ThrowsArgumentNullException()
    {
        using var client = CreateClient();
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.ValidateAsync(null!));
    }

    [Fact]
    public async Task ValidateAsync_EmptyEmail_ThrowsArgumentNullException()
    {
        using var client = CreateClient();
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.ValidateAsync(""));
    }

    [Fact]
    public async Task ValidateAsync_SendsCorrectRequest()
    {
        _handler.ResponseJson = @"{""state"":""valid"",""sub_state"":""ok"",""free_email"":false,""role"":false,""disposable"":false}";

        using var client = CreateClient();
        await client.ValidateAsync("user@example.com");

        Assert.NotNull(_handler.LastRequest);
        Assert.Equal(HttpMethod.Post, _handler.LastRequest!.Method);
        Assert.Contains("api/v1/verify", _handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer", _handler.LastRequest.Headers.Authorization?.Scheme);
        Assert.Equal("test-api-key", _handler.LastRequest.Headers.Authorization?.Parameter);

        var body = await _handler.LastRequest.Content!.ReadAsStringAsync();
        Assert.Contains("user@example.com", body);
    }

    [Fact]
    public async Task ValidateAsync_WithSuggestion_ReturnsSuggestion()
    {
        _handler.ResponseJson = @"{
            ""state"": ""invalid"",
            ""sub_state"": ""failed_syntax_check"",
            ""suggestion"": ""user@gmail.com"",
            ""free_email"": false,
            ""role"": false,
            ""disposable"": false
        }";

        using var client = CreateClient();
        var result = await client.ValidateAsync("user@gmial.com");

        Assert.Equal("user@gmail.com", result.Suggestion);
    }

    #endregion

    #region FormValidateAsync Tests

    [Fact]
    public async Task FormValidateAsync_UsesFormApiKey()
    {
        _handler.ResponseJson = @"{""state"":""valid"",""sub_state"":""ok"",""free_email"":false,""role"":false,""disposable"":false}";

        var options = new TruelistOptions { FormApiKey = "form-test-key" };
        using var client = CreateClient(options);
        await client.FormValidateAsync("user@example.com");

        Assert.NotNull(_handler.LastRequest);
        Assert.Equal("Bearer", _handler.LastRequest!.Headers.Authorization?.Scheme);
        Assert.Equal("form-test-key", _handler.LastRequest.Headers.Authorization?.Parameter);
        Assert.Contains("api/v1/form_verify", _handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task FormValidateAsync_NoFormApiKey_ThrowsInvalidOperationException()
    {
        using var client = CreateClient();
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.FormValidateAsync("user@example.com"));
    }

    [Fact]
    public async Task FormValidateAsync_NullEmail_ThrowsArgumentNullException()
    {
        var options = new TruelistOptions { FormApiKey = "form-test-key" };
        using var client = CreateClient(options);
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.FormValidateAsync(null!));
    }

    #endregion

    #region GetAccountAsync Tests

    [Fact]
    public async Task GetAccountAsync_ReturnsAccountInfo()
    {
        _handler.ResponseJson = @"{
            ""email"": ""test@truelist.io"",
            ""plan"": ""pro"",
            ""credits"": 9542
        }";

        using var client = CreateClient();
        var account = await client.GetAccountAsync();

        Assert.Equal("test@truelist.io", account.Email);
        Assert.Equal("pro", account.Plan);
        Assert.Equal(9542, account.Credits);
    }

    [Fact]
    public async Task GetAccountAsync_SendsCorrectRequest()
    {
        _handler.ResponseJson = @"{""email"":""t@t.io"",""plan"":""free"",""credits"":0}";

        using var client = CreateClient();
        await client.GetAccountAsync();

        Assert.NotNull(_handler.LastRequest);
        Assert.Equal(HttpMethod.Get, _handler.LastRequest!.Method);
        Assert.Contains("api/v1/account", _handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer", _handler.LastRequest.Headers.Authorization?.Scheme);
        Assert.Equal("test-api-key", _handler.LastRequest.Headers.Authorization?.Parameter);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ValidateAsync_401_ThrowsAuthenticationException()
    {
        _handler.ResponseStatusCode = HttpStatusCode.Unauthorized;
        _handler.ResponseJson = @"{""error"":""unauthorized""}";

        using var client = CreateClient();
        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => client.ValidateAsync("user@example.com"));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task GetAccountAsync_401_ThrowsAuthenticationException()
    {
        _handler.ResponseStatusCode = HttpStatusCode.Unauthorized;
        _handler.ResponseJson = @"{""error"":""unauthorized""}";

        using var client = CreateClient();
        await Assert.ThrowsAsync<AuthenticationException>(() => client.GetAccountAsync());
    }

    [Fact]
    public async Task FormValidateAsync_401_ThrowsAuthenticationException()
    {
        _handler.ResponseStatusCode = HttpStatusCode.Unauthorized;
        _handler.ResponseJson = @"{""error"":""unauthorized""}";

        var options = new TruelistOptions { FormApiKey = "bad-key" };
        using var client = CreateClient(options);
        await Assert.ThrowsAsync<AuthenticationException>(() => client.FormValidateAsync("user@example.com"));
    }

    [Fact]
    public async Task ValidateAsync_401_NeverRetries()
    {
        _handler.ResponseStatusCode = HttpStatusCode.Unauthorized;
        _handler.ResponseJson = @"{""error"":""unauthorized""}";

        var options = new TruelistOptions { MaxRetries = 3 };
        using var client = CreateClient(options);

        await Assert.ThrowsAsync<AuthenticationException>(() => client.ValidateAsync("user@example.com"));
        Assert.Equal(1, _handler.RequestCount);
    }

    [Fact]
    public async Task ValidateAsync_429_ThrowsRateLimitException()
    {
        _handler.ResponseStatusCode = (HttpStatusCode)429;
        _handler.ResponseJson = @"{""error"":""rate_limit_exceeded""}";

        var options = new TruelistOptions { MaxRetries = 0 };
        using var client = CreateClient(options);

        var ex = await Assert.ThrowsAsync<RateLimitException>(() => client.ValidateAsync("user@example.com"));
        Assert.Equal(429, ex.StatusCode);
    }

    [Fact]
    public async Task ValidateAsync_500_ThrowsApiException()
    {
        _handler.ResponseStatusCode = HttpStatusCode.InternalServerError;
        _handler.ResponseJson = @"{""error"":""internal_error""}";

        var options = new TruelistOptions { MaxRetries = 0 };
        using var client = CreateClient(options);

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.ValidateAsync("user@example.com"));
        Assert.Equal(500, ex.StatusCode);
    }

    [Fact]
    public async Task ValidateAsync_400_ThrowsApiException()
    {
        _handler.ResponseStatusCode = HttpStatusCode.BadRequest;
        _handler.ResponseJson = @"{""error"":""bad_request""}";

        using var client = CreateClient();
        var ex = await Assert.ThrowsAsync<ApiException>(() => client.ValidateAsync("user@example.com"));

        Assert.Equal(400, ex.StatusCode);
    }

    #endregion

    #region Retry Tests

    [Fact]
    public async Task ValidateAsync_500_RetriesUpToMaxRetries()
    {
        _handler.ResponseStatusCode = HttpStatusCode.InternalServerError;
        _handler.ResponseJson = @"{""error"":""internal_error""}";

        var options = new TruelistOptions { MaxRetries = 2 };
        using var client = CreateClient(options);

        await Assert.ThrowsAsync<ApiException>(() => client.ValidateAsync("user@example.com"));
        Assert.Equal(3, _handler.RequestCount); // 1 initial + 2 retries
    }

    [Fact]
    public async Task ValidateAsync_429_RetriesUpToMaxRetries()
    {
        _handler.ResponseStatusCode = (HttpStatusCode)429;
        _handler.ResponseJson = @"{""error"":""rate_limit""}";

        var options = new TruelistOptions { MaxRetries = 1 };
        using var client = CreateClient(options);

        await Assert.ThrowsAsync<RateLimitException>(() => client.ValidateAsync("user@example.com"));
        Assert.Equal(2, _handler.RequestCount); // 1 initial + 1 retry
    }

    [Fact]
    public async Task ValidateAsync_NoRetries_WhenMaxRetriesIsZero()
    {
        _handler.ResponseStatusCode = HttpStatusCode.InternalServerError;
        _handler.ResponseJson = @"{""error"":""error""}";

        var options = new TruelistOptions { MaxRetries = 0 };
        using var client = CreateClient(options);

        await Assert.ThrowsAsync<ApiException>(() => client.ValidateAsync("user@example.com"));
        Assert.Equal(1, _handler.RequestCount);
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task ValidateAsync_CancellationToken_Cancels()
    {
        _handler.Delay = TimeSpan.FromSeconds(5);
        _handler.ResponseJson = @"{""state"":""valid"",""sub_state"":""ok"",""free_email"":false,""role"":false,""disposable"":false}";

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        using var client = CreateClient();

        await Assert.ThrowsAsync<TaskCanceledException>(() => client.ValidateAsync("user@example.com", cts.Token));
    }

    [Fact]
    public async Task GetAccountAsync_CancellationToken_Cancels()
    {
        _handler.Delay = TimeSpan.FromSeconds(5);
        _handler.ResponseJson = @"{""email"":""t@t.io"",""plan"":""free"",""credits"":0}";

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        using var client = CreateClient();

        await Assert.ThrowsAsync<TaskCanceledException>(() => client.GetAccountAsync(cts.Token));
    }

    #endregion
}

/// <summary>
/// A mock HttpMessageHandler for testing.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    public string ResponseJson { get; set; } = "{}";
    public HttpStatusCode ResponseStatusCode { get; set; } = HttpStatusCode.OK;
    public HttpRequestMessage? LastRequest { get; private set; }
    public int RequestCount { get; private set; }
    public TimeSpan? Delay { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;

        // Clone the request content before it gets disposed
        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            var clonedRequest = new HttpRequestMessage(request.Method, request.RequestUri);
            clonedRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
            foreach (var header in request.Headers)
            {
                clonedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            LastRequest = clonedRequest;
        }
        else
        {
            LastRequest = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                LastRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (Delay.HasValue)
        {
            await Task.Delay(Delay.Value, cancellationToken);
        }

        return new HttpResponseMessage(ResponseStatusCode)
        {
            Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
        };
    }
}
