# Passkeys sample server (relying party)

The developer-facing **relying-party (RP) server** for the cross-platform **Passkeys (WebAuthn / FIDO2)**
Essentials API sample. The MAUI app registers and signs in against it, so testing is "launch one web
app, run the MAUI app".

It is a **headless minimal-API app** (no web UI) built on **ASP.NET Core Identity**, which has had
built-in passkey support since .NET 10. It exposes only what the native sample exercises:
username/password accounts, the passkey ceremony, and the platform domain-association documents.
Because the ceremony logic is the *official* ASP.NET Core Identity passkey implementation, this doubles
as an interop conformance check across Apple, Android, and Windows.

> ⚠️ This is a **local dev tool** — it relaxes a default for convenience (no email confirmation) and its
> native `/passkeys/*` API authenticates the session with a **cookie** rather than a bearer token. That's
> fine here but is **not** how a production native app should do it — see
> [Authentication & CSRF](#authentication--csrf-why-cookies-here) below.

## What's in here

| File | Purpose |
| --- | --- |
| `Program.cs` | The whole app: an in-memory SQLite database + Identity, cookie/bearer auth, `MapIdentityApi` under `/account`, and the passkey + well-known endpoints. |
| `PasskeyApiEndpoints.cs` | Native-app-facing passkey ceremony API — `GET /passkeys/list`, `POST /passkeys/register/begin` · `/register/finish` · `/login/begin` · `/login/finish`, `DELETE /passkeys/delete`. JSON in / JSON out, cookie-correlated, no antiforgery. |
| `IdentityNoOpEmailSender.cs` | A no-op `IEmailSender<IdentityUser>` — Identity's registration flow requires one, but this server never sends email. |
| `WellKnownEndpoints.cs` | Serves `/.well-known/assetlinks.json` (Android Digital Asset Links) and `/.well-known/apple-app-site-association` (Apple) from config, so real devices trust this domain as the credential provider. |

There is no `DbContext` or `ApplicationUser` type — it uses the framework's `IdentityDbContext` and
`IdentityUser` directly, with the schema created in-memory at startup (no migrations). All passkey
relying-party config (RP ID / origins / Android + Apple association values) comes from **user-secrets**;
nothing sensitive lives in `appsettings.json`.

Username/password auth (`/account/register`, `/account/login?useCookies=true`, plus the rest of
`MapIdentityApi`) and `/account/logout` come from `Program.cs` directly.

## Run it

```bash
dotnet run --project src/Essentials/samples/Samples.Server.Passkeys --launch-profile http
# listens on http://localhost:5177  (https://localhost:7235 with the "https" profile)
```

The database is **in-memory** (real SQLite, held in RAM — no file on disk) and is created empty at
startup. There is no web UI — point the MAUI **Passkeys** sample page at the base URL, or drive the
endpoints with `curl`.

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

Then build/run the MAUI sample — its **Passkeys** page defaults to the full `https://…devtunnels.ms`
URL (baked in by `Configure.ps1`; editable at runtime via the Server toolbar button).

## Passkeys configuration keys

All passkey relying-party config is provided via **user-secrets** (written for you by `Configure.ps1`,
never committed) — nothing sensitive is in `appsettings.json`. With none set, a bare `dotnet run` works
on localhost with ASP.NET Core Identity's default RP settings. The keys:

| Key | Meaning |
| --- | --- |
| `Passkeys:ServerDomain` | The RP ID = the public host, no scheme/port (e.g. `abcd1234-5177.usw3.devtunnels.ms`). Empty ⇒ Identity's localhost defaults. |
| `Passkeys:AllowedOrigins:<n>` | Extra accepted WebAuthn origins. `https://<ServerDomain>` is always allowed; add each Android `android:apk-key-hash:<hash>` origin. |
| `Passkeys:Android:PackageName` | The Android app id, served in `/.well-known/assetlinks.json`. |
| `Passkeys:Android:Sha256CertFingerprints:<n>` | The app's signing-cert SHA-256 fingerprint(s) for Digital Asset Links. |
| `Passkeys:Apple:AppIds:<n>` | `<TeamID>.<BundleID>` entries served in `/.well-known/apple-app-site-association`. |

Set one by hand like:

```bash
dotnet user-secrets --project src/Essentials/samples/Samples.Server.Passkeys \
  set "Passkeys:ServerDomain" "<id>-5177.<region>.devtunnels.ms"
```

## The native API

The endpoints the MAUI app calls:

```
POST   /account/register                                    (body: { email, password })
POST   /account/login?useCookies=true                       (body: { email, password }) -> sets auth cookie
POST   /account/logout
GET    /passkeys/list                (signed-in required)   -> { username, passkeyCount, passkeys[] }
POST   /passkeys/register/begin      (signed-in required)   -> PublicKeyCredentialCreationOptions JSON
POST   /passkeys/register/finish?name=<label>  (body: attestation JSON)  -> { registered, username, name }
DELETE /passkeys/delete?credentialId=<base64url>  (signed-in required)   -> { removed }
POST   /passkeys/login/begin?username=<email>               -> PublicKeyCredentialRequestOptions JSON
POST   /passkeys/login/finish        (body: assertion JSON) -> { authenticated, username }
```

The WebAuthn challenge is correlated between `begin` and `finish` by a short-lived framework cookie
(`Identity.TwoFactorUserId`, set by the built-in ceremony methods), so the native client **must** use a
cookie container (the MAUI sample does).

`register/begin` enrolls a passkey for the **currently signed-in** user (identified by the Identity
session cookie), so the caller must sign in first — the "add a passkey after you log in" flow. Anonymous
requests get a `401`; the server never creates an account from an arbitrary posted username.
`login/begin` may be called **without** `username` for username-less / discoverable-credential sign-in.

## Authentication & CSRF (why cookies here)

The native `/passkeys/*` (and `/account/logout`) endpoints are driven by a native `HttpClient`, not a
browser `<form>`, and a native client has no antiforgery token to send. The `/passkeys` group calls
`.DisableAntiforgery()` so the framework's antiforgery middleware doesn't reject them. Combined with the
fact that the **login session** is a cookie (`/login/finish` calls `SignInAsync`), that's the classic
CSRF-susceptible shape: a cookie-authed, state-changing POST with no antiforgery token. **Do not copy
this pattern to non-WebAuthn endpoints.**

Why it's nonetheless safe *here*: a WebAuthn `finish` payload is a signature over
`(challenge + origin + rpId)` from a private key that never leaves the authenticator, so it **cannot be
forged or replayed** — the ceremony is CSRF-resistant by construction.

**How a production native app should do it:** authenticate the session with a **bearer token**, not a
cookie. `MapIdentityApi` already supports it — `POST /account/login` *without* `?useCookies=true` returns
an `access_token`; the app sends `Authorization: Bearer <token>`. Because browsers never auto-attach
`Authorization` headers cross-site, CSRF becomes structurally impossible (no antiforgery needed).

One nuance worth knowing: switching the *session* to bearer does **not** make passkeys fully cookieless.
The built-in `MakePasskey*OptionsAsync` / `PerformPasskey*Async` methods stash the per-ceremony challenge
in that transient `Identity.TwoFactorUserId` cookie regardless — so a native client still round-trips
that one short-lived cookie between `begin` and `finish`. It carries only a challenge (no identity) and,
per the point above, can't be abused. This sample keeps the session on a cookie purely to stay a minimal,
single-auth-mode reference.

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
once you know your signing identities — `Configure.ps1` fills the Android values for you.
`https://<ServerDomain>` is always accepted automatically. To compute the Android hash origin from a
SHA-256 fingerprint, base64url-encode the raw 32 bytes and prefix `android:apk-key-hash:`.

## Reset

The database is in-memory, so just restart the server — all registered users and passkeys are wiped and
the schema is re-created empty on every launch.

See [`../README.md`](../README.md) for the condensed end-to-end steps.
