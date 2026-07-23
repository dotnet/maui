# Testing the Essentials auth samples

Short, practical steps to run the reference backend and exercise it from the **Essentials.Sample**
app. Full detail lives in the server's [`Samples.WebServer/README.md`](Samples.WebServer/README.md).

**One server hosts both samples**, so you launch one web app and it serves both:

| Sample page | Endpoints | Local http port |
| --- | --- | --- |
| **Passkeys** | `/passkeys/*` (Blazor + ASP.NET Core Identity / WebAuthn) | 5177 |
| **Web Authenticator** | `/mobileauth/{scheme}` (OAuth pass-through) | 5177 |

`Samples.WebServer` is part of the solution and builds in CI; you also run it locally (`dotnet run`) to
test on real devices.

## Why a dev tunnel?

Passkeys are bound to a domain (the RP ID) and OAuth redirect URIs must be a fixed public HTTPS URL —
`localhost` won't work from a phone. A **dev tunnel with a persistent id** gives you a stable
`https://<id>-<port>.<region>.devtunnels.ms` domain that stays the same every run, so you configure
each provider/app **once**. It's the **same URL for both sample pages**.

Install once: `brew install --cask devtunnel` (macOS) · `winget install Microsoft.devtunnel` (Windows).

## Fast path (automated)

From `src/Essentials/samples`:

```bash
# Provisions the tunnel AND writes the passkeys domain into the server's user-secrets.
pwsh ./setup-devtunnel.ps1
```

The script prints the public `https://…devtunnels.ms` URL. Then, in two terminals:

```bash
devtunnel host maui-essentials                                # 1) hold the tunnel open
dotnet run --project Samples.WebServer --launch-profile http      # 2) run the server
```

Finally, in the **Essentials.Sample** app, paste that URL into the server field on **both** the
Passkeys and Web Authenticator pages.

## Passkeys — end to end

1. `pwsh ./setup-devtunnel.ps1` → note the URL, then host + run (above).
2. **Native trust** (real devices only): the RP must accept each app's native origin and serve its
   domain-association doc.
   - Android: put your signing cert SHA-256 in `Passkeys:Android:Sha256CertFingerprints` and add the
     `android:apk-key-hash:<hash>` origin to `Passkeys:AllowedOrigins`. Served at
     `/.well-known/assetlinks.json`.
   - Apple: add `<TeamID>.<BundleID>` to `Passkeys:Apple:AppIds` and the app's associated-domains
     entitlement `webcredentials:<host>`. Served at `/.well-known/apple-app-site-association`.
   - Windows: nothing extra (Win11 platform trusts the https origin).
3. In the app's **Passkeys** page: set the server URL, enter a username, tap **Register a passkey**,
   then **Sign in with a passkey**.

## Web Authenticator — end to end

External providers only redirect back to a public HTTPS domain, so a dev tunnel is **required**
(no localhost). Full per-provider setup (redirect URIs, where to get each id/secret, and the Apple
special cases) is in the server README's **Web Authenticator sample** section:
[`Samples.WebServer/README.md`](Samples.WebServer/README.md#web-authenticator-sample).

1. `pwsh ./setup-devtunnel.ps1` → note the `https://<tunnel-host>` URL (same one as passkeys).
2. For each provider you want, create an OAuth app and register the redirect URI
   `https://<tunnel-host>/signin-<provider>` (`/signin-google`, `/signin-microsoft`,
   `/signin-facebook`, `/signin-apple`). Apple additionally needs a Services ID, a downloaded `.p8`
   key, and domain verification (the server serves the association file for you).
3. Put the ids/secrets into user-secrets, e.g. Google:
   ```bash
   dotnet user-secrets --project Samples.WebServer set "GoogleClientId" "<id>"
   dotnet user-secrets --project Samples.WebServer set "GoogleClientSecret" "<secret>"
   ```
4. Host the tunnel + run the server (same server as passkeys — no second app):
   ```bash
   devtunnel host maui-essentials
   dotnet run --project Samples.WebServer --launch-profile http
   ```
5. In the app's **Web Authenticator** page: set the **OAuth server base URL** to `https://<tunnel-host>`
   and tap a provider.

## Local-only smoke test

A dev tunnel is the supported path. If you only want to check that the app ↔ server round-trip works
(this will **not** complete a real passkey ceremony or an external OAuth sign-in — providers won't
redirect back to a non-public host), you can temporarily point the pages at localhost:

- iOS simulator / Mac / Windows → `https://localhost:7235`
- Android emulator → `http://10.0.2.2:5177`
