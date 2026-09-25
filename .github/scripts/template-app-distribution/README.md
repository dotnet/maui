# Template App Distribution

Builds a fresh .NET MAUI app from the packaged templates for each platform/variant and
either (a) uploads the results as GitHub artifacts (`publish=false`, a **dry run**) or
(b) signs and publishes them to Google Play / TestFlight (`publish=true`).

Both the framework packages and template content are built from the same resolved source
commit. Installing the platform workload alone is not sufficient: it supplies the platform
toolchain, not the MAUI framework used by these apps. The generated app sets `MauiVersion`
to the source-built version and maps every `Microsoft.Maui.*` restore exclusively to the
local source package directory. A missing package fails restore rather than using a release
from NuGet. The generated project also disables the installed MAUI workload's SDK import and
imports `Sdk/Sdk.targets` from the verified source-built `Microsoft.Maui.Sdk` package;
the installed Android/Apple/Windows platform toolchains remain in use.

Each dry-run download includes `provenance.json`: checked package repository commits and
hashes, the MAUI packages resolved during each build, and informational versions and hashes
read from the MAUI assemblies inside the final app archives. Separate `source-packages-*`
artifacts contain the matching NuGet packages and source manifest. Archive labels and
workflow inputs alone are not proof of which framework an app uses. Provenance failure
prevents publishing successful app outputs. Android dry runs disable assembly-store packing
and assembly compression so their managed assemblies can be inspected directly.

The workflow lives in `.github/workflows/template-app-distribution.yml`. Trigger it from the
**Actions** tab with *Run workflow* and pick the source branch plus whether to publish.
Dry runs accept any safe ref. Publishing always accepts the default branch and accepts
additional exact protected branches only when an administrator lists them in the repository
variable `TEMPLATE_APP_TRUSTED_PUBLISH_BRANCHES`; wildcard branch conventions are not trusted.

### iOS sample only: signed TestFlight publishing

Set **`ios_sample_testflight=true`, `publish=true`, `windows_test_msix=false`** to
select exactly the standard `MauiTemplateSample` iOS device build. Leaving the new
option off preserves the existing default matrix; **`publish=true` alone still
selects all platforms/variants**, not just iOS. Incompatible selections and custom
overrides of the sample project/template/bundle ID are rejected.

Publishing must run from the repository's **default workflow branch**, not a PR
branch. The selected source commit must be reachable from the default branch or an
explicitly trusted protected source branch. A full SHA is accepted only when that
ancestry check passes. Pin the intended merged upstream main commit once, verify it
is available through that trusted source history, and keep framework, SDK targets
and templates on that same SHA. A fork branch or a matching version label alone
does not satisfy this gate; do not disable it to run a PR-only workflow.

Before an authorized dispatch, an administrator must securely configure:

- A protected **`template-app-distribution`** environment with appropriate reviewers
  and deployment-branch restrictions.
- Environment secrets `TEMPLATE_APP_IOS_CERTIFICATE_BASE64` (the authorized Apple
  Distribution signing identity as a password-protected P12),
  `TEMPLATE_APP_IOS_CERTIFICATE_PASSWORD`, and
  `TEMPLATE_APP_SAMPLE_IOS_PROVISIONING_PROFILE_BASE64` (a current **App Store**
  profile matching that identity, team, entitlements and sample bundle ID).
- Environment secrets `TEMPLATE_APPSTORE_CONNECT_ISSUER_ID`,
  `TEMPLATE_APPSTORE_CONNECT_KEY_ID`, `TEMPLATE_APPSTORE_CONNECT_PRIVATE_KEY`;
  the authorized App Store Connect API key must have access to the real app record
  and permission to upload/distribute its TestFlight builds.
- Repository variable `TEMPLATE_APP_SAMPLE_IOS_BUNDLE_ID`, explicitly matching the
  existing Apple App ID/profile/App Store Connect app record. No generated default
  bundle ID is accepted in this mode.
- Repository variable `TEMPLATE_APP_SAMPLE_IOS_TESTFLIGHT_GROUPS`, a comma-separated
  list of explicitly approved external tester groups for this sample. Arrange the
  intended tester's membership/invitation in App Store Connect; the workflow does
  not create accounts, discover email addresses or invent invite links.

Use GitHub's secure secret configuration, never chat, source files or uploaded
artifacts. This mode does not use Android/Mac publishing, blank apps, optional
ad-hoc profiles, broad `TEMPLATE_APP_TESTFLIGHT_GROUPS`, tester notification blasts,
or replacement/rejection of a build already waiting for beta review. Missing group
configuration fails rather than falling back to upload-only behavior.

The store IPA must pass the same restored source-package hash and final MAUI
assembly provenance gates as dry runs, plus its embedded template provenance
check, **before** artifact outputs or TestFlight upload. Matching `source-packages-*`
are uploaded for independent verification. Publish jobs produce no binlogs.
Uninspectable payloads fail closed, including a future toolchain that removes the
managed metadata required by the checker.

The Actions download contains the **App Store IPA and provenance**, not a
direct-install device IPA. An unsigned dry run is not a substitute for TestFlight
and cannot fix device-installation error `0xe800801c`. TestFlight installation
requires Apple processing, beta review when applicable, and access for the intended
tester. Confirm the actual app version/build and invitation or approved public
TestFlight link before announcing delivery. A successful upload alone is not proof
of tester access, successful launch or accessibility behavior.

## What you get, per platform

Artifacts have different installation requirements. In particular, unsigned iOS
dry runs and App Store IPAs are not directly installable on a tester's device.
The build script emits:

- `package_path` — the **store** package (`.aab` / App Store `.ipa` / Mac App Store `.pkg`).
  Consumed only by the Google Play / TestFlight upload steps.
- `sideload_package_path` — the preferred downloadable artifact. This is what the dry-run
  job and the publish "artifact copy" step upload for testers. When optional Apple sideload
  signing is not configured, the publish job intentionally falls back to the store package.
- `additional_package_path` — an optional extra file uploaded next to the sideload one. Used on
  iOS to include the Simulator `.app` zip alongside the device `.ipa`.

| Platform | Dry-run artifact (`publish=false`) | Publish store target | Sideloadable artifact on publish |
| --- | --- | --- | --- |
| **Android** | Debug-signed **APK** (installs via `adb install` / file manager) | `.aab` → Google Play | Release-signed **APK** |
| **Windows** | **Self-contained** unpackaged zip (no runtime install needed) | same zip | same zip |
| **iOS** | unsigned device **`.ipa`** (not directly installable) + Simulator `.app` zip | App Store `.ipa` → TestFlight | ad-hoc `.ipa` *(only if configured outside iOS-sample-only mode and the profile covers the device)* |
| **macOS (Mac Catalyst)** | Native **arm64** `.app` zip (Apple Silicon) | Mac App Store `.pkg` → TestFlight | notarized `.app` zip *(only if the Developer ID secrets are set — see below)* |

### Windows sample test installer

Set **`windows_test_msix=true` and `publish=false`** to build only the Windows x64
sample as a standard MSBuild-generated, self-contained **MSIX**, rather than an
unpackaged app ZIP. The default eight-app matrix and Windows ZIP remain unchanged.
Combining this option with store publishing is rejected before any build/upload.

The `template-app-test-msix-*` artifact is one outer GitHub ZIP containing the MSIX,
a public test-signing certificate, `Install-WindowsTestApp.ps1`, `install.json`,
`WINDOWS-INSTALL.txt`, provenance and CI validation evidence. Any required framework
packages (for example VCLibs) are included under `Dependencies`; missing dependencies
fail packaging. No private key/PFX, password, nested app ZIP or SDK installation is
required. The ephemeral, non-exportable signing key is deleted after signing.

Extract the artifact once. In **Windows PowerShell as administrator using your own
account**, change to that folder and run `.\Install-WindowsTestApp.ps1`. The script
shows the publisher/certificate thumbprint and asks you to type `TRUST` before adding
the test certificate to **LocalMachine\\TrustedPeople**, never the root CA store.
It checks signatures and invokes Windows' `Add-AppxPackage`, then the app appears in
Start. Cancel before approval to leave trust unchanged. New builds may use new test
certificates. On managed PCs, obtain IT approval; this does not bypass execution
policy, sideloading policy, elevation requirements or application-control restrictions.

The build never changes a local developer/tester's trust store. A separate CI-only
step explicitly trusts the public certificate on a disposable GitHub-hosted Windows
runner, checks signatures with the Windows SDK, installs and activates the packaged
app, then removes the app and test trust. `validation.json` distinguishes successful
signature verification, installation and launch. The download can remain available
when that runner cannot install/activate apps, but the workflow fails and records the
exact limitation rather than claiming a successful launch. A failed signature never
exposes an installer artifact.

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
  build as a `Payload/*.app` **`.ipa`** for inspection; it is not a ready-to-install
  device delivery. A device build needs the correctly provisioned ad-hoc IPA
  (secret-gated, registered devices only) or TestFlight.
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
- **iOS** — use the approved TestFlight invitation/link and the actual processed build
  provided by the maintainer. The dry-run artifact is not a physical-device delivery:
  - **`MyApp.ipa`** — an **unsigned device** build; Finder drag / double-click will
    **not** install it. Do not offer this as a replacement for a signed TestFlight build.
  - **`MyApp.app.zip`** — an **arm64 iOS Simulator** build. Unzip and run it in the Simulator:
    `xcrun simctl install booted MyApp.app && xcrun simctl launch booted <bundle-id>`. It is ad-hoc
    re-signed so the Simulator (which enforces code signing on macOS 15+/26) launches it.

  For a **directly installable** device build (no AltStore, no re-signing) use one of the
  secret-gated publish paths: **TestFlight** (use the iOS-sample-only selection above for
  that sample; testers install from the TestFlight app, no UDID needed) or the
  **ad-hoc `.ipa`** (below) with each tester's device
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

**Required for default all-platform `publish=true`** (protected `template-app-distribution` environment): the Android
keystore, the Google Play service account JSON, the Apple distribution certificate, the App Store
/ Mac App Store provisioning profiles, and the App Store Connect API key. `publish=false` needs
**none** of these — it produces the installable Android APK and self-contained Windows zip
immediately.
The opt-in iOS-sample-only mode requires only its Apple setup and explicit repository
variables listed above, not Android or Mac credentials.

**Optional publish-source policy:**

- `TEMPLATE_APP_TRUSTED_PUBLISH_BRANCHES` — repository variable containing comma- or
  newline-separated exact additional protected branch names that may be used with
  `publish=true`. The default branch is always trusted; wildcards are rejected.

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
the Android and Windows fixes. If optional Apple signing is configured but its publish, signing,
or notarization step fails, the build fails instead of silently uploading the non-installable
store package as though it were a sideload artifact.

The workflow runs the behavioral Pester suite before preparing the build matrix. Dry-run builds
upload MSBuild binlogs for diagnostics because they have no publishing credentials. Publish jobs
do not create or upload binlogs: MSBuild can capture environment-derived signing and store
credentials in structured logs, so publish diagnostics remain in the ordinary masked job log.
