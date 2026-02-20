# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-02-20

### Added

- Initial release
- `TruelistClient` with `ValidateAsync`, `FormValidateAsync`, and `GetAccountAsync` methods
- `ValidationResult` record with state properties and `IsValidEmail` helper
- `AccountInfo` record for account information
- Automatic retry with exponential backoff for 429 and 5xx errors
- `CancellationToken` support on all async methods
- Exception hierarchy: `TruelistException`, `AuthenticationException`, `RateLimitException`, `ApiException`
- Dependency injection support via `AddTruelist` extension method
- Targets .NET 6, .NET 8, and .NET Standard 2.1
