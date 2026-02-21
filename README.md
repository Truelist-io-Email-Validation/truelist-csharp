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

Console.WriteLine(result.Email);       // "user@example.com"
Console.WriteLine(result.Domain);      // "example.com"
Console.WriteLine(result.State);       // "ok", "email_invalid", "accept_all", or "unknown"
Console.WriteLine(result.SubState);    // "email_ok", "is_disposable", "is_role", etc.
Console.WriteLine(result.IsValid);     // true
Console.WriteLine(result.IsDisposable);// false
Console.WriteLine(result.IsRole);      // false
```

### Check Specific States

```csharp
var result = await client.ValidateAsync("user@example.com");

if (result.IsValid)
{
    Console.WriteLine("Email is valid");
}
else if (result.IsAcceptAll)
{
    Console.WriteLine("Domain accepts all addresses");
}
else if (result.IsInvalid)
{
    Console.WriteLine("Email is invalid");

    if (result.Suggestion != null)
    {
        Console.WriteLine($"Did you mean: {result.Suggestion}?");
    }
}
```

### Account Info

```csharp
var account = await client.GetAccountAsync();

Console.WriteLine(account.Email);                // "you@company.com"
Console.WriteLine(account.Name);                 // "Your Name"
Console.WriteLine(account.Uuid);                 // "a3828d19-..."
Console.WriteLine(account.TimeZone);             // "America/New_York"
Console.WriteLine(account.IsAdminRole);          // true
Console.WriteLine(account.Account?.PaymentPlan); // "pro"
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
});
```

| Option | Default | Description |
|--------|---------|-------------|
| `BaseUrl` | `https://api.truelist.io` | API base URL |
| `Timeout` | `10s` | HTTP request timeout |
| `MaxRetries` | `2` | Max retries for 429 and 5xx errors. Auth errors (401) are never retried. |

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
        return result.IsValid;
    }
}
```

## Validation Result Properties

| Property | Type | JSON Field | Description |
|----------|------|------------|-------------|
| `Email` | `string` | `address` | The validated email address |
| `Domain` | `string` | `domain` | The email domain |
| `Canonical` | `string?` | `canonical` | The local part of the email |
| `MxRecord` | `string?` | `mx_record` | MX record for the domain |
| `FirstName` | `string?` | `first_name` | First name if available |
| `LastName` | `string?` | `last_name` | Last name if available |
| `State` | `string` | `email_state` | State: `ok`, `email_invalid`, `accept_all`, `unknown` |
| `SubState` | `string` | `email_sub_state` | Detailed sub-state |
| `VerifiedAt` | `string?` | `verified_at` | Verification timestamp |
| `Suggestion` | `string?` | `did_you_mean` | Suggested email correction |
| `IsValid` | `bool` | - | `State == "ok"` |
| `IsInvalid` | `bool` | - | `State == "email_invalid"` |
| `IsAcceptAll` | `bool` | - | `State == "accept_all"` |
| `IsUnknown` | `bool` | - | `State == "unknown"` |
| `IsDisposable` | `bool` | - | `SubState == "is_disposable"` |
| `IsRole` | `bool` | - | `SubState == "is_role"` |

### States

| State | Description |
|-------|-------------|
| `ok` | Email is valid |
| `email_invalid` | Email is invalid |
| `accept_all` | Domain accepts all addresses |
| `unknown` | Could not determine validity |

### Sub-states

| Sub-state | Description |
|-----------|-------------|
| `email_ok` | Email is valid |
| `is_disposable` | Disposable email provider |
| `is_role` | Role-based address |
| `failed_smtp_check` | SMTP check failed |
| `unknown_error` | Could not determine validity |

## Testing

The SDK is designed for easy testing. Pass a custom `HttpClient` with a mocked handler:

```csharp
public class MockHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var json = """{"emails":[{"address":"user@example.com","domain":"example.com","canonical":"user","mx_record":null,"first_name":null,"last_name":null,"email_state":"ok","email_sub_state":"email_ok","verified_at":"2026-02-21T10:00:00.000Z","did_you_mean":null}]}""";
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
