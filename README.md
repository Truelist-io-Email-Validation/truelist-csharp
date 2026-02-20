# Truelist C# SDK

Official C# SDK for the [Truelist](https://truelist.io) email validation API.

## Installation

### NuGet Package Manager

```
Install-Package Truelist
```

### .NET CLI

```bash
dotnet add package Truelist
```

### PackageReference

```xml
<PackageReference Include="Truelist" Version="0.1.0" />
```

## Quick Start

```csharp
using Truelist;

var client = new TruelistClient("your-api-key");

var result = await client.ValidateAsync("user@example.com");

if (result.IsValid)
{
    Console.WriteLine("Email is valid!");
}
```

## Usage

### Validate an Email

```csharp
var result = await client.ValidateAsync("user@example.com");

Console.WriteLine(result.State);       // "valid", "invalid", "risky", or "unknown"
Console.WriteLine(result.SubState);    // "ok", "accept_all", "disposable_address", etc.
Console.WriteLine(result.IsValid);     // true
Console.WriteLine(result.IsFreeEmail); // true
Console.WriteLine(result.IsRole);      // false
Console.WriteLine(result.IsDisposable);// false
```

### Check with Risky Allowed

```csharp
var result = await client.ValidateAsync("user@example.com");

// Treat both "valid" and "risky" as acceptable
if (result.IsValidEmail(allowRisky: true))
{
    Console.WriteLine("Email is acceptable");
}
```

### Form Validation (Frontend)

For client-side validation with a form API key:

```csharp
var client = new TruelistClient("your-api-key", new TruelistOptions
{
    FormApiKey = "your-form-api-key"
});

var result = await client.FormValidateAsync("user@example.com");
```

### Account Info

```csharp
var account = await client.GetAccountAsync();

Console.WriteLine(account.Email);   // "you@company.com"
Console.WriteLine(account.Plan);    // "pro"
Console.WriteLine(account.Credits); // 9542
```

### CancellationToken Support

All methods accept an optional `CancellationToken`:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

var result = await client.ValidateAsync("user@example.com", cts.Token);
var account = await client.GetAccountAsync(cts.Token);
```

## Configuration

```csharp
var client = new TruelistClient("your-api-key", new TruelistOptions
{
    BaseUrl = "https://api.truelist.io",    // API base URL
    Timeout = TimeSpan.FromSeconds(10),     // Request timeout
    MaxRetries = 2,                         // Retry count for 429/5xx errors
    FormApiKey = "your-form-key",           // Form API key for FormValidateAsync
});
```

| Option | Default | Description |
|--------|---------|-------------|
| `BaseUrl` | `https://api.truelist.io` | API base URL |
| `Timeout` | `10s` | HTTP request timeout |
| `MaxRetries` | `2` | Max retries for 429 and 5xx errors. Auth errors (401) are never retried. |
| `FormApiKey` | `null` | Form API key for `FormValidateAsync` |

## Error Handling

The SDK throws typed exceptions for different error scenarios:

```csharp
using Truelist.Exceptions;

try
{
    var result = await client.ValidateAsync("user@example.com");
}
catch (AuthenticationException ex)
{
    // 401 - Invalid API key. Always thrown, never retried.
    Console.WriteLine(ex.Message);
}
catch (RateLimitException ex)
{
    // 429 - Rate limit exceeded (after retries exhausted)
    Console.WriteLine(ex.RetryAfterSeconds);
}
catch (ApiException ex)
{
    // Other API errors (400, 500, etc.)
    Console.WriteLine($"{ex.StatusCode}: {ex.ResponseBody}");
}
catch (TruelistException ex)
{
    // Base exception for all SDK errors
    Console.WriteLine(ex.Message);
}
```

### Exception Hierarchy

| Exception | Status Code | Retried | Description |
|-----------|-------------|---------|-------------|
| `AuthenticationException` | 401 | Never | Invalid API key |
| `RateLimitException` | 429 | Yes | Rate limit exceeded |
| `ApiException` | 4xx/5xx | 5xx only | API error response |
| `TruelistException` | - | - | Base SDK exception |

### Retry Behavior

- **401 errors** are never retried and always throw immediately
- **429 errors** are retried up to `MaxRetries` times with exponential backoff
- **5xx errors** are retried up to `MaxRetries` times with exponential backoff
- **4xx errors** (except 401, 429) are not retried

## Dependency Injection

Register `TruelistClient` with the .NET DI container:

```csharp
using Truelist;

// In Program.cs or Startup.cs
builder.Services.AddTruelist("your-api-key");

// With options
builder.Services.AddTruelist("your-api-key", new TruelistOptions
{
    Timeout = TimeSpan.FromSeconds(15),
    MaxRetries = 3,
});

// With configuration action
builder.Services.AddTruelist("your-api-key", options =>
{
    options.Timeout = TimeSpan.FromSeconds(15);
    options.MaxRetries = 3;
});
```

Then inject it into your services:

```csharp
public class EmailService
{
    private readonly TruelistClient _truelist;

    public EmailService(TruelistClient truelist)
    {
        _truelist = truelist;
    }

    public async Task<bool> IsEmailValidAsync(string email)
    {
        var result = await _truelist.ValidateAsync(email);
        return result.IsValidEmail(allowRisky: true);
    }
}
```

## Validation Result Properties

| Property | Type | Description |
|----------|------|-------------|
| `State` | `string` | Validation state: `valid`, `invalid`, `risky`, `unknown` |
| `SubState` | `string` | Detailed sub-state |
| `Suggestion` | `string?` | Suggested email correction |
| `FreeEmail` | `bool` | From a free email provider |
| `Role` | `bool` | Role-based address (info@, support@) |
| `Disposable` | `bool` | Disposable email provider |
| `IsValid` | `bool` | `State == "valid"` |
| `IsInvalid` | `bool` | `State == "invalid"` |
| `IsRisky` | `bool` | `State == "risky"` |
| `IsUnknown` | `bool` | `State == "unknown"` |
| `IsFreeEmail` | `bool` | Alias for `FreeEmail` |
| `IsRole` | `bool` | Alias for `Role` |
| `IsDisposable` | `bool` | Alias for `Disposable` |

### Sub-states

| Sub-state | Description |
|-----------|-------------|
| `ok` | Email is valid |
| `accept_all` | Domain accepts all addresses |
| `disposable_address` | Disposable email provider |
| `role_address` | Role-based address |
| `failed_mx_check` | No valid MX records |
| `failed_spam_trap` | Known spam trap |
| `failed_no_mailbox` | Mailbox does not exist |
| `failed_greylisted` | Server temporarily rejected |
| `failed_syntax_check` | Invalid email syntax |
| `unknown` | Could not determine validity |

## Testing

The SDK is designed for easy testing. Pass a custom `HttpClient` with a mocked handler:

```csharp
public class MockHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var json = """{"state":"valid","sub_state":"ok","free_email":false,"role":false,"disposable":false}""";
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}

var httpClient = new HttpClient(new MockHandler())
{
    BaseAddress = new Uri("https://api.truelist.io/")
};
var client = new TruelistClient("test-key", new TruelistOptions(), httpClient);
```

## Requirements

- .NET 6.0+, .NET 8.0+, or .NET Standard 2.1
- No external dependencies (uses built-in `System.Net.Http` and `System.Text.Json`)

## License

MIT
