# .NET MAUI Playground

A feature-first .NET 11 HybridWebView playground with a sibling Vite/TypeScript
frontend. The portable prototype is contained in this sample folder; a Labs
migration and DevFlow integration are deferred. No accounts, backend, telemetry or mock native
values are used by the app.

## Standalone JavaScript SDK

The frontend consumes the app-independent
[`@maui/api` SDK](JavaScript/README.md) through a normal
local npm dependency. Its prepared preview is `0.1.0-preview.1`: emitted ESM,
TypeScript declarations, MIT license and product/API/protocol/release docs are
the standalone shipping artifact. `@maui/api` is a working name; scope/name
ownership is unverified and nothing has been published to npm.

Generic app/device information, clipboard, preferences and transport modules live in
the independently packable sibling `JavaScript/`, not in the frontend. Custom `sample.*` helpers remain
in `Web/src/native/sample.ts`; the SDK includes no UI, sample commands, Vite
dependency or sample preferences namespace. Ordinary SDK consumers can use
`clipboard()`/`preferences()`/`getAppInfo()`/`getDeviceInfo()` without supplying a view or client;
lazy startup still rejects browser-only native calls, never emulates them.

This is one npm product with isolated modules, not per-feature packages. The
root exports infrastructure/lifecycle and core types only; Essentials helpers
live under `@maui/api/essentials/*`. The unpublished `app/info`, `getInfo` and
SDK `Identity` exports were removed rather than retained as ambiguous aliases.
The app owns process architecture, PID and startup diagnostics and composes
them with the real SDK AppInfo/DeviceInfo helpers for its reload experiment.

Public symbols and members are documented in TypeScript; emitted declarations
retain the comments for editor tooltips. `npm --prefix JavaScript run docs` from this folder
generates an ignored per-entrypoint Markdown reference and DocFX navigation.
The package's [release documentation](JavaScript/docs/RELEASING.md)
explains standard tooling, packed-reference equality checks and the separate
Microsoft Learn onboarding step. No Learn publication is performed.

A matching native protocol host remains required. Stock MAUI 11 invocation does
not automatically supply this app's command hub. The native registration and
providers below are the current reference implementation; no companion NuGet
SDK or template is shipped by this extraction.

## Run

After the one-time prerequisites below, from any working directory:

```powershell
# PowerShell 7: defaults to Mac Catalyst on macOS, WinUI on Windows.
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1

# Explicitly select your own booted device.
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Platform android -Device emulator-5554
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Platform ios -Device YOUR-SIMULATOR-UDID

# Share the first terminal's Vite server; never stop or replace its owner.
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Platform android -Device emulator-5554 -UseExistingWeb

# Same playground, no Vite. Build/stage runs explicitly before native build.
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Offline
pwsh -File src/Controls/samples/Controls.Sample.HybridWebApp/run-demo.ps1 -Offline -Configuration Release
```

`-NoBuild` reuses previously staged/built output. Close the existing native app
before relaunching with new environment settings. The launcher runs
`npm run stage`, starts Vite for live development, then separately builds and
deploys with the repository SDK in-tree, or normal `dotnet` in packaged mode.
`-DotNetPath` explicitly selects an installed executable. It never installs npm dependencies on launch.
Ctrl+C stops only its owned child process tree; close the native app separately.
Native MSBuild never invokes npm. Release rejects live forwarding and excludes
`Development/**`, the inspector and its server.

The standard Android/iOS/MacCatalyst/Windows heads and Apple scene delegates are
retained. No Tizen target is included. iOS uses the host architecture's simulator
RID; Android deployment uses `BuildProjectReferences=false` after the explicit
build to preserve source-reference resource inputs. The launcher isolates each
platform's intermediate and binary outputs under
this folder's `artifacts/{obj,bin}/hybrid-playground/PLATFORM/`, so concurrent native launches
cannot overwrite each other's restore graph. `-NoBuild` therefore reuses a build
made by this launcher for that platform and configuration.
Android uses explicit
`adb -s DEVICE reverse tcp:5173 tcp:5173`; iOS simulators share host loopback.
Physical iOS-device networking is not configured by this loopback-only launcher.

## Copying to MAUI Labs

Copy this **whole folder**, not just `Web/`. `JavaScript/` remains one standalone
npm product (`@maui/api` with six entrypoints), consumed via `file:../JavaScript`;
it is not compiled as app source. `Native/` owns the host/providers/protocol,
platform heads/assets and Debug forwarding/inspection; `Tests/`, staging,
launchers, documentation and the [121-family backlog](FEATURES.md) travel with it.
Do not copy generated `node_modules`, `dist`, `artifacts`, `bin`, `obj`, tarballs
or stamps; restore and regenerate them in the destination.

Sample-local `Directory.Build.props/targets` detect this specific MAUI checkout
using its Core project and in-tree props. Only that mode imports MAUI repository
build configuration and uses project references. A copied folder defaults to
packaged `UseMaui=true` with its own outputs and explicit .NET 11 targets.
`-p:HybridWebAppInTree=false` (launcher `-Packaged`) selects that mode explicitly,
even in this checkout. It does not set `HYBRIDWEBAPP_IN_TREE`. Tests retain
in-tree versions/targets; outside the checkout they explicitly reference xUnit
2.9.3, test SDK 18.0.1 and VS runner 2.8.2, linking only pure .NET host files.

The copied-folder check also controls SDK and NuGet configuration discovery:
its ignored validation fixture has its own exact SDK selection and a dedicated
NuGet configuration passed with `--configfile`. The 25 standalone tests pass
with only that configuration recorded in restore assets, no MAUI project
references or inherited MAUI build imports. The explicitly selected installed
SDK was repository-local; this is not a claim that it exists on another machine
or that an external native MAUI package has been run.

From the copied bundle root, with Node 22.18+ and an explicitly installed .NET 11 SDK:

```sh
npm --prefix JavaScript ci
npm --prefix Web ci
npm --prefix JavaScript test
npm --prefix JavaScript run pack-check
npm --prefix Web test
npm --prefix Web run type-check
npm --prefix Web run stage
dotnet test Tests/Playground.UnitTests.csproj --disable-build-servers
pwsh -File Tests/launcher.test.ps1 -DotNetPath /absolute/path/to/an/installed/dotnet
```

The SDK's real-framework double-bootstrap regression uses an attributed,
test-only [snapshot](JavaScript/tests/fixtures/README.md). It is neither shipped
nor loaded in production; the runtime framework script stays in Core.

External **native execution is a migration gate**, not proven by these portable
tests. Supply a compatible .NET MAUI **11** package version and its NuGet feed,
matching .NET 11 SDK/workloads and platform tooling. No working public package
version/feed is guessed or automatically installed. For packaged launch:

```powershell
pwsh -File ./run-demo.ps1 -Offline -MauiVersion YOUR-MAUI-11-VERSION
# Evaluate the selected TFM/mode/arguments without staging, launch or device changes.
pwsh -File ./run-demo.ps1 -Offline -MauiVersion YOUR-MAUI-11-VERSION -PrintPlan
# Optional: -DotNetPath /absolute/path/to/an/installed/dotnet
```

`Native/MauiProgram.cs` still rejects Vite forwarding in packaged builds.
Consuming a MAUI version with the actual Core stopped-task fix and separately
validating its runtime is required before reviewing that guard for removal.
Never fake the in-tree define, reflect into private handlers or suppress it.
Protocol registration, generated .NET 11 interop and platform APIs still depend
on the consumed MAUI runtime. DevFlow integration, NuGet/SDK/feed readiness,
Labs onboarding and native platform acceptance remain separate migration work.
No npm/Learn/NuGet publication or full external native run is claimed.

The framework-only keep-list (paths relative to `src/`) is:

- `Core/src/Handlers/HybridWebView/HybridWebViewHandler.iOS.cs`
- `Core/src/Platform/iOS/WebRequestInterceptingWebView.cs`
- `Core/src/Primitives/WebResourceRequestedEventArgs.cs`
- `Core/src/Primitives/WebViewRequestLifetime.cs`
- `Controls/src/Core/WebRequestInterceptingWebView/PlatformWebViewWebResourceRequestedEventArgs.cs`
- `Controls/src/Core/WebRequestInterceptingWebView/WebViewWebResourceRequestedEventArgs.cs`
- `Core/AppleNative/PlatformInterop/MauiPlatformInterop.xcodeproj/project.pbxproj`
- `Core/tests/UnitTests/WebViewRequestLifetimeTests.cs`

These stay outside the bundle: native WebKit stopped-task lifetime,
dispatch/tracking, event-args liveness and public remarks cannot safely be
implemented by copying app code. Xcode deployment floors affect the native
framework itself. The six targeted Core lifetime tests belong with those
framework changes, not the portable host tests.

The lifetime/adapter changes fix existing asynchronous response callbacks after
WebKit stops a request or the handler disconnects; they are not new public APIs.
The deployment floors are native build compatibility corrections. Unit coverage
currently exercises the guard state machine; real Apple reload checks are
recorded native acceptance, not a committed WebKit device regression test.

`run-demo.sh` is intentionally an **in-tree Mac Catalyst validation harness**,
not the portable launcher. It checks repository SDK/packs/build tasks and uses
repository output paths; a copied bundle fails early with directions to
`run-demo.ps1`. Its `--no-build` refers to its own in-tree build, not a PowerShell
launcher's platform-isolated build.

## Experiments

The [implementation backlog](FEATURES.md) records 121 families and marks only
fully implemented, verified experiment scopes. Partial work stays unchecked,
with missing acceptance called out. The groups contain 19 P0, 29 P1, 47 P2 and 26 P3 items.
The playground shows only implemented experiments, with category/search and
short descriptions. Priorities and implementation tracking stay in the
development document, not the user interface; later families are hidden and
have no dummy commands. P0 foundations share
focused experiments rather than pretending each architectural concern is a
different native API:

- **Identity:** app/device/version/architecture and native PID, startup UUID,
  startup UTC time. These originate in native code, not a frontend counter.
- **Clipboard:** explicitly copy/paste Unicode text through the native OS API.
- **Preferences:** read/write/remove text under `maui.playground.v1.`; restart
  the app to check persistence. Keys: 1–48 ASCII letters/digits/underscore/hyphen;
  text: at most 4096 UTF-16 code units. No clear-all or arbitrary namespace access.
- **DTO echo:** asynchronous nullable text, finite number, boolean, string array
  (at most 16 entries), native instance counter; Unicode/escape-safe transport.
- **Errors/policy:** intentional error, unknown command, disabled command and
  invalid arguments. OS denial and unsupported native API failures are distinct
  from app exposure policy. The bridge remains usable after rejection.
- **Lifetime/events:** native service counter survives frontend reload. Explicit
  native subscriptions/unsubscriptions emit correlated service changes.
- **Work:** bounded native async steps, correlated progress, visible cancellation.
  At most 50 steps, 100–500 ms/step, 15 seconds of work, 16 simultaneous calls.
  The hub applies a 20-second call deadline; the frontend applies 23 seconds.

Browser-only mode provides the real catalog/UI but says **Native host
unavailable** and disables native actions. There is no silent clipboard,
preferences, identity, progress or device emulation. Test doubles are confined
to `Tests/` and `Web/tests/`.

The document is a fixed-height app shell, not a scrolling web page. The header
stays in place; the catalog and experiment pane scroll independently, with
contained overscroll. On narrow or keyboard-reduced viewports, navigation is
bounded so the experiment remains reachable. Inputs/results can still scroll;
native WebView touch scrolling is not globally disabled.

## Prototype architecture

`Native/Playground/HybridAppTargetObject.cs` is instantiated once per WebView.
`MainPage` composes the command registry, Essentials adapter and sample service,
then supplies the router's readiness metadata and native event callback.
Providers explicitly register their allowed event topics. The session/router
does not choose sample providers, hard-code their topics or own MAUI UI objects.
Complete registration before attaching the host; treat the registry as immutable
while sessions are active.

`MainPage` calls the existing .NET 11
`SetInvokeJavaScriptTarget<IHybridAppTarget>(hub, PlaygroundJsonContext.Default)`.
The narrow interface exposes only `Task<string> Invoke(string request)`.
The framework source generator intercepts this call; there is no reflection
fallback, assembly scanning or arbitrary-public-member invocation.

Providers explicitly register names, typed handlers and source-generated
`JsonTypeInfo<TArgs>` / `JsonTypeInfo<TResult>` in `CommandRegistry`.
To add an application command, add DTO metadata to `PlaygroundJsonContext`,
register it in an opted-in provider, and author its typed TypeScript helper.
The router needs no new forwarding switch. **No command attribute or client
generator has been implemented.** TypeScript modules are authored and tested:

```ts
import { clipboard } from '@maui/api/essentials/clipboard'
import { preferences } from '@maui/api/essentials/preferences'
import { getAppInfo } from '@maui/api/essentials/app-info'
import { getDeviceInfo } from '@maui/api/essentials/device-info'
import { sample } from './native/sample.ts'
```

Feature helpers depend on the SDK client contract, not the app or Controls.
An explicit `NativeClient(Bridge)` can use a compatible native transport without
MAUI's automatic origin/script bootstrap. Automatic `connect()` remains a
HybridWebView convenience. The integration owns events, leases and platform
initialization; an alternative native .NET for Android/iOS host adapter is not
implemented or shipped. Essentials and Graphics do not require Controls/Core,
but MAUI Core contains UI handlers, so do not pull it into reusable service code
merely to obtain a clipboard/preferences provider. No companion NuGet split,
plugin discovery framework or placeholder Graphics API is introduced.

All DTO/envelope runtime serialization uses System.Text.Json source-generated
metadata with camelCase field names, nullable/required-constructor checks and
unknown-member rejection. Native adapters dispatch Essentials APIs onto the
main thread and check document cancellation before/after the native call.

The framework `InvokeDotNet` places JSON in `X-Maui-Request-Body`, a Fetch
ByteString header. The sample therefore passes **one ASCII-escaped JSON string**
argument; UTF-16 code units above ASCII are `\uXXXX` escaped before the
framework serializes it. Unicode, surrogate pairs, CR/LF, NUL, quotes and
backslashes round-trip without changing the framework or losing text.

Requests carry protocol version, document/session UUID, request UUID, registered
command name and typed args. Responses are explicit success/error/cancelled
envelopes; raw native events carry the same session and originating request ID.
Inputs are bounded; unknown/disabled commands are rejected. This is an
application-owned allowlist, not a substitute for trusting packaged/frontend
content, OS permissions or a general-purpose sandbox.

Each document attaches one event listener and owns topic subscriptions.
`pagehide`, HMR teardown, handler disconnect and owning-window destruction
clean up; a new document connect cancels obsolete work. Heartbeats every two
seconds maintain a 15-second lease for abrupt disconnects. If an OS suspends the
frontend long enough to expire its lease, reload to reconnect. App/native
service state remains. Native send callbacks recheck the session on the UI
thread; stale events are also rejected by the frontend. Retired document
identities cannot reconnect over a newer document; after 4096 retired documents,
restart the native app to begin a new bounded lifetime. Command failures log
their command/code/exception type without request values or clipboard contents.
Cancellation cannot undo
an OS side effect already completed, such as copying text.

The native origin stays `app://0.0.0.1/` on Apple and `https://0.0.0.1/` on
Android/Windows. Bootstrap loads `/_framework/hybridwebview.js` only there.
All three reserved routes (that script, `/__hwvInvokeDotNet`,
`/__hwvSendMessage`) remain framework-owned. Debug Vite forwarding still buffers
at most 8 MiB per asset, preserves status/MIME/escaped paths/queries, supports
HEAD, rejects redirects, and reports 502/504 rather than substituting stale
packaged content. Apple's internal stopped-task guard is required for live
forwarding; this sample does not introduce a public lifecycle cancellation API.

## Packaged assets and frontend editing

```sh
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run stage
```

This rebuilds the standalone SDK, type-checks/builds the app and records
app/SDK input/output SHA-256 hashes in an ignored
`Web/dist/playground-assets.props`. `Native/FrontendAssets.targets` collects the
manifest-listed assets as `wwwroot/`, preserving asset paths and dynamic chunks.
Extra unstaged files are never packaged. Native builds
fail on missing staging or stale source/output hashes. The generated manifest
also checks added/deleted SDK source/output files without adding npm to native
MSBuild. Staging verifies the SDK's deterministic build stamp, so modified SDK
source cannot be marked fresh over obsolete emitted JavaScript.
Regenerate with `stage` after edits, including SDK source/config/build scripts,
package metadata and lockfiles. Direct `npm run build` intentionally
invalidates staging until `stage` is run. Generated output and `node_modules`
are not committed. Offline Debug and Release contain the same playground and
load the framework bridge, not the old fallback page.

Vite remains strict-port `127.0.0.1:5173` with loopback HMR WebSocket. Live-edit
markers are `#html-message`, `#typescript-message` (`message.ts`),
`#module-message` (dynamically imported `details.ts`) and `#style-target`
(`style.css`); the last three are in the native-identity disclosure.

`dev`, `test`, `type-check` and `build` rebuild the local SDK dependency before
running. SDK edits are not direct Vite TypeScript imports: run
`npm --prefix …/Web run sdk:build` to update emitted modules, and restart Vite
when changing dependency exports. UI-only edits retain normal Vite HMR.

## Optional Debug inspection

No inspector is needed for the command hub. Opt in with
`HYBRIDWEBAPP_INSPECT_TOKEN` (32–128 random base64url characters) and
`HYBRIDWEBAPP_INSPECT_PORT` (an unused 1024–65535 port other than 5173), inherited
by the launcher and explicitly passed to Debug apps with the SDK's environment
handoff (mobile apps do not inherit the host shell environment). Keep tokens out
of logs/source control. The existing long
`run-demo.sh` is an optional Mac-only validation harness, **not** the user path;
stage the frontend explicitly before using it.

Authenticated loopback, exact-Host, body-free GET endpoints:

- `/state`: native PID/startup identity, URI, content mode, bounded fetch metrics.
- `/source`: actual DOM as inert plain text with no-script CSP.
- `/probe`: fixed DOM/CSS/resources plus `playground.hostState`, `session`,
  `startupId`, `identity`, selected `feature`, `resultState`, `result`, native
  `progress`, `lastEventId` and latest 40 operation-log entries.
- `/snapshot`: native WebView PNG.

No `/eval` exists; callers cannot supply JavaScript. Inspector bounds remain
8 KiB headers, 2 MiB responses, ten-second request and eight-second UI waits.
For Android, forward the same unused inspector port on host/device to preserve
the exact Host check. No forward is needed for iOS simulator.

For native checks, use catalog buttons `[data-feature="identity"]`,
`clipboard`, `preferences`, `arguments`, `errors`, `policy`, `lifetime`,
`events`, `work`; action selectors are `button[data-action="…"]`.
Read `#host-status[data-state="ready"]`, `#native-identity`, `#result-state`,
`#result-json`, `#operation-log` and `#progress`. Verify native startup identity
does not change after frontend reload, preferences survive app restart, errors
recover, unsubscribe prevents events, and cancellation ends as **Cancelled**.
The fixed probe observes results; it does not execute experiments.

## Prerequisites and targeted checks

Use root `global.json`'s SDK via `bash eng/common/dotnet.sh` on macOS (the
PowerShell wrapper on Windows), installed repository-local Apple/Android
workloads and compiled `Microsoft.Maui.BuildTasks.slnf`. Keep the currently
selected Xcode 27; do not downgrade `DEVELOPER_DIR`. Android requires OpenJDK 17
and the matching SDK. Restore the standalone package and the frontend once
before launch (the launcher does not restore either):

```sh
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/JavaScript ci
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web ci
```

Node 22.18+ is required (app tests use Node's TypeScript stripping;
verified with Node 24). The platform minimums remain Android 24, iOS 15,
Mac Catalyst 17, Windows 10.0.17763.

```sh
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/JavaScript test
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/JavaScript run pack-check
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/JavaScript run docs
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web test
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run type-check
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web run stage
bash eng/common/dotnet.sh test src/Controls/samples/Controls.Sample.HybridWebApp/Tests/Playground.UnitTests.csproj --disable-build-servers
bash eng/common/dotnet.sh build src/Controls/samples/Controls.Sample.HybridWebApp/Native/Maui.Controls.Sample.HybridWebApp.csproj \
  -f net11.0-maccatalyst -c Debug --disable-build-servers -p:UseWorkload=false \
  -p:IncludeAndroidTargetFrameworks=false -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacCatalystTargetFrameworks=true -p:IncludeMacOSTargetFrameworks=false \
  -p:IncludeWindowsTargetFrameworks=false
```

Pure .NET tests link only the hub/contracts/providers, using a fake platform
boundary (not a fake native UI). Node built-in tests exercise the actual
SDK's built public exports and app-owned helpers with explicit test doubles.
Package `pack-check` additionally inspects the actual tarball, installs it in a
fresh offline consumer and compiles/runs against shipped JS/declarations
without the source tree. These checks do not
claim Android/iOS/Windows native validation or store/signing readiness.

### Initial native milestone evidence

On 2026-10-12, actual Mac Catalyst, iOS 27 simulator and Android emulator WebViews
completed the same bounded UI-driven acceptance sequence: native identity,
Unicode clipboard, namespaced preferences, asynchronous DTO/array/NUL
round-trips, explicit errors and recovery, subscribe/unsubscribe, correlated
progress and terminal cancellation. Reload during native work preserved the
native PID/startup ID and service counter, rejected a retired document's
reconnect, and kept obsolete progress out of the new document.

The final explorer hides unimplemented families and shows descriptions instead
of priority or implementation-tracking labels. Desktop/mobile browser checks
confirmed browser-only native actions stay disabled, search/category work, and
there is no horizontal overflow. An explicit browser test double verified
delayed paste/read and work startup while navigating between experiments; those
results are not counted as native verification.

The final targeted checks passed: `Web test` (11 tests), `Web run stage`, and
`Playground.UnitTests.csproj` (24 tests). Release Mac Catalyst built with zero
warnings/errors; its four packaged frontend assets matched staging
byte-for-byte. Extra unlisted output was excluded from native asset collection.
Android packaged Debug launched with no listener on Vite port 5173, reported
`packaged` with zero proxy requests, and retained a native preference across an
actual app restart. Temporary acceptance scripts were removed before staging.

### Standalone SDK milestone evidence

After extraction, the package's 18 built-export tests passed, including delayed
handshake cancellation/replacement, single framework initialization and bounded
startup. `pack-check` verified the 16-file tarball, a fresh offline tarball
installation, production ESM imports and declaration compilation without
`skipLibCheck`. The seven app-owned contract tests and explicit staging passed.
The test count moved from the app to the SDK with the reusable contracts.

The actual Android WebView repeated the full native acceptance sequence using
the package dependency. Additional native checks called lazy `getInfo()`,
`clipboard()` and `preferences()` without a view/client argument, exercised
custom commands/events/errors, and disconnected/reconnected without restarting
native. Android touch gestures scrolled the catalog and experiment independently:
the document remained at scroll offset zero and its height equalled the viewport;
the header did not move. Browser geometry also passed desktop, phone, landscape
and short-height viewports. The iOS 27 app remained connected after the package
update; simulator touch automation was unavailable, so its touch/bounce/keyboard
behavior is not claimed as verified by these new checks.

The package-backed Android Debug build then passed with zero warnings/errors.
Its four APK frontend assets matched staging byte-for-byte. The same full
native acceptance and touch-scroll checks passed in offline mode, with only
packaged production assets and framework routes requested, not Vite resources.

### Modular API and host-boundary evidence

The Essentials AppInfo/DeviceInfo split, infrastructure-only root and source
documentation passed 24 SDK tests and nine app tests. The 18-file tarball installs
without SDK sources and exposes exactly the intended entrypoints. Generated
local and packed references match all six entrypoints' exact signatures, public
members and TSDoc; 97 modeled items match, rather than relying on counts alone.
Explicit transport tests exercise the feature helpers without MAUI origin
bootstrap or Controls.

Host-owned providers, readiness metadata and event topics passed 25 pure .NET
tests, including a custom host with no implicit Essentials/sample providers.
The new Android Debug build passed with zero warnings/errors; its four SDK-backed
assets matched staging. Full native acceptance, frontend reload/service lifetime,
and independent touch scrolling passed again in its packaged offline app.
Alternative native .NET for Android/iOS adapters and DocFX site rendering remain
future integration work, not additional packages or verified runtime support.

### Relocation verification

The same SDK pack-check (including build/docs and 24 tests), 18-file offline
tarball install, six-entrypoint API/TSDoc equality, nine frontend tests,
type-check/stage and 25 pure .NET host tests passed after relocation.
They also passed after copying **only this folder's source** into ignored
`artifacts/portable-copy/Controls.Sample.HybridWebApp/`, restoring npm dependencies
offline and restoring the test packages against nuget.org. The proof explicitly
used the checkout's `.dotnet/dotnet` executable, not a claimed globally installed SDK.

Copied native/test project evaluation and preprocessed imports showed
`HybridWebAppInTree=false`, no root MAUI Directory.Build imports, project
references, repository wrapper or `HYBRIDWEBAPP_IN_TREE` define. The copied
launcher automatically chose packaged Android mode. Launcher plan tests passed
for Android/iOS/MacCatalyst and confirmed explicit-version and Vite guards;
the in-tree-only shell harness rejected the copy before starting any process.

Copied staging rejected SDK source/script/config/metadata/output edits and
source additions/deletions/output additions. Restoring those inputs restored
freshness. Exactly four logical `wwwroot` assets were collected; unlisted
output was excluded. An in-tree Android Debug build then succeeded with three
existing Gradle Java-8 deprecation warnings and no errors, using the launcher's
sample-owned platform output directories; all four APK frontend assets matched
staging byte-for-byte. No native device launch or external NuGet native run was
performed for this relocation.

Evidence is kept in ignored `artifacts/playground-*`, `artifacts/relocation-*`,
`artifacts/copied-*` and the isolated `artifacts/portable-copy/`. Windows runtime, physical
devices, actual Release execution, NativeAOT certification and signed
installation remain unverified. Native runtime checks here do not establish CI
or merge readiness.
