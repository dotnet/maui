# Passkeys sample server (relying party)

The developer-facing **relying-party (RP) server** for the cross-platform **Passkeys (WebAuthn / FIDO2)**
Essentials API sample. The MAUI app registers and signs in against it, so testing is "launch one web
app, run the MAUI app".

It is the **stock .NET Blazor Web App template with ASP.NET Core Identity (Individual Accounts)** —
which has had built-in passkey support since .NET 10 — plus a small, clearly-scoped set of additions
for driving passkeys from a *native* client. Because the ceremony logic is the *official* ASP.NET Core
Identity passkey implementation, this doubles as an interop conformance check across Apple, Android, and
Windows.

> ⚠️ This is a **local dev tool** — it relaxes a default for convenience (no email confirmation) and its
> native `/passkeys/*` API authenticates the session with a **cookie** rather than a bearer token. That's
> fine here but is **not** how a production native app should do it — see
> [Authentication & CSRF](#authentication--csrf-why-cookies-here) below.

## How this was scaffolded (and exactly what we changed)

This project starts from the default template:

```bash
dotnet new blazor --auth Individual -n Essentials.Samples.WebServer
```

Everything else is stock. The complete set of changes on top of that template is:

### Files added (4)

| File | Why |
| --- | --- |
| `Components/Account/PasskeyApiEndpoints.cs` | Native-app-facing passkey ceremony API: `POST /passkeys/register/begin` · `/register/finish` · `/login/begin` · `/login/finish`. Lives next to the template's own identity endpoints. The template's passkey UI (`/Account/Manage/Passkeys`) is browser + antiforgery based; a native `HttpClient` needs a plain JSON-in/JSON-out API with the WebAuthn challenge correlated through the Identity cookie. |
| `WellKnownEndpoints.cs` | Serves `/.well-known/assetlinks.json` (Android Digital Asset Links) and `/.well-known/apple-app-site-association` (Apple) from config, so real devices trust this domain as the credential provider. |
| `.gitignore` | Ignores the runtime SQLite database (`Data/app.db*`). |
| `README.md` | This file. |

### Files modified (4)

| File | Change |
| --- | --- |
| `Program.cs` | See the itemized list below. |
| `Essentials.Samples.WebServer.csproj` | TFM set to the repo's `$(_MauiDotNetTfm)`; EF Core package versions pinned to the repo's servicing band via `_PasskeysEfCoreVersion` (so they restore from the repo's own feeds, no nuget.org); `IsPackable=false`; repo-friendly `RootNamespace` / `UserSecretsId`. Removed the template's committed `Data/app.db` `<None>` item — we create the schema at runtime instead (see `Program.cs`) and git-ignore the database. |
| `appsettings.json` | Added the `Passkeys` config section (`ServerDomain`, `AllowedOrigins`, `Android`, `Apple`). Nothing else touched. |
| `Properties/launchSettings.json` | Changed the dev HTTP port to **5177** (and HTTPS to 7235) so it matches the dev tunnel port and `Configure.ps1`. |

`Program.cs` additions, all clearly grouped:

1. **Native username/password bootstrap** — `authBuilder.AddBearerToken(IdentityConstants.BearerScheme)`,
   `.AddApiEndpoints()`, and `app.MapGroup("/account").MapIdentityApi<ApplicationUser>()`. This gives the
   native app `/account/register` and `/account/login?useCookies=true`, so it can authenticate (and set
   the Identity cookie) **before** enrolling a passkey — no browser, no OAuth.
2. **Passkey relying-party config** — `IdentityPasskeyOptions.ServerDomain` (the RP ID) and a
   `ValidateOrigin` that additionally accepts the platform *native* origins (Android
   `android:apk-key-hash:<hash>`, Apple `https://<domain>`).
3. **Forwarded headers** — `ForwardedHeadersOptions` + `app.UseForwardedHeaders()` so that, behind a dev
   tunnel (public HTTPS → local HTTP), the effective origin still matches the RP ID.
4. **Runtime DB creation** — `db.Database.Migrate()` at startup, so the server runs with no committed
   `app.db` and no manual `ef database update`.
5. **Dev convenience** — `options.SignIn.RequireConfirmedAccount = false` (there is no real email sender).
6. **Endpoint wiring** — `app.MapNativePasskeyApi()` and `app.MapDomainAssociation(...)` to expose the two
   added files above.

## What's in here

| Piece | Where | Purpose |
| --- | --- | --- |
| Blazor Web App + Identity | `Components/`, `Data/`, most of `Program.cs` | The stock template. Its web passkey UI (`/Account/Manage/Passkeys`) works out of the box. |
| Native passkey API | `Components/Account/PasskeyApiEndpoints.cs` | `/passkeys/register/begin` · `/register/finish` · `/login/begin` · `/login/finish` — JSON in / JSON out, cookie-correlated, no antiforgery. |
| Native bootstrap auth | `MapIdentityApi` under `/account` | `/account/register`, `/account/login?useCookies=true` — username/password sign-in before enrolling a passkey. |
| Domain association | `WellKnownEndpoints.cs` | Serves `/.well-known/assetlinks.json` (Android) and `/.well-known/apple-app-site-association` (Apple) from config. |
| Config | `appsettings.json` → `Passkeys`, user-secrets | RP ID / origins / fingerprints. |

## Run it

```bash
dotnet run --project src/Essentials/samples/Samples.WebServer --launch-profile http
# listens on http://localhost:5177  (https://localhost:7235 with the "https" profile)
```

The SQLite database (`Data/app.db`) is created automatically on first run. Browse to the printed URL for
the built-in web passkey UI, or point the MAUI **Passkeys** sample page at the base URL.

Local URLs only exercise the round-trip; a **real** on-device passkey ceremony needs a public HTTPS
domain — see the next section.

## Get a stable public domain (dev tunnel)

Passkeys are bound to a domain (the RP ID) and `localhost` won't work from a phone. Use a **dev tunnel**
so the server is reachable at a real public HTTPS host. Install once:
`brew install --cask devtunnel` (macOS) · `winget install Microsoft.devtunnel` (Windows) ·
<https://aka.ms/devtunnels/download> (Linux).

### Automated (recommended)

From `src/Essentials/samples`, run the helper — it provisions the tunnel and writes the resulting domain
(plus the Android package/fingerprint origins) straight into this server's user-secrets, so you don't
edit any files:

```bash
pwsh ./Configure.ps1
```

It prints the public `https://…devtunnels.ms` URL to paste into the sample page, and sets
`Passkeys:ServerDomain` + the origins for you. Then host the tunnel and run the server:

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

Note the printed `https://<id>-5177.<region>.devtunnels.ms` URL. The host (no scheme) is your passkeys
**RP ID** / `ServerDomain`; set it via user-secrets:

```bash
dotnet user-secrets --project src/Essentials/samples/Samples.WebServer \
  set "Passkeys:ServerDomain" "<id>-5177.<region>.devtunnels.ms"
```

Then in the MAUI sample, set the **Passkeys** page server URL to the full `https://…devtunnels.ms` URL.

## The native API

All four endpoints are `POST`. The WebAuthn challenge is correlated between `begin` and `finish` by a
short-lived framework cookie (`Identity.TwoFactorUserId`, set by the built-in ceremony methods), so the
native client **must** use a cookie container (the MAUI sample does).

```
POST /passkeys/register/begin    (signed-in session required)  -> PublicKeyCredentialCreationOptions JSON
POST /passkeys/register/finish   (body: attestation JSON)      -> { registered, username }
POST /passkeys/login/begin?username=<email>                    -> PublicKeyCredentialRequestOptions JSON
POST /passkeys/login/finish      (body: assertion JSON)        -> { authenticated, username }
```

`register/begin` enrolls a passkey for the **currently signed-in** user (identified by the Identity
session cookie), so the caller must sign in first — the "add a passkey after you log in" flow. Anonymous
requests get a `401`; the server never creates an account from an arbitrary posted username.
`login/begin` may be called **without** `username` for username-less / discoverable-credential sign-in.

## Authentication & CSRF (why cookies here)

The native `/passkeys/*` (and `/account/logout`) endpoints are driven by a native `HttpClient`, not a
browser `<form>`, and a native client has no antiforgery token to send. They call `.DisableAntiforgery()`
so the `app.UseAntiforgery()` middleware doesn't reject them. (Without it the middleware inconsistently
blocks the endpoints that inject `HttpContext` with an opaque `400 "The request has an incorrect
Content-type."`.) Combined with the fact that the **login session** is a cookie (`/login/finish` calls
`SignInAsync`), that's the classic CSRF-susceptible shape: a cookie-authed, state-changing POST with no
antiforgery token. **Do not copy this pattern to non-WebAuthn endpoints.**

Why it's nonetheless safe *here*: a WebAuthn `finish` payload is a signature over
`(challenge + origin + rpId)` from a private key that never leaves the authenticator, so it **cannot be
forged or replayed** — the ceremony is CSRF-resistant by construction.

**How a production native app should do it:** authenticate the session with a **bearer token**, not a
cookie. `MapIdentityApi` already supports it — `POST /account/login` *without* `?useCookies=true` returns
an `access_token`; the app sends `Authorization: Bearer …`. Because browsers never auto-attach
`Authorization` headers cross-site, CSRF becomes structurally impossible (no antiforgery needed).

One nuance worth knowing: switching the *session* to bearer does **not** make passkeys fully cookieless.
The built-in `MakePasskey*OptionsAsync` / `PerformPasskey*Async` methods stash the per-ceremony challenge
in that transient `Identity.TwoFactorUserId` cookie regardless — so a native client still round-trips
that one short-lived cookie between `begin` and `finish`. It carries only a challenge (no identity) and,
per the point above, can't be abused. This sample keeps the session on a cookie purely to stay a minimal,
single-auth-mode reference; the switch to bearer is small (~30 lines) if you want to demonstrate it.

## Native origin / domain association

For on-device ceremonies the RP must trust each app's native origin, and each platform must trust the RP
domain back:

| Platform | App proves domain via | RP must accept origin | RP serves |
| --- | --- | --- | --- |
| **Apple** (iOS/iPadOS/Mac Catalyst) | `webcredentials:<domain>` associated-domains entitlement | `https://<domain>` | `/.well-known/apple-app-site-association` listing `<TeamID>.<BundleID>` |
| **Android** | `assetlinks.json` digital asset link | `android:apk-key-hash:<base64url-sha256-of-signing-cert>` | `/.well-known/assetlinks.json` with package + SHA-256 fingerprint |
| **Windows** | n/a (Win11 platform) | `https://<domain>` | — |

Fill in `Passkeys:AllowedOrigins` (add the Android `android:apk-key-hash:…` origin), `Passkeys:Android`
(package name + `keytool`/`apksigner` SHA-256 fingerprints), and `Passkeys:Apple` (`<TeamID>.<BundleID>`)
once you know your signing identities — `Configure.ps1` fills the Android values for you. `https://<ServerDomain>`
is always accepted automatically. To compute the Android hash origin from a SHA-256 fingerprint,
base64url-encode the raw 32 bytes and prefix `android:apk-key-hash:`.

## Reset

Delete `Data/app.db*` to wipe all registered users/passkeys, or just restart — the schema is re-created
on startup.

See [`../TESTING.md`](../TESTING.md) for the condensed end-to-end steps.
