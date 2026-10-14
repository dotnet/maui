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

Generic app/device information, clipboard, preferences, power/display, file-picking,
page/window UI and transport modules live in
the independently packable sibling `JavaScript/`, not in the frontend. Custom `sample.*` helpers remain
in `Web/src/native/sample.ts`; the SDK includes no UI, sample commands, Vite
dependency or sample preferences namespace. Ordinary SDK consumers can use
direct `getText()`/`setText()`, preference `get()`/`set()`, `getAppInfo()` and `getDeviceInfo()` without supplying a view or client;
lazy startup still rejects browser-only native calls, never emulates them.

This is one npm product with isolated modules, not per-feature packages. The
root exports infrastructure/lifecycle and core types only; Essentials helpers
live under `@maui/api/essentials/*`; page/window helpers live under
`@maui/api/ui/*`. The unpublished `app/info`, `getInfo` and
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
providers below are the current reference implementation; no official NuGet
release or template is claimed.

## Managed product and sample separation

[`Managed/Maui.HybridApp.csproj`](Managed/README.md) is one real independently
packable class library, still under this samples folder while WIP. Its working
identity `Maui.HybridApp` / `0.1.0-preview.1` is unverified and unpublished.
`Native` references its assembly, not its source. Product code owns the generic
session/registry/protocol, AppInfo, DeviceInfo, Clipboard, Preferences,
Battery, Display, FilePicker, Connectivity, TextToSpeech, external URL launching
and MAUI page/window UI plugins. Each feature family is one attributed class with
typed parameters, real options/results and owned event/resource lifecycle.
They use the existing Essentials interfaces directly, with one small
main-thread/cancellation boundary; there is no combined platform interface.
The Catalyst battery correction now belongs to **Essentials**, not this product:
`Battery.Default` reads actual IOKit capacity, charging and power-source state
and uses native power-source change notifications. UIKit had returned about 1%
while macOS reported about 60%. The product uses `IBattery` on every platform;
no sample-specific power adapter or numerical correction remains.
The current user-approved WIP boundary keeps UI providers in this same managed
product. It now depends on Controls.Core and Core transitively, in addition to
Essentials and minimal standard DI abstractions; it still has no sample/app
dependency. There is no second integration NuGet. An Essentials-only split is deferred.

Startup now has one product registration and one sample composition call:

```csharp
builder.Services
    .AddHybridApp(new HybridAppOptions(SamplePolicy.PreferenceNamespace, SamplePolicy.MaximumTextLength))
    .AddPlayground(StartupId, StartedAt)
    .AddScoped<MainPage>();
```

`AddHybridApp` owns the implemented product providers, default existing Essentials
interfaces, dispatcher, clock, policy and scoped registry/hub. It calls the
generated `AddMauiHybridAppActions()` once for the entire managed assembly.
A Toolkit assembly has its own single generated extension, such as
`AddMyToolkitActions()`. Identical product registration is
idempotent; conflicting policy fails explicitly, and host-supplied interfaces,
dispatcher and clock are retained.

Annotate accessible methods with `[HybridAction("name")]`; the compiler gathers
the entire assembly into one explicit registration method. The app's
`AddPlayground` calls `AddMauiControlsSampleHybridWebAppActions()` once for all
sample actions. There is no `IHybridCommandProvider`, per-type opt-in or manual
command registration layer. Ordinary parameters come from named JSON arguments or
a positional array; `[FromServices]` and the existing `[FromKeyedServices]`
resolve only native DI services. Cancellation and operation context never come
from web input. Instance actions use the owning window's scope rather than a
root/global service instance. See [the managed product](Managed/README.md) for
the supported signatures, diagnostics and package-analyzer details.
`HybridTopic` and paired `HybridEvent` start/stop attributes declare subscription
lifecycle without another provider interface. Generated bindings capture the
owning scoped plugin, so teardown can detach native handlers during DI disposal.
The WIP wire contracts were modernized together with the JS modules: preferences
use get/set/contains/remove/clear, and single-file picking returns nullable
metadata directly, not a legacy `{ file }` envelope. No compatibility adapters
or old command aliases remain.

Native enum contracts now use exact camelCase names rather than numeric values
or handwritten string switches. Dialog flow direction, application theme,
battery/network/display results and native event payloads use the actual MAUI
enums; flags use explicit arrays of names. Unknown names, numeric input and
duplicate flags reject before invocation. The SDK exposes matching closed
TypeScript unions and runtime validation. `unspecified` is the native theme
preference that follows the OS, not the old `system` wire alias.
Text-to-speech accepts native `SpeechOptions` and returns native `Locale` data
without duplicate managed DTOs. DevicePlatform remains an extensible string.
SDK-level annotations, referenced-interface discovery, contextual page service
resolution and structural Share unions are design investigations, not shipped
bridge features.

### Binary data: three established paths

The prototype follows Tauri's API patterns with an independently implemented
MAUI transport:

| Path | Public surface | Ownership |
|---|---|---|
| Whole-buffer invocation | `invokeBinary` accepts Blob/ArrayBuffer/views and returns Uint8Array; `binaryBlob` supplies caller-known MIME | The existing scoped hub authorizes the action, document and request |
| Ordered native output | `Channel<T>` / `HybridChannel<T>`; `Channel.bytes` for raw chunks | The owning action; native sends await JS consumption acknowledgements |
| Native reader resources | Selected-file `openRead`, `readBytes`, `readText`, `seek`, `close` | The owning document; existing readers now return binary chunks, not base64 JSON |

Core supplies an optional `IHybridWebViewBinary` capability. The host explicitly
sets `HybridWebView.BinaryInvoker` before boot; `HybridAppBinaryInvoker` admits
only the SDK's authorized route. Existing framework JSON invocation remains
unchanged. Whole input/output buffers are at most 4 MiB each; binary action
metadata has a separate 2400-character budget, not the JSON action's 100000
budget. Apple/Windows use raw request bodies; Android uses sequential bounded
encoded fragments because intercepted requests expose no body. All three
return raw resource response bytes. This is not zero-copy or arbitrary
Stream serialization.

Channels have one send in flight and at most four channels per action.
An async JS consumer is awaited before acknowledging consumption. Binary
chunks use binary fetches; small typed messages use generated JSON metadata.
Three ordinary binary actions leave one Core transfer slot for serialized
channel fetches; four reserved native control slots remain available alongside
the 16 ordinary hub operations. The SDK's control queue is bounded to 64 calls.
Completion, consumer errors, cancellation and document retirement release
pending channel data. Channels do not outlive their action. Core's 30-second
binary transfer deadline still applies, even to an interactive binary action;
ordinary JSON dialogs retain their existing five-minute budget.

The existing Arguments experiment demonstrates a 256 KiB Blob roundtrip,
a native-produced PDF returned as a browser Blob, and ordered raw chunks with
typed progress. PDF browser download is not native OS sharing. Selected-file
handles remain read-only; no arbitrary paths, native file writer or live-stream
upload API is exposed. See the managed and SDK documentation for signatures,
limits and explicit native host requirements.

Actual Mac Catalyst and Android acceptance round-trips empty, all-byte-value,
256 KiB and exact 4 MiB buffers, validates sliced-view offsets, receives a
native PDF and ordered raw chunks with awaited consumers, rejects oversized
input, recovers after consumer failure and repeats after document reload.
Android additionally selects a real 200035-byte file, reads four binary chunks
with EOF, seeks/reads, releases its resources and cancels the real picker.
Windows runtime and iOS/physical-device acceptance of these new paths remain
unverified.

For custom web calls, the shared-client core/root `invoke` function avoids a
client factory at each call site:

```ts
import { invoke } from '@maui/api'

const result = await invoke('my.action', { title: 'Hello', options: {} })
// result is unknown until validated; pass a runtime decoder for a typed result.
await invoke('clipboard.write', ['Hello from a generated action'])
```

Named feature modules remain the simplest way to use stock capabilities.
No AI runtime, runtime assembly scanning or application TypeScript generator
is required. Attributes do not grant OS permissions or bypass host policy.

Connectivity reads actual OS access/profiles and can subscribe to scoped changes;
it is not an HTTP reachability probe. Text-to-speech exposes actual installed
locales, native options and operation-specific cancellation. External links use
the OS launcher with the existing bounded public-HTTPS policy, not a guest,
custom scheme or shell command. Their separate Svelte pages bring navigation to
22 unique experiments plus the two guides.

`MainPage` receives the scoped hub and view context and does only view/event glue. `App.CreateWindow`
is the explicit owning composition root: it creates a service scope, resolves the
page there, detaches page events on destruction and disposes that scope. Handler
changes disconnect only the document; frontend reload preserves that scope's
counter, while another native view/window has independent providers and state.
Hub disposal cancels native work, releases the lease timer and clears listeners.
No page factory cycle or startup `BuildServiceProvider` is introduced.
The NavigationPage is constructed before assigning the Window's page, rather
than reparenting an already window-owned ContentPage. Context binding checks the
actual page/window relationship; a detached page cannot silently report a
cancelled dialog that was never displayed.

The sample owns UI, platform heads, Development, workload/launching,
`maui.playground.v1.` preference policy, all `sample.*` commands/demo topics,
counter state and architecture/PID/startup diagnostics. `app.info` returns only
appName/version/build, `device.info` only platform/osVersion/model, and
`sample.diagnostics` only architecture/pid/startupId/startedAt. The app composes
these reads into its unchanged visible identity shape. This is an internal wire
change in the unpublished preview, not a public JavaScript API rename.
Relaunch the native host after updating it; the old live host lacks the new commands.

Assembly-based unit tests reference the managed project; only sample command
source is shared for managed-only reload/cancel tests. `Tests/ExternalProvider` is a
separate plugin assembly with its own generated DTO metadata.
`Tests/PackageConsumer` references the locally packed NuGet, never product source.
See the product README for build/package/standalone version gates and native
AOT/publication limitations.

## Welcome, navigation and native interaction milestone

Welcome is the default landing page; its second page, **Try it hands-on**, uses
the same public SDK modules as the detailed experiments. These two guides are
not additions to the 121-family backlog. Search spans collapsible navigation
groups and temporarily reveals matches without replacing collapse preferences.
Each group is a keyboard-operable heading disclosure with a chevron, without
separate Collapse/Expand button copy. There are 22 unique experiments and two guides.
Alert, confirmation, prompt and action sheet share one sectioned Native dialogs page.
Bridge/development family aliases no longer create duplicate navigation entries;
old fragment bookmarks resolve to the corresponding canonical experiment.
Desktop navigation and session diagnostics can collapse; diagnostics has a
draggable/keyboard-resizable divider. On mobile both panes are modal flyouts,
not miniature permanent strips. Frontend reload is in session diagnostics.
The header/document remain fixed; navigation and content scroll independently.
On mobile the bounded navigation leaves the majority of the viewport to content.
`MainPage.SafeAreaEdges = All` delegates status-bar, home-indicator and keyboard
insets to MAUI, without hardcoded CSS/native offsets.

Routes are `#/welcome`, `#/showcase` or `#/<implemented-feature-id>`.
Unknown/malformed fragments show explicit recovery. Back/forward/hash changes
and frontend reload retain the selected route. A child window receives its validated bundle-relative location at native handshake
bootstrap; only the frontend knows the fragment's route meaning, and reload preserves any newer hash.
These are **internal app routes**, not OS deep links. Custom schemes, universal
links and app actions remain deferred, with no activation manifests added.

| Feature | Implemented scope / capability | Native acceptance |
| --- | --- | --- |
| Window basics | Owning ID/title/bounds; title writes; finite desktop size/position requests | Mac title/size setters and native size events passed; Android capabilities/mobile placement rejection passed. Position/activation/Windows runtime remain unverified. |
| Multiple windows | Trusted bundle-relative URLs; immutable optional developer IDs/private generations; independent scope/hub/counter; sample budget four; caller may list/close its children | Android child-route/counter isolation and OS-close cleanup passed; corrected Mac compiled scene manifest opened two actual attached windows. Caller-driven close and other platforms remain unverified. |
| Native dialogs | Calling-page alert/confirmation/prompt/action sheet with native options; ordinary user cancellation | Android actual alert acknowledgement, confirmation accept/cancel, Unicode prompt/cancel, action-sheet normal/cancel/destructive selection and a visible dialog pending beyond 25 seconds passed. Other platforms remain unverified. |
| File picker | Single/multiple system selection and metadata; separate OpenReadAsync readers with ≤64 KiB binary/incremental UTF-8 chunks, seeking where supported; no arbitrary paths/persistent handles | Android picker cancellation, 54-byte Unicode content stream, 200035-byte selection with explicit 64 KiB preview truncation and binary selection/UTF-8 rejection passed. Multiple selection and native seek remain unverified. |
| Battery / Display | Native read/refresh only, no background streams or keep-screen-awake changes | Actual Android charge/power/energy-saver and 1080×2424/density/orientation/refresh-rate reads passed. Other platforms and change streams are not claimed. |
| Desktop browser guest | Ordinary unbridged WebView anchored to a measured CSS slot in BrowserPanel; real HTML toolbar; viewport/ancestor clipping, resize/DPI mapping, visibility and disposal | Mac actual navigation followed dot.net/maui to the Microsoft MAUI website, reached loaded, accepted focus and closed on feature exit. Visual clipping/DPI and runtime bridge-isolation acceptance remain outstanding. |
| Desktop native menu bar | Real Page.MenuBarItems on a NavigationPage host with hidden navigation bar; scoped File/Navigate/View/Help callbacks and Cmd/Ctrl accelerators | Mac startup installed the TypeScript-defined menu. Actual native clicks/shortcuts/checks/focus are reserved for manual testing. |
| Application theme / window events | App-wide theme/CSS; coalesced native size changes; Windows move events and Windows/Mac centering | Theme/size evidence retained; Mac global-coordinate centering compiled/reviewed, manual/mixed-display checks remain outstanding. |

Desktop bounds are supported on Windows and Mac Catalyst, not mobile. Multiple
windows are supported by MAUI on Windows, Mac Catalyst, Android and iPad; iPhone
returns explicit unsupported. Android uses `LaunchMode.Multiple`: app-owned
windows become separate activities instead of reusing SingleTop. This is not
an OS deep-link implementation. Existing Apple scene delegates are reused with
multiple-scenes enabled. Windows/iPad/iPhone runtime behavior is not certified
by a local Android build. This Mac Catalyst demo deliberately runs without App
Sandbox, matching the Essentials sample; no App Store readiness is claimed.
MAUI's Open-mode picker requests read/write sandbox extensions, so the former
read-only selected-file grant returned null even after selection. Removing that
demo-only sandbox configuration restored actual file selection and preview.
The JavaScript reader API remains read-only. Android adds no broad storage permission. Its battery
provider requires the Essentials `BATTERY_STATS` manifest declaration, not a
runtime permission prompt.

Apple's current SDK omitted the primary Info.plist from its incremental manifest
inputs. A cached compiled bundle could therefore retain single-scene support
after the source enabled multiple windows. `Native/Directory.Build.targets`
tracks that primary file for iOS/Mac Catalyst; a normal incremental rebuild now
updates the signed bundle without requiring a clean. Native Apple window opening
checks actual multiple-scene support before allocating a child, so a disabled
compiled configuration rejects instead of returning a phantom handle.
`Tests/apple-manifest.test.ps1 -DotNetPath PATH-TO-DOTNET` checks the actual SDK
input item lists on both Apple targets.

Debug Vite forwarding is per view. The configured loopback inspector port has
one owning development view, normally the first window; additional windows do
not bind a duplicate socket or acquire cross-window command dispatch. Its existing
probe/selectors continue to describe that owning window. Closing it releases its
listener; a subsequently created development view may own the port. Runtime
commands and child windows never depend on inspection.
The authenticated read-only state endpoint also reports actual application
windows and native handler attachment, independently of the product registry.

Dialogs and file selection use an explicit five-minute interactive budget,
advertised by typed registration metadata at connect. All other commands keep
the 20-second native / 23-second client deadlines and 15-second heartbeat lease.
Only a live interactive operation holds that lease while OS UI pauses heartbeats.
Disconnect/disposal still cancels immediately; stock MAUI cannot forcibly dismiss
its alert/prompt/picker UI, so cancellation does not undo OS effects. The file sample previews the first 64 KiB as text and labels truncation; that is
sample presentation, not a product whole-file limit. Bodies are excluded from the operation log.

The desktop browser is a separate native compositing surface, **not a DOM
WebView element**. Its HTML address/back/forward/reload/focus/close controls live
in `BrowserPanel.svelte`; a real ordinary MAUI WebView overlays only its measured
placeholder. The trusted HybridWebView keeps its full layout: no separate window,
modal, iframe or native split row. CSS geometry is intersected with the viewport
and scrolling ancestors, then scaled to actual native host dimensions rather than
guessing OS offsets/DPI. A clipped native container preserves the full guest size
when partially scrolled. Geometry updates are bounded to 10/s, with a 250 ms
visibility/resize pulse and one in-flight update. Native resize hides stale geometry.

The guest closes on feature teardown, document disconnect and scope destruction.
Native compositing cannot participate in arbitrary HTML z-order: standard DOM
dialogs/popovers are detected, and custom overlays must mark
`data-browser-obscured="true"` and await the public `hide` control before showing.
Mac Catalyst and Windows are the first proof; mobile rejects guest creation, with
no iframe fallback or framing-header bypass. `https://dot.net/maui` is explicitly
HTTPS; the redirected Microsoft page blocks ordinary cross-origin iframes.
Actual Mac navigation/focus/teardown were exercised through the same public SDK
and Svelte controls over normal Vite content. Visual clipping/DPI and the remote
guest's runtime bridge/inspector absence still need acceptance; source inspection
and a successful build are not that proof.

Menus use the same product window actions as JS. Typed current-document
`menus.clicked` notifications have independent UUIDs, registered-topic/subscription
checks and scope disposal. They do not loosen active `operation.progress` checks.
Out-of-band OS interactions hold only that document's lease for up to five minutes;
disconnect/disposal still cancels it. No arbitrary JavaScript is evaluated.
All sample menu definitions/actions are TypeScript in `Web/src/native/menus.ts`.
`Window.menu()/setMenu()` explicitly gets/attaches the resource; item getters and
tiny setters preserve installation/callback ownership. Mac Catalyst renders the
focused owning page in the application menubar, not independent hardware window
menus; native checks work there, while the Windows MAUI check adapter rejects.
The OS-owned application menu is untouched; mobile exposes no fake HTML menubar.

Feature handlers are plain async public SDK snippets. Shared `Action.svelte`
presentation owns busy/availability/error/result/cancel/log isolation rather than
requiring `session.run` wrappers or client injection in each example. SDK contents
are never automatically logged. See the [public export/parity audit](JavaScript/docs/API.md).

### Native acceptance checklist for this milestone

Automated contracts/builds do not certify these OS interactions. On the owning
test app, after explicitly staging/building and relaunching the matching host:

- Open Welcome, the hands-on page and grouped feature pages; test an unknown hash,
  history/reload and Android status/home/keyboard insets without CSS offsets.
- Open a second bundled feature URL with a distinct ID, change its title and
  counter, reload, verify independent state and close from its owner. A duplicate
  ID must reject; reopen after close and verify old resources cannot target it.
- Exercise alert/confirmation/prompt/action-sheet labels and picker cancellation.
  Select Unicode text, a binary file and a file larger than 64 KiB; multiple
  selection and incremental reads must remain genuine resources, with preview
  truncation shown and no private contents/paths in logs.
- Drag/resize the native window manually, not just through setters; verify size
  callbacks and remount/unsubscribe cleanup. Test native moves and resize-then-
  center on Windows/Mac with multiple displays; mobile must report unsupported.
- Set light/dark/system; change the OS theme while following system. The native
  effective event must repaint every connected trusted frontend without listeners
  multiplying after feature remount/reload. The remote site must not be forced themed.
- On desktop, verify real OS menu focus/accelerators/actions, Mac native checks,
  item text/enabled patches without callback replacement and stale-item rejection.
  Windows checks/mobile menus must reject explicitly, not simulate glyphs.
- Scroll/resize/zoom the browser slot and show an obscuring DOM dialog; verify
  native clipping/focus/hiding/teardown and the remote guest's absence of bridge,
  SDK/invoke target/messages/inspector. Mobile guest creation stays unsupported.

Keep partially accepted FEATURES families unchecked. The current automated
checks pass 220 managed tests, 50 SDK tests, 71 Svelte/geometry tests and 11
frontend contracts. The SDK's real 44-file tarball, 17 entrypoints and 314
declaration/TSDoc members passed fresh offline-consumer equality checks.
A refreshed NuGet-only consumer also passed with a fresh package cache and
reflection serialization disabled. Both selected Android/Mac Debug builds passed
with zero warnings/errors. These counts belong to this milestone, not the older
evidence below.
The upstream Essentials suite separately passes 528 tests, including 19 battery
cases. Actual Android acceptance repeated identity/clipboard/preferences/DTOs,
events, progress/cancellation, reload/state retention and stale-session isolation,
then selected a content-backed file through the new nullable picker contract.
Normal picker cancellation passed after waiting for the actual system picker
to be foreground. Mac registered 61 generated actions and exercised real geometry
events, child-window creation/owner close and direct upstream `IBattery` reads.

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
npm product (`@maui/api` with seventeen WIP entrypoints), consumed via `file:../JavaScript`;
it is not compiled as app source. `Managed/` is the standalone WIP C# product;
`Native/` owns the host, sample providers, platform heads/assets and Debug forwarding/inspection; `Tests/`, staging,
launchers, documentation and the [121-family backlog](FEATURES.md) travel with it.
Do not copy generated `node_modules`, `dist`, `artifacts`, `bin`, `obj`, tarballs
or stamps; restore and regenerate them in the destination.

Sample-local `Directory.Build.props/targets` detect this specific MAUI checkout
using its Core project and in-tree props. Only that mode imports MAUI repository
build configuration and uses project references. A copied folder defaults to
packaged `UseMaui=true` with its own outputs and explicit .NET 11 targets.
`-p:HybridWebAppInTree=false` (launcher `-Packaged`) selects that mode explicitly,
even in this checkout. It does not set `HYBRIDWEBAPP_IN_TREE`. The managed library
has its own MSBuild island with explicit MAUI UI dependencies but no app/sample
SDK import. Tests explicitly reference xUnit 2.9.3,
test SDK 18.0.1 and VS runner 2.8.2, the product assembly/project and a separate
provider assembly; only the sample command source is linked.

The copied-folder check also controls SDK and NuGet configuration discovery:
its ignored validation fixture has its own exact SDK selection and a dedicated
NuGet configuration passed to `dotnet restore` with `--configfile`. The managed
standalone tests use locally packed, genuine in-tree Essentials/Graphics dependencies,
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
dotnet restore Tests/Playground.UnitTests.csproj -p:TargetFrameworks=net11.0 \
  -p:TargetFramework=net11.0 -p:MauiVersion=YOUR-MAUI-11-VERSION --configfile NuGet.Config
dotnet test Tests/Playground.UnitTests.csproj -p:TargetFrameworks=net11.0 \
  -p:TargetFramework=net11.0 -p:MauiVersion=YOUR-MAUI-11-VERSION --no-restore --disable-build-servers
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
- `Essentials/src/Battery/Battery.ios.watchos.cs`
- `Essentials/src/Battery/Battery.maccatalyst.cs`
- `Essentials/src/Battery/BatteryCapacity.shared.cs`
- `Essentials/test/UnitTests/Battery_Tests.cs`

These stay outside the bundle: native WebKit stopped-task lifetime,
dispatch/tracking, event-args liveness and public remarks cannot safely be
implemented by copying app code. Xcode deployment floors affect the native
framework itself. The six targeted Core lifetime tests belong with those
framework changes, not the portable host tests.
Likewise, Mac battery readings and notifications are an Essentials correctness
fix. Its capacity/state regressions live in Essentials tests, not copied product
adapters. The separately requested Apple scene adoption in the existing
Essentials sample also remains outside the portable bundle.

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

## Frontend structure

The UI uses **Svelte 5 + TypeScript + Vite**, without SvelteKit, SSR or a server.
`Web/src/main.ts` is startup-only; `shell.ts` mounts `App.svelte` and the native
status component. Small `components/` files own navigation, the experiment frame,
results, operation log and live-edit targets. The typed `registry.ts` maps ten
experiment components to the nineteen implemented catalog families.

Open one flat file in `Web/src/features/` to edit its template, form state and
typed SDK/helper calls: `Identity.svelte`, `Clipboard.svelte`, `Preferences.svelte`,
`Commands.svelte` (DTO echo), `Errors.svelte`, `Policy.svelte`, `Lifetime.svelte`,
`Events.svelte`, `Work.svelte` or `Development.svelte`. Results, errors and logs
are plain-text template bindings, never injected HTML.

`connection.ts` connects once. `session.ts` shares the document's native client,
result/log, busy guard, active work and subscriptions. Navigation unmounts only
the form, not native work or subscriptions; returning to Work can still cancel.
Delayed form reads cannot update replacement forms, and selection versions isolate
results. Pagehide/HMR disposes the document client. App-specific command mapping
remains in `native/sample.ts`: identity composes SDK `app.info` and `device.info`
with app-owned `sample.diagnostics`; the SDK stays framework-independent.

`npm --prefix Web test` runs Node SDK contracts and Vitest/jsdom tests compiled
by the official Svelte Vite plugin, including navigation, delayed responses,
subscriptions and cancellation. `npm --prefix Web run type-check` runs the
existing TypeScript 7 compiler plus `svelte-check`. Its dev-only
`tools/svelte-check` workspace isolates supported TypeScript 6 because the
published checker does not yet support TypeScript 7; app/SDK compiler versions
are unchanged. Svelte is the only added production UI dependency; compiler and
test tools are not part of the independent SDK or packaged native assets.

## Prototype architecture

`Managed/HybridAppTargetObject.cs` is instantiated once per WebView.
`MauiProgram` composes product/sample services through `AddHybridApp` and
`AddPlayground`; one scoped registry is built from explicit generated assembly
registrations and the hub derives readiness names from it. `MainPage`
subscribes to native events and handles only WebView integration.
Plugins declare their event topics and paired lifecycle hooks with attributes. The session/router
does not choose sample providers, hard-code their topics or own MAUI UI objects.
Complete registration before attaching the host; treat the registry as immutable
while sessions are active.

`MainPage` calls the existing .NET 11
`SetInvokeJavaScriptTarget<IHybridAppTarget>(hub, ProtocolJsonContext.Default)`.
The narrow interface exposes only `Task<string> Invoke(string request)`.
The framework source generator intercepts this call; there is no reflection
fallback, assembly scanning or arbitrary-public-member invocation.

Attributed providers generate names, typed handlers and strict automatic JSON
metadata for `CommandRegistry`. Add an application/Toolkit action with
`HybridAction` and one generated assembly-level opt-in, without a handwritten
request DTO or registration function for every scalar method. Subscription
lifecycle is declared with `HybridEvent`; small notification/index JSON codecs
remain where they describe actual stored or published data.
Never add feature/plugin DTOs to the protocol context. No application TypeScript
generator is claimed; TypeScript modules are authored and tested:

```ts
import { getText, setText } from '@maui/api/essentials/clipboard'
import { get, set } from '@maui/api/essentials/preferences'
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
merely to obtain a clipboard/preferences provider. The managed WIP NuGet split
references Essentials only. No plugin discovery framework or placeholder Graphics
API is introduced.

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
bash eng/common/dotnet.sh test src/Controls/samples/Controls.Sample.HybridWebApp/Tests/Playground.UnitTests.csproj \
  --framework net11.0 --disable-build-servers \
  -p:IncludeAndroidTargetFrameworks=false -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacCatalystTargetFrameworks=false -p:IncludeWindowsTargetFrameworks=false \
  -p:IncludeMacOSTargetFrameworks=false
bash eng/common/dotnet.sh build src/Controls/samples/Controls.Sample.HybridWebApp/Native/Maui.Controls.Sample.HybridWebApp.csproj \
  -f net11.0-maccatalyst -c Debug --disable-build-servers -p:UseWorkload=false \
  -p:IncludeAndroidTargetFrameworks=false -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacCatalystTargetFrameworks=true -p:IncludeMacOSTargetFrameworks=false \
  -p:IncludeWindowsTargetFrameworks=false
```

Managed-only tests consume the actual product assembly and fake the existing
individual Essentials interfaces, never a combined platform/service abstraction.
Sample-only source sharing tests counter/reload/work behavior. Node built-in tests exercise the actual
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

### Managed product extraction evidence

The managed extraction and DI refinement preserve the original session bounds (4096 retired
documents, 256 request IDs, 16 active calls, native 20-second deadline/frontend
23-second deadline) and source-generated narrow target. Unit tests now also cover
actual separate-assembly providers, strict per-feature metadata, host preference
prefix/text policy, OS/configuration errors and cancellation before/after native
side effects. Scoped DI tests additionally verify two-view isolation, same-scope
reload counter preservation, external metadata, idempotent product registration,
host overrides and scope-owned cancellation/listener/timer/provider cleanup.
The product package references Essentials and minimal standard DI abstractions;
its genuine transitive dependency is Graphics, not Core/Controls.

An offline fresh consumer with no product source/project uses the local NuGet,
reflection-disabled JSON, plugin-owned DTO/context, Unicode, commands/events,
disabled/unknown commands and duplicate rejections. A copied source bundle tests
and packs against real local Essentials/Graphics NuGets with an explicit SDK pin
and dedicated cleared NuGet sources; its product imports no ancestor MAUI
configuration. These local `11.0.0-dev` dependency artifacts are not evidence of a
public MAUI 11 feed/version. An in-tree Android Debug build succeeds; its emitted
invocation adapter references `Maui.HybridApp.IHybridAppTarget` in the library and
exposes only `Invoke(string)` with required generated string metadata.

After the extraction, parent validation reran 58 managed tests and the real
NuGet-only DI consumer. The consumer verifies separate provider assemblies,
source-generated JSON without reflection, two independent scopes, commands,
events and disposal without product source or sample types.

### Svelte and scoped managed-host acceptance

The frontend now uses actual Svelte 5 components, not HTML strings. The 34
component tests and nine application contract tests passed; TypeScript and the
isolated Svelte checker reported zero errors/warnings. The standalone npm
package's 24 tests, 18-file tarball consumer and exact six-entrypoint API/TSDoc
comparison still passed without including Svelte or sample UI.

Native MSBuild accepted the Svelte source/config/checker freshness mirror and
collected exactly four manifest-listed assets. Those assets matched the Android
APK byte-for-byte. Actual packaged Android UI acceptance then passed the new
`app.info`, `device.info` and sample diagnostics composition, Unicode clipboard,
namespaced preferences, DTO arrays/NUL, errors/recovery, subscriptions/unsubscribe,
progress/cancellation and reload during work. The native PID/startup identity and
the actual numeric service counter survived reload; retired reconnects were
rejected and obsolete progress did not reach the new document. Native touch
gestures scrolled both panes independently while the root/header stayed fixed.
The temporary harness dispatched real input events for Svelte bindings and waited
for rendered navigation; it was not included in the app or package.

Mac Catalyst also built with the current Xcode 27 with zero warnings/errors,
launched the packaged Svelte/product-backed app and remained running. The full
new UI-driven sequence above was performed on Android; it is not a claim of
repeating every provider scenario on Apple, Windows or physical devices.
Independent focused Svelte and managed DI/lifetime reviews found no significant
issues. New code stays inside this sample bundle; the eight framework fixes and
their existing regression tests remain unchanged.

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
