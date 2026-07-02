# Usage

This document covers common patterns for integrating `Approov.Service.Maui` into a .NET MAUI app using [`Refit`](https://github.com/reactiveui/refit). For the full API reference see the [service layer USAGE.md](https://github.com/approov/approov-service-net-httpclient/blob/main/USAGE.md).

## Setting Up in MauiProgram.cs

Initialize `ApproovService` once at application startup in `MauiProgram.cs`, before any HTTP client is created:

```csharp
using Approov;
using Refit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        ApproovService.Initialize("<enter-your-config-string-here>");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register a Refit client backed by ApproovHttpClient
        builder.Services.AddSingleton<IApiInterface>(_ =>
            RestService.For<IApiInterface>(
                new ApproovHttpClient { BaseAddress = new Uri("https://shapes.approov.io") }));

        return builder.Build();
    }
}
```

Inject the typed client into pages or view models:

```csharp
public class MyViewModel
{
    private readonly IApiInterface _api;

    public MyViewModel(IApiInterface api)
    {
        _api = api;
    }
}
```

## Using ApproovHttpClient with Refit Directly

For simpler apps, pass `ApproovHttpClient` directly to `RestService.For<T>` after calling `Initialize`:

```csharp
ApproovService.Initialize("<enter-your-config-string-here>");
var httpClient = new ApproovHttpClient { BaseAddress = new Uri("https://shapes.approov.io") };
IApiInterface apiClient = RestService.For<IApiInterface>(httpClient);
```

`ApproovHttpClient` is a subclass of `HttpClient`. No separate Approov-specific Refit package is required — pass it directly.

## Bypass Mode for Development

Initialize with an empty config string to use the service layer without Approov protection:

```csharp
#if DEBUG
    ApproovService.Initialize(""); // bypass mode: standard HttpClient behavior
#else
    ApproovService.Initialize("<enter-your-config-string-here>");
#endif
```

Use `IsApproovEnabled()` to gate Approov-specific behavior at runtime:

```csharp
if (ApproovService.IsApproovEnabled())
{
    // Approov protection is active
}
else
{
    // Bypass mode or not yet initialized
}
```

## Substitution Headers for API Keys

Register a header for secret substitution after initialization. The header value in each request will be replaced at runtime with the Approov-managed secret:

```csharp
ApproovService.Initialize("<enter-your-config-string-here>");
ApproovService.AddSubstitutionHeader("Api-Key", null);

var httpClient = new ApproovHttpClient();
httpClient.DefaultRequestHeaders.Add("Api-Key", "shapes_api_key_placeholder"); // replaced at runtime
IApiInterface apiClient = RestService.For<IApiInterface>(httpClient);
```

See [SECRETS-PROTECTION.md](SECRETS-PROTECTION.md) for full setup instructions.

## Handling ApproovException

Refit wraps HTTP errors in `ApiException`. Wrap calls to also catch Approov exceptions from the underlying `ApproovHttpClient`:

```csharp
try
{
    var shape = await apiClient.GetShape();
    // handle success
}
catch (NetworkingErrorException)
{
    // Transient network failure — prompt the user to retry
    DisplayAlert("Network Error", "Please check your connection and try again.", "Retry");
}
catch (RejectionException e)
{
    // Device rejected by Approov — do not retry automatically
    DisplayAlert("Access Denied", "This device has been rejected.", "OK");
}
catch (PinningErrorException)
{
    // TLS pinning failure
    DisplayAlert("Security Error", "A certificate pinning failure was detected.", "OK");
}
catch (ApproovException e)
{
    // Other Approov error
    DisplayAlert("Error", e.Message, "OK");
}
catch (ApiException e)
{
    // Refit HTTP error (non-Approov)
    DisplayAlert("HTTP Error", e.ReasonPhrase ?? e.Message, "OK");
}
```

## Enabling Debug Logging

Enable verbose logging during development:

```csharp
ApproovService.SetLoggingLevel(ApproovLogLevel.Debug);
```

Levels: `Off`, `Error`, `Warning`, `Info` (default), `Debug`. Set to `Info` or `Off` in production builds.
