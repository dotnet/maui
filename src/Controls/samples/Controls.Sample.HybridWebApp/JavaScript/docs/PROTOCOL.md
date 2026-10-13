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
[MainPage.cs](https://github.com/dotnet/maui/blob/7b631a9f255d7b69e74e90746df43cd70934b258/src/Controls/samples/Controls.Sample.HybridWebApp/Native/MainPage.cs)
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
| `core.connect` | `{}` | `{ commands: string[], preferenceNamespace: string, maximumTextLength: number }` | Session establishment/retirement |
| `core.cancel` | `{ requestId: string }` | `{ accepted: boolean }` | Cooperative request cancellation |
| `core.subscribe`, `core.unsubscribe` | `{ topic: string }` | `{ accepted: boolean }` | Explicit allowed topic list |
| `app.info` | `{}` | `{ appName: string, version: string, build: string }` | Independently registered IAppInfo provider |
| `device.info` | `{}` | `{ platform: string, osVersion: string, model: string }` | Independently registered IDeviceInfo provider |
| `clipboard.read` | `{}` | `{ text: string \| null }` | Clipboard provider |
| `clipboard.write` | `{ text: string }` | `{ accepted: boolean }` | Clipboard provider |
| `preferences.read`, `preferences.remove` | `{ key: string }` | `{ key: string, value: string \| null }` | Namespace-restricted settings provider |
| `preferences.write` | `{ key: string, value: string }` | Same preference result | Same provider |

Current source is the sibling `Managed` class library (`Protocol/`,
`CommandRegistry.cs`, `HybridAppTargetObject.cs`, `Essentials/*`) plus the `Native`
host app. The library has no Core/Controls or sample dependency. Feature providers
own their generated contexts; the protocol context contains only envelopes and
control DTOs. External Toolkit assemblies register their own commands/topics and
strict generated `JsonTypeInfo<T>` explicitly, without global metadata edits.
The host, not either SDK, selects its preferences
namespace and the existing Essentials interfaces. The reference host also registers application-only
`sample.*` commands and `service.changed` / `operation.progress` topics; those
helpers remain in the playground and are **not** npm SDK features.

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
timeout. Cancel is an additional correlated native call, not Promise abortion
or an undo guarantee. A cancelled request has explicit cancellation state;
service state and completed OS writes can survive a frontend reload.

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
