---
name: maui-aspire
description: >
  Guide for .NET MAUI apps consuming .NET Aspire-hosted backend services.
  Covers AppHost configuration, service discovery for mobile clients,
  HttpClient DI setup, Entra ID authentication with MSAL.NET for calling
  protected APIs, development workflow (Aspire dashboard + MAUI debugging),
  Blazor Hybrid considerations, and platform-specific networking for
  Android emulators and iOS simulators.
  USE FOR: "MAUI with Aspire", "Aspire service discovery", "AppHost MAUI",
  "Aspire backend", "MAUI Aspire networking", "mobile Aspire", "Aspire dashboard MAUI".
  DO NOT USE FOR: standalone REST API calls without Aspire (use maui-rest-api),
  authentication without Aspire (use maui-authentication), or Aspire-only projects without MAUI.
---

# .NET MAUI with .NET Aspire

## Overview

A .NET MAUI app acts as a **client** to .NET Aspire-orchestrated backend
services. Unlike Blazor Server or API projects, MAUI apps are not orchestrated
by the Aspire AppHost — they run on devices/simulators and connect to Aspire
services over the network.

**Key differences from other Aspire clients:**
- MAUI cannot use Aspire service discovery URIs (`https+http://apiservice`)
- MAUI needs real network-reachable endpoints
- Authentication uses MSAL.NET (public client) not OIDC (confidential client)
- The MAUI project is NOT added as a `.WithReference()` in the AppHost

## Architecture

```
┌─────────────────────┐     HTTPS + Bearer Token     ┌─────────────────────┐
│   MAUI App          │ ──────────────────────────►   │  Aspire API Service │
│   (device/emulator) │                               │  (JWT validation)   │
│                     │     Sign-in via system browser │                     │
│   MSAL.NET ─────────┼──► Entra ID ──► Access Token  │  Microsoft.Identity │
│                     │                               │  .Web               │
└─────────────────────┘                               └─────────────────────┘
                                                              ▲
                                                              │ Orchestrated by
                                                      ┌──────┴──────────────┐
                                                      │  Aspire AppHost     │
                                                      │  (dashboard, config)│
                                                      └─────────────────────┘
```

## AppHost Configuration

The MAUI project is NOT directly orchestrated. The AppHost only manages
backend services. However, you can reference MAUI in the solution and use
the AppHost to surface API endpoints.

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MyApp_ApiService>("apiservice");

// The MAUI project is NOT added here — it runs independently on device.
// The API endpoint is available via the Aspire dashboard.

builder.Build().Run();
```

## Service Discovery for MAUI

MAUI cannot use `https+http://apiservice` — it needs real HTTP(S) URLs.

### Option 1: Configuration-based (recommended for production)

Store the API base URL in `appsettings.json` or a config class:

```csharp
public static class ApiConfig
{
    // Development — points to local Aspire service
    // Production — points to deployed Azure Container App, App Service, etc.
    public static string BaseUrl =>
#if DEBUG
        DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7001"   // Android emulator → host loopback
            : "https://localhost:7001"   // iOS sim, Mac Catalyst, Windows
#else
        "https://myapi.azurecontainerapps.io"
#endif
    ;
}
```

### Option 2: Aspire dashboard endpoint discovery

1. Run `aspire run` to start the AppHost
2. Open the Aspire dashboard (default: `https://localhost:17178`)
3. Find your API service's endpoint URL
4. Use that URL as the base address in MAUI

### Option 3: Environment-injected (CI/CD)

Pass the API URL as a build property or environment variable:

```xml
<!-- In your MAUI .csproj -->
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <ApiBaseUrl>https://localhost:7001</ApiBaseUrl>
</PropertyGroup>
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <ApiBaseUrl>https://myapi.azurecontainerapps.io</ApiBaseUrl>
</PropertyGroup>
```

## HttpClient Setup

Register a typed `HttpClient` pointing to the Aspire API service:

```csharp
// MauiProgram.cs
builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
{
    client.BaseAddress = new Uri(ApiConfig.BaseUrl);
});
```

```csharp
public interface IWeatherApiClient
{
    Task<WeatherForecast[]?> GetForecastAsync(CancellationToken ct = default);
}

public class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _http;

    public WeatherApiClient(HttpClient http) => _http = http;

    public async Task<WeatherForecast[]?> GetForecastAsync(CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<WeatherForecast[]>(
            "/weatherforecast", ct);
    }
}
```

## Authentication (Entra ID)

When the Aspire API is protected with JWT Bearer authentication, the MAUI app
needs to acquire access tokens via **MSAL.NET**. This is covered in detail by
the **`maui-authentication` skill** (Entra ID / MSAL.NET section).

### Quick summary

1. **Provision Entra app registrations** — Use the Entra team's provisioning skill:

```bash
# Install the Entra provisioning skill in your Aspire solution:
mkdir -p .github/skills && cd .github/skills
curl -LO https://aka.ms/msidweb/aspire/entra-id-provisioning-skill
# Or clone from: https://github.com/AzureAD/microsoft-identity-web/tree/master/.github/skills
```

2. **Protect the API** — Use the Entra team's authentication skill on the Aspire API project:

```bash
curl -LO https://aka.ms/msidweb/aspire/entra-id-code-skill
```

Ask your AI assistant: **"Add Entra ID authentication to my Aspire app"**

3. **Wire up MSAL.NET in MAUI** — Follow the `maui-authentication` skill's
   MSAL.NET section for `PublicClientApplication` setup, platform configs, and
   `IAuthService`.

4. **Attach tokens to API calls** — Use a `DelegatingHandler`:

```csharp
public class AuthTokenHandler : DelegatingHandler
{
    private readonly IAuthService _authService;
    private readonly string[] _scopes;

    public AuthTokenHandler(IAuthService authService, string[] scopes)
    {
        _authService = authService;
        _scopes = scopes;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _authService.GetAccessTokenAsync(_scopes, ct);
        if (token != null)
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, ct);
    }
}
```

Register with the HttpClient:

```csharp
builder.Services.AddTransient(sp =>
    new AuthTokenHandler(
        sp.GetRequiredService<IAuthService>(),
        new[] { "api://<api-client-id>/access_as_user" }));

builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
{
    client.BaseAddress = new Uri(ApiConfig.BaseUrl);
})
.AddHttpMessageHandler<AuthTokenHandler>();
```

## Development Workflow

### Running Aspire + MAUI simultaneously

1. **Start the Aspire AppHost** (backend services):
   ```bash
   cd MyApp.AppHost
   aspire run
   # Or: dotnet run
   ```
   The Aspire dashboard opens at `https://localhost:17178`

2. **Note the API endpoint** from the dashboard (e.g., `https://localhost:7001`)

3. **Run the MAUI app** targeting your platform:
   ```bash
   # Mac Catalyst
   dotnet build -f net10.0-maccatalyst -t:Run

   # Android emulator
   dotnet build -f net10.0-android -t:Install
   adb shell am start -n com.companyname.myapp/crc64XXX.MainActivity

   # iOS simulator
   dotnet build -f net10.0-ios -t:Run -p:_DeviceName=:v2:udid=<UDID>
   ```

4. The MAUI app connects to the Aspire-hosted API using the configured base URL

### Debugging both simultaneously

- **Visual Studio**: Open two instances — one for the Aspire AppHost, one for MAUI
- **VS Code**: Use two terminal sessions, or use the Aspire extension + MAUI extension
- **CLI**: Run `aspire run` in one terminal, `dotnet build -t:Run` in another

## Platform-Specific Networking

### Android Emulator

The Android emulator cannot reach `localhost` directly. Use `10.0.2.2` to
access the host machine's loopback interface:

```csharp
#if ANDROID && DEBUG
    client.BaseAddress = new Uri("https://10.0.2.2:7001");
#endif
```

If using HTTP (not HTTPS) during development, add a network security config:

```xml
<!-- Platforms/Android/Resources/xml/network_security_config.xml -->
<network-security-config>
  <domain-config cleartextTrafficPermitted="true">
    <domain includeSubdomains="true">10.0.2.2</domain>
  </domain-config>
</network-security-config>
```

Reference in `AndroidManifest.xml`:
```xml
<application android:networkSecurityConfig="@xml/network_security_config" />
```

> ⚠️ **Development only.** Never allow clear-text traffic in production.

### Android HTTPS with dev certificates

The Android emulator doesn't trust the .NET dev certificate. Options:
1. Use HTTP during local dev (with network security config above)
2. Install the dev certificate on the emulator
3. Add `HttpClientHandler` that bypasses SSL validation in DEBUG only:

```csharp
#if ANDROID && DEBUG
builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
{
    client.BaseAddress = new Uri("https://10.0.2.2:7001");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});
#endif
```

### iOS Simulator

iOS simulators share the host's network stack — `localhost` works directly.
For HTTPS with dev certificates, the simulator trusts the macOS keychain.

If you need HTTP (not HTTPS) during development, add an ATS exception in
`Info.plist`:

```xml
<key>NSAppTransportSecurity</key>
<dict>
  <key>NSAllowsLocalNetworking</key>
  <true/>
</dict>
```

### Mac Catalyst

Uses the host network directly. `localhost` and dev certificates work as-is.

## Blazor Hybrid + Aspire

For MAUI Blazor Hybrid apps calling Aspire services, authentication happens
at the **MAUI layer** (MSAL.NET), not in the Blazor WebView. The pattern is:

1. MAUI handles sign-in via `IAuthService` (MSAL.NET)
2. A custom `AuthenticationStateProvider` exposes auth state to Blazor
3. `HttpClient` with `DelegatingHandler` attaches bearer tokens automatically
4. Blazor components use `@inject IWeatherApiClient` normally

See the `maui-authentication` skill's "Blazor Hybrid" section for the
`MsalAuthenticationStateProvider` implementation.

## Aspire Service Defaults in MAUI

MAUI projects typically do **not** use Aspire's `AddServiceDefaults()` extension
(which adds OpenTelemetry, health checks, etc.) because:

- MAUI apps are not ASP.NET Core hosts
- OpenTelemetry for mobile has different requirements
- Health check endpoints don't apply to client apps

If you want telemetry from MAUI to correlate with Aspire traces, configure
OpenTelemetry separately using the `System.Diagnostics` APIs.

## Deployment

When deploying the Aspire backend to Azure (Container Apps, App Service, etc.),
update the MAUI app's API base URL to point to the deployed endpoint:

```csharp
// Release config
public static string BaseUrl => "https://myapi.azurecontainerapps.io";
```

The Entra ID app registration's redirect URIs are platform-specific and
don't change between local dev and production.

## Common Gotchas

1. **MAUI is NOT an Aspire project.** Don't try to add it with
   `builder.AddProject<Projects.MyMauiApp>()` — it doesn't implement
   `IServiceMetadata` and can't be orchestrated.

2. **Android emulator can't reach `localhost`.** Always use `10.0.2.2` for the
   host loopback on Android emulators.

3. **HTTPS certificate trust on Android.** The emulator doesn't trust the .NET
   dev cert. Use HTTP or bypass SSL validation during local development only.

4. **Don't use `https+http://` URIs in MAUI.** These are Aspire service
   discovery URIs — they only resolve inside the Aspire AppHost orchestration.

5. **Auth is public client, not confidential.** MAUI uses
   `PublicClientApplication` (MSAL.NET), not `AddMicrosoftIdentityWebApp`
   (confidential client / OIDC). The Entra Aspire auth skill is for the
   API/Blazor Server side; the MAUI side uses the `maui-authentication` skill.

6. **Aspire runs .NET 10+.** Ensure your MAUI project also targets `net10.0-*`.

## Related Skills & Resources

- **`maui-authentication`** — MSAL.NET setup, platform configs, auth service, bearer tokens
- **`maui-rest-api`** — HttpClient DI, JSON serialization, error handling
- **Entra ID skills** (from Microsoft): https://github.com/AzureAD/microsoft-identity-web/tree/master/.github/skills
  - `entra-id-aspire-authentication` — API JWT protection + Blazor Server auth
  - `entra-id-aspire-provisioning` — Automated app registration via Graph PowerShell
- **Azure-Samples/ms-identity-dotnetcore-maui** — MSAL.NET MAUI sample: https://github.com/Azure-Samples/ms-identity-dotnetcore-maui
- **Aspire docs**: https://learn.microsoft.com/dotnet/aspire
