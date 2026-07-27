# .NET MAUI Essentials — Passkeys sample

This folder contains the **Passkeys** Essentials sample and everything needed to run it end to end:

- **[`Samples/`](Samples)** — the .NET MAUI **Essentials.Sample** app; the **Passkeys** page is the
  passkeys demo.
- **[`Samples.WebServer/`](Samples.WebServer)** — the reference relying-party (RP) server (ASP.NET
  Core Identity + WebAuthn) that does the server half. See its
  [`README.md`](Samples.WebServer/README.md) for detail.
- **[`Configure.ps1`](Configure.ps1)** — provisions a dev tunnel and writes the RP trust config into
  the server's user-secrets (and, with `-AppleTeamId`, the Apple bits).

The rest of this page is the **testing guide**: run the RP server and exercise it from the app's
**Passkeys** page on each platform.

| Sample page | Endpoints | Local http port |
| --- | --- | --- |
| **Passkeys** | `/passkeys/*` (ASP.NET Core Identity / WebAuthn) | 5177 |

**Flow:** set up the [**server**](#1-server-shared-by-all-platforms) once (it's shared by every
platform), then do the platform setup for what you're testing —
[**Apple**](#2-apple-ios--ipados--mac-catalyst), [**Android**](#3-android-emulator), or
[**Windows**](#4-windows-windows-11) — then exercise the app.

## Prerequisites

- **.NET SDK** (per `global.json`) and the ability to build the solution. `Samples.WebServer` is part
  of the solution and also runs locally with `dotnet run`.
- **Dev tunnels CLI** — passkeys are bound to a domain (the RP ID), so `localhost` won't work from a
  real device; the server is exposed on a public `https://…devtunnels.ms` host:
  - macOS: `brew install --cask devtunnel`
  - Windows: `winget install Microsoft.devtunnel`
  - Linux: <https://aka.ms/devtunnels/download>
- Platform-specific prerequisites are listed under each platform below.

## Using the app

On every platform the in-app steps are the same: open the **Passkeys** page (its **server URL** already
defaults to your tunnel — `Configure.ps1` bakes it in; edit it via the Server toolbar button if needed),
sign up (or sign in) with a username + password, tap **Create a passkey** (approve the device prompt),
then **Sign in with a passkey**.

> **Testing the web UI in a browser?** Load it via the **tunnel URL** (`https://<tunnel-host>`, clicking
> through the dev-tunnel warning), not `http://localhost:5177`. Passkeys are bound to the RP ID domain,
> so a `localhost` origin fails with *"relying party ID is not a registrable domain suffix…"*.

## 1. Server (shared by all platforms)

From `src/Essentials/samples`, provision the tunnel and write the server config into user-secrets:

```bash
pwsh ./Configure.ps1
```

`Configure.ps1` provisions a persistent dev tunnel and writes the RP domain + web origin (and the
**Android** origins — see section 3) into the SERVER's user-secrets. It prints the public
`https://…devtunnels.ms` URL — keep it, you type it into the app. Re-run it with the platform flags
below to add Apple/Android trust.

Then, in two terminals, host the tunnel and run the server:

```bash
devtunnel host maui-essentials                                # 1) hold the tunnel open
dotnet run --project Samples.WebServer --launch-profile http   # 2) run the server
```

Now pick your platform.

## 2. Apple (iOS / iPadOS / Mac Catalyst)

Apple only does passkeys when the app is set up like a **real, shipping app** — there is no localhost
shortcut. Three things must line up:

1. **Associated Domains entitlement** — the app declares `webcredentials:<your-domain>`.
2. **App Site Association (AASA)** — `https://<your-domain>/.well-known/apple-app-site-association`
   lists your `<TeamID>.<BundleID>`; Apple fetches and caches it over the public internet.
3. **Signing** — signed by your Apple Developer **Team** with a profile that includes the Associated
   Domains capability.

At runtime the OS matches the app's entitlement against the AASA it fetched for that domain; only then
will Face ID / Touch ID create or use a passkey.

**Prerequisites:** a **paid Apple Developer account** (free/personal teams can't provision Associated
Domains), your 10-character **Team ID**, **macOS + Xcode** (Apple apps build only on a Mac), and a
target on **iOS 16+** or **Mac Catalyst 16+**.

**Steps:**

1. On developer.apple.com → **Identifiers**, register your app's bundle id as an **explicit App ID** and
   enable the **Associated Domains** capability on it.

   > **Heads up — an explicit App ID is globally unique to one team.** The sample's default
   > `com.microsoft.maui.essentials` is owned by the MAUI team, so **you can't register it under your
   > own team**. Use your own reverse-DNS id (e.g. `com.yourname.mauiessentials`): set it once in
   > `<ApplicationId>` in `Samples/Essentials.Sample.csproj` (a local edit, don't commit) — it's the
   > single app id shared by every platform, and `Configure.ps1` reads it from there. Wildcard App IDs
   > can't carry Associated Domains, so it must be explicit.

2. **Register this Mac as a device** and create a **macOS App Development** provisioning profile for that
   App ID (your Development certificate + this Mac). Install it (double-click, or drop it in
   `~/Library/MobileDevice/Provisioning Profiles`). An IDE with automatic provisioning (VS Code C# Dev
   Kit, Rider) or Xcode can generate + install it for you; find this Mac's provisioning UDID with
   `system_profiler SPHardwareDataType | grep "Provisioning UDID"`.

3. Configure the server + app (from `src/Essentials/samples`):
   ```bash
   pwsh ./Configure.ps1 -AppleTeamId <TEAMID>          # reads the app id from <ApplicationId>
   ```
   This is the one-stop setup, and it writes **only git-ignored files** (no committed file is edited):
   - `Passkeys:Apple:AppIds:0 = <TeamID>.<BundleID>` into the server user-secrets (served in the AASA);
   - a git-ignored `Samples/Platforms/iOS/Entitlements.Local.plist` (a copy of the committed base plus
     `com.apple.developer.associated-domains` → `webcredentials:<domain>`); and
   - a git-ignored `Samples/Passkeys.Local.props` (imported by the app csproj) carrying the default
     server URL and — **auto-detected** from step 2 — your Apple Development signing identity and the
     matching provisioning profile. So **Mac Catalyst is then ready to build and run** with no extra
     flags. (If auto-detect can't find them, pass `-AppleSigningIdentity` / `-AppleProvisioningProfile`;
     or copy `Passkeys.Local.in.props` → `Passkeys.Local.props` and fill it in by hand.)

   The iOS **Simulator** doesn't need the signing bits — the entitlement alone is applied and it runs.
4. With the server running (section 1), verify the AASA is reachable **from the public internet** and
   is JSON (not an HTML page):
   ```bash
   curl -sS https://<your-domain>/.well-known/apple-app-site-association
   # {"webcredentials":{"apps":["ABCDE12345.com.microsoft.maui.essentials"]}}
   ```
5. Build + run, then follow [**Using the app**](#using-the-app):
   - **iOS Simulator** (unsigned): `dotnet build Samples/Essentials.Sample.csproj -f net11.0-ios -p:RuntimeIdentifier=iossimulator-arm64`, then `xcrun simctl install booted <app>` + `xcrun simctl launch booted <bundle-id>`.
   - **Mac Catalyst** (signed via `Signing.local.props`): `dotnet build Samples/Essentials.Sample.csproj -f net11.0-maccatalyst -p:RuntimeIdentifier=maccatalyst-arm64`, then `open` the built `.app`.
   - **Real iOS device**: signed the same way, deploy from your IDE.

**Apple troubleshooting:**

| Symptom | Cause / fix |
| --- | --- |
| `… is not associated with domain …` | AASA not reachable as JSON, its app-id ≠ your signed `<TeamID>.<BundleID>`, or the entitlement domain ≠ the server's `Passkeys:ServerDomain`. Verify with the step-4 `curl`. |
| AASA `curl` returns HTML | The dev tunnel's anti-phishing interstitial is answering; create a tunnel access token / disable anti-phishing for the port so raw JSON is served. |
| `Could not resolve host …devtunnels.ms` on the device | Local-network DNS won't resolve `*.devtunnels.ms`. Point the device at a public resolver (iOS Wi-Fi → Configure DNS → Manual → `8.8.8.8`) or restart the router. Apple's CDN resolves it fine over the public internet. |
| `no profiles for '<bundle id>' were found` | The App ID belongs to another team — set your own reverse-DNS `<ApplicationId>` in `Essentials.Sample.csproj` and register it under your team. |
| Browser: *"relying party ID is not a registrable domain suffix of … the current domain"* | You opened the web UI on `localhost` (e.g. `http://localhost:5177`). Passkeys are domain-bound — browse to the **tunnel URL** `https://<your-domain>` instead (click through the dev-tunnel warning), so the page origin matches the RP ID. |
| Build error `MT7139: … requests the entitlement 'com.apple.developer.associated-domains', but no provisioning profile has been specified` | A **device** or **Mac Catalyst** build needs an explicit provisioning profile with Associated Domains. Generate one for your Team (IDE automatic provisioning or Xcode) and set `CodesignProvision` to its name. The iOS **Simulator** doesn't need this. |

## 3. Android (emulator)

No paid account and **no app manifest changes** are needed (Digital Asset Links live on the server;
intent-filters are only for App Links, a different feature). You do need the right emulator.

**Prerequisites** (one-time):
- An **API 34+** AVD on a **Google Play** system image (not AOSP) so Google Password Manager is present.
- The emulator signed into a **Google account** (Settings → Passwords, passkeys & accounts).
- A **secure screen lock** (PIN/pattern) — passkeys require device authentication.

**Steps:**

1. Configure the server (from `src/Essentials/samples`):
   ```bash
   pwsh ./Configure.ps1
   ```
   Beyond the RP domain, this writes the Android package (`com.microsoft.maui.essentials`), your debug
   keystore SHA-256, and the `android:apk-key-hash:` origin. It reads the keystore .NET for Android
   signs debug builds with — `<LocalApplicationData>/Xamarin/Mono for Android/debug.keystore`, **not**
   `~/.android/debug.keystore`; build the Android app once first if it doesn't exist yet.
2. Host the tunnel + run the server (section 1). Verify Google can see the asset links: open
   `https://<tunnel-host>/.well-known/assetlinks.json` — it should list your package + fingerprint.
3. Run the sample on the emulator:
   ```bash
   dotnet build Samples/Essentials.Sample.csproj -t:Run -f net11.0-android
   ```
4. Follow [**Using the app**](#using-the-app).

If registration fails with a "no create options" / provider error, re-check the three emulator
prerequisites above — that's the usual cause.

## 4. Windows (Windows 11)

Nothing extra: the Windows platform trusts the `https` origin directly (no domain-association file).
Passkeys require **Windows 11 with Windows Hello** configured. With the server running (section 1),
deploy the Windows head from your IDE (or `dotnet build Samples/Essentials.Sample.csproj -t:Run -f
net11.0-windows10.0.<version>` matching the project's Windows TFM), then follow
[**Using the app**](#using-the-app).

## Local-only smoke test

A dev tunnel is the supported path. To only check the app ↔ server round-trip (this will **not**
complete a real passkey ceremony — the platform won't trust a non-public host), point the page at
localhost temporarily:

- iOS simulator / Mac / Windows → `https://localhost:7235`
- Android emulator → `http://10.0.2.2:5177`

## Don't commit

`Configure.ps1` writes only git-ignored files (`Samples/Passkeys.Local.props`,
`Samples/Platforms/iOS/Entitlements.Local.plist`) and the server user-secrets — nothing committed. The
one manual exception is if you override the bundle id: your `<ApplicationId>` change in
`Essentials.Sample.csproj` is a local edit — keep it out of commits.
