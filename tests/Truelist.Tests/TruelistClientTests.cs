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

    private const string ValidResponseJson = @"{""emails"":[{""address"":""user@example.com"",""domain"":""example.com"",""canonical"":""user"",""mx_record"":null,""first_name"":null,""last_name"":null,""email_state"":""ok"",""email_sub_state"":""email_ok"",""verified_at"":""2026-02-21T10:00:00.000Z"",""did_you_mean"":null}]}";

    private const string InvalidResponseJson = @"{""emails"":[{""address"":""bad@invalid.com"",""domain"":""invalid.com"",""canonical"":""bad"",""mx_record"":null,""first_name"":null,""last_name"":null,""email_state"":""email_invalid"",""email_sub_state"":""failed_smtp_check"",""verified_at"":""2026-02-21T10:00:00.000Z"",""did_you_mean"":null}]}";

    private const string AcceptAllResponseJson = @"{""emails"":[{""address"":""risky@example.com"",""domain"":""example.com"",""canonical"":""risky"",""mx_record"":null,""first_name"":null,""last_name"":null,""email_state"":""accept_all"",""email_sub_state"":""email_ok"",""verified_at"":""2026-02-21T10:00:00.000Z"",""did_you_mean"":null}]}";

    private const string UnknownResponseJson = @"{""emails"":[{""address"":""unknown@example.com"",""domain"":""example.com"",""canonical"":""unknown"",""mx_record"":null,""first_name"":null,""last_name"":null,""email_state"":""unknown"",""email_sub_state"":""unknown_error"",""verified_at"":null,""did_you_mean"":null}]}";

    private const string SuggestionResponseJson = @"{""emails"":[{""address"":""user@gmial.com"",""domain"":""gmial.com"",""canonical"":""user"",""mx_record"":null,""first_name"":null,""last_name"":null,""email_state"":""email_invalid"",""email_sub_state"":""failed_smtp_check"",""verified_at"":""2026-02-21T10:00:00.000Z"",""did_you_mean"":""user@gmail.com""}]}";

    private const string AccountResponseJson = @"{""email"":""team@company.com"",""name"":""Team Lead"",""uuid"":""a3828d19-1234-5678-9abc-def012345678"",""time_zone"":""America/New_York"",""is_admin_role"":true,""token"":""test_token"",""api_keys"":[],""account"":{""name"":""Company Inc"",""payment_plan"":""pro"",""users"":[]}}";

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
        _handler.ResponseJson = ValidResponseJson;

        using var client = CreateClient();
        var result = await client.ValidateAsync("user@example.com");

        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("example.com", result.Domain);
        Assert.Equal("user", result.Canonical);
        Assert.Equal("ok", result.State);
        Assert.Equal("email_ok", result.SubState);
        Assert.True(result.IsValid);
        Assert.False(result.IsInvalid);
        Assert.False(result.IsDisposable);
        Assert.False(result.IsRole);
    }

    [Fact]
    public async Task ValidateAsync_InvalidEmail_ReturnsInvalidResult()
    {
        _handler.ResponseJson = InvalidResponseJson;

        using var client = CreateClient();
        var result = await client.ValidateAsync("bad@invalid.com");

        Assert.Equal("email_invalid", result.State);
        Assert.Equal("failed_smtp_check", result.SubState);
        Assert.True(result.IsInvalid);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_AcceptAllEmail_ReturnsAcceptAllResult()
    {
        _handler.ResponseJson = AcceptAllResponseJson;

        using var client = CreateClient();
        var result = await client.ValidateAsync("risky@example.com");

        Assert.Equal("accept_all", result.State);
        Assert.True(result.IsAcceptAll);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_UnknownEmail_ReturnsUnknownResult()
    {
        _handler.ResponseJson = UnknownResponseJson;

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
        _handler.ResponseJson = ValidResponseJson;

        using var client = CreateClient();
        await client.ValidateAsync("user@example.com");

        Assert.NotNull(_handler.LastRequest);
        Assert.Equal(HttpMethod.Post, _handler.LastRequest!.Method);
        Assert.Contains("api/v1/verify_inline", _handler.LastRequest.RequestUri!.ToString());
        Assert.Contains("email=user%40example.com", _handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer", _handler.LastRequest.Headers.Authorization?.Scheme);
        Assert.Equal("test-api-key", _handler.LastRequest.Headers.Authorization?.Parameter);
        Assert.Null(_handler.LastRequest.Content);
    }

    [Fact]
    public async Task ValidateAsync_WithSuggestion_ReturnsSuggestion()
    {
        _handler.ResponseJson = SuggestionResponseJson;

        using var client = CreateClient();
        var result = await client.ValidateAsync("user@gmial.com");

        Assert.Equal("user@gmail.com", result.Suggestion);
    }

    #endregion

    #region GetAccountAsync Tests

    [Fact]
    public async Task GetAccountAsync_ReturnsAccountInfo()
    {
        _handler.ResponseJson = AccountResponseJson;

        using var client = CreateClient();
        var account = await client.GetAccountAsync();

        Assert.Equal("team@company.com", account.Email);
        Assert.Equal("Team Lead", account.Name);
        Assert.Equal("a3828d19-1234-5678-9abc-def012345678", account.Uuid);
        Assert.Equal("America/New_York", account.TimeZone);
        Assert.True(account.IsAdminRole);
        Assert.NotNull(account.Account);
        Assert.Equal("Company Inc", account.Account!.Name);
        Assert.Equal("pro", account.Account.PaymentPlan);
    }

    [Fact]
    public async Task GetAccountAsync_SendsCorrectRequest()
    {
        _handler.ResponseJson = AccountResponseJson;

        using var client = CreateClient();
        await client.GetAccountAsync();

        Assert.NotNull(_handler.LastRequest);
        Assert.Equal(HttpMethod.Get, _handler.LastRequest!.Method);
        Assert.EndsWith("me", _handler.LastRequest.RequestUri!.ToString());
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
        _handler.ResponseJson = ValidResponseJson;

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        using var client = CreateClient();

        await Assert.ThrowsAsync<TaskCanceledException>(() => client.ValidateAsync("user@example.com", cts.Token));
    }

    [Fact]
    public async Task GetAccountAsync_CancellationToken_Cancels()
    {
        _handler.Delay = TimeSpan.FromSeconds(5);
        _handler.ResponseJson = AccountResponseJson;

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
