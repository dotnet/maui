# Essentials auth sample server

A single developer-facing web app that backs **both** .NET MAUI Essentials auth samples, so testing
is "launch one web app, run the MAUI app":

- **Passkeys (WebAuthn / FIDO2)** — the relying-party (RP) server for the cross-platform Passkeys
  Essentials API. It's the **default .NET Blazor Web App template with ASP.NET Core Identity
  (Individual Accounts)** (built-in passkey support since .NET 10) plus a small native-app-facing JSON
  API. Because it's the *official* ASP.NET Core Identity passkey implementation, it doubles as an
  interop conformance check across Apple, Android, and Windows.
- **Web Authenticator** — a "pass-through" OAuth backend (Microsoft, Google, Facebook, Apple). The app
  opens `/mobileauth/{scheme}` in a browser; the server challenges the provider and redirects back to
  the app's `xamarinessentials://` callback with the tokens.

Both share the same authentication stack: the OAuth providers sign into
`IdentityConstants.ExternalScheme` (the app's default sign-in scheme), which `/mobileauth` reads back —
the standard ASP.NET Core external-login pattern.

> ⚠️ This is a **local dev tool**. It auto-creates passkey users with no password, and the OAuth flow
> is a thin pass-through. Do **not** use these shortcuts in production.

## What's in here

| Piece | Where | Purpose |
| --- | --- | --- |
| Blazor Web App + Identity | `Components/`, `Data/`, `Program.cs` | The stock template. Its web UI (`/Account/Manage/Passkeys`) works out of the box. |
| Native passkey API | `PasskeyApiEndpoints.cs` | `/passkeys/register/begin` · `/register/finish` · `/login/begin` · `/login/finish` — JSON in / JSON out, cookie-correlated, no antiforgery. |
| OAuth pass-through | `OAuthPassthroughEndpoints.cs` | `/mobileauth/{scheme}` for the WebAuthenticator sample + the Apple domain-verification file. |
| Domain association | `WellKnownEndpoints.cs` | Serves `/.well-known/assetlinks.json` (Android) and `/.well-known/apple-app-site-association` (Apple) from config. |
| Config | `appsettings.json` → `Passkeys`, user-secrets | Passkeys RP ID / origins / fingerprints, and OAuth provider client ids/secrets. |

## Run it

```bash
dotnet run --project src/Essentials/samples/Samples.WebServer --launch-profile http
# listens on http://localhost:5177  (https://localhost:7235 with the "https" profile)
```

The SQLite database (`Data/app.db`) is created automatically on first run. Browse to the printed URL
for the built-in web passkey UI, or point the MAUI sample pages at the base URL.

Local URLs only exercise the round-trip; a **real** passkey ceremony or external OAuth sign-in needs a
public HTTPS domain — see the next section.

## Get a stable public domain (dev tunnel)

Passkeys are bound to a domain (the RP ID) and OAuth redirect URIs must be a fixed public HTTPS URL —
`localhost` won't work from a phone. Use a **dev tunnel with a persistent id** so the domain stays the
same every run (and is shared by both samples). Install once: `brew install --cask devtunnel` (macOS) ·
`winget install Microsoft.devtunnel` (Windows) · <https://aka.ms/devtunnels/download> (Linux).

### Automated (recommended)

From `src/Essentials/samples`, run the helper — it provisions a persistent tunnel and writes the
resulting domain straight into this server's user-secrets, so you don't edit any files:

```bash
pwsh ./Configure.ps1
```

It prints the public `https://…devtunnels.ms` URL to paste into **both** sample pages, and sets
`Passkeys:ServerDomain` + the web origin for you. Then host the tunnel and run the server:

```bash
devtunnel host maui-essentials                                  # terminal 1
dotnet run --project . --launch-profile http                    # terminal 2
```

### Manual

```bash
devtunnel user login
devtunnel create maui-essentials --allow-anonymous
devtunnel port create maui-essentials -p 5177 --protocol http
devtunnel host maui-essentials
```

Note the printed `https://<id>-5177.<region>.devtunnels.ms` URL. The host (no scheme) is your
passkeys **RP ID** / `ServerDomain`; set it via user-secrets:

```bash
dotnet user-secrets --project src/Essentials/samples/Samples.WebServer \
  set "Passkeys:ServerDomain" "<id>-5177.<region>.devtunnels.ms"
```

Then in the MAUI sample, set the **Passkeys** and **Web Authenticator** server URLs to the full
`https://…devtunnels.ms` URL.

---

## Passkeys sample

### The native API

All four endpoints are `POST`. The challenge state is stored in the ASP.NET Core Identity auth cookie
between `begin` and `finish`, so the native client **must** use a cookie container (the MAUI sample
does).

```
POST /passkeys/register/begin?username=alice@example.com   -> PublicKeyCredentialCreationOptions JSON
POST /passkeys/register/finish   (body: attestation JSON)  -> { registered, username }
POST /passkeys/login/begin?username=alice@example.com      -> PublicKeyCredentialRequestOptions JSON
POST /passkeys/login/finish      (body: assertion JSON)    -> { authenticated, username }
```

`/login/begin` may be called **without** `username` for username-less / discoverable-credential
sign-in.

### Native origin / domain association

For on-device ceremonies the RP must trust each app's native origin, and each platform must trust the
RP domain back:

| Platform | App proves domain via | RP must accept origin | RP serves |
| --- | --- | --- | --- |
| **Apple** (iOS/iPadOS/Mac Catalyst) | `webcredentials:<domain>` associated-domains entitlement | `https://<domain>` | `/.well-known/apple-app-site-association` listing `<TeamID>.<BundleID>` |
| **Android** | `assetlinks.json` digital asset link | `android:apk-key-hash:<base64url-sha256-of-signing-cert>` | `/.well-known/assetlinks.json` with package + SHA-256 fingerprint |
| **Windows** | n/a (Win11 platform) | `https://<domain>` | — |

Fill in `Passkeys:AllowedOrigins` (add the Android `android:apk-key-hash:…` origin),
`Passkeys:Android` (package name + `keytool`/`apksigner` SHA-256 fingerprints), and `Passkeys:Apple`
(`<TeamID>.<BundleID>`) once you know your signing identities. `https://<ServerDomain>` is always
accepted automatically. To compute the Android hash origin from a SHA-256 fingerprint, base64url-encode
the raw 32 bytes and prefix `android:apk-key-hash:`.

### Reset

Delete `Data/app.db*` to wipe all registered users/passkeys, or just restart — the schema is
re-created on startup.

---

## Web Authenticator sample

The redirect (callback) path is fixed by ASP.NET Core per provider. Register
`https://<tunnel-host>/signin-<provider>` **exactly** with each provider you want to test:

| Provider  | Redirect URI to register                    | Where to create the app / get id + secret | Guide |
| --------- | ------------------------------------------- | ----------------------------------------- | ----- |
| Google    | `https://<tunnel-host>/signin-google`       | [Google Cloud Console → Credentials](https://console.cloud.google.com/apis/credentials) | [MS docs](https://learn.microsoft.com/aspnet/core/security/authentication/social/google-logins) |
| Microsoft | `https://<tunnel-host>/signin-microsoft`    | [Entra ID → App registrations](https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade) | [MS docs](https://learn.microsoft.com/aspnet/core/security/authentication/social/microsoft-logins) |
| Facebook  | `https://<tunnel-host>/signin-facebook`     | [Meta for Developers → Apps](https://developers.facebook.com/apps) | [MS docs](https://learn.microsoft.com/aspnet/core/security/authentication/social/facebook-logins) |
| Apple     | `https://<tunnel-host>/signin-apple`        | [Apple Developer → Identifiers](https://developer.apple.com/account/resources/identifiers/list) (see [Apple](#apple-sign-in-with-apple) below) | [Apple docs](https://developer.apple.com/help/account/configure-app-capabilities/configure-sign-in-with-apple-for-the-web/) |

Only configured providers are enabled; an unconfigured one returns a friendly `400`.

### Google

1. Open [Google Cloud Console → Credentials](https://console.cloud.google.com/apis/credentials) and
   pick/create a project.
2. Configure the **OAuth consent screen** (User type: External). While it's in "Testing", add your
   own Google account under **Test users**.
3. **Create credentials → OAuth client ID → Application type: Web application**.
4. Under **Authorized redirect URIs** add `https://<tunnel-host>/signin-google`.
5. Copy the **Client ID** → `GoogleClientId` and **Client secret** → `GoogleClientSecret`.

### Microsoft

1. Open [Entra ID → App registrations → New registration](https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade).
2. For consumer sign-in choose **Accounts in any organizational directory and personal Microsoft
   accounts**.
3. Add a **Redirect URI**: platform **Web**, value `https://<tunnel-host>/signin-microsoft`.
4. Copy the **Application (client) ID** → `MicrosoftClientId`.
5. **Certificates & secrets → New client secret**; copy its **Value** → `MicrosoftClientSecret`.

### Facebook

1. Open [Meta for Developers → Apps](https://developers.facebook.com/apps) → **Create app** →
   add the **Facebook Login** product.
2. **App settings → Basic**: copy **App ID** → `FacebookAppId` and **App secret** →
   `FacebookAppSecret`.
3. **Facebook Login → Settings → Valid OAuth Redirect URIs**: add
   `https://<tunnel-host>/signin-facebook`.

### Apple (Sign in with Apple)

Apple is the most involved and needs a **paid Apple Developer Program** membership. You create three
things — an **App ID**, a **Services ID** (this is your client id), and a **key** — then verify your
domain.

1. **App ID** — [Identifiers](https://developer.apple.com/account/resources/identifiers/list) → **+**
   → **App IDs** → enable the **Sign In with Apple** capability.
2. **Services ID** (this is `AppleClientId`) — Identifiers → **+** → **Services IDs**
   (e.g. `com.yourorg.maui.webauth`). Enable **Sign In with Apple**, then **Configure**:
   - **Primary App ID**: the App ID from step 1.
   - **Domains and Subdomains**: `<tunnel-host>` (host only, no `https://`, no path, no wildcards).
   - **Return URLs**: `https://<tunnel-host>/signin-apple`.
3. **Verify the domain** — Apple gives you an `apple-developer-domain-association.txt` file. Save it
   next to this project; the server already serves it at
   `/.well-known/apple-developer-domain-association.txt`. Start the tunnel + server, then click
   **Verify** in the Apple portal.
4. **Key** — [Keys](https://developer.apple.com/account/resources/authkeys/list) → **+** → enable
   **Sign In with Apple** → **Download** the `.p8` (you only get it once). Save it next to this
   project as `AuthKey_<KeyId>.p8`. The **Key ID** → `AppleKeyId`.
5. **Team ID** — shown under **Membership** → `AppleTeamId`.
6. Set `AppleClientId` to the **Services ID** identifier from step 2 (NOT the App ID).

Apple gotchas:
- Apple returns the user's **name/email only on the first authorization** for a given app+user. To
  test the first-run flow again, remove the app at
  [account.apple.com → Sign in & Security → Sign in with Apple](https://account.apple.com/account/manage).
- Apple requires a **public HTTPS** domain — a dev tunnel works, `localhost` does not.
- The `.p8` private key and `apple-developer-domain-association.txt` are git-ignored here — keep them
  safe and don't commit them.

Reference: [Apple: Configure Sign in with Apple for the web](https://developer.apple.com/help/account/configure-app-capabilities/configure-sign-in-with-apple-for-the-web/) ·
[AspNet.Security.OAuth.Providers (Apple)](https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers).

### Configure secrets

Store OAuth secrets with **user-secrets** (recommended) or environment variables — only configured
providers are enabled:

```bash
cd src/Essentials/samples/Samples.WebServer

dotnet user-secrets set "GoogleClientId"     "<id>"
dotnet user-secrets set "GoogleClientSecret" "<secret>"

dotnet user-secrets set "MicrosoftClientId"     "<id>"
dotnet user-secrets set "MicrosoftClientSecret" "<secret>"

dotnet user-secrets set "FacebookAppId"     "<id>"
dotnet user-secrets set "FacebookAppSecret" "<secret>"

# Apple: the Services ID + key ids. Also drop AuthKey_<KeyId>.p8 next to the project.
dotnet user-secrets set "AppleClientId" "<services-id>"
dotnet user-secrets set "AppleKeyId"    "<key-id>"
dotnet user-secrets set "AppleTeamId"   "<team-id>"
```

Then run the server + host the tunnel, and set the **OAuth server base URL** on the sample's **Web
Authenticator** page to `https://<tunnel-host>`.

See [`../TESTING.md`](../TESTING.md) for the condensed end-to-end steps.
