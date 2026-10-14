# Protocol 1 and native host compatibility

This document describes the contract required by `@maui/api` preview
`0.1.0-preview.1`, not a protocol automatically supplied by MAUI 11. Root SDK
modules are authored; consumers must verify their native registrations match.

## Framework transport and registration

The bootstrap recognizes `app://0.0.0.1` (Apple) and `https://0.0.0.1`
(Android/Windows), no explicit port, and loads
`/_framework/hybridwebview.js`. Framework routes `/__hwvInvokeDotNet` and
`/__hwvSendMessage` remain framework-owned. An ordinary `http://localhost` Vite
page is not a native host even if a script supplies a similarly named global.
That restriction applies to **automatic MAUI bootstrap**, not explicitly
injected transports. `NativeClient(bridge)` accepts a compatible protocol
transport independently of origin or Controls and performs no script loading.
Injection changes neither envelope/error validation nor session/lifetime rules;
custom hosts must supply their own forwarding registration, event delivery and
document lease integration.

`Bridge` uses the operation names provided by the existing MAUI framework;
an explicit transport can implement the same contract:

```ts
interface Bridge {
  InvokeDotNet(method: string, args: string[]): Promise<unknown>
  SendRawMessage(message: string): void
}
```

The SDK calls **`InvokeDotNet('Invoke', [asciiEscapedRequest])`**, with one string
argument; the native target's only forwarding method is
`Task<string> Invoke(string request)`. The reference host registers its narrow
`IHybridAppTarget` with
`SetInvokeJavaScriptTarget<IHybridAppTarget>(hub, ProtocolJsonContext.Default)`
in `Native/MainPage.cs` in the source bundle. For tarball readers,
[MainPage.cs](https://github.com/dotnet/maui/blob/8d20834520af806e031f86b560a310b5584e6a66/src/Controls/samples/Controls.Sample.HybridWebApp/Native/MainPage.cs)
pins the matching scoped host and separate product/identity wire mapping.
Refresh source links to the selected release commit/tag before publication.
The .NET 11 invocation source generator and explicit System.Text.Json
`JsonTypeInfo<T>` metadata are required by that host. Installing the npm package
does not add this registration or a native command hub.

## Envelopes (camelCase)

Request, serialized as an ASCII-only JSON **string**, with canonical `D` UUIDs:

```json
{"version":1,"session":"11111111-1111-4111-8111-111111111111","id":"22222222-2222-4222-8222-222222222222","command":"clipboard.write","args":{"text":"\u4f60\u597d \ud83d\udc4b"}}
```

`InvokeDotNet` serializes this string again into the Fetch
`X-Maui-Request-Body` ByteString header. Escape UTF-16 code units U+007F–U+FFFF
with `\uXXXX`, including both surrogate halves; do not strip/replace Unicode or
switch to arbitrary object arguments. Quotes, backslashes, control characters,
null, arrays and numbers remain ordinary JSON. Native accepts at most **100000
ASCII characters**, version 1, UUID session/id and command length 1–64.

Native returns a JSON **string** (not an object):

```json
{"version":1,"session":"11111111-1111-4111-8111-111111111111","id":"22222222-2222-4222-8222-222222222222","status":"success","result":{"accepted":true},"error":null}
```

Error/cancellation use status `error` / `cancelled`, result null and
`error: {"code":"…","message":"…"}`. The SDK checks version/session/id and
status, rejects errors, and applies the feature/custom result decoder. Native
source-generated metadata rejects unknown members, invalid nullability and
missing required constructor parameters; the TypeScript decoder checks required
result fields but is not a native authorization mechanism.

Raw native events use:

```json
{"version":1,"session":"11111111-1111-4111-8111-111111111111","id":"22222222-2222-4222-8222-222222222222","topic":"my.topic","data":{"accepted":true}}
```

Deliver through `SendRawMessage`; framework JavaScript dispatches
`HybridWebViewMessageReceived` with `event.detail.message` containing that JSON
string. Hosts recheck session on the UI thread before sending. The SDK ignores
stale version/session and validates topic data. Topic availability is a host
registration decision; this example topic is not part of the reference host.

## Registered commands

| Name | Args | Result | Requirement |
| --- | --- | --- | --- |
| `core.connect` | `{}` | Commands, namespace, text limit; optional interactiveCommands / interactiveTimeoutMilliseconds / initialLocation | Session establishment/retirement |
| `core.cancel` | `{ requestId: string }` | `{ accepted: boolean }` | Cooperative request cancellation |
| `core.subscribe`, `core.unsubscribe` | `{ topic: string }` | `{ accepted: boolean }` | Explicit allowed topic list |
| `app.info` | `{}` | `{ appName: string, version: string, build: string }` | Independently registered IAppInfo provider |
| `device.info` | `{}` | `{ platform: string, osVersion: string, model: string }` | Independently registered IDeviceInfo provider |
| `clipboard.read` | `{}` | `{ text: string \| null }` | Clipboard provider |
| `clipboard.write` | `{ text: string }` | `{ accepted: boolean }` | Clipboard provider |
| `clipboard.hasText` | `{}` | `{ hasText: boolean }` | Actual native clipboard state |
| `preferences.get`, `preferences.set` | key + typed primitive/defaultValue + optional sharedName | Typed native primitive, `{ accepted }` | Host-namespaced primitive settings |
| `preferences.contains`, `preferences.delete`, `preferences.clear` | key/container or container only | `{ accepted: boolean }` | Clear removes only tracked namespace-owned keys |
| `battery.info` | `{}` | chargeLevel/state/powerSource/energySaverStatus | IBattery; Catalyst reads actual IOKit capacity instead of the incorrect UIKit value |
| `connectivity.info` | `{}` | networkAccess/connectionProfiles | Actual IConnectivity snapshot, not an endpoint reachability test |
| `speech.locales` | `{}` | `{ locales }` | Actual installed native speech locales |
| `speech.speak` | text + optional native locale/pitch/volume/rate | `{ accepted }` | User-triggered ITextToSpeech; interactive deadline and operation-specific cancellation |
| `launcher.openUrl` | `{ url }` | `{ accepted }` | Actual OS external launch; bounded public HTTPS policy, not arbitrary schemes/paths |
| `display.info` | `{}` | width/height/density/orientation/rotation/refreshRate | Direct IDeviceDisplay snapshot |
| `filePicker.pick`, `filePicker.pickMultiple` | pickerTitle/platform fileTypes | `{ file }`/`{ files }` metadata/resources | Actual OS selection, interactive policy; cancellation null/empty |
| `file.openRead`, `file.release` | Selected resource ID | Reader resource / `{ accepted }` | FileResult.OpenReadAsync, scope/document authority |
| `file.readBytes`, `file.seek`, `file.closeReader` | Reader ID + count/position | base64/eof or acceptance | ≤65536 bytes per read; real partial/EOF/seek/close |
| `dialogs.alert`, `dialogs.confirm` | title/message + native labels/flowDirection | `{ accepted: boolean }` | Calling native page, interactive policy |
| `dialogs.prompt`, `dialogs.actionSheet` | Native prompt/action-sheet options | Prompt cancellation/text or selected action | Calling native page, interactive policy |
| `window.info`, `window.title`, `window.size`, `window.position` | Optional target ID/private lease + corresponding changes | Private native snapshot or acceptance | Scoped MAUI window; writes validate actual capability |
| `window.find`, `window.center`, `window.activate` | ID or native target/private lease | Snapshot/acceptance | Caller-owned lookup; center Windows work area or Mac global display coordinates |
| `window.open` | `{ url, options? }` | `{ id, windowLease }` | Generic safe bundle-relative URL, immutable optional developer ID; no remote bridge destinations |
| `window.list`, `window.close`, `window.closeCurrent` | `{}`, `{ id, windowLease }`, `{}` | `{ windows }`, `{ accepted }` | Caller-owned current/child generations; no cross-window hub |
| `remote.info`, `remote.open` | `{}`, `{ url }` | supported/open/id/url/status/canGoBack/canGoForward | Desktop ordinary browser guest, initially hidden |
| `remote.bounds` | id + CSS slot/clip/viewport geometry + visible | Same guest state | Scoped guest identity and finite clipped geometry |
| `remote.control` | `{ id, action }` | Same guest state | Allowlisted back/forward/reload/focus/hide/close only |
| `remote.navigate` | `{ id, url }` | Guest navigation state | Same validated top-level HTTPS policy |
| `menus.info`, `menus.install`, `menus.clear` | windowId + descriptions/sourceId | Native capabilities/installation identity | TypeScript-defined real desktop menus; callbacks never serialized |
| `menus.item`, `menus.update` | windowId/sourceId/item ID + tiny patch | Native item state/capabilities | Installation/document ownership, not global item-ID authority |
| `application.theme`, `application.setTheme` | `{}`, `{ theme }` | Effective theme/preference | Application-level native preference, not Window/remote theme |

Current source is the sibling `Managed` class library (`Protocol/`,
`CommandRegistry.cs`, `HybridAppTargetObject.cs`, `Essentials/*`) plus the `Native`
host app. The one WIP managed library now references Controls.Core/Core for UI
providers, but has no sample dependency. Feature providers
own their generated contexts; the protocol context contains only envelopes and
control DTOs. External Toolkit assemblies register their own commands/topics and
strict generated `JsonTypeInfo<T>` explicitly, without global metadata edits.
The host, not either SDK, selects its preferences
namespace and the existing Essentials interfaces. The reference host also registers application-only
`sample.*` commands and `service.changed` / `operation.progress` topics; those
helpers remain in the playground and are **not** npm SDK features.

Product notification topics include `clipboard.changed`, `window.geometryChanged`,
`application.themeChanged`, `menus.clicked` and `remote.changed`. They originate
from typed provider-owned sources and use current-document/subscription checks.
First/last subscriptions attach/detach native sources; JS listeners share a topic
multiplexer. Progress retains its separate active originating-operation validation.
Private window leases bind acquisition to a native generation; never substitute a
lookup by reused ID when replaying a stale handle. Neither leases nor UUIDs are authentication.

### Preview AppInfo/DeviceInfo mapping

`getAppInfo` invokes `app.info`; `getDeviceInfo` invokes `device.info`. Their
public module names and result types are unchanged. These separate providers and
DTOs cannot expose or depend on the sample's process identity. The sample-only
`sample.diagnostics` command returns architecture/pid/startupId/startedAt.
The playground composes the three reads on the same exact client into its
unchanged visible reload-identity shape. The counter still belongs to the native
sample instance and survives document reload.

This **unpublished preview's internal wire mapping changed** from the former
combined `app.info` response. Update/relaunch the matching native host and stage
the frontend together; an old host will correctly reject `device.info` rather
than emulate data. The protocol version remains 1 because envelopes/lifetimes
did not change; there is no existing published compatibility promise.

## Session ownership and bounded lifetime

The first operation is `core.connect`. A new document identity retires the old
document, cancels its outstanding operations and clears native subscriptions.
An already retired identity cannot displace a newer session. The reference host
remembers up to 4096 retired documents and a bounded request-ID replay window.
Never share clients across independent documents or replay request IDs.

`SendRawMessage` document notices omit a protocol version by design:
`{"session":"UUID","kind":"heartbeat"}` every two seconds, and
`{"session":"UUID","kind":"disconnect"}` on disposal/pagehide. Native validates
kind/UUID and caps notices at 256 characters. A 15-second lease covers abrupt
disconnects; host handler/window destruction must also clean up. Lease expiry
does not resume old work. Reload creates a fresh session.

The reference host allows 16 simultaneous calls and a 20-second native
deadline. SDK frontend waits have a 23-second deadline and signal disconnect on
timeout. Explicit `Register(..., interactive: true)` metadata opts only native
dialogs and file selection into a 300000 ms budget; the handshake advertises
their names and budget, and the SDK allows 3000 additional transport milliseconds.
Older hosts without interactive metadata retain the default timeout. Invalid,
unregistered or unbounded interactive policies are rejected by the client.
An active interactive operation holds the native heartbeat lease while OS UI
pauses browser timers. On completion, a fresh bounded lease starts. Explicit
disconnect, cancellation and window/scope destruction still retire it immediately.
Native handlers that lack cancellation must not delay the hub's cancelled
response or apply their late result to a new document. Stock MAUI dialogs/pickers
cannot be force-dismissed through their public API; dismiss remaining OS UI yourself.

Cancel is an additional correlated native call, not Promise abortion
or an undo guarantee. A cancelled request has explicit cancellation state;
service state and completed OS writes can survive a frontend reload.

Provider-owned `remote.changed` and `menus.clicked` notifications can originate
outside an active command. The scoped source/hub validates current document,
registered topic and subscription before emitting an independent UUID. Existing
operation progress publishing still requires its originating active request.
Native menu OS UI may hold only the current document lease for a maximum five
minutes; reset/disposal retires that hold. No arbitrary callback or JS expression
is accepted by these commands.

Cancelling a frontend promise does not abort the framework transport. The shared
SDK client waits for an old handshake's transport completion before sending a
replacement handshake, and signals disconnect again if that completion belongs
to a disposed client. Shared startup has a 28-second overall deadline; an
unsettled predecessor requires reload, not overlapping session establishment.

## Compatibility boundaries

The current native reference targets repository SDK
`11.0.100-rtm.26480.113` and MAUI `net11.0`. Platform heads are not evidence that
every provider has been validated on every OS. Custom hosts must preserve this
wire contract and lifecycle or coordinate a protocol/package version change.
The sibling `Maui.HybridApp` library is independently packable locally with an
unverified working identity, not a published official NuGet release.
There is no source-generated TypeScript catalog,
automatic provider discovery or template in this preview.
