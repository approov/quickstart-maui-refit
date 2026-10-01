# Usage

This document covers common patterns for integrating `Approov.Service.Maui` into a .NET MAUI app using [`Refit`](https://github.com/reactiveui/refit). For the full API reference see the [service layer USAGE.md](https://github.com/approov/approov-service-net-httpclient/blob/3.5.5/USAGE.md).

## Setting Up in MauiProgram.cs

Complete the [service-layer and native SDK setup](README.md#adding-the-approov-service-layer) first. These examples use the Shapes app's `IApiInterface`, whose paths are `/hello/` and `/shapes/`, so the base URL includes the endpoint version. The API key below is the public Shapes demonstration key; replace it and the endpoint when using your own API.

Initialize `ApproovService` once at application startup in `MauiProgram.cs`, before any HTTP client is created:

```csharp
using Approov;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
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
        {
            var httpClient = new ApproovHttpClient
            {
                BaseAddress = new Uri("https://shapes.approov.io/v3")
            };
            httpClient.DefaultRequestHeaders.Add("Api-Key", "yXClypapWNHIifHUWmBIyPFAm");
            return RestService.For<IApiInterface>(httpClient);
        });

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
var httpClient = new ApproovHttpClient { BaseAddress = new Uri("https://shapes.approov.io/v3") };
httpClient.DefaultRequestHeaders.Add("Api-Key", "yXClypapWNHIifHUWmBIyPFAm");
IApiInterface apiClient = RestService.For<IApiInterface>(httpClient);
```

`ApproovHttpClient` is a subclass of `HttpClient`. No separate Approov-specific Refit package is required — pass it directly.

The pinned service layer attempts installation message signing by default. To demonstrate token-only protection, call `ApproovService.SetServiceMutator(ApproovServiceMutatorDefault.Shared)` after initialization. See [Stage 3 and its negative control](SHAPES-EXAMPLE.md#shapes-app-with-message-signing-v5) for enabling and explicitly disabling signing. Reapply custom configuration after any subsequent initialization, which resets the service mutator.

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

var httpClient = new ApproovHttpClient { BaseAddress = new Uri("https://shapes.approov.io/v1") };
httpClient.DefaultRequestHeaders.Add("Api-Key", "shapes_api_key_placeholder"); // replaced at runtime
IApiInterface apiClient = RestService.For<IApiInterface>(httpClient);
```

Before running this snippet, register `shapes.approov.io` with Approov and map `shapes_api_key_placeholder` to the Shapes demonstration key. Follow the [worked secrets-protection example](SHAPES-EXAMPLE.md#shapes-app-with-secrets-protection) for those steps, or [SECRETS-PROTECTION.md](SECRETS-PROTECTION.md) for your own API.

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
