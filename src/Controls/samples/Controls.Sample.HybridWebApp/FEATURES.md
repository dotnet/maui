# MAUI + Tauri playground implementation backlog

**Purpose:** A feature-first playground, not a themed app or a production product. Each experiment gets a name, a short explanation, editable inputs, an observable result and an API/documentation link. This document is the implementation backlog; priorities and completion tracking do not appear in the playground UI.

**Scope:** .NET 11 MAUI Controls, Essentials and tooling; Tauri v2 core APIs and official plugins. Shared capabilities appear once. This is a feature-family inventory, not a list of every property, method or third-party package.

| Group | Meaning | Complete |
|---|---|---|
| **P0 - Critical for v1** | 19 items: make a useful playground that proves the real native bridge. | 17 |
| **P1 - Must have** | 29 items: cover the normal day-to-day native app toolkit. | 4 |
| **P2 - Need to have** | 47 items: add broad UI, device and desktop integration coverage. | 1 |
| **P3 - Good to have** | 26 items: add specialised integrations, extension mechanisms and distribution scenarios. | 1 |

**Completion rule:** A checked item means the complete experiment described in that row is implemented end-to-end and verified within the documented scope below. It does not mean every member of the corresponding .NET API is wrapped, or that every OS/release configuration is certified. Partially implemented acceptance targets stay unchecked. For example, historical clipboard/preferences acceptance covered text read/write and bounded text settings. New typed preference and clipboard-event contracts have automated coverage but still require native acceptance.

**API notes:** A MAUI documentation link does not imply a built-in JavaScript wrapper. The standalone experimental `@maui/api` package currently exposes a deliberately small Essentials and MAUI UI subset through separate modules. Tauri plugins also require native registration and permissions. Entries marked **adapter/package** need additional .NET or platform code; they are not first-party MAUI API equivalents.

**Implementation home:** [This sample][sample-readme] hosts the native app and frontend. The [standalone JavaScript SDK][sdk-readme] is one npm package with isolated infrastructure/feature modules. The single WIP managed product now includes reusable MAUI UI providers and depends on Controls.Core/Core alongside Essentials, as explicitly approved. It never depends on sample counters, diagnostics or catalog. The app binds an owning page/window and trusted factory; alternative native backends, a future Essentials-only split, Labs migration and DevFlow integration remain deferred.

## Completion evidence and remaining acceptance

The initial native milestone verified app identity, text clipboard/preferences, asynchronous DTOs, errors, events, cancellation and frontend reload state on Mac Catalyst, iOS 27 simulator and Android emulator. Android also verified packaged execution and preferences across native restart. After extraction, Android repeated native acceptance using the SDK dependency, tested lazy feature helpers and reconnect, and repeated acceptance offline. Its four APK frontend assets matched staging byte-for-byte. The sample README records exact commands, scopes and historical versus current evidence.

The reusable SDK ships built JavaScript and declarations, not app-local TypeScript. Its packed-consumer checks use a real tarball without the source tree. Source comments document public APIs. These are local preview artifacts, not evidence of npm publication or registry-scope ownership.

Page dialogs now use all four directly annotated native methods, with no prompt
action/result wrapper. Prompt returns native text or null. A declared strict
Keyboard converter preserves singleton identities (`{ name: 'text' }`) and
`Keyboard.Create` flags (`{ flags: ['spellcheck'] }`); keyboard names are not a
native enum. Label/length/button-count semantics belong to Page. This change
does not add native acceptance; the dialog runtime evidence below is historical.

**Platform/release gaps:** Windows runtime, physical devices, installed Release execution, NativeAOT and signed installation are unverified. The updated iOS app connected, but the new simulator touch/bounce/keyboard checks were not completed. Local success does not establish CI or merge readiness.

The expanded milestone additionally exercised real Android battery/display and
light/dark/system CSS, confirmation accept/cancel beyond the ordinary timeout,
Unicode prompt acceptance/cancellation, picker cancellation/Unicode content
streams/large preview truncation/binary rejection, and a selected-route child
window with an independent counter. OS Back released that child's scope; the
parent retained its own counter after reconnecting its suspended document.
Mac exercised native title/size and actual size events, shared theme subscribers,
and the BrowserPanel's real HTTPS redirect/load/focus/feature teardown. Native menu
click/shortcut/check/focus acceptance is reserved for manual testing. Visual guest
clipping/DPI and runtime bridge-isolation checks remain outstanding.
Most of this remains partial family acceptance. Native dialogs now have complete
Android experiment evidence: actual alert acknowledgement, confirmation accept/
cancel, prompt text/cancel, and action-sheet normal/cancel/destructive selection.
Action sheets now share the sectioned Native dialogs page with alerts,
confirmations and prompts. That cleanup produced 19 unique experiments and two guides:
duplicate Bridge/Development family pages were consolidated, while this
121-family inventory and aliases for old bookmarks remain.
Navigation and session diagnostics collapse on desktop; diagnostics resizes
with a pointer or keyboard. Mobile uses flyouts, not permanent miniature panes.
Refresh is in session diagnostics. Window basics shows live native geometry;
Multiple windows is a separate page.
The Mac picker returned null because its Open-mode native picker requested a
read/write sandbox extension that the demo's read-only grant rejected. Logs
confirmed our cancellation token stayed false. The user requested the same
unsandboxed configuration as Essentials and confirmed selection/preview works.
No store deployment work or App Store readiness is intended.

### Implemented subsets versus family completion

The inventory below remains a family-level checklist. This finer-grained table
makes completed implementations visible without declaring an unfinished family
complete or requiring every platform to be certified.

| Capability | Implemented and verified | Remaining |
|---|---|---|
| Native dialogs | Alert, confirmation, prompt and action sheet; native Android acceptance, editable labels and normal cancellation | Additional native platform acceptance |
| Multiple windows | Native Mac attached child windows and generated owner-close; Android selected route, independent counters/documents, parent-state preservation and OS-close cleanup | Windows/iPad runtime |
| Window basics | Native title and size setters, live size events, separate current-window page; tested Mac display-origin centering math and compiled adapter | Manual centering/multiple displays, activation/close and Windows move events |
| File picker | Actual Android content-backed stream, binary chunks/EOF, seek/read and normal cancellation; user-confirmed Mac preview and bounded preview truncation | Multiple selection UI/native acceptance and additional platform seek acceptance |
| Theme | Native Android/Mac system/light/dark setting and effective CSS subscriptions | OS-originated theme changes and multi-window propagation acceptance |
| Native menus | TypeScript-authored definitions/callbacks, item getters/setters, owning-window snapshot rebuild regressions | User confirmation of visible rename/check updates; toolbar/context menus |
| Explorer shell | Unique feature pages; collapsible desktop panes, draggable/keyboard diagnostics divider, mobile flyouts and refresh in diagnostics | Broader native/mobile accessibility acceptance |
| Battery | Product uses IBattery everywhere. Catalyst capacity/state and power-source notifications are fixed in Essentials; all 528 Essentials tests pass, including 19 battery cases. Plain Essentials sample read 100%/Full/AC matching pmset and repeated native subscription/removal passed | Live power-change delivery and JS battery-change experiment remain unverified |
| Connectivity | Native Mac Internet/WiFi read, shared listener setup/unsubscribe; automated actual-provider event/lifetime tests | Live OS network-change delivery and other platforms unverified; no network settings changed during acceptance |
| Text-to-speech | Native Mac reported 180 installed locales and completed SpeakAsync; typed options and operation cancellation tested | Physical audio listening/cancellation and other platform runtime acceptance |
| Open external links | Real Mac OS accepted public HTTPS launch; URL validation and launch failures tested | Other platform runtime; acceptance does not prove website loading |

Connectivity/events, text-to-speech and external URL launching are now distinct
implemented experiments, bringing navigation to 22 unique experiments plus two
guides without changing the 121-family inventory. Native Mac acceptance uses
the real shared SDK connection; the network was not toggled or mocked.
Attributed actions and automatic binding are implemented, not only proposed:
all product, sample and core actions now use annotations. One generated method
registers each entire assembly, with no command-provider interface, per-type
registration or compatibility layer. Window actions/events and menu
actions/rendering are consolidated into one class per feature; sample work and
counter actions share one sample plugin. Simple actions use typed multiple
parameters instead of per-operation request records.
The WIP JS contracts were updated with native code: obsolete preference commands
and the single-picker result envelope were removed, not aliased. Resource
lifetimes and window/document isolation remain. The packed analyzer executes in a fresh
NuGet-only host and separate plugin with reflection serialization disabled.
Independent review found nullable-enum defaults and normalized constructor
nullability reader bugs; isolated compilation/runtime regressions cover both.
The complete assembly migration passes 220 managed tests, 50 SDK tests and
71 Svelte/geometry tests plus 11 contracts. A fresh NuGet-only host/external
assembly consumes the packed analyzer with reflection JSON disabled. Native Mac
registered 61 actions and exercised named/positional calls, geometry events and
child open/close; Android repeated the original native bridge/event/cancellation/
reload invariants and the new selected-file/cancel contract. Both native builds
passed without warnings. The independent assembly/lifecycle review found no
significant issues.

The enum-contract migration additionally passes 277 managed tests, 55 SDK tests,
72 Svelte/geometry tests and 11 contracts; TypeScript/Svelte checking is clean.
The packed analyzer also runs in a fresh-cache NuGet-only host with reflection
serialization disabled. Matching Mac Catalyst and Android Debug builds have
zero warnings/errors. Native enums use strict camelCase names across actions
and callbacks, with explicit flags arrays; speech uses actual MAUI
SpeechOptions/Locale contracts. These contract improvements did not complete
additional feature families. At that milestone, lower-layer SDK annotations,
contextual page injection, geolocation and structural Share dispatch were
unimplemented; the later annotation prototype below supersedes that status.
Fresh native Mac acceptance confirms named battery/network/display reads,
light/dark theme callbacks, numeric/unknown enum and invalid flags rejection
before UI, native speech locale/options binding, menu kind/modifier binding and
rename, and separate browser status/error/navigation-result callbacks. Native
menu check readback still needs the UIKit menu snapshot to be built; visible
check/click acceptance remains manual and is not claimed by this milestone.

The three-path binary implementation adds whole-buffer invocation, ordered
action-owned channels and raw binary selected-file reads. The existing Arguments
page demonstrates a 256 KiB Blob roundtrip, a native-produced PDF/browser Blob
and async-consumed raw chunks with typed progress, without adding a duplicate
experiment or checking another feature family. At that milestone native OS sharing and writeable
file handles were not implemented; the later Share prototype below adds sharing,
not writeable handles. Whole transfers are bounded to 4 MiB;
Android uploads use bounded encoded fragments while responses are binary.
Channels await explicit consumption acknowledgements and retire with the action.
Portable validation passes 308 managed tests, 67 SDK tests and 74 UI tests plus
11 contracts; the actual 44-file tarball preserves 17 entrypoints and 358
documented public declarations/members. Core's targeted binary/lifetime suite
passes 37 tests plus 16 framework-JavaScript tests. Native acceptance is recorded
separately; source-level coverage is not a platform runtime or NativeAOT claim.
Actual Mac/Android hosts register 67 actions and pass exact empty/all-byte-value,
256 KiB/4 MiB Blob roundtrips, sliced-view offsets, a native PDF, ordered raw
chunks with awaited JS consumption, typed progress, consumer-error recovery
and full repetition after document reload. Android also verifies a real
200035-byte selection, four binary reads/EOF, seek/read, close/release and picker
cancellation. Independent reviews reproduced and corrected completion-token
cancellation, cancelled Windows-navigation retirement, consumer-rejection
channel reuse and control-plane starvation; regressions cover the affected
state machines. Native Mac exposed empty NSData.AsStream failing before
dispatch, now corrected. Windows execution remains unverified; the general
Controls unit project is blocked by pre-existing RS0030 calls outside this work.

### Native annotation and hybrid-local service prototype

Metadata-only attributes now live in `Microsoft.Maui.Interop` in the existing
Essentials assembly, with no new metadata package or bridge runtime dependency.
Curated assembly roots import annotated native interfaces, getters and supported
events. Generated factories construct local targets through an owning provider
overlay; normal/keyed business services and explicitly shared targets retain host
ownership. Contextual Page/WebView/Window/dispatcher lookups follow the live owner.
This replaces the earlier proposal-only status without exposing every loaded SDK.

Native clipboard and selected page APIs are projected directly. Geolocation uses
real request/result types and named native enum values. Typed converters and
explicit strict STJ contexts support extensible DevicePlatform and actual .NET 11
structural Share unions; unknown, duplicate, ambiguous and unauthorized file
shapes reject. PDF/text bytes stage into bounded app-owned files with opaque
document authority and finite post-share retention. At that milestone prompt,
preferences and complex native resources remained adapters; the direct-native
correction below removes the prompt action adapter.

The new Geolocation and Share pages bring navigation to 24 experiments plus two
guides. The Lifetime page demonstrates local versus explicitly shared targets.
At that annotation milestone, portable validation passed 353 managed tests,
71 SDK tests and 77 UI tests plus 12 contracts; the packed SDK had
48 files/19 entrypoints/398 documented members.
Fresh-cache reflection-disabled NuGet-only generation and neutral SDK API
validation pass. Independent review regressions cover custom-converter nullability,
inherited member shadows, geolocation timeout policy, concurrent disposal and
factory reentrant retirement. These are implementation evidence, not complete
platform or release certification.

The speech/preferences/dialog correction removes routine speech and prompt wrappers:
`ITextToSpeech` itself carries MainThread locales and MainThread/Interactive
speech annotations. The curated root generates normal async calls with native
`SpeechOptions`/`Locale`, an enumerable JSON array and void/null completion.
Only JSON/schema/finite representation and transport checks remain; speech text,
range and locale semantics belong to Essentials, not the SDK.
All four Page dialogs, including `DisplayPromptAsync`, now use direct generated
calls and native scalar/null results. A declared Keyboard JSON converter maps
actual singleton names or `Keyboard.Create` flags; it is not a prompt action
wrapper. Button labels, action counts, maxLength and initialValue are passed to
Page without duplicate semantic validation.
Preferences keep the generic-scalar adapter but no prefix, container rewriting,
arbitrary key/string quota or ownership index. Native-existing settings are
intentionally shared, and Clear calls the actual container-wide native Clear.
The page explicitly warns about that destructive behavior and requires confirmation.
Clipboard retains its unrelated text budget. These changed semantics have portable
fake-destination and SDK/UI coverage; earlier native acceptance does not certify
this correction's native execution or packed artifacts.
Current portable validation passes 396 managed tests, 77 SDK tests, 80 UI/geometry
tests and 12 frontend contracts. SDK reference generation validates 19 entrypoints
and 393 documented declarations/members; frontend type-check/build/staging passes
with zero TypeScript/Svelte errors or warnings. Fresh-cache reflection-disabled
NuGet-only consumption also verifies native async locales, direct speech options,
host interface override, cancellation and void completion. Final selected Mac
Catalyst/Android Debug builds pass with API validation and zero warnings/errors.
Actual native execution of this correction remains a separate acceptance gap.

Actual Mac Catalyst and Android acceptance verifies the imported clipboard
methods/getter/change event/unsubscribe and contextual window getter. Mac queried
child windows reach ready, have different local target IDs and the same explicit
shared target ID, and retain exact 256 KiB binary transfers. The Core request
guard now accepts a parsed same-origin document Referer with a path/query while
preserving token, exact origin, scheme/host/port and credential checks; 49 targeted
Core header/binary/lifetime tests and 16 framework JavaScript tests pass.
Android's real two-file share chooser grants recipient URIs whose bytes match
the staged lengths and SHA256 hashes. The temporary proof receiver was removed
and the clean APK reinstalled. Native PDF recipient consumption is not claimed.
The actual Android permission dialog was granted and the generated geolocation
action returned the controlled emulator OS fix (51.5, -0.12), including the named
`geoid` altitude reference. This is emulator location injection, not physical GPS
or proof of permission denial/settings-disabled behavior. The native Location's
`isFromMockProvider` field is reported unchanged, not inferred from the fixture.
Location updates and wider platform/physical-device acceptance remain deferred.

The new-window follow-up reproduced a Mac build-cache failure: the compiled
Info.plist still disabled multiple scenes after the source enabled them. The
installed Apple SDK tracked the manifest path string but omitted the primary
file from its incremental inputs. The sample now tracks the actual primary
manifest for iOS/Mac Catalyst. A rebuild without cleaning changed the signed
bundle's flag to true and opened two actual attached Mac windows instead of
returning a registry-only handle. Native Apple preflight rejects disabled
multiple-scene configuration explicitly. Windows and wider lifecycle acceptance
are still not certified.

| Unchecked item with partial implementation | What exists | What remains |
|---|---|---|
| Explicit exposure and permissions | Unknown/disabled command rejection, bounded inputs, distinct native error codes | An actual OS permission check/request/denial experiment |
| Platform-aware results | Unsupported/permission/configuration error classification | Observable native scenarios for each classification, not just policy failures/test doubles |
| Plugin/provider extension | DI registration, scoped targets and a separate provider assembly verified through a real NuGet-only consumer | A complete optional provider playground experiment with its JS integration |
| Installable artifacts; signing and installed execution | Debug native deployment and Mac Catalyst Release build/asset checks | Install and exercise the intended signed Release artifacts |
| Diagnostics and performance | Startup identity, request metrics and bounded operation log | Reproducible command-latency, startup and memory scenarios |
| Browser mocks and native automation | Committed contract tests and actual native acceptance evidence | A committed, repeatable native automation entrypoint |
| Window basics | Live current-window page; Mac size events and compiled global-display centering adapter | Manual/mixed-display Mac centering, activation/close; Windows runtime |
| File picker | Android stream/large preview/binary/seek/cancel acceptance; user-confirmed Mac selected-file preview after removing demo sandbox | Multiple selection and additional platform seek acceptance; 64 KiB is only per-message budget |
| Theme and appearance | Native Android/Mac system/light/dark preference and effective Svelte CSS accepted | Actual OS theme change while following system and cross-window event acceptance; remote sites are not forced themed |
| Battery / Display and DPI | Android readouts plus corrected Catalyst IOKit capacity/state matching actual Mac power output | Native change subscriptions deferred; broader platform runtime unverified |
| Remote WebView controls | Measured BrowserPanel native guest; actual Mac HTTPS redirect/load/focus/feature teardown accepted | Visual clipping/DPI and runtime bridge-isolation evidence; mobile guests unsupported |
| Native menus, toolbars and context menus | JS-defined native menus, stable installation/item handles, tiny property patches, JS callbacks and Cmd/Ctrl accelerators; Mac actual checks, Windows checks explicitly unsupported | Actual OS-menu/focus/shortcut/callback acceptance; toolbar/context-menu experiments remain unimplemented |

Welcome and Try it hands-on are distinct guide pages outside this 121-family
inventory. Grouped navigation and internal fragment routes do not implement OS
deep links, custom schemes or app-action activation. The new native families
remain unchecked while their described acceptance scope is partial, even when
the implemented subset has native evidence and unit tests/builds pass.

## P0 - Critical for v1

### Playground and native bridge

- [x] **Feature browser** - Search implemented experiments, navigate categories and open API docs. [MAUI controls][m-controls] / [Tauri example][t-example].
- [x] **Bridge bootstrap and readiness** - Load the bridge and clearly show native-host versus browser-only mode. [HybridWebView][m-hybrid] / [Tauri core][t-core].
- [x] **One native target hub** - Route feature modules and application commands through one explicit, instance-backed entry point. [MAUI invocation][m-invoke] / [Tauri commands][t-commands].
- [x] **Custom command registration** - Annotate static/instance methods with HybridAction, explicitly opt in action classes, and generate strict JSON/DI/keyed-service/cancellation binding without runtime reflection. Separate NuGet-only host/plugin runs with reflection serialization disabled; actual Mac named/positional calls accepted. Native AOT publication is not certified. [MAUI generated invocation][m-net11] / [Tauri registration][t-commands].
- [x] **Arguments and return values** - Round-trip primitives, nulls, arrays, DTOs and asynchronous results. [MAUI invocation][m-invoke] / [Tauri commands][t-commands].
- [x] **Typed JavaScript modules** - Import named helpers with parameter/result types rather than hand-writing wire messages. [MAUI foundation][m-net11] / [Tauri API][t-api]. Generated app-specific TypeScript is additional tooling, not claimed by this checkbox.
- [x] **Errors and operation log** - Display rejected calls, native errors and transport failures with useful details. [MAUI invocation][m-invoke] / [Tauri logging][t-log].
- [ ] **Explicit exposure and permissions** - Reject unknown/disabled commands and distinguish app policy from OS permissions using an actual permission scenario. [MAUI permissions][m-permissions] / [Tauri capabilities][t-capabilities].
- [x] **Threading and service lifetime** - Run UI APIs on the UI thread and preserve the registered service instance across frontend reloads. [MAUI MainThread][m-mainthread] / [Tauri state][t-state].
- [x] **Reload and disconnect behaviour** - Reconnect listeners without duplicates; terminate obsolete calls and release resources. [MAUI bridge][m-hybrid] / [Tauri events][t-events].
- [x] **Events and subscriptions** - Receive native changes and explicitly unsubscribe. [MAUI messaging][m-hybrid] / [Tauri events][t-events].
- [x] **Progress and cancellation** - Show a long-running native operation, stream progress and cancel it. [MAUI messaging foundation][m-hybrid] / [Tauri channels][t-channels]. The experiment uses the explicit command protocol; it does not claim a shipped general-purpose MAUI channel API.

### A runnable first version

- [x] **App and device identity** - Show app version, platform, OS, architecture and native startup identity. [AppInfo][m-appinfo], [DeviceInfo][m-deviceinfo] / [Tauri app][t-app], [OS][t-os]. Process/startup diagnostics are app-owned, not Essentials APIs.
- [x] **Text clipboard** - Copy and explicitly paste sample text through the native clipboard. [MAUI][m-clipboard] / [Tauri][t-clipboard].
- [x] **Preferences** - Save, read/remove actual native settings; native restart persistence was accepted at the earlier text-setting milestone. Direct keys/shared containers and destructive native-wide Clear now have portable coverage; their revised native acceptance remains unverified. [MAUI][m-preferences] / [Tauri store][t-store].
- [x] **Vite live development** - Edit HTML/CSS/JS inside the running app on selected desktop/mobile targets. [Current sample][sample-readme] / [Tauri development][t-develop].
- [x] **Packaged offline content** - Run the same feature UI and bridge without a development server. [MAUI assets][m-assets] / [Tauri frontend configuration][t-config].
- [x] **Native inspection and repeatable checks** - Inspect actual in-app DOM/results; support frontend mocks without confusing them with native verification. [MAUI developer tools][m-devtools] / [Tauri mocks][t-mocks].
- [ ] **Platform-aware results** - Show actual unsupported, permission-denied and configuration-required scenarios rather than fake success. [MAUI platform features][m-platform] / [Tauri plugin support][t-plugins].

## P1 - Must have

### Files, storage and networking

- [ ] **File picker** - Select one or several files using the OS picker. [MAUI][m-filepicker] / [Tauri dialogs][t-dialog].
- [ ] **File access** - Read/write/delete sample files in app-owned storage with explicit size and scope limits. [MAUI file helpers][m-files] / [Tauri filesystem][t-fs].
- [ ] **Save/export destination** - Export a result to a selected destination. **MAUI adapter/package** beyond its read-oriented FilePicker. [MAUI sharing][m-share] / [Tauri save dialog][t-dialog].
- [ ] **Paths and packaged resources** - Resolve app data/cache/resource locations without assuming mobile files have desktop paths. [MAUI][m-files] / [Tauri paths][t-path].
- [ ] **Structured local store** - Persist a small JSON document, distinct from primitive preferences. [.NET JSON][net-json] / [Tauri store][t-store].
- [ ] **Secure key/value storage** - Store and remove a non-sensitive test value; never use real secrets in the experiment. [MAUI][m-secure].
- [ ] **HTTP requests** - Send a native request and show status, headers, body and duration. [.NET HttpClient][net-http] / [Tauri HTTP][t-http].
- [ ] **Uploads and downloads** - Transfer a bounded sample file with progress and cancellation. [.NET HttpClient][net-http] / [Tauri upload][t-upload].
- [ ] **WebSocket** - Connect, send/receive messages and disconnect cleanly. [.NET ClientWebSocket][net-websocket] / [Tauri WebSocket][t-websocket].
- [x] **Connectivity** - Report network access and subscribe to connectivity changes. Native Mac read/listener setup plus automated event delivery/cleanup verified; live OS changes and other platforms remain unverified. [MAUI][m-connectivity]. Do not infer internet reachability from connection type alone.

### OS interactions

- [x] **Native dialogs** - Display alerts, confirmations, prompts and action sheets. Verified Android calling-page acknowledgement, text/selection and normal cancellation; other platforms remain unverified. [MAUI][m-dialogs] / [Tauri][t-dialog].
- [x] **Open external links** - Open an allowed URL in the system browser or another app. Actual Mac native HTTPS launch accepted; this experiment intentionally excludes arbitrary/custom schemes. [MAUI Launcher][m-launcher] / [Tauri opener][t-opener].
- [ ] **In-app browser** - Open documentation in the platform browser presentation, separate from the embedded app WebView. [MAUI Browser][m-browser].
- [ ] **Share text and files** - Invoke the native share experience. [MAUI][m-share]. Tauri may need a mobile/platform adapter.
- [ ] **Window basics** - Read/set title and supported size/position; activate or close the owning window. [MAUI][m-window] / [Tauri][t-window].
- [x] **Multiple windows** - Create a second experiment window and verify state and message isolation. Verified Android independent document/hub/counter and parent-state preservation; Mac creates actual attached child windows. Windows/iPad runtime and caller-driven close remain unverified. [MAUI][m-window] / [Tauri][t-window].
- [ ] **Application lifecycle** - Observe creation, activation, deactivation, backgrounding and destruction. [MAUI][m-lifecycle] / [Tauri][t-app].

### Native and web UI together

- [ ] **Mixed native/web surface** - Open a real MAUI panel next to the web playground and exchange state. [MAUI embedding][m-embedding], [HybridWebView][m-hybrid]. Tauri's web UI is not a native-controls equivalent.
- [ ] **Navigation** - Exercise native and web navigation, back behaviour and modal presentation. [MAUI Shell][m-shell], [pages][m-pages] / [Tauri webviews][t-webview].
- [ ] **Text and actions** - Showcase Label, Entry, Editor, Button and ImageButton with native focus/input. [MAUI controls][m-controls].
- [ ] **Choices and values** - Showcase Picker, CheckBox, RadioButton, Switch, Slider and Stepper. [MAUI controls][m-controls].
- [ ] **Date/time and busy states** - Showcase DatePicker, TimePicker, ActivityIndicator and ProgressBar. [MAUI controls][m-controls].
- [ ] **Lists and search** - Showcase CollectionView selection/templates and SearchBar. [MAUI CollectionView][m-collection] / browser-side lists in Tauri.
- [ ] **Responsive layouts** - Exercise Grid, AbsoluteLayout, horizontal/vertical stacks and FlexLayout, safe areas and mobile/desktop arrangements. [MAUI layouts][m-layouts], [safe areas][m-safe].
- [ ] **Theme and appearance** - Follow system light/dark mode and allow an explicit override. [MAUI][m-theme] / [Tauri window theme][t-window].
- [ ] **Accessibility** - Verify names, roles, focus, keyboard navigation, screen-reader announcements and large text. [MAUI semantics and SemanticScreenReader][m-accessibility] / native WebView plus accessible HTML.
- [ ] **Native menus, toolbars and context menus** - Add actions and keyboard accelerators, then verify callbacks. [MAUI menu bar][m-menu], [context menus][m-contextmenu], [keyboard accelerators][m-keyboard] / [Tauri menus][t-menu].
- [ ] **Images, fonts, icons and splash** - Load packaged/dynamic images and verify the app's shared asset pipeline. [MAUI images][m-images], [fonts][m-fonts], [icon][m-icon], [splash][m-splash] / [Tauri images][t-image].
- [ ] **Resources, styles and bindings** - Demonstrate resource dictionaries, XAML/CSS styles, compiled bindings, templates and visual states. [MAUI resources][m-resources], [styles][m-styles], [CSS][m-css], [bindings][m-bindings].

## P2 - Need to have

### Device and media APIs

- [ ] **Battery** - Read charge/power information and observe changes. [MAUI][m-battery].
- [ ] **Display and DPI** - Read size/density/orientation and handle display changes. [MAUI DeviceDisplay][m-display] / [Tauri DPI][t-dpi], [monitors][t-window].
- [ ] **Keep screen awake** - Enable and restore keep-screen-on behaviour. [MAUI DeviceDisplay][m-display].
- [ ] **Location** - Request current location and observe permitted updates. [MAUI][m-location] / [Tauri][t-geolocation].
- [ ] **Geocoding** - Convert a sample place to coordinates and back. [MAUI][m-geocoding].
- [ ] **Launch system maps** - Open a location in the installed maps application. [MAUI Map][m-maplaunch].
- [ ] **Embedded native map** - Show pins and regions; keep provider/key requirements explicit. [MAUI Maps][m-maps].
- [ ] **Photo/video picker and capture** - Select or capture media through native APIs. [MAUI][m-media].
- [ ] **Screenshot** - Capture the app and export the result. [MAUI][m-screenshot].
- [x] **Text-to-speech** - Speak sample text and select a supported voice. Native Mac locale enumeration and speech completion accepted; options/cancellation have automated coverage. Other platforms and listening acceptance remain unverified. [MAUI][m-speech].
- [ ] **Haptic feedback** - Trigger supported native interaction feedback. [MAUI][m-haptics] / [Tauri][t-haptics].
- [ ] **Vibration** - Start and stop a bounded vibration. [MAUI][m-vibration].
- [ ] **Flashlight** - Enable/disable the camera light where available. [MAUI][m-flashlight].
- [ ] **Accelerometer and shake** - Stream acceleration readings and detect a shake. [MAUI sensors][m-sensors].
- [ ] **Gyroscope** - Stream rotation-rate readings. [MAUI sensors][m-sensors].
- [ ] **Compass** - Stream heading readings. [MAUI sensors][m-sensors].
- [ ] **Magnetometer** - Stream magnetic-field readings. [MAUI sensors][m-sensors].
- [ ] **Barometer** - Stream pressure readings. [MAUI sensors][m-sensors].
- [ ] **Orientation sensor** - Stream orientation readings and clean up listeners. [MAUI sensors][m-sensors].
- [ ] **Conversions** - Demonstrate unit and colour conversion helpers with deterministic results. [MAUI unit conversions][m-units], [colour helpers][m-colors].

### Desktop and application integration

- [ ] **Drag and drop** - Drop text/files and drag an app-owned result out where supported. [MAUI][m-drag] / [Tauri window events][t-window].
- [ ] **Tray/status icon** - Add a desktop tray icon, menu and click callback. **MAUI platform adapter.** [Official MAUI sample][m-weather] / [Tauri tray][t-tray].
- [ ] **Global shortcuts** - Register/release a desktop-wide keyboard shortcut. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri][t-shortcut].
- [ ] **Deep links and file associations** - Route an incoming URI or associated file to the correct experiment. [MAUI Apple links][m-applinks], [Android links][m-androidapplinks] / [Tauri][t-deeplink].
- [ ] **Single instance** - Hand a second launch's arguments to the existing desktop app. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri][t-single].
- [ ] **Notifications** - Request permission, show a local notification and handle activation. **Scheduling needs a MAUI adapter/package.** [MAUI guidance][m-notifications] / [Tauri][t-notification].
- [ ] **App actions** - Launch an experiment from an app-icon shortcut. [MAUI][m-actions].
- [ ] **Version tracking** - Report first launch and version/build changes. [MAUI][m-version].
- [ ] **Persisted window state** - Restore a valid desktop size/position. **MAUI composition over Window + storage.** [MAUI Window][m-window] / [Tauri][t-windowstate].
- [ ] **Advanced window behaviour** - Explore fullscreen, decorations/title bar, resizing, cursor, attention and supported window effects. [MAUI TitleBar][m-titlebar], [platform APIs][m-native] / [Tauri][t-window].
- [ ] **Window positioning** - Place a supported desktop window relative to a monitor or tray icon. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri positioner][t-positioner].
- [ ] **WebView controls** - Exercise initialization settings, navigation, multiple webviews and resource interception. [MAUI WebView][m-webview], [HybridWebView][m-hybrid] / [Tauri webview][t-webview], [WebviewWindow][t-webviewwindow].
- [ ] **Local SQL** - Query a tiny sample database and reset it. **Additional .NET SQLite package.** [MAUI tutorial][m-sql] / [Tauri SQL][t-sql].
- [ ] **Persisted file access scopes** - Retain explicitly granted file access across launches where supported. **Platform adapter; not ordinary Preferences.** [MAUI platform APIs][m-native] / [Tauri persisted scope][t-persistedscope].

### More native UI and communication

- [ ] **Graphics and drawing** - Draw an interactive native canvas with GraphicsView and compare web rendering. [MAUI][m-graphics].
- [ ] **Shapes, brushes and decoration** - Demonstrate Border, BoxView, shapes, gradients and shadows. [MAUI controls][m-controls], [shapes][m-shapes].
- [ ] **Gestures and animation** - Exercise tap, long press, pointer, pan, pinch, swipe and bounded animations. [MAUI gestures][m-gestures], [animations][m-animations].
- [ ] **Scrolling, refresh and swiping** - Showcase ScrollView, RefreshView and SwipeView. [MAUI controls][m-controls]. Independent HTML pane scrolling is not proof of these native controls.
- [ ] **Carousel and indicators** - Showcase CarouselView and IndicatorView. [MAUI controls][m-controls].
- [ ] **Page/layout variants** - Showcase ContentPage/ContentView, NavigationPage, TabbedPage, FlyoutPage and BindableLayout. [MAUI pages][m-pages], [layouts][m-layouts].
- [ ] **UI behaviours and triggers** - Attach reusable behaviour and react to native control/state changes. [MAUI behaviours][m-behaviors], [triggers][m-triggers].
- [ ] **Localization and RTL** - Switch language/direction and verify native/web text and layout. [MAUI][m-localization].
- [ ] **Contacts** - Pick a contact with explicit consent; do not collect an address book by default. [MAUI][m-contacts].
- [ ] **Email** - Launch a prefilled email composer without sending automatically. [MAUI][m-email].
- [ ] **SMS** - Launch a prefilled message composer. [MAUI][m-sms].
- [ ] **Phone dialer** - Open the dialer without placing a call automatically. [MAUI][m-phone].
- [ ] **Web camera/microphone** - Compare permitted WebView browser media access with native media APIs. [Tauri example][t-example] / [browser getUserMedia][web-media].

## P3 - Good to have

### Specialised native integrations

- [ ] **Browser authentication** - Exercise browser launch, callback and explicit cancellation. [MAUI WebAuthenticator][m-auth].
- [ ] **Passkeys** - Register/assert a sample credential against a configured relying party. [MAUI .NET 11][m-passkeys]. This is not the same operation as a generic biometric prompt.
- [ ] **Local biometrics** - Request a local Face ID/Touch ID/biometric confirmation. **MAUI platform adapter/package.** [Platform APIs][m-native] / [Tauri][t-biometric].
- [ ] **Barcode/QR scanning** - Scan a sample code and return its value. **MAUI scanner package/adapter.** [Platform APIs][m-native] / [Tauri][t-barcode].
- [ ] **NFC** - Read a supported sample tag with explicit permission. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri][t-nfc].
- [ ] **Encrypted vault** - Exercise a disposable vault and key lifecycle. **Not equivalent to SecureStorage; extra implementation/package.** [Tauri Stronghold][t-stronghold].
- [ ] **Autostart** - Opt into and undo desktop launch-at-login. **MAUI platform adapter.** [Platform APIs][m-native] / [Tauri][t-autostart].
- [ ] **Process lifecycle** - Demonstrate explicit app restart/exit on supported desktop targets. [.NET/platform adapter][net-process] / [Tauri process][t-process].
- [ ] **Shell and sidecar process** - Run one allowlisted desktop helper and stream its output; never accept arbitrary shell commands. [.NET Process][net-process] / [Tauri shell][t-shell].
- [ ] **Command-line arguments** - Parse a sample launch option and route it into the UI. [.NET command-line overview][net-cli] / [Tauri CLI plugin][t-cli].
- [ ] **Localhost asset server** - Compare an opt-in local HTTP origin with the virtual app origin; not required for Vite forwarding. **Additional server/adapter.** [Tauri localhost][t-localhost].
- [ ] **Native resource handles** - Create/use/dispose a bounded native resource and reject stale handles. [MAUI platform APIs][m-native] / [Tauri core Resource][t-core], [images][t-image].
- [ ] **Plugin/provider extension** - Add one optional command provider without changing the app UI or core router. [MAUI DI][m-di], [custom handlers][m-handlers] / [Tauri plugin authoring][t-plugin-authoring].
- [ ] **Platform-specific custom UI** - Demonstrate handler/effect customization and explicit platform differences. [MAUI handlers][m-handlers], [effects][m-effects], [platform-specifics][m-platformspecifics].
- [ ] **Blazor Hybrid companion** - Compare one equivalent Razor/native interop scenario without making Blazor a dependency of the JS playground. [MAUI Blazor Hybrid][m-blazor].
- [ ] **Legacy control compatibility** - Record older ListView/TableView/cell and Frame behaviour only when needed; do not build new samples around obsolete APIs. [MAUI controls][m-controls].
- [ ] **Age signals** - Optional reference-sample experiment for platform age-consent APIs, not a built-in Essentials API. [Official sample][m-age].
- [ ] **In-app purchases** - Optional store-sandbox experiment, not a built-in Essentials billing service. [Official sample][m-billing].

### Development and distribution coverage

- [ ] **Native Hot Reload** - Verify supported C#/XAML edits independently of Vite HMR. [MAUI XAML Hot Reload][m-hotreload] / [Tauri native watch/restart][t-develop]. These are different mechanisms.
- [x] **Build/stage frontend assets** - Produce and package fresh frontend output without stale files or per-target races. [MAUI assets][m-assets] / [Tauri build configuration][t-config].
- [ ] **Installable artifacts** - Build, install and run platform-appropriate application packages. [MAUI deployment][m-deployment] / [Tauri distribution][t-distribution].
- [ ] **Signing and installed execution** - Verify signatures, entitlements and offline native interop after installation. [MAUI deployment][m-deployment] / [Tauri distribution][t-distribution].
- [ ] **Updates** - Check/download/apply a signed update through the chosen desktop distribution channel. **No direct universal MAUI updater.** [Tauri updater][t-updater].
- [ ] **Trimming and NativeAOT** - Verify registered commands/DTOs in supported published configurations. [MAUI NativeAOT][m-aot] / [Tauri commands][t-commands]. Do not equate Rust compilation with .NET trimming.
- [ ] **Diagnostics and performance** - Capture command latency, startup, memory and reload behaviour with reproducible scenarios. [MAUI profiling][m-profiling] / [Tauri logging][t-log].
- [ ] **Browser mocks and native automation** - Run frontend contract checks separately from device/native tests through repeatable checked-in entrypoints. [MAUI UI testing][m-uitesting] / [Tauri mocks][t-mocks], [WebDriver][t-webdriver].

## Next implementation focus

Finish the unchecked P0 acceptance targets without replacing working explicit registration or inventing mock OS permission results. Then add small P1 experiments for files, HTTP, window operations and a real native control panel.

Keep one feature catalog and one command hub. The feature page and diagnostic experiment must call the same modules; do not create a second test-only implementation. Keep the playground small: no account system, backend administration, social features or themed story. A feature is a small experiment, not a complete application.

## API and platform gaps

- Tauri primarily supplies web UI plus native APIs. MAUI also supplies native controls, layouts, bindings and graphics; browser controls are not proof of those MAUI features.
- Tauri official plugins are inventoried, but not all are installed/enabled by default or supported on every platform.
- MAUI does not ship universal APIs for tray icons, global shortcuts, arbitrary shell execution, a save dialog, NFC, barcode scanning or a generic encrypted vault. Those rows identify adapters/packages rather than invented Essentials APIs.
- The MAUI target set here is Android, iOS, Mac Catalyst and Windows. Tauri Linux/native macOS support is a separate platform difference, not an unchecked API experiment. Tizen is excluded.
- Experiments needing real hardware, credentials, entitlements or an external service must document that requirement. Setup effort alone does not determine priority.
- Authentication, purchases, signed updates and installed execution cannot be completed using only a fake in-app response. Simulations must be explicitly labelled.
- This list groups low-level classes/control properties into user-visible feature families. It does not exhaustively catalogue third-party ecosystems.

## Inventory completeness

**Tauri core modules represented:** `core`, `app`, `event`, `window`, `webview`, `webviewWindow`, `path`, `dpi`, `image`, `menu`, `tray`, `mocks`. Root namespace exports and declarations are packaging/typing, not additional feature families.

**All 30 official Tauri plugins represented:** `autostart`, `barcode-scanner`, `biometric`, `cli`, `clipboard-manager`, `deep-link`, `dialog`, `fs`, `geolocation`, `global-shortcut`, `haptics`, `http`, `localhost`, `log`, `nfc`, `notification`, `opener`, `os`, `persisted-scope`, `positioner`, `process`, `shell`, `single-instance`, `sql`, `store`, `stronghold`, `updater`, `upload`, `websocket`, `window-state`.

**MAUI Essentials families represented:** AppActions, AppInfo, Browser, Launcher, MainThread, Map, Permissions, VersionTracking, Contacts, Email, Connectivity, Passkeys, PhoneDialer, Sms, WebAuthenticator, Battery, DeviceDisplay, DeviceInfo, Accelerometer, Barometer, Compass, Gyroscope, Magnetometer, OrientationSensor, Flashlight, Geocoding, Geolocation, HapticFeedback, Vibration, MediaPicker, Screenshot, TextToSpeech, UnitConverters, Clipboard, Share, FilePicker, FileSystem, Preferences, SecureStorage and SemanticScreenReader.

Native UI coverage additionally includes pages/navigation, layouts, modern controls, graphics, shapes, gestures, animations, resources/styles/templates/bindings, localization, accessibility, windows/menus, WebView/HybridWebView, handlers and legacy compatibility.

## Sources

Inventoried on **2026-10-12**. API documentation is live; source snapshots pin the original inventory.

- [Current sample, implementation and acceptance scope][sample-readme].
- [Standalone SDK imports, source documentation and release preparation][sdk-readme].
- [MAUI platform feature index][m-platform] and [native controls index][m-controls].
- [Tauri core source inventory](https://github.com/tauri-apps/tauri/tree/d62e5d9c0878b6fb327543cc9be50aa1696ee23c/packages/api/src).
- [Tauri plugin source inventory](https://github.com/tauri-apps/plugins-workspace/tree/c91a9e6af5e51d75d03ae289eb69534ef22fb923/plugins).
- [Official MAUI samples](https://github.com/dotnet/maui-samples), references rather than automatic net11 compatibility guarantees.

[sample-readme]: README.md
[sdk-readme]: JavaScript/README.md
[m-platform]: https://learn.microsoft.com/dotnet/maui/platform-integration/?view=net-maui-11.0
[m-controls]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/?view=net-maui-11.0
[m-hybrid]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-11.0
[m-invoke]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-11.0#invoke-c-from-javascript
[m-net11]: https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-11?view=net-maui-11.0#hybridwebview-js-to-net-invocation
[m-appinfo]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/app-information?view=net-maui-11.0
[m-deviceinfo]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/information?view=net-maui-11.0
[m-clipboard]: https://learn.microsoft.com/dotnet/maui/platform-integration/data/clipboard?view=net-maui-11.0
[m-preferences]: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/preferences?view=net-maui-11.0
[m-permissions]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/permissions?view=net-maui-11.0
[m-mainthread]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/main-thread?view=net-maui-11.0
[m-assets]: https://learn.microsoft.com/dotnet/maui/fundamentals/single-project?view=net-maui-11.0#resource-files
[m-devtools]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-11.0#enable-browser-developer-tools
[m-filepicker]: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/file-picker?view=net-maui-11.0
[m-files]: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/file-system-helpers?view=net-maui-11.0
[m-share]: https://learn.microsoft.com/dotnet/maui/platform-integration/data/share?view=net-maui-11.0
[m-secure]: https://learn.microsoft.com/dotnet/maui/platform-integration/storage/secure-storage?view=net-maui-11.0
[m-connectivity]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/networking?view=net-maui-11.0
[m-dialogs]: https://learn.microsoft.com/dotnet/maui/user-interface/pop-ups?view=net-maui-11.0
[m-launcher]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/launcher?view=net-maui-11.0
[m-browser]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/open-browser?view=net-maui-11.0
[m-window]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/window?view=net-maui-11.0
[m-lifecycle]: https://learn.microsoft.com/dotnet/maui/fundamentals/app-lifecycle?view=net-maui-11.0
[m-embedding]: https://learn.microsoft.com/dotnet/maui/platform-integration/native-embedding?view=net-maui-11.0
[m-shell]: https://learn.microsoft.com/dotnet/maui/fundamentals/shell/?view=net-maui-11.0
[m-pages]: https://learn.microsoft.com/dotnet/maui/user-interface/pages/?view=net-maui-11.0
[m-collection]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/collectionview/?view=net-maui-11.0
[m-layouts]: https://learn.microsoft.com/dotnet/maui/user-interface/layouts/?view=net-maui-11.0
[m-safe]: https://learn.microsoft.com/dotnet/maui/user-interface/safe-area?view=net-maui-11.0
[m-theme]: https://learn.microsoft.com/dotnet/maui/user-interface/system-theme-changes?view=net-maui-11.0
[m-accessibility]: https://learn.microsoft.com/dotnet/maui/fundamentals/accessibility?view=net-maui-11.0
[m-menu]: https://learn.microsoft.com/dotnet/maui/user-interface/menu-bar?view=net-maui-11.0
[m-contextmenu]: https://learn.microsoft.com/dotnet/maui/user-interface/context-menu?view=net-maui-11.0
[m-keyboard]: https://learn.microsoft.com/dotnet/maui/user-interface/keyboard-accelerators?view=net-maui-11.0
[m-images]: https://learn.microsoft.com/dotnet/maui/user-interface/images/images?view=net-maui-11.0
[m-fonts]: https://learn.microsoft.com/dotnet/maui/user-interface/fonts?view=net-maui-11.0
[m-icon]: https://learn.microsoft.com/dotnet/maui/user-interface/images/app-icons?view=net-maui-11.0
[m-splash]: https://learn.microsoft.com/dotnet/maui/user-interface/images/splashscreen?view=net-maui-11.0
[m-resources]: https://learn.microsoft.com/dotnet/maui/fundamentals/resource-dictionaries?view=net-maui-11.0
[m-styles]: https://learn.microsoft.com/dotnet/maui/user-interface/styles/xaml?view=net-maui-11.0
[m-css]: https://learn.microsoft.com/dotnet/maui/user-interface/styles/css?view=net-maui-11.0
[m-bindings]: https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/?view=net-maui-11.0
[m-battery]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/battery?view=net-maui-11.0
[m-display]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/display?view=net-maui-11.0
[m-location]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/geolocation?view=net-maui-11.0
[m-geocoding]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/geocoding?view=net-maui-11.0
[m-maplaunch]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/maps?view=net-maui-11.0
[m-maps]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/map?view=net-maui-11.0
[m-media]: https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/picker?view=net-maui-11.0
[m-screenshot]: https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/screenshot?view=net-maui-11.0
[m-speech]: https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/text-to-speech?view=net-maui-11.0
[m-haptics]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/haptic-feedback?view=net-maui-11.0
[m-vibration]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/vibration?view=net-maui-11.0
[m-flashlight]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/flashlight?view=net-maui-11.0
[m-sensors]: https://learn.microsoft.com/dotnet/maui/platform-integration/device/sensors?view=net-maui-11.0
[m-units]: https://learn.microsoft.com/dotnet/maui/platform-integration/device-media/unit-converters?view=net-maui-11.0
[m-colors]: https://learn.microsoft.com/dotnet/maui/user-interface/graphics/colors?view=net-maui-11.0
[m-drag]: https://learn.microsoft.com/dotnet/maui/fundamentals/gestures/drag-and-drop?view=net-maui-11.0
[m-native]: https://learn.microsoft.com/dotnet/maui/platform-integration/invoke-platform-code?view=net-maui-11.0
[m-weather]: https://github.com/dotnet/maui-samples/tree/main/10.0/Apps/WeatherTwentyOne
[m-applinks]: https://learn.microsoft.com/dotnet/maui/macios/universal-links?view=net-maui-11.0
[m-androidapplinks]: https://learn.microsoft.com/dotnet/maui/android/app-links?view=net-maui-11.0
[m-notifications]: https://learn.microsoft.com/dotnet/maui/platform-integration/local-notifications?view=net-maui-11.0
[m-actions]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/app-actions?view=net-maui-11.0
[m-version]: https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/version-tracking?view=net-maui-11.0
[m-titlebar]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/titlebar?view=net-maui-11.0
[m-webview]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/webview?view=net-maui-11.0
[m-sql]: https://learn.microsoft.com/dotnet/maui/data-cloud/database-sqlite?view=net-maui-11.0
[m-graphics]: https://learn.microsoft.com/dotnet/maui/user-interface/graphics/?view=net-maui-11.0
[m-shapes]: https://learn.microsoft.com/dotnet/maui/user-interface/controls/shapes/?view=net-maui-11.0
[m-gestures]: https://learn.microsoft.com/dotnet/maui/fundamentals/gestures/tap?view=net-maui-11.0
[m-animations]: https://learn.microsoft.com/dotnet/maui/user-interface/animation/basic?view=net-maui-11.0
[m-behaviors]: https://learn.microsoft.com/dotnet/maui/fundamentals/behaviors?view=net-maui-11.0
[m-triggers]: https://learn.microsoft.com/dotnet/maui/fundamentals/triggers?view=net-maui-11.0
[m-localization]: https://learn.microsoft.com/dotnet/maui/fundamentals/localization?view=net-maui-11.0
[m-contacts]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/contacts?view=net-maui-11.0
[m-email]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/email?view=net-maui-11.0
[m-sms]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/sms?view=net-maui-11.0
[m-phone]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/phone-dialer?view=net-maui-11.0
[m-auth]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/authentication?view=net-maui-11.0
[m-passkeys]: https://learn.microsoft.com/dotnet/maui/platform-integration/communication/passkeys?view=net-maui-11.0
[m-di]: https://learn.microsoft.com/dotnet/maui/fundamentals/dependency-injection?view=net-maui-11.0
[m-handlers]: https://learn.microsoft.com/dotnet/maui/user-interface/handlers/?view=net-maui-11.0
[m-effects]: https://learn.microsoft.com/dotnet/maui/migration/effects?view=net-maui-11.0
[m-platformspecifics]: https://learn.microsoft.com/dotnet/maui/platform-integration/?view=net-maui-11.0#access-platform-apis
[m-blazor]: https://learn.microsoft.com/aspnet/core/blazor/hybrid/?view=aspnetcore-11.0
[m-age]: https://github.com/dotnet/maui-samples/tree/main/10.0/AgeSignals
[m-billing]: https://github.com/dotnet/maui-samples/tree/main/10.0/BillingService
[m-hotreload]: https://learn.microsoft.com/dotnet/maui/xaml/hot-reload?view=net-maui-11.0
[m-deployment]: https://learn.microsoft.com/dotnet/maui/deployment/?view=net-maui-11.0
[m-aot]: https://learn.microsoft.com/dotnet/maui/deployment/nativeaot?view=net-maui-11.0
[m-profiling]: https://learn.microsoft.com/dotnet/maui/deployment/performance?view=net-maui-11.0
[m-uitesting]: https://learn.microsoft.com/dotnet/maui/deployment/ui-testing?view=net-maui-11.0
[net-json]: https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/overview
[net-http]: https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient
[net-websocket]: https://learn.microsoft.com/dotnet/api/system.net.websockets.clientwebsocket
[net-process]: https://learn.microsoft.com/dotnet/api/system.diagnostics.process
[net-cli]: https://learn.microsoft.com/dotnet/standard/commandline/
[web-media]: https://developer.mozilla.org/docs/Web/API/MediaDevices/getUserMedia
[t-api]: https://v2.tauri.app/reference/javascript/api/
[t-core]: https://v2.tauri.app/reference/javascript/api/namespacecore/
[t-app]: https://v2.tauri.app/reference/javascript/api/namespaceapp/
[t-window]: https://v2.tauri.app/reference/javascript/api/namespacewindow/
[t-webview]: https://v2.tauri.app/reference/javascript/api/namespacewebview/
[t-webviewwindow]: https://v2.tauri.app/reference/javascript/api/namespacewebviewwindow/
[t-path]: https://v2.tauri.app/reference/javascript/api/namespacepath/
[t-dpi]: https://v2.tauri.app/reference/javascript/api/namespacedpi/
[t-image]: https://v2.tauri.app/reference/javascript/api/namespaceimage/
[t-menu]: https://v2.tauri.app/reference/javascript/api/namespacemenu/
[t-tray]: https://v2.tauri.app/reference/javascript/api/namespacetray/
[t-mocks]: https://v2.tauri.app/develop/tests/mocking/
[t-commands]: https://v2.tauri.app/develop/calling-rust/
[t-state]: https://v2.tauri.app/develop/state-management/
[t-events]: https://v2.tauri.app/develop/calling-frontend/#event-system
[t-channels]: https://v2.tauri.app/develop/calling-frontend/#channels
[t-capabilities]: https://v2.tauri.app/security/capabilities/
[t-config]: https://v2.tauri.app/reference/config/
[t-develop]: https://v2.tauri.app/develop/
[t-example]: https://github.com/tauri-apps/tauri/tree/d62e5d9c0878b6fb327543cc9be50aa1696ee23c/examples/api
[t-plugins]: https://v2.tauri.app/develop/plugins/
[t-clipboard]: https://v2.tauri.app/plugin/clipboard/
[t-dialog]: https://v2.tauri.app/plugin/dialog/
[t-fs]: https://v2.tauri.app/plugin/file-system/
[t-store]: https://v2.tauri.app/plugin/store/
[t-http]: https://v2.tauri.app/plugin/http-client/
[t-upload]: https://v2.tauri.app/plugin/upload/
[t-websocket]: https://v2.tauri.app/plugin/websocket/
[t-opener]: https://v2.tauri.app/plugin/opener/
[t-os]: https://v2.tauri.app/plugin/os-info/
[t-log]: https://v2.tauri.app/plugin/logging/
[t-geolocation]: https://v2.tauri.app/plugin/geolocation/
[t-haptics]: https://v2.tauri.app/plugin/haptics/
[t-shortcut]: https://v2.tauri.app/plugin/global-shortcut/
[t-deeplink]: https://v2.tauri.app/plugin/deep-linking/
[t-single]: https://v2.tauri.app/plugin/single-instance/
[t-notification]: https://v2.tauri.app/plugin/notification/
[t-windowstate]: https://v2.tauri.app/plugin/window-state/
[t-positioner]: https://v2.tauri.app/plugin/positioner/
[t-sql]: https://v2.tauri.app/plugin/sql/
[t-persistedscope]: https://v2.tauri.app/plugin/persisted-scope/
[t-biometric]: https://v2.tauri.app/plugin/biometric/
[t-barcode]: https://v2.tauri.app/plugin/barcode-scanner/
[t-nfc]: https://v2.tauri.app/plugin/nfc/
[t-stronghold]: https://v2.tauri.app/plugin/stronghold/
[t-autostart]: https://v2.tauri.app/plugin/autostart/
[t-process]: https://v2.tauri.app/plugin/process/
[t-shell]: https://v2.tauri.app/plugin/shell/
[t-cli]: https://v2.tauri.app/plugin/cli/
[t-localhost]: https://v2.tauri.app/plugin/localhost/
[t-plugin-authoring]: https://v2.tauri.app/develop/plugins/
[t-distribution]: https://v2.tauri.app/distribute/
[t-updater]: https://v2.tauri.app/plugin/updater/
[t-webdriver]: https://v2.tauri.app/develop/tests/webdriver/
