# Hybrid web app

A small .NET 11 `HybridWebView` app with a sibling Vite/TypeScript frontend.
The native project uses the standard `$(MauiSamplePlatforms)` selection and
includes Android, iOS, Mac Catalyst and WinUI heads (and optional Tizen when
selected by the repository). iOS and Mac Catalyst use the current template's
`MauiUISceneDelegate` and single-scene manifest, without a storyboard or duplicate
lifecycle forwarding.

## Run it

From the repository root, after the one-time prerequisites below:

```powershell
# PowerShell 7. On a Mac the default is Mac Catalyst; on Windows it is WinUI.
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1

# Existing in-tree Debug build:
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Platform maccatalyst -NoBuild

# Explicitly select your own booted emulator or simulator:
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Platform android -Device emulator-5554
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Platform ios -Device YOUR-SIMULATOR-UDID

# Windows host:
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Platform windows
```

The script can be invoked from any working directory. `-Platform` chooses the
explicit target framework; `-Device` is passed to the SDK's `--device` option.
The mobile platforms require a device identifier rather than guessing which
of somebody else's running devices to use.

**Mac Catalyst and Android together:** launch Mac Catalyst in one terminal,
then run this in a second terminal:

```powershell
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Platform android -Device emulator-5554 -UseExistingWeb
```

`-UseExistingWeb` shares the already-running sample Vite server; it does not
start or stop it. The same HTML/CSS/TypeScript edits update both native apps.
For Android, `adb -s DEVICE reverse tcp:5173 tcp:5173` makes the host's Vite HTTP
and HMR WebSocket available at the selected device's loopback address. Set
`ANDROID_HOME` or put `adb` on PATH. The reverse mapping is intentionally left
in place so other sample sessions on that device can continue using it.
iOS simulators share the Mac's loopback network. Physical iOS devices need a
separate network configuration, which this loopback-only demo does not provide.

The owner terminal keeps Vite alive even if desktop `dotnet run` returns after
launching the window. Close the native app when finished, then press Ctrl+C in
the owner terminal: cleanup stops only its own child process tree, not another
terminal's server or unrelated apps. The short PS1 does not discover native
process identities or generate inspection tokens. Startup/readiness, server
exit and SDK build/launch failures are reported with nonzero exit codes.

### What runs what? Is this like Tauri?

In Tauri development, the CLI can run a configured frontend development command
(`beforeDevCommand`), then build and launch the native shell against its configured
`devUrl`. The target/device comes from the selected desktop or mobile command and
target options, not from the web frontend. It does not automatically launch every
platform.

Here, the small PS1 fills that local orchestration role:

```text
run-demo.ps1
  +-- npm run dev in Web/       Vite owns JS/HTML/CSS watching and HMR
  +-- dotnet run in Native/     repository SDK owns native build/deploy/launch
        +-- HybridWebView keeps its virtual app origin
              ordinary assets --> Debug forwarding --> Vite on 127.0.0.1:5173
              HMR WebSocket ------------------------> Vite on 127.0.0.1:5173
              reserved bridge routes ---------------> MAUI, unchanged
```

`-Platform maccatalyst`, `android`, `ios` or `windows` selects one app per
invocation. Use separate invocations to launch multiple platforms, sharing Vite
with `-UseExistingWeb`. There is no extra JavaScript watcher or C# Hot Reload.

## One-time prerequisites

Use the SDK pinned by `global.json`, repository-pinned platform workloads, and
the current selected Xcode (`/Applications/Xcode.app`, Xcode 27 for the current
validation). Do not downgrade Xcode to work around the native interop deployment
target: the framework project now specifies the current Mac Catalyst minimum.
The sample inherits repository OS minima (Mac Catalyst 17, Android 24, iOS 13,
Windows 10.0.17763). No global SDK/workload or Xcode changes are needed.

The repo wrapper acquires its SDK locally, but platform workloads are separate.
For a fresh Mac checkout, acquire the pinned manifests, install the needed local
workloads, then build the repository tasks:

```sh
bash eng/common/dotnet.sh --version
bash eng/common/dotnet.sh build src/DotNet/DotNet.csproj \
  -p:InstallDotNet=false -p:InstallWorkloadPacks=false
bash eng/common/dotnet.sh workload install maccatalyst ios android \
  --skip-manifest-update --configfile NuGet.Config
bash eng/common/dotnet.sh build Microsoft.Maui.BuildTasks.slnf \
  -p:IncludeAndroidTargetFrameworks=false -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacCatalystTargetFrameworks=false -p:IncludeMacOSTargetFrameworks=false
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web ci
```

Android also needs OpenJDK 17 and the SDK matching the repository workload.
Node 20.19+ within 20.x, or 22.12+ (including newer majors), is required.
On Windows, use the equivalent repo PowerShell wrapper and the Windows SDK.
No npm command runs during native MSBuild; the sibling `Web/` tree and Node
dependencies are never implicitly compiled or bundled by the native project.

## Layout and edit targets

```text
Controls.Sample.HybridWebApp/
  run-demo.ps1                   focused user launcher
  run-demo.sh                    optional Mac Catalyst validation harness
  Native/
    Maui.Controls.Sample.HybridWebApp.csproj
    Development/                Debug-only forwarding and optional inspector
    Platforms/                  Android, iOS, MacCatalyst, Windows, Tizen
    Resources/Raw/wwwroot/       packaged fallback; no Node required
  Web/
    package.json
    vite.config.ts
    index.html                  #html-message
    src/main.ts, message.ts      #typescript-message, ordinary module import
    src/style.css               #style-target
    src/details.ts              #module-message, dynamic import
```

Vite binds only to `127.0.0.1:5173` with strict-port behavior. Its HMR client uses
`ws://127.0.0.1:5173`, not the virtual app hostname. To use the frontend alone:

```sh
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run dev
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run type-check
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run build
```

`Web/dist/` is not automatically packaged. With no development settings, native
launch loads **HybridWebView is loading bundled content** from its packaged
page. Release excludes all development code. The default project references
this checkout's framework source; explicit `UseWorkload=true` follows the other
samples' convention, but development forwarding refuses that mode because an
older published framework might lack the Apple stopped-task lifetime guard.

## Debug forwarding and optional inspection

The single setting `HYBRIDWEBAPP_DEV_URL=http://127.0.0.1:5173/` enables live
frontend loading. **No inspection token or port is required to run the app.**
The inspector is a separate opt-in: set both `HYBRIDWEBAPP_INSPECT_TOKEN` (a
32–128 character random base64url token) and `HYBRIDWEBAPP_INSPECT_PORT` (an
unused port from 1024–65535, other than 5173).

The optional `run-demo.sh [--no-build]` is still a Mac Catalyst-only validation
harness. It generates an owner-only ephemeral token, records authenticated
native PID/startup identity, keeps Vite alive, and supervises its owned native
instance. Use the small PS1 for ordinary development, not this advanced harness.
Its printed session directory contains `token` and `state.json` while running:

```sh
session=/path/printed/by/run-demo.sh
token=$(cat "$session/token")
port=$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["inspectionPort"])' "$session/state.json")
curl --fail -H "Authorization: Bearer $token" "http://127.0.0.1:$port/probe"
```

Read-only endpoints inspect the actual embedded native WebView, on its UI thread:

- `/state`: native PID/startup identity, URI, mode and bounded fetch metadata.
- `/source`: current DOM as inert plain text with a no-script CSP.
- `/probe`: fixed DOM text, computed CSS, resource URLs and document state.
- `/snapshot`: native PNG (WKWebView snapshot on Apple, WebView drawing on
  Android, WebView2 capture on Windows).

The inspector binds loopback, validates the bearer token and exact loopback Host,
accepts body-free GET only, and has no arbitrary-eval endpoint. Header, response
and request/UI waits are bounded (8 KiB, 2 MiB, ten/eight seconds). For Android,
forward a separate unused host inspection port to the selected device using
`adb -s DEVICE forward tcp:HOST_PORT tcp:INSPECTION_PORT`; use that same number
for both ports because the inspector checks Host. No forwarding is needed for
the iOS simulator. Keep tokens out of source control/logs.

Assets retain `app://0.0.0.1/` on Apple and `https://0.0.0.1/` on Android/Windows.
The provider preserves escaped paths, query, upstream status and MIME, buffers
at most 8 MiB per asset, disables caching/redirects/cookies, and returns explicit
502/504 errors rather than packaged fallback. Redirects are rejected with 502.
HEAD is body-free, including error responses. The reserved routes
`/_framework/hybridwebview.js`, `/__hwvInvokeDotNet`, `/__hwvSendMessage`
remain owned by MAUI.

Apple requests complete asynchronously on the UI thread and retain Core's
stopped-task guard. Windows holds a native deferral before awaiting upstream.
Android's resource callback runs off the UI thread and returns complete response
metadata synchronously; its bounded network work never blocks the UI thread.
Navigation/window destruction cancels outstanding sample fetches. Android's
Debug-only network policy permits cleartext only to `127.0.0.1`, and mixed
content is enabled only for the development view's loopback HMR.

## Validation history and current limits

Earlier Mac Catalyst milestones on 2026-10-09 used SDK
`11.0.100-rtm.26480.113`, Apple `26.5.12253-net11-rc.2`,
Node 24.4.1/npm 11.5.2, Vite 8.3.4 and TypeScript 7.0.2.
An initial Xcode 27 native-framework build was blocked by the old macOS 10.15
deployment target. Those **historical** native validations used a per-process
Xcode 26.6 override; that is not a current prerequisite or recommended workaround.

Historical results:

- Debug/Release native builds, packaged native launch, frontend type-check/build
  and six `WebViewRequestLifetimeTests` passed.
- In-app DOM/source and native PNG confirmed transformed modules and preserved
  app origin. **22 HTML/TypeScript/CSS changes** updated one unchanged native
  PID/startup identity; CSS used HMR, HTML/JS used document reload where needed.
- Embedded fixtures verified status/MIME/escaped query, 404, body-free HEAD,
  explicit 502, redirect rejection, reserved-route bypass, slow fetch abort and
  navigation cancellation without stopped-task callbacks.
- Inspector authentication/Host checks and absent `/eval` passed. The advanced
  Bash harness exercised startup, `--no-build`, occupied ports, failure and
  owned-process cleanup. Logs remain in ignored `artifacts/`.

The current expansion uses selected Xcode 27 and standard scene delegates;
the framework deployment-target correction is recorded separately. Updated
cross-platform build/runtime and PS1 results will be recorded below.
Windows and Tizen execution cannot be validated on this Mac. Physical iOS,
release packaging of the Vite output, NativeAOT certification, generated command
APIs/JS modules, native C# Hot Reload, DevFlow integration, global CLI/MSBuild
integration and automatic all-platform launch are outside this demo.
