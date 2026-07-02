# Approov Quickstart: .NET MAUI Refit

This quickstart is written specifically for mobile iOS and Android apps written in C# and .NET MAUI that make API calls using [`Refit`](https://github.com/reactiveui/refit) that you wish to protect with Approov. If this is not your situation then check if there is a more relevant quickstart guide available.

This page provides all the steps for integrating Approov into your app. Additionally, a step-by-step tutorial guide using our [Shapes App Example](SHAPES-EXAMPLE.md) is also available.

To follow this guide you should have received an onboarding email for a trial or paid Approov account.

Note that the minimum OS requirement is iOS 15 and Android API 23 (Android 6.0). You cannot use Approov in apps that support OS versions older than these.

## ADDING THE APPROOV SERVICE LAYER

The Approov integration is provided by the [approov-service-net-httpclient](https://github.com/approov/approov-service-net-httpclient) repository. Clone it alongside your app:

```
git clone https://github.com/approov/approov-service-net-httpclient.git
```

Then add a `ProjectReference` to your app's `.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="path\to\approov-service-net-httpclient\ApproovService.MAUI\ApproovService.MAUI.csproj" />
</ItemGroup>
```

This project is an open source wrapper layer that allows you to easily use Approov with `HttpClient` and Refit. It includes the native [Approov SDK](https://github.com/approov/approov-ios-sdk) for iOS and the Approov Android SDK for Android, so no additional setup is required. The service layer works alongside the `Refit` package in the same project — no separate Approov-specific Refit package is required.

## ANDROID MANIFEST CHANGES

The following app permissions need to be available in the manifest to use Approov:

```xml
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.INTERNET" />
```

Please [read this](https://approov.io/docs/latest/approov-usage-documentation/#targeting-android-11-and-above) section of the reference documentation if targeting Android 11 (API level 30) or above.

## USING APPROOV SERVICE

Before using `ApproovService`, initialize it with a configuration string. This will have been provided in your Approov onboarding email (it will be something like `#123456#K/XPlLtfcwnWkzv99Wj5VmAxo4CrU267J1KlQyoz8Qo=`). After initializing, create an `ApproovHttpClient` and pass it to `RestService.For<T>`:

```csharp
ApproovService.Initialize("<enter-your-config-string-here>");
var httpClient = new ApproovHttpClient();
apiClient = RestService.For<IApiInterface>(httpClient);
```

`ApproovHttpClient` is a drop-in replacement for `HttpClient`. It automatically adds the `Approov-Token` header, applies TLS certificate pinning, and substitutes protected header and query parameter values on every request.

## ERROR TYPES

`ApproovService` methods may throw the following exceptions:

- `InitializationFailureException` — the service failed to initialize
- `ConfigurationFailureException` — the SDK is misconfigured or re-initialized with a different config string
- `PinningErrorException` — a TLS certificate pinning failure was detected
- `NetworkingErrorException` — a transient network error occurred; safe to retry
- `PermanentException` — an unrecoverable error; do not retry
- `RejectionException` — the device was rejected by Approov; check the `ARC` and `RejectionReasons` fields

## CHECKING IT WORKS

Initially you won't have set which API domains to protect, so the interceptor will not add anything. It will have called Approov though and made contact with the Approov cloud service. You will see logging from Approov saying `unknown URL`.

Your Approov onboarding email should contain a link allowing you to access [Live Metrics Graphs](https://approov.io/docs/latest/approov-usage-documentation/#metrics-graphs). After you've run your app with Approov integration you should be able to see the results in the live metrics within a minute or so.

## NEXT STEPS

To protect your APIs and/or secrets there are further steps. Approov provides two options:

* [API PROTECTION](API-PROTECTION.md): Use this if you control the backend API(s) and can verify Approov tokens server-side.

* [SECRETS PROTECTION](SECRETS-PROTECTION.md): Use this to protect app secrets and API keys so they are no longer embedded in the app binary.

See [USAGE](USAGE.md) for practical integration patterns including DI setup, bypass mode, and error handling.

See [REFERENCE](https://github.com/approov/approov-service-net-httpclient/blob/main/REFERENCE.md) for the full `ApproovService` API surface.
