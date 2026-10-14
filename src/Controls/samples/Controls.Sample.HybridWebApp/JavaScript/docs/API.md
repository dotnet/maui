# Public API and operational behavior

All subpaths resolve to built `.js` and `.d.ts` files, never original `.ts`.
There are no wildcard/private-source exports.
Public symbols and members are documented in the TypeScript source and shipped
declarations; this page explains organization and host compatibility. Generate
the detailed source-derived reference with `npm run docs` (see [release
documentation](RELEASING.md#source-reference-and-docfxmicrosoft-learn)).

| Import | Exports |
| --- | --- |
| `@maui/api` | `connect`, `disconnect`, `NativeClient`, `NativeError`; core types `Bridge`, `Json`, `Decoder`, `Ready`, `Operation`, `Progress` only |
| `@maui/api/core` | Infrastructure above, `configureBridge`, `resetBridge`, connection generation/readiness, `subscribeShared`, bootstrap and JSON validators |
| `@maui/api/application` | Services `windows`, `window(id)`, `openWindow(options)`, `getTheme`, `setTheme`, `onThemeChanged`; theme types |
| `@maui/api/essentials/app-info` | Service `getAppInfo()`; `AppInfo` |
| `@maui/api/essentials/device-info` | Service `getDeviceInfo()`; `DeviceInfo` |
| `@maui/api/essentials/clipboard` | Services `getText`, `setText`, `hasText`, `onContentChanged` |
| `@maui/api/essentials/preferences` | Services `get`, `set`, `contains`, `remove`, `clear`; `PreferenceValue`, `PreferenceOptions` |
| `@maui/api/essentials/battery` | Service `getBatteryInfo()`; `BatteryInfo` |
| `@maui/api/essentials/display` | Service `getDisplayInfo()`; `DisplayInfo` |
| `@maui/api/essentials/connectivity` | Services `getConnectivity()`, `onConnectivityChanged(listener)`; `ConnectivityInfo` |
| `@maui/api/essentials/text-to-speech` | Services `getLocales()`, `speak(text, options?)`; `SpeechLocale`, `SpeechOptions`; core `Operation<void>` cancellation |
| `@maui/api/essentials/launcher` | Service `openUrl(url)`; public HTTPS external native system launch only |
| `@maui/api/essentials/file-picker` | Services `pickFile(options?)`, `pickMultipleFiles(options?)`; resources `PickedFile`, `FileReader`; picker/chunk types |
| `@maui/api/ui/windows` | Synchronous local acquisition `currentWindow(): Window`; single resource class `Window`; size/position/options/capability types |
| `@maui/api/ui/dialogs` | Async services `alert(message?, options?)`, `confirm`, `prompt`, `actionSheet`; native option/result types |
| `@maui/api/ui/remote-webview` | Capability `supportsBrowserView`; resource creation `createBrowserView`; `BrowserView`, geometry/navigation types |
| `@maui/api/ui/menus` | Resource `Menu.create`; item resource `MenuItem`; definition/shortcut/patch/capability types |

The earlier root re-export table described the code accurately, but mixed
infrastructure with Essentials and process diagnostics. This unpublished preview
now follows MAUI containers: features and feature DTOs are imported only from
their Essentials or UI subpaths. `@maui/api/app/info`, `getInfo` and SDK `Identity`
were intentionally removed rather than retained as confusing aliases.

All stock feature functions/factories are client-free. Infrastructure configures
one shared connection per WebView document/realm before connect, and the sample
bootstrap uses that same connection. Native windows have separate realms/scopes,
not a process-global client. Resource handles capture their exact private client
and generation; disconnect/reload cannot rebind an old handle. This unpublished
preview removes factory/client/controller aliases rather than retaining two contracts.
All modules are in one npm package. Feature subpaths depend on core transport
types and validators, not Controls or the playground; no extra packages or
transport framework are introduced by this organization.

## Features

`getAppInfo()` resolves an `AppInfo`: native appName, version and build.
`getDeviceInfo()` resolves a `DeviceInfo`: native platform, osVersion and model.
These are supported subsets of MAUI Essentials, not a claim that every .NET
AppInfo/DeviceInfo member or every platform is implemented. There is no
browser/user-agent/package-version fallback.

`getAppInfo` uses `app.info`; `getDeviceInfo` uses the independently registered
`device.info`. Each native provider owns only its corresponding Essentials API,
DTOs and generated JSON context. Extra fields are not leaked into helper results.
This unpublished preview's internal wire mapping changed from the former combined
identity response; public module names and return types remain unchanged.
Process architecture, numeric pid, startupId and startedAt are returned only by
the sample-owned `sample.diagnostics` command and decoded by `Web/src/native/sample.ts`.
That app-owned helper composes the installed AppInfo/DeviceInfo helpers with its
own diagnostics decoder to preserve the existing reload-identity UI.

Clipboard has direct `setText(text)`, `getText(): Promise<string|null>`,
`hasText()` and `onContentChanged(listener)` with explicit unsubscribe.
No browser clipboard fallback exists. Preferences use
`get(key, defaultValue, options?)`, `set(key, primitive, options?)`, `contains`,
`remove` and `clear`. String/bool/number/Date are supported with native numeric
representation options; JS long values must be safe integers. Optional shared
containers remain host-namespaced. Clear deletes only tracked owned keys, never
the whole native store. Preferences are not SecureStorage or a JSON-object store.
No sample namespace is embedded in the product.

Connectivity reads current native `IConnectivity.NetworkAccess` and
`ConnectionProfiles`. `onConnectivityChanged` delivers actual native
`ConnectivityChanged` notifications, never a synthetic initial event. Read
separately for a fresh snapshot. Multiple JS listeners multiplex one native
document topic; the last asynchronous unsubscribe detaches this scope's OS
handler. Page cleanup does not detach another page/scope's subscription.
Disconnect/reconnect ignores retired callbacks. Even native `Internet` does
not guarantee internet or endpoint reachability: there is no HTTP probe,
`navigator.onLine` or browser event fallback.

Speech queries real native `ITextToSpeech.GetLocalesAsync` and preserves its
locale fields (including empty strings). `speak` starts actual `SpeakAsync` with
optional native locale, pitch (0–2), volume (0–1) and rate (0.1–2). Omitted
options retain native defaults. The returned operation uses the existing core
cancellation protocol and passes the exact native operation token to speech.
It is not a global stop or a promise that already-played audio is undone.
Speak only from explicit user actions, await `operation.result`, and cancel
that operation when its owner disappears. Text fits the host-advertised UTF-16
budget; speech has the bounded native interactive deadline, not unlimited
background execution. Unsupported engines, missing voices or native failures
are explicit, never browser `speechSynthesis`.

`openUrl` calls actual native `ILauncher.OpenAsync(Uri)` after the shared
`RemoteWebAddress` public HTTPS policy. Credentials, IP literals, local/internal
names, non-HTTPS schemes, custom ports and oversized/non-ASCII addresses are
rejected before launch. It exposes no file, shell or custom-scheme authority.
A native false return is `launch_failed`, not successful site loading. Trigger
launch only on an explicit user click. Neither an in-app browser nor embedded
guest nor `window.open` is used. This is an address policy, not a DNS/redirect
firewall. Platform/permission/unsupported errors remain explicit.

```ts
import { getConnectivity, onConnectivityChanged } from '@maui/api/essentials/connectivity'
import { getLocales, speak } from '@maui/api/essentials/text-to-speech'
import { openUrl } from '@maui/api/essentials/launcher'

const network = await getConnectivity()
const unsubscribe = await onConnectivityChanged(value => console.log(value.networkAccess))
await unsubscribe()
// Speech and launching belong in user-action handlers.
const locales = await getLocales()
const speech = await speak('Hello', { locale: locales[0], pitch: 1, volume: 0.5, rate: 1 })
// Another user action/owner cleanup may call await speech.cancel().
await speech.result
await openUrl('https://learn.microsoft.com/dotnet/maui/')
```

Native host composition opts into `ConnectivityProvider`, `TextToSpeechProvider`
and `LauncherProvider` independently. Inject `IConnectivity` using
`Connectivity.Current`, `ITextToSpeech` using `TextToSpeech.Default` and
`ILauncher` using `Launcher.Default`. Providers register explicit strict
source-generated DTO metadata and the network topic; they are product
capabilities, not C# sample recipes. Android hosts need `ACCESS_NETWORK_STATE`
for Connectivity and the `android.intent.action.TTS_SERVICE` package query
for Android 11+ speech-engine visibility. No runtime permission or OS side
effect is requested merely by mounting these playground pages.

Each helper validates its result fields at runtime, including nullability.
Core numeric validators require finite numbers. Inputs are typed but
**native validation is authoritative**:
TypeScript is not a security boundary and JavaScript callers can supply invalid
values. The reference provider bounds text to 4096 UTF-16 code units and keys
to 1–48 ASCII letters/digits/underscore/hyphen. Other hosts must define their
limits consistently with their registration/contracts.

`currentWindow()` is synchronous and performs no IPC. Its single `Window` handle
offers async `id()`, `title()/setTitle`, `size()/setSize`, `position()/setPosition`,
`url`, `capabilities`, `close`, `activate`, `center`, `onSizeChanged` and
`onPositionChanged`. Identity is native-authoritative and async, never a fake
placeholder or a title/route alias. Application `windows`, `window(id)` and
`openWindow` return the same resource type, not CurrentWindow/ChildWindow DTOs.
Acquisition retains native private generations; closed/reopened IDs do not let
stale handles target a replacement. Duplicate IDs reject.

```ts
import * as app from '@maui/api/application'
import { currentWindow } from '@maui/api/ui/windows'

await currentWindow().setTitle('Hello World')
const settings = await app.openWindow({
  id: 'settings', url: 'index.html#/settings', title: 'Settings',
})
await settings.setSize({ width: 900, height: 650 })
if ((await settings.capabilities()).center) await settings.center()
```

Bundle-relative URLs/query/fragments are generic frontend locations, not native
route registrations. Remote/special-scheme/traversal destinations cannot inherit
the trusted bridge through window opening. IDs are 1–64 ASCII
letters/digits/dot/underscore/hyphen, unique and immutable within the app registry.
New scopes isolate providers/counters, not the OS process. Separate windows are
unsupported on iPhone. Mac/Windows desktop geometry uses logical units; Windows
centering uses actual current-monitor work area/size/density. Mac centering uses
global CoreGraphics display bounds and the owning scene's effective system frame.
Mobile centering and Mac native position observation explicitly reject.

Dialogs use the calling native page for alerts, confirmations, prompts and action sheets;
false confirmation or `{ cancelled: true, text: null }` prompt is normal user
dismissal. Both dialog and file-picker commands require interactive handshake
metadata for their bounded five-minute native budget.

Options expose native labels/title/placeholder/initial value/maxLength/keyboard
and flow direction; unsupported custom dialog kinds/layouts are not fabricated.
These services return promises, not synchronous browser globals.

`pickFile`/`pickMultipleFiles` select with actual MAUI pickerTitle/platform
fileTypes. Selection returns metadata and a PickedFile resource, not contents.
`openRead()` returns a distinct reader with bounded binary/incremental strict
UTF-8 reads, partial-read/EOF states, actual seek where supported, and close.
Reads/seek/close are serialized per reader. 65536 bytes is the per-message chunk
budget, **not file capacity**; binary/large files remain genuine selected resources.
Invalid UTF-8 rejects text decoding but binary reading is available from another
reader. Folder/save/defaultPath require another adapter and reject here.
No FullPath, arbitrary read path or persistent authority is exposed. Release
unused files/readers; document teardown invalidates them. Treat file text as
private, render it as text and omit it from logs.
Battery and Display helpers read/refresh native snapshots without web fallbacks,
subscriptions, sensor streaming or keep-screen-awake changes.

`createBrowserView()` creates a hidden ordinary desktop guest, then accepts measured
CSS slot/viewport/clipping bounds and allowlisted back/forward/reload/focus/hide/close
controls. It is native composition, not an iframe or DOM element. Close on
component teardown; hide before custom DOM overlays. Mobile guests reject
explicitly. Remote content receives no native target, SDK, messages or inspector.
Menus are TypeScript definitions/callbacks, not C# recipes. Use
`const menu = Menu.create(definitions); await currentWindow().setMenu(menu)`,
`await currentWindow().menu()` and `menu.item(id)` getter/setter resources.
`setMenu(null)` removes. Item text/enabled/checked patches contain only the
installation/window/item/property, preserve callbacks, and reject replaced handles.
Explicit setters perform IPC; ordinary object assignment does not.
Mac Catalyst checks use real UIKit state; Windows MAUI checks reject explicitly.
Mac focused owning-page menus render in the app menubar, not separate per-hardware-
window menubars. Mobile native menus/desktop guests reject; no HTML/iframe fallback.
Scoped `menus.clicked` notifications carry native item/source IDs. A
notification UUID is not an active operation's correlation ID.

Theme is application-level: `getTheme`, `setTheme('system'|'light'|'dark')`,
`onThemeChanged`. Native effective callbacks apply trusted frontend CSS; the
native preference alone cannot paint the webpage, and remote websites are not
force-themed. Geometry/theme/clipboard callbacks observe native sources,
multiplex JS listeners and detach the last source; teardown suppresses stale events.

## MAUI / Tauri parity audit

This is a bounded implemented subset, not complete parity with either framework.
Primary contracts: [MAUI APIs](https://learn.microsoft.com/dotnet/maui/),
[Tauri dialog](https://v2.tauri.app/reference/javascript/dialog/),
[filesystem](https://v2.tauri.app/reference/javascript/fs/),
[window](https://v2.tauri.app/reference/javascript/api/namespacewindow/) and
[menu](https://v2.tauri.app/reference/javascript/api/namespacemenu/).

| Family | Actual implementation / difference |
| --- | --- |
| Picker/readers | MAUI PickAsync/PickMultipleAsync + FileResult.OpenReadAsync; Tauri-style distinct selection/read lifetimes. No save/folder/defaultPath/persistent path authority. |
| Dialogs | Native MAUI Page options and promises; no arbitrary Tauri custom kinds/layouts. Cancellation is a normal result. |
| Windows | MAUI title/size/position/activation/lifecycle plus guarded desktop center adapter. Frontend owns URL/router; generation token is private and developer ID is immutable. No OS deep-link activation in this milestone. |
| Menus | Page.MenuBarItems/KeyboardAccelerators; actual Mac checks, Windows check gap explicit. Logical owning page becomes focused Mac app menu. Item updates supported; general append/prepend/remove/reparent trees remain future work. |
| Clipboard/preferences | Text/change native clipboard; MAUI primitive preferences/defaults/existence/owned removal/clear/shared containers. No binary clipboard/secure storage/JSON store. |
| Battery/display | Charge may be -1; native charge/state/power/energy-saver and main-display dimensions/density/orientation. Events, KeepScreenOn and all-monitor enumeration remain unimplemented. |
| Browser guest | Ordinary desktop MAUI WebView in a measured clipped native surface, no bridge/inspector; not Tauri WebviewWindow or DOM iframe, not a DNS/subresource firewall. |
| Theme/events | Native Application effective theme + coalesced actual window frames; Windows move source only. No per-window general theme or forced remote website theme. |

## Low-level client

```ts
import { NativeClient, object, text } from '@maui/api/core'
import type { Bridge, Decoder } from '@maui/api/core'

const decode: Decoder<{ message: string }> = value => ({
  message: text(object(value).message),
})
// bridge is supplied by your host integration, not a browser emulator.
async function run(bridge: Bridge) {
  const client = new NativeClient(bridge)
  try {
    client.attachToDocument()
    const ready = await client.connect()
    if (!ready.commands.includes('custom.greet')) throw new Error('Provider unavailable')
    return await client.invoke('custom.greet', { name: 'World' }, decode)
  } finally { client.dispose() }
}
```

The host must explicitly register `custom.greet` with matching native JSON
metadata. No arbitrary native member invocation or command generation exists.
The injected `Bridge` need not come from MAUI Controls or a MAUI virtual origin.
`new NativeClient(bridge)` and its instance `connect()` do not invoke automatic
bootstrap. Stock features instead use core's configured shared connection.
The shared root `connect()`/`loadNativeBridge()` remains MAUI-specific. See the
[explicit transport example](../README.md#explicit-transport-without-maui-automatic-bootstrap).

- `NativeClient(bridge | undefined)`: generates a document UUID on construction;
  `available` reports transport presence and non-disposed state, **not** provider
  availability, handshake success, OS authorization or an unexpired lease.
- `connect(): Promise<Ready>`: invokes `core.connect`; returns commands,
  preferenceNamespace and maximumTextLength. Metadata is descriptive; the
  reference command list includes disabled registered names too.
- `invoke(command, args, decode): Promise<T>`: validates response version and
  identities, then the supplied `Decoder<T>`. Use JSON-compatible args.
- `start(...)`: returns `{ id, result, cancel }`. `cancel()` invokes
  `core.cancel` using the originating request UUID and returns `Promise<boolean>`.
  Cancellation is cooperative; it does not erase an already completed OS side
  effect and cannot guarantee success if all host call slots are occupied.
- `subscribe(topic, decode, listener)`: resolves an async idempotent unsubscribe
  function. One listener per topic per client; subscribe failure removes it.
  The host must opt in to each topic. Events contain the originating request ID.
- `receive(raw)`: accepts a native raw JSON event string; ignores stale sessions,
  versions and oversized/non-string input. Invalid events/decoder failures are
  rejected with a warning, not delivered as successful application values.
- `attachToDocument()`: idempotently attaches receive/pagehide/heartbeat cleanup;
  no-op without `window`. An injected headless host must manage its lease itself.
- `dispose()`: idempotently sends best-effort disconnect, cancels pending
  frontend waits, removes listeners, clears subscriptions and stops heartbeat.
  A disposed client cannot be reused. Disconnect delivery failure is warned
  without preventing local cleanup.

Root/core `connect(): Promise<NativeClient>` is the **shared** lifecycle function,
not the instance method returning metadata. It loads the native bridge lazily,
attaches document cleanup and awaits the handshake. Concurrent calls share one
startup. `disconnect()` cancels/disposes that default instance; it does not own
explicitly constructed clients. Startup failures clear the failed default;
later calls can retry. A replacement waits for a cancelled native handshake to
settle and retires its late session before connecting. Shared startup has a
28-second overall deadline, including bridge loading and any predecessor wait.
Framework bootstrap is shared independently: disconnect/reconnect cannot inject
duplicate framework scripts/listeners. If its five-second deadline expires, a
retry shares that same load until it actually finishes or errors; reload rather
than injecting a second potentially executable script.
No automatic reconnect/replay of operations exists.

## Errors and limits

`NativeError` extends Error with `code` and `message`. Rejecting native calls
never return error-shaped success values.

| Code | Meaning/action |
| --- | --- |
| `native_unavailable` | No recognized native host, or explicitly disposed/unavailable client. Launch the matching app; no fallback. |
| `transport_error` | Bridge script/invocation failure or invalid JSON parsing. Script loading has a five-second deadline. |
| `transport_timeout` | A call's 23-second deadline or shared startup's 28-second deadline expired; disconnect is signalled when a client exists. Reload to reconnect. |
| `invalid_response` | Invalid typed output/envelope identity, version or state. Fix host/client compatibility. |
| `invalid_arguments` | ASCII-escaped request exceeds 100000 characters, or provider input validation failed. |
| `duplicate_subscription` | Unsubscribe before registering a second listener for the topic. |
| `cancelled` | Cancellation, document teardown or native deadline. An OS side effect may already have happened. |

Native codes also include `invalid_request`, `unknown_command`,
`disabled_command`, `unknown_topic`, `stale_session`, `document_limit`,
`duplicate_request`, `busy`, `disconnected`, `unsupported`, `permission_denied`
and `native_error`. They are preserved, not conflated with browser availability.
Unknown/disabled exposure is host policy; it is not an OS permission prompt.
The reference host allows 16 simultaneous calls, a 20-second deadline, a
15-second document lease and 4096 retired documents. Reload after a stale lease;
restart the native app after its retired-document limit.

## Security and privacy

Trust the frontend as part of the native app. A UUID identifies a document; it
is not authentication or a sandbox. XSS in trusted web content can reach exposed
commands. Do not forward bridge routes to remote servers, register arbitrary
reflection-based invocations or use untrusted HTML as a command client.

Providers must validate args, enforce bounded inputs/namespace ownership,
explicitly request any necessary OS permissions and dispatch OS APIs on the
appropriate thread. Preferences are not a place for credentials; expose a
separate reviewed secure-storage provider if needed. The package contains no
telemetry, credentials or platform permission declarations. Do not log clipboard
contents, preference values or requests.
