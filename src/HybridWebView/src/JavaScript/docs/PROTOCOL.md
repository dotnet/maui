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
  InvokeDotNetBinary?(method: string, payload: ArrayBuffer | ArrayBufferView,
    options?: { headers?: Record<string, string>; signal?: AbortSignal }): Promise<ArrayBuffer>
  SendRawMessage(message: string): void
}
```

The SDK calls **`InvokeDotNet('Invoke', [asciiEscapedRequest])`**, with one string
argument; the native target's only forwarding method is
`Task<string> Invoke(string request)`. The reference host registers its narrow
`IHybridAppTarget` with
`SetInvokeJavaScriptTarget<IHybridAppTarget>(hub, ProtocolJsonContext.Default)`
in `Native/MainPage.cs` in the source bundle. For tarball readers,
[MainPage.cs](https://github.com/dotnet/maui/blob/f0c36030b40a35daaae7256df2b79e1058774e82/src/HybridWebView/samples/HybridWebApp/Native/MainPage.cs)
pins the matching scoped host and separate product/identity wire mapping.
Refresh source links to the selected release commit/tag before publication.
The .NET 11 invocation source generator and explicit System.Text.Json
`JsonTypeInfo<T>` metadata are required by that host. Installing the npm package
does not add this registration or a native command hub.

## Envelopes (camelCase)

### Binary actions and ordered channels

The host explicitly assigns `BinaryInvoker = new HybridAppBinaryInvoker(hub)`
before document boot. Only `hybrid.invoke` is approved. The SDK calls
`InvokeDotNetBinary('hybrid.invoke', payload, { headers, signal })`.
`headers` contains **1–10** consecutive `maui-request-00` … `maui-request-09`
values, each **1–240 printable ASCII characters**. Concatenating their values
reconstructs the **same protocol-1 request** below, limited to **2400 ASCII-escaped
characters**, containing version/session/id/command/args and **no byte payload**.
Unknown/missing/extra fragments reject. Core's separate metadata envelope remains
bounded (16 header entries, names ≤64, values ≤256, decoded JSON ≤8 KiB).
No JSON/base64 success bytes or arbitrary stream serialization are used.

The hub rejects binary connect; establish core.connect on JSON first. It shares
the same authorization, replay cache, 16 ordinary-operation budget and registered action
policies. Core separately bounds inputs/results to 4 MiB, active transfers to
four and their lifetime to 30 seconds. Android transparently fragments upload
under that Core primitive; SDK actions still receive/return whole byte buffers.
The hub reserves four additional control slots. Ordinary binary actions are
limited to three, leaving one Core slot for channel data fetches. The SDK
serializes channel fetch/ack/close and cancellation through a 64-call control
queue; normal topic unsubscribe keeps its existing immediate dispatch.
The adapter throws genuine Core errors with a bounded `{code,message}` JSON
exception message; the SDK maps that rejection to NativeError. A successful
ArrayBuffer can never be a coded-error fallback.

Channel arguments serialize to UUIDs, bound by generated native HybridChannel<T>
parameters. Internal `core.channel` events do **not** require topic subscription:
their session and event `id` identify the exact owning action. Event `data` has
`channelId`, increasing `sequence`, `kind` (`json`, `bytes`, `complete`, `error`,
`cancelled`), optional JSON `value`, raw `dataId`, or terminal `code`. At most four
channels/action and one message in flight/channel; 16 KiB JSON or 64 KiB raw.
Terminal notifications also consume the next sequence.

`core.channel.fetch` is a binary action with empty input and structured args
`{channelId,ownerId,dataId,sequence}`; result is exactly the chunk bytes. Fetch is
single-consumption. `core.channel.ack` is JSON with
`{channelId,ownerId,sequence,accepted}`; accepted true is valid only after any
raw fetch and awaited JS callback, accepted false fails the send with
`consumer_error`. `core.channel.close` is JSON `{channelId,ownerId}`. Unknown,
late/replayed, wrong-owner/session or premature requests reject
`invalid_channel`, never obtain another owner's bytes. Disposal/unsubscribe and
owner completion/error/cancel drop pending callbacks, byte data and ack waiters.
Channels do not survive command lifetime.

Selected-file `file.readBytes` now uses this binary invocation with empty input
and `{id,count}` args. Result is one explicit EOF byte (0 or 1) followed by
0–count bytes (count ≤65536). The SDK removes that **resource-specific** marker;
ordinary byte-result actions have no SDK prefix. This retains exact partial and
nonseekable EOF while reusing safe FileResult authority, serialized reader
read/seek/close and strict incremental UTF-8. No arbitrary path/writer exists.

### JSON request and response

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
| `core.connect` | `{}` | Commands, clipboard/share text limit; optional interactiveCommands / interactiveTimeoutMilliseconds / initialLocation | Session establishment/retirement |
| `core.cancel` | `{ requestId: string }` | `{ accepted: boolean }` | Cooperative request cancellation |
| `core.subscribe`, `core.unsubscribe` | `{ topic: string }` | `{ accepted: boolean }` | Explicit allowed topic list |
| `app.info` | `{}` | `{ appName: string, version: string, build: string }` | Independently registered IAppInfo provider |
| `device.info` | `{}` | `{ platform: string, osVersion: string, model: string }` | Independently registered IDeviceInfo provider |
| `clipboard.read` | `{}` | `string \| null` | Curated native `IClipboard.GetTextAsync` |
| `clipboard.write` | `{ text: string \| null }` | `null` (void completion) | Curated native `IClipboard.SetTextAsync`, host text budget |
| `clipboard.hasText` | `{}` | `boolean` | Curated native `IClipboard.HasText` accessor |
| `preferences.get`, `preferences.set` | actual key + typed primitive/defaultValue + optional sharedName | Typed native primitive, `{ accepted }` | Closed scalar adapter over native generic IPreferences; keys/containers unchanged |
| `preferences.contains`, `preferences.remove`, `preferences.clear` | actual key/container or container only | `{ accepted: boolean }` | Clear calls native container-wide Clear, including native-existing settings |
| `battery.info` | `{}` | chargeLevel/state/powerSource/energySaverStatus | IBattery on every platform; the Catalyst IOKit correction now belongs to Essentials itself |
| `connectivity.info` | `{}` | networkAccess/connectionProfiles | Actual IConnectivity snapshot, not an endpoint reachability test |
| `speech.locales` | `{}` | `Locale[]` | Curated annotated native ITextToSpeech.GetLocalesAsync, no result envelope |
| `speech.speak` | text + optional nullable native SpeechOptions | `null` (void completion) | Curated ITextToSpeech.SpeakAsync; destination validates values; interactive deadline and operation-specific cancellation |
| `launcher.openUrl` | `{ url }` | `{ accepted }` | Actual OS external launch; bounded public HTTPS policy, not arbitrary schemes/paths |
| `display.info` | `{}` | width/height/density/orientation/rotation/refreshRate | Direct IDeviceDisplay snapshot |
| `filePicker.pick`, `filePicker.pickMultiple` | pickerTitle/platform fileTypes | nullable file metadata/`{ files }` resources | Actual OS selection, interactive policy; cancellation null/empty |
| `file.openRead`, `file.release` | Selected resource ID | Reader resource / `{ accepted }` | FileResult.OpenReadAsync, scope/document authority |
| `file.readBytes`, `file.seek`, `file.closeReader` | Reader ID + count/position | raw binary EOF byte + bytes, or JSON acceptance | ≤65536 file bytes per read; real partial/EOF/seek/close |
| `dialogs.alert`, `dialogs.confirm` | title/message + required native labels/flowDirection | `null` / `boolean` | Curated calling Page methods and interactive ownership |
| `dialogs.prompt`, `dialogs.actionSheet` | Native prompt/action-sheet parameters; prompt keyboard is null, `{ name }` or `{ flags }` | Native `string \| null` text / selection | Curated Page methods; declared strict Keyboard value converter |
| `window.flowDirection` | `{}` | Native flow-direction enum name | Curated contextual Core `IWindow` getter |
| `geolocation.location` | `{ request: GeolocationRequest }` | Actual native `Location \| null` | Curated native interface, permission/timeout/owning interaction policy |
| `share.stage` | `{fileName, contentType}` plus raw binary bytes | Small UTF-8 JSON resource metadata as bytes | Bounded app-owned staging, no native paths |
| `share.request` | `{request: native text/file/multi-file shape}` | `{ accepted }` | Explicit .NET 11 structural union and authorized nested file converter |
| `share.release` | `{id}` | `{ accepted }` | Revokes document authority without deleting young OS-retained files |
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
own automatically generated action codecs; the protocol context contains only envelopes and
control DTOs. External Toolkit assemblies opt into their one compiler-generated
assembly extension, without global metadata edits or runtime assembly scanning.
Methods/accessors declare metadata with `[Microsoft.Maui.Interop.HybridAction]`;
referenced contracts require explicit assembly `[HybridExport(typeof(T))]` roots.
These attributes live below the optional runtime in existing Essentials; they
do not register bridge services themselves. Ordinary parameters bind directly
from named objects or positional arrays, not per-operation Args wrappers.
Class `[HybridTopic]` declarations expose progress/notification topics, while
paired `[HybridEvent(topic, Start/Stop)]` methods bind first/last subscription lifetimes.
The host selects the existing Essentials interfaces. Speech and preferences
retain JSON/schema/finite representation checks and transport limits, not business
text/range/key/locale quotas. Preference keys and shared containers intentionally
share actual native storage. The reference host also registers application-only
`sample.*` commands and `service.changed` / `operation.progress` topics; those
helpers remain in the playground and are **not** npm SDK features.

Product notification topics include `clipboard.changed`, `window.geometryChanged`,
`application.themeChanged`, `menus.clicked` and `remote.changed`. They originate
from typed provider-owned sources and use current-document/subscription checks.
First/last subscriptions attach/detach native sources; JS listeners share a topic
multiplexer. Progress retains its separate active originating-operation validation.
Direct native empty events, including `IClipboard.ClipboardContentChanged`,
carry JSON null, not an acknowledgement object. `Application.RequestedThemeChanged`
carries the actual `AppThemeChangedEventArgs` shape, for example
`{ "requestedTheme": "dark" }`; this is distinct from the effective/preference
snapshot returned by theme read/write commands. Generated bindings omit the
conventional sender. Multiple payload parameters use their declared names.
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

The reference host allows 16 ordinary simultaneous calls, four reserved control
calls, and a 20-second native
deadline. SDK frontend waits have a 23-second deadline and signal disconnect on
timeout. Explicit `[HybridAction(..., Interactive = true)]` metadata opts only native
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

Closed enums use exact native camelCase strings, not numeric values or case-insensitive
names. Flags use arrays of declared names; zero/all stand alone and undefined bits
reject. Nullable/default enum contracts preserve null/omission semantics.
Native theme `unspecified` replaces the former preview-only `system` mapping.
Battery/display/connectivity names likewise use native camelCase (`charging`,
`portrait`, `internet`, `wiFi`). Browser navigation uses a closed `status`, separate
nullable `error` text and typed `navigationResult`. Update the matching native host
and frontend together; these unpublished preview wire changes do not change protocol
1 envelopes, resource authority or guest isolation.

The current native reference targets repository SDK
`11.0.100-rtm.26480.113` and MAUI `net11.0`. Platform heads are not evidence that
every provider has been validated on every OS. Custom hosts must preserve this
wire contract and lifecycle or coordinate a protocol/package version change.
The sibling `Microsoft.Maui.Controls.HybridWebApps` library is independently packable locally with an
unverified working identity, not a published official NuGet release.
There is no source-generated TypeScript catalog,
automatic provider discovery or template in this preview.
