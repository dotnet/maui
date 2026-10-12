# .NET MAUI Playground

A feature-first .NET 11 HybridWebView playground with a sibling Vite/TypeScript
frontend. This in-tree prototype stays in the MAUI PR; a Labs migration and
DevFlow integration are deferred. No accounts, backend, telemetry or mock native
values are used by the app.

## Standalone JavaScript SDK

The frontend consumes the app-independent
[`@maui/api` SDK](../../../HybridWebView/JavaScript/README.md) through a normal
local npm dependency. Its prepared preview is `0.1.0-preview.1`: emitted ESM,
TypeScript declarations, MIT license and product/API/protocol/release docs are
the standalone shipping artifact. `@maui/api` is a working name; scope/name
ownership is unverified and nothing has been published to npm.

Generic identity, clipboard, preferences and transport modules live in
`src/HybridWebView/JavaScript/`, not in this app. Custom `sample.*` helpers remain
in `Web/src/native/sample.ts`; the SDK includes no UI, sample commands, Vite
dependency or sample preferences namespace. Ordinary SDK consumers can use
`clipboard()`/`preferences()`/`getInfo()` without supplying a view or client;
lazy startup still rejects browser-only native calls, never emulates them.

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
deploys with the repository SDK. It never installs npm dependencies on launch.
Ctrl+C stops only its owned child process tree; close the native app separately.
Native MSBuild never invokes npm. Release rejects live forwarding and excludes
`Development/**`, the inspector and its server.

The standard Android/iOS/MacCatalyst/Windows heads and Apple scene delegates are
retained. No Tizen target is included. iOS uses the host architecture's simulator
RID; Android deployment uses `BuildProjectReferences=false` after the explicit
build to preserve source-reference resource inputs. The launcher isolates each
platform's intermediate and binary outputs under
`artifacts/{obj,bin}/hybrid-playground/PLATFORM/`, so concurrent native launches
cannot overwrite each other's restore graph. `-NoBuild` therefore reuses a build
made by this launcher for that platform and configuration.
Android uses explicit
`adb -s DEVICE reverse tcp:5173 tcp:5173`; iOS simulators share host loopback.
Physical iOS-device networking is not configured by this loopback-only launcher.

## Experiments

The development catalog records 121 families: 19 P0, 29 P1, 47 P2, 26 P3.
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
import { getInfo } from '@maui/api/app/info'
import { sample } from './native/sample.ts'
```

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
npm --prefix src/HybridWebView/JavaScript ci
npm --prefix src/Controls/samples/Controls.Sample.HybridWebApp/Web ci
```

Node 22.18+ is required (app tests use Node's TypeScript stripping;
verified with Node 24). The platform minimums remain Android 24, iOS 15,
Mac Catalyst 17, Windows 10.0.17763.

```sh
npm --prefix src/HybridWebView/JavaScript test
npm --prefix src/HybridWebView/JavaScript run pack-check
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

Evidence is kept in ignored `artifacts/playground-*`. Windows runtime, physical
devices, actual Release execution, NativeAOT certification and signed
installation remain unverified. Native runtime checks here do not establish CI
or merge readiness.
