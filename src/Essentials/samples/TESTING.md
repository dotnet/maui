# Testing the Passkeys sample

Short, practical steps to run the reference relying-party (RP) server and exercise it from the
**Essentials.Sample** app's **Passkeys** page. Full detail lives in the server's
[`Samples.WebServer/README.md`](Samples.WebServer/README.md).

| Sample page | Endpoints | Local http port |
| --- | --- | --- |
| **Passkeys** | `/passkeys/*` (Blazor + ASP.NET Core Identity / WebAuthn) | 5177 |

`Samples.WebServer` is part of the solution and builds in CI; you also run it locally (`dotnet run`) to
test on real devices.

## Why a dev tunnel?

Passkeys are bound to a domain (the RP ID) and `localhost` won't work from a phone. A **dev tunnel**
gives you a public `https://<id>-<port>.<region>.devtunnels.ms` domain the device can reach.

Install once: `brew install --cask devtunnel` (macOS) · `winget install Microsoft.devtunnel` (Windows).

## Fast path (automated)

From `src/Essentials/samples`:

```bash
# Provisions the tunnel AND writes the passkeys domain (+ Android origins) into the server's user-secrets.
pwsh ./Configure.ps1
```

The script prints the public `https://…devtunnels.ms` URL. Then, in two terminals:

```bash
devtunnel host maui-essentials                                   # 1) hold the tunnel open
dotnet run --project Samples.WebServer --launch-profile http      # 2) run the server
```

Finally, in the **Essentials.Sample** app, paste that URL into the server field on the **Passkeys** page.

## Passkeys — end to end

1. `pwsh ./Configure.ps1` → note the URL, then host + run (above).
2. **Native trust** (real devices only): the RP must accept each app's native origin and serve its
   domain-association doc. `Configure.ps1` writes the **Android** values for you (package,
   debug-key SHA-256, and `android:apk-key-hash:` origin). For the others:
   - Apple: add `<TeamID>.<BundleID>` to `Passkeys:Apple:AppIds` and the app's associated-domains
     entitlement `webcredentials:<host>`. Served at `/.well-known/apple-app-site-association`.
   - Windows: nothing extra (Win11 platform trusts the https origin).
3. In the app's **Passkeys** page: set the server URL, sign in (or sign up) with a username + password,
   tap **Create a passkey**, then **Sign in with a passkey**.

### Android emulator — concrete walkthrough

No paid account and **no app manifest changes** are needed (Digital Asset Links live on the server;
intent-filters are only for App Links, a different feature). You do need the right emulator.

**Emulator prerequisites** (one-time):
- An **API 34+** AVD using a **Google Play** system image (not AOSP) so Google Password Manager is
  present.
- Sign the emulator into a **Google account** (Settings → Passwords, passkeys & accounts).
- Set a **secure screen lock** (PIN/pattern) — passkeys require device authentication.

**Steps:**
1. `cd src/Essentials/samples && pwsh ./Configure.ps1`
   This provisions the tunnel and writes into the server's user-secrets: the RP domain + web origin,
   **and** the Android package (`com.microsoft.maui.essentials`), your debug keystore SHA-256, and the
   `android:apk-key-hash:` origin. (It reads the keystore .NET for Android signs debug builds with —
   `<LocalApplicationData>/Xamarin/Mono for Android/debug.keystore`, **not** `~/.android/debug.keystore`;
   build the Android app once first if it doesn't exist yet.)
2. Host the tunnel and run the server (two terminals):
   ```bash
   devtunnel host maui-essentials
   dotnet run --project Samples.WebServer --launch-profile http
   ```
   Verify Google can see the asset links: open `https://<tunnel-host>/.well-known/assetlinks.json`
   in a browser — it should list your package + fingerprint.
3. Run the sample on the emulator:
   ```bash
   dotnet build src/Essentials/samples/Samples/Essentials.Sample.csproj -t:Run -f net11.0-android
   ```
4. In the app open **Passkeys**, set the server URL to your `https://<tunnel-host>`, sign in/up with a
   username + password, tap **Create a passkey** (approve the device prompt), then
   **Sign in with a passkey**.

If registration fails with a "no create options"/provider error, re-check the three emulator
prerequisites above — that's the usual cause.

## Local-only smoke test

A dev tunnel is the supported path. If you only want to check that the app ↔ server round-trip works
(this will **not** complete a real passkey ceremony — the platform won't trust a non-public host), you
can temporarily point the page at localhost:

- iOS simulator / Mac / Windows → `https://localhost:7235`
- Android emulator → `http://10.0.2.2:5177`
