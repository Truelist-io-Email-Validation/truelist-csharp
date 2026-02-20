using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Truelist.Exceptions;

namespace Truelist;

/// <summary>
/// Client for the Truelist email validation API.
/// </summary>
public class TruelistClient : IDisposable
{
    private readonly string _apiKey;
    private readonly TruelistOptions _options;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Creates a new TruelistClient with the specified API key and default options.
    /// </summary>
    /// <param name="apiKey">Your Truelist API key.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiKey is null or empty.</exception>
    public TruelistClient(string apiKey) : this(apiKey, new TruelistOptions()) { }

    /// <summary>
    /// Creates a new TruelistClient with the specified API key and options.
    /// </summary>
    /// <param name="apiKey">Your Truelist API key.</param>
    /// <param name="options">Configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiKey is null or empty.</exception>
    public TruelistClient(string apiKey, TruelistOptions options) : this(apiKey, options, null) { }

    /// <summary>
    /// Creates a new TruelistClient with the specified API key, options, and HttpClient.
    /// Use this constructor for dependency injection or testing.
    /// </summary>
    /// <param name="apiKey">Your Truelist API key.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="httpClient">An optional HttpClient instance. If null, a new one is created.</param>
    /// <exception cref="ArgumentNullException">Thrown when apiKey is null or empty.</exception>
    public TruelistClient(string apiKey, TruelistOptions options, HttpClient? httpClient)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey), "API key is required.");

        _apiKey = apiKey;
        _options = options ?? new TruelistOptions();

        if (httpClient is not null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new HttpClient();
            _ownsHttpClient = true;
        }

        _httpClient.BaseAddress ??= new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = _options.Timeout;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("truelist-csharp/0.1.0");
    }

    /// <summary>
    /// Validates an email address using the server-side validation endpoint.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null or empty.</exception>
    /// <exception cref="AuthenticationException">Thrown when the API key is invalid.</exception>
    /// <exception cref="RateLimitException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an unexpected error.</exception>
    public async Task<ValidationResult> ValidateAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email is required.");

        var payload = JsonSerializer.Serialize(new { email }, JsonOptions);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await SendWithRetriesAsync(
            () => CreateRequest(HttpMethod.Post, "api/v1/verify", _apiKey, content),
            cancellationToken
        );

        return await DeserializeAsync<ValidationResult>(response, cancellationToken);
    }

    /// <summary>
    /// Validates an email address using the frontend form validation endpoint.
    /// Requires <see cref="TruelistOptions.FormApiKey"/> to be set.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when FormApiKey is not configured.</exception>
    /// <exception cref="AuthenticationException">Thrown when the form API key is invalid.</exception>
    /// <exception cref="RateLimitException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an unexpected error.</exception>
    public async Task<ValidationResult> FormValidateAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email), "Email is required.");

        if (string.IsNullOrWhiteSpace(_options.FormApiKey))
            throw new InvalidOperationException("FormApiKey must be set in TruelistOptions to use FormValidateAsync.");

        var payload = JsonSerializer.Serialize(new { email }, JsonOptions);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await SendWithRetriesAsync(
            () => CreateRequest(HttpMethod.Post, "api/v1/form_verify", _options.FormApiKey, content),
            cancellationToken
        );

        return await DeserializeAsync<ValidationResult>(response, cancellationToken);
    }

    /// <summary>
    /// Retrieves account information including plan and remaining credits.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The account information.</returns>
    /// <exception cref="AuthenticationException">Thrown when the API key is invalid.</exception>
    /// <exception cref="RateLimitException">Thrown when the rate limit is exceeded.</exception>
    /// <exception cref="ApiException">Thrown when the API returns an unexpected error.</exception>
    public async Task<AccountInfo> GetAccountAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendWithRetriesAsync(
            () => CreateRequest(HttpMethod.Get, "api/v1/account", _apiKey),
            cancellationToken
        );

        return await DeserializeAsync<AccountInfo>(response, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string path, string bearerToken, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        if (content is not null)
        {
            request.Content = content;
        }

        return request;
    }

    private async Task<HttpResponseMessage> SendWithRetriesAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        var maxAttempts = _options.MaxRetries + 1;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (attempt > 0)
            {
                var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt - 1) * 500);
                await Task.Delay(delay, cancellationToken);
            }

            using var request = requestFactory();
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                throw new AuthenticationException();
            }

            if (response.StatusCode == (HttpStatusCode)429)
            {
                if (attempt < maxAttempts - 1)
                {
                    response.Dispose();
                    var backoff = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 500);
                    await Task.Delay(backoff, cancellationToken);
                    continue;
                }

                using (response)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta?.Seconds;
                    throw retryAfter.HasValue
                        ? new RateLimitException("Rate limit exceeded.", (int)retryAfter.Value)
                        : new RateLimitException();
                }
            }

            if (response.StatusCode >= HttpStatusCode.InternalServerError)
            {
                if (attempt < maxAttempts - 1)
                {
                    response.Dispose();
                    continue;
                }

                using (response)
                {
                    var body = await response.Content.ReadAsStringAsync(
#if !NETSTANDARD2_1
                        cancellationToken
#endif
                    );
                    throw new ApiException(
                        $"Server error: {(int)response.StatusCode}",
                        (int)response.StatusCode,
                        body
                    );
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                using (response)
                {
                    var body = await response.Content.ReadAsStringAsync(
#if !NETSTANDARD2_1
                        cancellationToken
#endif
                    );
                    throw new ApiException(
                        $"API error: {(int)response.StatusCode}",
                        (int)response.StatusCode,
                        body
                    );
                }
            }

            return response;
        }

        throw new TruelistException("Request failed after all retry attempts.");
    }

    private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string json;
        using (response)
        {
            json = await response.Content.ReadAsStringAsync(
#if !NETSTANDARD2_1
                cancellationToken
#endif
            );
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                ?? throw new TruelistException("Failed to deserialize API response.");
        }
        catch (JsonException ex)
        {
            throw new TruelistException("Failed to deserialize API response.", ex);
        }
    }

    /// <summary>
    /// Disposes the underlying HttpClient if it was created by this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the underlying HttpClient if it was created by this instance.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing && _ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        _disposed = true;
    }
}
