# Template App Distribution

Builds a fresh .NET MAUI app from the packaged templates for each platform/variant and
either (a) uploads the results as GitHub artifacts (`publish=false`, a **dry run**) or
(b) signs and publishes them to Google Play / TestFlight (`publish=true`).

The workflow lives in `.github/workflows/template-app-distribution.yml`. Trigger it from the
**Actions** tab with *Run workflow* and pick the source branch (`main`, `net10.0`, `net11.0`
or a `release/*` branch) plus whether to publish.

## What you get, per platform

The goal is that **every artifact a tester downloads can actually be installed** without an
App Store / Play account. The build script therefore emits two things:

- `package_path` — the **store** package (`.aab` / App Store `.ipa` / Mac App Store `.pkg`).
  Consumed only by the Google Play / TestFlight upload steps.
- `sideload_package_path` — the **directly installable** artifact. This is what the dry-run
  job and the publish "artifact copy" step upload for testers.
- `additional_package_path` — an optional extra file uploaded next to the sideload one. Used on
  iOS to include the Simulator `.app` zip alongside the device `.ipa`.

| Platform | Dry-run artifact (`publish=false`) | Publish store target | Sideloadable artifact on publish |
| --- | --- | --- | --- |
| **Android** | Debug-signed **APK** (installs via `adb install` / file manager) | `.aab` → Google Play | Release-signed **APK** |
| **Windows** | **Self-contained** unpackaged zip (no runtime install needed) | same zip | same zip |
| **iOS** | unsigned device **`.ipa`** (AltStore/Sideloadly) + Simulator `.app` zip | App Store `.ipa` → TestFlight | ad-hoc `.ipa` *(only if the ad-hoc secret is set — see below)* |
| **macOS (Mac Catalyst)** | Native **arm64** `.app` zip (Apple Silicon) | Mac App Store `.pkg` → TestFlight | notarized `.app` zip *(only if the Developer ID secrets are set — see below)* |

### Why the previous artifacts failed to install

- **Android** — only an `.aab` was produced. An `.aab` can *only* be consumed by Google Play,
  so the ZIP had nothing to sideload. Fixed by also building an installable APK.
- **Windows** — published framework-dependent, so it needed the exact .NET preview desktop
  runtime and still showed the "install .NET" screen. Fixed with `-p:SelfContained=true`.
- **iOS** — two problems. (1) The publish IPA was signed with the App Store / TestFlight profile,
  which Apple refuses to install directly (`0xe800801f "Attempted to install a Beta profile without
  the proper entitlement"`) — fixed by an optional ad-hoc-signed IPA (secret-gated). (2) The dry-run
  `.app` was an unsigned *device* (`ios-arm64`, iPhoneOS) build that installs nowhere: it can't go on
  hardware (unsigned) and won't launch in the Simulator (device platform — launch is denied). Fixed by
  building an **arm64 iOS Simulator** app (`dotnet build -r iossimulator-arm64`; `dotnet publish`
  rejects simulator RIDs) and ad-hoc re-signing it so the Simulator (which enforces code signing on
  macOS 15+/26) actually launches it. The dry-run **also** wraps an unsigned `ios-arm64` device
  build as a `Payload/*.app` **`.ipa`** so testers who want to run on real hardware have an IPA to
  sideload with AltStore/Sideloadly (which re-sign it with their own Apple ID). A *directly*
  installable device build still needs the ad-hoc IPA (secret-gated) or TestFlight.
- **macOS** — the `.pkg` was Mac App Store signed and defaulted to `maccatalyst-x64` (Rosetta),
  so launching it outside the store gave `SIGKILL (Code Signature Invalid)` /
  `Taskgated Invalid Signature`. Fixed by shipping a directly-launchable **arm64-native** `.app`
  that is (1) zipped with `ditto` so the framework symlinks, exec bits and signature survive the
  round-trip, and (2) **re-signed ad-hoc from the inside out** so macOS 15+/26 accepts it (the
  stock .NET linker-signed bundle is SIGKILL'd with "Invalid Page" — reproduced on macOS 26.5.2 /
  M2). It runs natively on Apple Silicon (the reporting Mac was an M2) with no Rosetta. For a
  seamless, notarized experience there is an optional Developer-ID-signed `.app`. (net11 Mac
  Catalyst can't publish the SDK's default universal `maccatalyst-x64;maccatalyst-arm64`
  unattended — the multi-RID publish trips `PublishReadyToRun couldn't be inferred` — so a single
  native RID is pinned; Intel Macs would need a separate `maccatalyst-x64` build.)

## Install instructions for testers

- **Android** — download the APK, then `adb install app.apk` (or copy to the device and open
  it; enable "install unknown apps"). The dry-run APK is debug-signed and installs on any
  device/emulator.
- **Windows** — unzip and run the `.exe`. Because the app is self-contained no .NET runtime
  install is required. (SmartScreen may warn for an unsigned app — *More info → Run anyway*.)
- **iOS** — the dry-run artifact contains two files:
  - **`MyApp.ipa`** — an **unsigned device** build for a **physical iPhone/iPad**. iOS refuses to
    run unsigned or ad-hoc code on a device, so install it with **[AltStore](https://altstore.io)**
    or **[Sideloadly](https://sideloadly.io)**, which re-sign the app with your own Apple ID (a free
    Apple ID works but must be refreshed every 7 days; a paid Developer account lasts a year). A
    plain Finder drag / double-click will *not* install an unsigned IPA.
  - **`MyApp.app.zip`** — an **arm64 iOS Simulator** build. Unzip and run it in the Simulator:
    `xcrun simctl install booted MyApp.app && xcrun simctl launch booted <bundle-id>`. It is ad-hoc
    re-signed so the Simulator (which enforces code signing on macOS 15+/26) launches it.

  For a **directly installable** device build (no AltStore, no re-signing) use one of the
  secret-gated publish paths: **TestFlight** (`publish=true`, the smoothest — testers install from
  the TestFlight app, no UDID needed) or the **ad-hoc `.ipa`** (below) with each tester's device
  UDID registered in the ad-hoc profile.
- **macOS** — the dry-run `.app` is **ad-hoc signed** (not notarized), so Gatekeeper blocks it on
  first launch. Clear quarantine and open it:
  `xattr -dr com.apple.quarantine "MyApp.app"` then double-click — **or** double-click, dismiss the
  warning, and approve it under *System Settings → Privacy & Security → Open Anyway*. (The bundle is
  re-signed ad-hoc during the build; without that, macOS 15+/26 SIGKILLs it at launch with "Code
  Signature Invalid".) A double-click-clean, launch-anywhere build for other users requires the
  notarized artifact (secret-gated, below).

## Secrets & variables

The header of `template-app-distribution.yml` is the source of truth. Summary:

**Required for `publish=true`** (protected `template-app-distribution` environment): the Android
keystore, the Google Play service account JSON, the Apple distribution certificate, the App Store
/ Mac App Store provisioning profiles, and the App Store Connect API key. `publish=false` needs
**none** of these — it produces the installable Android APK and self-contained Windows zip
immediately.

**Optional — enable the sideloadable iOS / macOS artifacts:**

- `TEMPLATE_APP_{BLANK,SAMPLE}_IOS_ADHOC_PROVISIONING_PROFILE_BASE64` — ad-hoc distribution
  provisioning profiles (they reuse the existing Apple Distribution certificate). With these set,
  `publish=true` also produces an installable ad-hoc `.ipa`.
- `TEMPLATE_APP_MAC_DEVELOPER_ID_APPLICATION_CERTIFICATE_BASE64` /
  `TEMPLATE_APP_MAC_DEVELOPER_ID_APPLICATION_CERTIFICATE_PASSWORD` — a **Developer ID
  Application** signing certificate (`.p12`).
- `TEMPLATE_APP_{BLANK,SAMPLE}_MACCATALYST_DEVELOPERID_PROVISIONING_PROFILE_BASE64` — Developer ID
  Mac Catalyst provisioning profiles. With these set, `publish=true` also produces a
  Developer-ID-signed, **notarized** `.app` zip that launches on any Mac. Notarization reuses the
  existing `TEMPLATE_APPSTORE_CONNECT_*` API key.

If the optional Apple secrets are absent, the workflow still succeeds and simply falls back to
uploading the store `.ipa` / `.pkg` (which stay TestFlight-only). No secret is ever required for
the Android and Windows fixes.
