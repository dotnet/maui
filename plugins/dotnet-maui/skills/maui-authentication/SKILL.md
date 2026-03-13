---
name: maui-authentication
description: >
  Add authentication to .NET MAUI apps. Covers WebAuthenticator for generic
  OAuth 2.0 / social login, and MSAL.NET for Microsoft Entra ID (Azure AD)
  with broker support (Microsoft Authenticator), token caching, Conditional
  Access, platform-specific setup (Android, iOS, Windows), DelegatingHandler
  for bearer token API calls, and Blazor Hybrid integration.
  USE FOR: "add authentication", "OAuth login", "MSAL.NET", "social login",
  "WebAuthenticator", "Entra ID MAUI", "Azure AD login", "Google login MAUI",
  "Apple login MAUI", "token caching", "bearer token", "sign in".
  DO NOT USE FOR: secure local storage of tokens (use maui-secure-storage),
  Aspire-specific auth setup (use maui-aspire), or server-side Entra ID provisioning
  (use entra-id-aspire-provisioning).
---

# .NET MAUI Web Authentication

Use `WebAuthenticator` to launch browser-based auth flows (OAuth 2.0, OpenID Connect, social login) and receive callback URIs back into the app.

## Core API

```csharp
try
{
    var result = await WebAuthenticator.Default.AuthenticateAsync(
        new WebAuthenticatorOptions
        {
            Url = new Uri("https://your-server.com/auth/login"),
            CallbackUrl = new Uri("myapp://callback"),
            PrefersEphemeralWebBrowserSession = true // iOS 13+: private session, no shared cookies
        });

    string accessToken = result.AccessToken;
    string refreshToken = result.Properties["refresh_token"];
}
catch (TaskCanceledException)
{
    // User cancelled the auth flow — do not treat as an error
}
```

- `Url` — the authorization endpoint (your server or identity provider).
- `CallbackUrl` — the URI scheme your app is registered to handle.
- `PrefersEphemeralWebBrowserSession` — when `true` (iOS 13+), uses a private browser session that does not share cookies or data with Safari. Useful for forcing login prompts.

## Platform Setup

### Android

#### 1. Callback Activity

Create a subclass of `WebAuthenticatorCallbackActivity` with an `IntentFilter` matching your callback URI scheme:

```csharp
using Android.App;
using Android.Content.PM;

namespace MyApp.Platforms.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(
    new[] { Android.Content.Intent.ActionView },
    Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
    DataScheme = "myapp",
    DataHost = "callback")]
public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}
```

#### 2. Package Visibility (Android 11+)

Add a `<queries>` element to `AndroidManifest.xml` so the app can resolve browser intents:

```xml
<manifest>
  <queries>
    <intent>
      <action android:name="android.support.customtabs.action.CustomTabsService" />
    </intent>
  </queries>
</manifest>
```

### iOS / Mac Catalyst

Register the callback URI scheme in `Info.plist`:

```xml
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleURLName</key>
    <string>myapp</string>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>myapp</string>
    </array>
  </dict>
</array>
```

No additional code is needed — MAUI handles the callback automatically on Apple platforms.

### Windows

Register the protocol in `Package.appxmanifest`:

```xml
<Extensions>
  <uap:Extension Category="windows.protocol">
    <uap:Protocol Name="myapp">
      <uap:DisplayName>My App Auth</uap:DisplayName>
    </uap:Protocol>
  </uap:Extension>
</Extensions>
```

> [!WARNING]
> Windows WebAuthenticator is currently broken. See [dotnet/maui#2702](https://github.com/dotnet/maui/issues/2702). Consider using MSAL or a WinUI-specific workaround for Windows auth flows.

## Apple Sign In

Use `AppleSignInAuthenticator` on iOS 13+ for native Sign in with Apple:

```csharp
var result = await AppleSignInAuthenticator.Default.AuthenticateAsync(
    new AppleSignInAuthenticator.Options
    {
        IncludeFullNameScope = true,
        IncludeEmailScope = true
    });

string idToken = result.IdToken;
string name = result.Properties["name"];
```

Apple only returns the user's name and email on **first** sign-in. Cache them immediately.

## Security: Use a Server Backend

> [!IMPORTANT]
> Never embed client secrets, API keys, or signing keys in a mobile app binary. They can be extracted trivially.

The recommended pattern:

1. App calls `WebAuthenticator` pointing to **your server** endpoint.
2. Server initiates the OAuth flow with the identity provider (holds the client secret).
3. Provider redirects back to your server with an auth code.
4. Server exchanges the code for tokens and returns them to the app via the callback URI.

## Token Persistence with SecureStorage

Store tokens securely using `SecureStorage` (Keychain on iOS, Keystore on Android):

```csharp
// Save
await SecureStorage.Default.SetAsync("access_token", accessToken);
await SecureStorage.Default.SetAsync("refresh_token", refreshToken);

// Retrieve
string token = await SecureStorage.Default.GetAsync("access_token");

// Clear on logout
SecureStorage.Default.RemoveAll();
```

## DI-Friendly Auth Service

Wrap authentication in an injectable service for testability:

```csharp
public interface IAuthService
{
    Task<AuthResult> LoginAsync(CancellationToken ct = default);
    Task LogoutAsync();
    Task<string?> GetAccessTokenAsync();
}

public record AuthResult(bool Success, string? ErrorMessage = null);

public class WebAuthService : IAuthService
{
    private const string AuthUrl = "https://your-server.com/auth/login";
    private const string CallbackUrl = "myapp://callback";

    public async Task<AuthResult> LoginAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = new Uri(AuthUrl),
                    CallbackUrl = new Uri(CallbackUrl),
                    PrefersEphemeralWebBrowserSession = true
                });

            await SecureStorage.Default.SetAsync("access_token", result.AccessToken);
            return new AuthResult(true);
        }
        catch (TaskCanceledException)
        {
            return new AuthResult(false, "Login cancelled.");
        }
    }

    public Task LogoutAsync()
    {
        SecureStorage.Default.RemoveAll();
        return Task.CompletedTask;
    }

    public Task<string?> GetAccessTokenAsync()
        => SecureStorage.Default.GetAsync("access_token");
}
```

Register in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<IAuthService, WebAuthService>();
```

## Checklist

- [ ] Callback URI scheme matches across all platform configs and `CallbackUrl`.
- [ ] Android has `WebAuthenticatorCallbackActivity` with correct `IntentFilter`.
- [ ] Android 11+ has `<queries>` for Custom Tabs in the manifest.
- [ ] iOS/Mac Catalyst has `CFBundleURLTypes` in `Info.plist`.
- [ ] Client secrets are on the server, not in the app.
- [ ] Tokens stored with `SecureStorage`, cleared on logout.
- [ ] `TaskCanceledException` handled gracefully in UI.

---

# Microsoft Entra ID Authentication with MSAL.NET

For authenticating users against **Microsoft Entra ID** (formerly Azure AD), use **MSAL.NET** (`Microsoft.Identity.Client`) instead of `WebAuthenticator`. MSAL.NET provides:

- Interactive sign-in via system browser or broker (Microsoft Authenticator)
- Silent token refresh from cache
- Conditional Access and MFA support
- Broker-based SSO across apps on mobile

## Entra ID App Registration (Cloud Side)

Before writing code, you need an Entra ID app registration. The Entra team provides
an **AI skill that automates this** using Microsoft Graph PowerShell.

### Install the Entra provisioning skill

```bash
# GitHub Copilot CLI — from your repo root:
mkdir -p .github/skills && cd .github/skills
curl -LO https://aka.ms/msidweb/aspire/entra-id-provisioning-skill
# Or clone the full repo:
git clone https://github.com/AzureAD/microsoft-identity-web.git /tmp/msidweb
cp -R /tmp/msidweb/.github/skills/entra-id-aspire-provisioning .github/skills/

# Claude Code:
cp -R /tmp/msidweb/.github/skills/entra-id-aspire-provisioning ~/.claude/skills/
```

Then ask your AI assistant: **"Provision Entra ID app registrations for my MAUI app"**

The skill source is at: https://github.com/AzureAD/microsoft-identity-web/tree/master/.github/skills

### Manual registration (if not using the skill)

1. Go to [Microsoft Entra admin center](https://entra.microsoft.com) → App registrations → New registration
2. Name: your app name
3. Supported account types: choose your scenario (single tenant, multi-tenant, personal accounts)
4. **Do NOT set a redirect URI yet** — add platform-specific URIs after:
   - Add a platform → **Mobile and desktop applications**
   - Android: `msal{ClientId}://auth`
   - iOS: `msauth.{BundleId}://auth`
   - Windows/macOS: `http://localhost`
5. Note the **Application (client) ID** and **Directory (tenant) ID**
6. Under API permissions, add `User.Read` (Microsoft Graph) for basic profile access
7. If calling your own API: register the API app separately, expose a scope (e.g., `access_as_user`), then add that scope as a permission to the client app

## Add MSAL.NET Package

```bash
dotnet add package Microsoft.Identity.Client
```

## Configuration

Add to your project (e.g., `appsettings.json` or a static config class):

```json
{
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/{TenantId}",
    "TenantId": "<your-tenant-id>",
    "ClientId": "<your-client-id>",
    "Scopes": "User.Read"
  }
}
```

Or use a config class for mobile (avoids file I/O issues):

```csharp
public static class AuthConfig
{
    public const string TenantId = "<your-tenant-id>";
    public const string ClientId = "<your-client-id>";
    public const string Authority = $"https://login.microsoftonline.com/{TenantId}";
    public static readonly string[] Scopes = ["User.Read"];

    // Platform-specific redirect URIs
    public const string AndroidRedirectUri = $"msal{ClientId}://auth";
    public const string IosRedirectUri = $"msauth.com.companyname.myapp://auth";
}
```

## MSAL Auth Service

Wrap MSAL in an injectable service:

```csharp
public interface IAuthService
{
    Task<AuthenticationResult?> SignInAsync(CancellationToken ct = default);
    Task<AuthenticationResult?> AcquireTokenSilentAsync(CancellationToken ct = default);
    Task SignOutAsync();
    Task<string?> GetAccessTokenAsync(string[] scopes, CancellationToken ct = default);
    bool IsSignedIn { get; }
}
```

```csharp
using Microsoft.Identity.Client;

public class MsalAuthService : IAuthService
{
    private readonly IPublicClientApplication _pca;
    private readonly string[] _defaultScopes;

    public bool IsSignedIn => _cachedAccount != null;
    private IAccount? _cachedAccount;

    public MsalAuthService()
    {
        _defaultScopes = AuthConfig.Scopes;

        var builder = PublicClientApplicationBuilder
            .Create(AuthConfig.ClientId)
            .WithAuthority(AuthConfig.Authority)
            .WithIosKeychainSecurityGroup("com.microsoft.adalcache");

#if ANDROID
        builder = builder.WithRedirectUri(AuthConfig.AndroidRedirectUri)
                         .WithParentActivityOrWindow(() => Platform.CurrentActivity);
#elif IOS || MACCATALYST
        builder = builder.WithRedirectUri(AuthConfig.IosRedirectUri);
#else
        builder = builder.WithRedirectUri("http://localhost");
#endif

        // Enable broker (Microsoft Authenticator / Company Portal) on mobile
#if ANDROID || IOS
        builder = builder.WithBroker();
#endif

        _pca = builder.Build();
    }

    public async Task<AuthenticationResult?> SignInAsync(CancellationToken ct = default)
    {
        // Try silent first (cached token / refresh token)
        var result = await AcquireTokenSilentAsync(ct);
        if (result != null) return result;

        // Interactive sign-in
        try
        {
            result = await _pca.AcquireTokenInteractive(_defaultScopes)
                .WithLoginHint(_cachedAccount?.Username)
#if ANDROID
                .WithParentActivityOrWindow(Platform.CurrentActivity)
#endif
                .ExecuteAsync(ct);

            _cachedAccount = result.Account;
            return result;
        }
        catch (MsalClientException ex) when (ex.ErrorCode == "authentication_canceled")
        {
            return null; // User cancelled — not an error
        }
    }

    public async Task<AuthenticationResult?> AcquireTokenSilentAsync(CancellationToken ct = default)
    {
        try
        {
            var accounts = await _pca.GetAccountsAsync();
            _cachedAccount = accounts.FirstOrDefault();

            if (_cachedAccount == null) return null;

            var result = await _pca.AcquireTokenSilent(_defaultScopes, _cachedAccount)
                .ExecuteAsync(ct);

            _cachedAccount = result.Account;
            return result;
        }
        catch (MsalUiRequiredException)
        {
            return null; // Token expired, interaction needed
        }
    }

    public async Task<string?> GetAccessTokenAsync(string[] scopes, CancellationToken ct = default)
    {
        var accounts = await _pca.GetAccountsAsync();
        var account = accounts.FirstOrDefault();
        if (account == null) return null;

        try
        {
            var result = await _pca.AcquireTokenSilent(scopes, account)
                .ExecuteAsync(ct);
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            // Re-authenticate interactively
            var result = await _pca.AcquireTokenInteractive(scopes)
#if ANDROID
                .WithParentActivityOrWindow(Platform.CurrentActivity)
#endif
                .ExecuteAsync(ct);
            _cachedAccount = result.Account;
            return result.AccessToken;
        }
    }

    public async Task SignOutAsync()
    {
        var accounts = await _pca.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _pca.RemoveAsync(account);
        }
        _cachedAccount = null;
    }
}
```

Register in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<IAuthService, MsalAuthService>();
```

## Platform Setup

### Android

#### 1. AndroidManifest.xml — Package visibility for broker & browsers

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
  <application android:allowBackup="true" />
  <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
  <uses-permission android:name="android.permission.INTERNET" />
  <queries>
    <package android:name="com.azure.authenticator" />
    <package android:name="com.microsoft.windowsintune.companyportal" />
    <intent>
      <action android:name="android.intent.action.VIEW" />
      <category android:name="android.intent.category.BROWSABLE" />
      <data android:scheme="https" />
    </intent>
    <intent>
      <action android:name="android.support.customtabs.action.CustomTabsService" />
    </intent>
  </queries>
</manifest>
```

#### 2. MainActivity.cs — Handle auth continuation

```csharp
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.Identity.Client;

namespace MyApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
    ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
    ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnActivityResult(int requestCode,
        [GeneratedEnum] Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        AuthenticationContinuationHelper
            .SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
    }
}
```

### iOS / Mac Catalyst

#### 1. Info.plist — Register redirect URI scheme

```xml
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleURLName</key>
    <string>com.companyname.myapp</string>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>msauth.com.companyname.myapp</string>
    </array>
  </dict>
</array>
```

#### 2. Entitlements.plist — Keychain sharing (required for token cache)

```xml
<key>keychain-access-groups</key>
<array>
  <string>$(AppIdentifierPrefix)com.microsoft.adalcache</string>
</array>
```

#### 3. AppDelegate.cs — Handle auth continuation (iOS)

```csharp
using Foundation;
using Microsoft.Identity.Client;
using UIKit;

namespace MyApp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool OpenUrl(UIApplication app, NSUrl url,
        NSDictionary options)
    {
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
        return base.OpenUrl(app, url, options);
    }
}
```

### Windows

No special platform setup. MSAL uses `http://localhost` redirect by default.
For broker (WAM) support on Windows, add:

```csharp
#if WINDOWS
using Microsoft.Identity.Client.Desktop;

builder = builder.WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows));
#endif
```

## Calling Protected APIs with Bearer Tokens

Create a `DelegatingHandler` that automatically attaches the access token:

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

Register in `MauiProgram.cs`:

```csharp
builder.Services.AddTransient(sp =>
    new AuthTokenHandler(
        sp.GetRequiredService<IAuthService>(),
        new[] { "api://<your-api-client-id>/access_as_user" }));

builder.Services.AddHttpClient<IMyApiClient, MyApiClient>(client =>
{
    client.BaseAddress = new Uri("https://your-api.azurewebsites.net/");
})
.AddHttpMessageHandler<AuthTokenHandler>();
```

## Login UI

### XAML

```xaml
<Button Text="{Binding LoginButtonText}"
        Command="{Binding LoginCommand}" />
```

```csharp
public partial class AuthViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty] string loginButtonText = "Sign In";
    [ObservableProperty] string? userName;

    public AuthViewModel(IAuthService auth) => _auth = auth;

    [RelayCommand]
    async Task Login()
    {
        if (_auth.IsSignedIn)
        {
            await _auth.SignOutAsync();
            UserName = null;
            LoginButtonText = "Sign In";
        }
        else
        {
            var result = await _auth.SignInAsync();
            if (result != null)
            {
                UserName = result.Account.Username;
                LoginButtonText = "Sign Out";
            }
        }
    }
}
```

### Blazor Hybrid

For MAUI Blazor Hybrid apps, authentication happens at the **MAUI layer** (not
in the WebView). Inject `IAuthService` into Blazor components:

```razor
@inject IAuthService Auth

<AuthorizeView>
    <Authorized>
        <span>Hello, @context.User.Identity?.Name</span>
        <button @onclick="SignOut">Sign Out</button>
    </Authorized>
    <NotAuthorized>
        <button @onclick="SignIn">Sign In</button>
    </NotAuthorized>
</AuthorizeView>

@code {
    async Task SignIn() => await Auth.SignInAsync();
    async Task SignOut() => await Auth.SignOutAsync();
}
```

You'll need a custom `AuthenticationStateProvider` that wraps MSAL:

```csharp
public class MsalAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _auth;

    public MsalAuthenticationStateProvider(IAuthService auth) => _auth = auth;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var result = await _auth.AcquireTokenSilentAsync();
        if (result == null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var identity = new ClaimsIdentity(result.ClaimsPrincipal.Claims, "msal");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
```

Register:

```csharp
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, MsalAuthenticationStateProvider>();
```

## Entra ID + Aspire Backend

If your MAUI app calls a **.NET Aspire**-hosted backend API, the API-side
JWT Bearer protection is handled by the Entra team's existing skills.

### Install the Entra authentication skill (for the API/backend)

```bash
# From your Aspire solution root:
mkdir -p .github/skills && cd .github/skills
curl -LO https://aka.ms/msidweb/aspire/entra-id-code-skill
# Or:
cp -R /tmp/msidweb/.github/skills/entra-id-aspire-authentication .github/skills/
```

Then ask: **"Add Entra ID authentication to my Aspire app"** — the skill handles
API JWT validation, token acquisition for service-to-service calls, and
`MicrosoftIdentityMessageHandler` setup.

Source: https://github.com/AzureAD/microsoft-identity-web/tree/master/.github/skills

## Choosing Between WebAuthenticator and MSAL.NET

| Criteria | WebAuthenticator | MSAL.NET |
|----------|-----------------|----------|
| Identity provider | Any OAuth 2.0 / OIDC | Microsoft Entra ID |
| Broker support (SSO) | ❌ No | ✅ Microsoft Authenticator, Company Portal |
| Conditional Access / MFA | ❌ Manual | ✅ Built-in |
| Token cache & refresh | ❌ Manual (SecureStorage) | ✅ Automatic |
| Complexity | Simple | More setup |
| Use when | Google, Apple, generic OIDC | Entra ID / Azure AD, Microsoft Graph |

## MSAL.NET Checklist

- [ ] `Microsoft.Identity.Client` NuGet package added
- [ ] App registration created in Entra ID with correct redirect URIs
- [ ] `AuthConfig` / `appsettings.json` has ClientId, TenantId, Scopes
- [ ] Android: `AndroidManifest.xml` has `<queries>` for broker and browsers
- [ ] Android: `MainActivity.OnActivityResult` calls `AuthenticationContinuationHelper`
- [ ] iOS: `Info.plist` has `CFBundleURLSchemes` with `msauth.{BundleId}`
- [ ] iOS: `Entitlements.plist` has keychain group `com.microsoft.adalcache`
- [ ] iOS: `AppDelegate.OpenUrl` calls `AuthenticationContinuationHelper`
- [ ] `IAuthService` registered as singleton in DI
- [ ] `DelegatingHandler` attached to `HttpClient` for API calls
- [ ] Login/logout UI wired up
- [ ] `MsalUiRequiredException` handled (triggers interactive sign-in)
- [ ] `MsalClientException` with `authentication_canceled` handled gracefully
