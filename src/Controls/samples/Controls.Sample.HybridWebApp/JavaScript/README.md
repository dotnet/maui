# .NET MAUI JavaScript API

`@maui/api` is an app-independent, experimental JavaScript/TypeScript SDK in the
.NET MAUI project. It provides typed native commands, document-scoped events
and separate Essentials / MAUI UI feature modules for app/device information,
clipboard, preferences, battery/display, native connectivity events, cancellable
text-to-speech, external HTTPS launching, actual native geolocation, bounded native
text/file/multi-file sharing, selected-file/read-stream resources, page-owned
dialogs and scoped windows.

**Preview availability:** `0.1.0-preview.1` is prepared for local tarball
installation, not yet published to npm. The `@maui` scope and package name must
be confirmed by the release maintainer before registry publication.

The shipping unit is an npm tarball containing ESM JavaScript, TypeScript
declarations, API/protocol/release documentation and the repository MIT license.
It has no runtime dependencies, UI, Vite requirement, sample commands, native
code, browser emulation or baked-in preferences namespace. The playground is a
consumer, not the package's runtime or distribution format.

## Install and use

For this prepared preview, obtain a locally built tarball from a maintainer and
install it in your application's frontend:

```sh
npm install ./maui-api-0.1.0-preview.1.tgz
```

After an authorized release exists, the intended registry install is
`npm install @maui/api@0.1.0-preview.1`. That command is **not evidence of an
existing release**. See [release preparation](docs/RELEASING.md).

```ts
import { getText, setText } from '@maui/api/essentials/clipboard'
import { set } from '@maui/api/essentials/preferences'
import { getAppInfo } from '@maui/api/essentials/app-info'
import { getDeviceInfo } from '@maui/api/essentials/device-info'

// Each feature lazily connects the shared document client on its first call.
const app = await getAppInfo()
const device = await getDeviceInfo()
await setText(`Hello from ${app.appName} on ${device.platform} — 你好 👋`)
const text = await getText() // string | null, from the native OS
await set('theme', 'dark') // actual native key; storage is shared with native code
```

No view object is needed in ordinary application code. The root contains only
infrastructure/lifecycle APIs and core types:
`import { connect, disconnect, invoke, invokeBinary, Channel, binaryBlob, NativeClient, NativeError } from '@maui/api'`.
Essentials helpers and their DTOs belong to explicit `essentials/*` subpaths;
page/window helpers belong to `ui/*`.
These are module boundaries in **one npm package**, not separately published
packages. Features depend on the core client contract, not the playground or
Controls, leaving room for future packaging choices without introducing them now.
Importing modules or acquiring a local currentWindow handle performs no native calls. An
ordinary browser/Node process without an explicitly injected native transport has
no native host: asynchronous default-client feature calls
reject with `NativeError` code `native_unavailable`; they never fall back to web
clipboard/storage or fake app/device data. Handle that rejection in your UI.

Native SDK members are deliberately curated by metadata in the existing
Essentials assembly (`Microsoft.Maui.Interop`), below the optional integration
runtime. The managed generator imports only explicitly rooted annotated
contracts; annotations alone expose nothing. The matching managed host creates
one local provider overlay per view over its owning business-service scope.
Clipboard, text-to-speech and routine dialogs now decode actual native scalar/array/void results,
not the previous preview's wrapper DTOs; no compatibility aliases are retained.
All four dialogs invoke annotated Page methods directly. Prompt resolves to
`string | null`; null is dismissal. Keyboard is a serialized native value:
`{ name: 'text' }` selects a singleton and `{ flags: ['spellcheck'] }` uses
`Keyboard.Create`; null/omission preserves native defaults. These names do not
represent a native Keyboard enum. Labels, maxLength and initial text are not
pre-validated or limited by the SDK; Page owns their semantics.

Text-to-speech imports the annotated `ITextToSpeech` directly: native
`SpeechOptions` and `Locale` map without wrappers, locales return an array, and
`speak(...).result` completes with native void. Speech values pass unchanged for
Essentials validation, without SDK text/range/locale policy.
Preferences adapt only generic native scalar types; keys, strings and shared
container names are not prefixed or quota-limited. `clear()` invokes destructive
native container-wide Clear, including settings created outside JavaScript.

```ts
import { stageFile, share, releaseFile } from '@maui/api/essentials/share'
import { getLocation } from '@maui/api/essentials/geolocation'

const file = await stageFile('browser.pdf', 'application/pdf', browserProducedPdfBytes)
await share({ file, title: 'Share browser document' }) // OS UI, not a browser download
await releaseFile(file) // revoke handle; OS files retain their finite reading window
const request = await getLocation({ desiredAccuracy: 'medium', timeout: '00:00:20' })
const location = await request.result // actual Location or null; never a web fallback
```

Sharing stages raw bytes separately from the longer interactive JSON request.
Native files remain opaque document-owned resources, not accepted browser paths.
Chooser completion (especially Android) is not recipient consumption. See
[policy and retention](docs/API.md#native-sharing-and-location) before use.

This **unpublished preview was intentionally reshaped**: the old `app/info`,
`getInfo` and SDK `Identity` exports were removed, not aliased. AppInfo exposes
only native appName/version/build; DeviceInfo exposes platform/osVersion/model.
They use independently registered `app.info` and `device.info` commands.
Process architecture, PID, startup UUID and timestamp are playground-owned
diagnostics from `sample.diagnostics`, not Essentials. This unpublished preview's
internal wire split requires the matching managed host; the public JavaScript
module names and result types are unchanged.

## Source documentation and generated reference

The public TypeScript symbols and members carry TSDoc comments that survive in
the shipped `.d.ts` files, so editor tooltips include parameters, return values,
errors, host requirements and session/limit notes without opening this README.
Generate a local Markdown reference and DocFX navigation from those declarations
from the source bundle root (`Controls.Sample.HybridWebApp/`):

```sh
npm --prefix JavaScript run docs
```

Open `JavaScript/artifacts/api-reference/index.md`. API Extractor
and API Documenter process each actual package entrypoint separately and label
its exact import path; feature APIs are not flattened into the root to generate
docs. Generated reference output is ignored and not shipped in the npm tarball.
See [the documentation/DocFX workflow](docs/RELEASING.md#source-reference-and-docfxmicrosoft-learn)
for tool compatibility and Microsoft Learn onboarding requirements. Generating
these files does **not** publish a Microsoft Learn page.

## Matching native host required

**Stock .NET MAUI 11 JavaScript invocation does not implement this SDK's command
hub protocol automatically.** Installing this npm artifact alone does not add
providers, permissions or native registrations to an app. A compatible host must
implement version 1 of [the protocol](docs/PROTOCOL.md), including
`Task<string> Invoke(string request)`, session commands and explicit providers.

The current reference host is the sibling `Native/`, referencing the independently
packable `Managed/Maui.HybridApp.csproj` in the source bundle. The links below pin
the matching product, scoped host and identity wire mapping:
[`Controls.Sample.HybridWebApp`](https://github.com/dotnet/maui/tree/653b2573265ad2c29140911e4bab4112975d5185/src/Controls/samples/Controls.Sample.HybridWebApp).
`AddHybridApp` registers the product infrastructure and implemented Essentials/UI
providers; the host adds sample or external providers explicitly.
The single WIP managed package now depends on Controls.Core/Core for reusable UI
providers. Its earlier Essentials-only boundary has intentionally changed; it
still has no sample dependency and no second integration NuGet.

- [`MainPage.cs`](https://github.com/dotnet/maui/blob/653b2573265ad2c29140911e4bab4112975d5185/src/Controls/samples/Controls.Sample.HybridWebApp/Native/MainPage.cs)
  attaches `SetInvokeJavaScriptTarget<IHybridAppTarget>(hub, ProtocolJsonContext.Default)`,
  handles raw document messages and sends session-checked events.
- [`Managed`](https://github.com/dotnet/maui/tree/653b2573265ad2c29140911e4bab4112975d5185/src/Controls/samples/Controls.Sample.HybridWebApp/Managed)
  owns protocol/control metadata, the bounded hub, registry and DI registration.
- [`Managed/Essentials`](https://github.com/dotnet/maui/tree/653b2573265ad2c29140911e4bab4112975d5185/src/Controls/samples/Controls.Sample.HybridWebApp/Managed/Essentials)
  contains one cohesive provider file per API using existing Essentials interfaces
  and feature-owned metadata.
- `Native/Playground` owns every sample command/topic and process diagnostic.

Refresh published source links to the selected release commit/tag before publication.
The WIP `Maui.HybridApp` NuGet identity is independently packable locally, but
unverified and unpublished. It is not included in the npm artifact.
No official companion NuGet release or integration template is claimed.
A future template could own bootstrap, UI and host registration so developers
rarely interact with the transport; that is a future integration, not a current
deliverable. The npm SDK itself is a standalone product artifact.

## Lifetime

### Whole buffers, ordered channels and read handles

The matching host must explicitly set
`HybridWebView.BinaryInvoker = new HybridAppBinaryInvoker(hub)` **before boot**.
Installing this npm package does not opt in stock Core or grant native methods.
The WIP requires the new Core binary capability; missing opt-in rejects
`binary_unavailable`, never silently falling back to JSON/base64.

```ts
import { invoke, invokeBinary, Channel, binaryBlob } from '@maui/api'
import { number, object } from '@maui/api/core'

// These routes must be attributed and explicitly registered by your own host.
const input = Uint8Array.from({ length: 262144 }, (_, i) => i % 256)
const echoed = await invokeBinary('documents.echo', { label: 'example' }, new Blob([input.buffer]))
const documentBytes = await invokeBinary('documents.pdf')
const pdf = binaryBlob(documentBytes, 'application/pdf') // MIME is known metadata, not inferred from bytes
const chunks = Channel.bytes(async bytes => { await consumeBytes(bytes) })
const updates = new Channel(value => number(object(value).index), async index => { console.log(index) })
await invoke('documents.chunks', { chunks, progress: updates })
await Promise.all([chunks.completion, updates.completion])
```

`consumeBytes` above is an application function. `invokeBinary(command, args?,
payload?, { signal }?)` accepts Blob, ArrayBuffer or ArrayBufferView input and
returns Uint8Array; omitted payload is empty. Structured args are bounded to
2400 ASCII-escaped request characters, split into at most ten 240-character
metadata headers. Whole input/output is at most 4 MiB. The same hub session,
16 ordinary-operation, replay and cancellation policies apply, with four reserved
native control slots. Three ordinary binary actions leave one Core slot for
serialized channel fetches; the control queue is bounded to 64 calls. Core has
its own four-transfer and 30-second limit. No global server, arbitrary Stream, binary
JSON arrays or transport compatibility alias exists.

Channels are single-use and **end when their owning action ends**, not durable
background subscriptions. Native allows four per action and one send in flight
per channel; raw chunks are at most 64 KiB and JSON messages 16 KiB. Native
SendAsync waits until JS has fetched raw bytes, decoded and **awaited** the
consumer, then sent an explicit consumption acknowledgement. Sequence, owner,
session and single-use data IDs are checked. Throwing consumers fail with
`consumer_error` without leaving the native send blocked. `unsubscribe()`,
disconnect, reload, cancellation/deadline and completion clear pending state.
An old channel cannot be rebound to a new document.

Existing selected-file FileReader handles reuse the binary path for readBytes
and strict incremental UTF-8 readText. Their 64 KiB reads, partial/EOF behavior,
seek/reset, serialized calls and close/owner teardown are unchanged. Files stay
read-only; no arbitrary path/write or OS share support is claimed.

```ts
import { connect, disconnect } from '@maui/api'
import type { Decoder } from '@maui/api'

const decodeEvent: Decoder<boolean> = value => {
  if (typeof value !== 'boolean') throw new Error('Expected a boolean event')
  return value
}
const client = await connect() // optional explicit startup; performs core.connect
const unsubscribe = await client.subscribe('my.topic', decodeEvent, (value, id) => {
  console.log(value, id)
})
// ... topic must be registered by your host
await unsubscribe()
disconnect() // pending calls cancelled, listeners/heartbeat/subscriptions cleaned up
```

The decoder validates the host's event shape; the listener receives its
validated value and originating request ID. Default feature calls share startup.
Startup failures are retriable. Successful startup attaches one document event
listener, a `pagehide` listener and a two-second heartbeat. Dispose on application
teardown; with Vite, use
`if (import.meta.hot) import.meta.hot.dispose(() => disconnect())`.
Vite is not required by the SDK.

After disconnect, a later call creates a new session. If an old handshake is
still in flight, replacement startup waits for it and cleans up its late
session; cancellation cannot abort a request already sent to native. Shared
startup is bounded to 28 seconds, including bridge loading and that wait.
There should be only one active client per WebView document: connecting another
retires the old session.
After suspension, lease expiry or back/forward-cache restoration, reload the
frontend to reconnect. Do not reuse an explicitly disposed `NativeClient`.

For custom transports/tests, configure the connection once through
`configureBridge(bridge)` in the infrastructure `core` module before startup.
All stock feature calls then use that same document connection; no feature
accepts a NativeClient parameter. Call `resetBridge()` at test teardown.
The advanced NativeClient constructor remains available for custom commands,
not as a second client for stock features. Injection never silently emulates native APIs.

### Explicit transport without MAUI automatic bootstrap

Automatic `connect()`/`loadNativeBridge()` is specifically a MAUI HybridWebView
integration: it recognizes MAUI's virtual origin and loads MAUI's framework
script. An explicit `NativeClient(transport)` does neither. A compatible native
protocol transport can be supplied on another origin or in a non-Controls host:

```ts
import { connect, disconnect } from '@maui/api'
import { configureBridge } from '@maui/api/core'
import type { Bridge } from '@maui/api'
import { getAppInfo } from '@maui/api/essentials/app-info'
import { set } from '@maui/api/essentials/preferences'

// The integration supplies a real transport implementing protocol 1, not a mock.
async function useNative(transport: Bridge) {
  configureBridge(transport)
  try {
    await connect() // one shared handshake; no automatic origin/script load for this transport
    const app = await getAppInfo()
    await set('theme', 'dark')
    return app
  } finally { disconnect() }
}
```

This preserves the same NativeError, runtime decoding, request limits,
cancellation and document-session semantics. The host must still register the
commands and own provider/permission policy; injection is not browser emulation
or permission bypass. The integration owns event delivery and document lease:
use `attachToDocument()` when supplying the framework-compatible browser events,
or call the connected client's `receive()` and manage heartbeats/teardown in a headless/custom host.
There are no npm dependencies on .NET MAUI Core or Controls, and no alternative
transport implementation/framework is shipped here.

## Compatibility and support

- **Experimental preview**, with no stable API/wire compatibility promise yet.
  Helpers are authored TypeScript, not generated from native DTOs.
- Emitted ESM targets ES2022; browsers need `crypto.randomUUID` in a secure
  context, promises, DOM events and timers. CommonJS is not provided.
  Importing from Node/SSR is safe; Node is not a native MAUI host.
- Built/tested using TypeScript **7.0.2**, Node **24.4.1**, npm **11.5.2**;
  package development requires Node **22.18+**. Node's type stripping is used
  only for the playground's own tests, not for package consumption.
- Native reference target: **.NET MAUI 11**, repository SDK
  **11.0.100-rtm.26480.113**, plus the protocol host above. This is not a promise
  of compatibility with every .NET 11 servicing/preview build.
- The reference app has Android 24+, iOS 15+, Mac Catalyst 17+ and Windows
  10.0.17763+ heads. Package/consumer tests do not constitute native validation
  on all those platforms, signing/store readiness or blanket Essentials support.
- Bridge bootstrap recognizes only `app://0.0.0.1` and `https://0.0.0.1` with no
  explicit port, and loads the framework-owned `/_framework/hybridwebview.js`.
  Other hosts must use explicit `Bridge` injection and matching contracts.

Commands are available only when opted in by the host. Neither TypeScript
declarations nor connect metadata grants OS permission. Clipboard may be
restricted by platform policy; preferences are plain app settings, not secure
storage. Package only trusted content, keep development servers loopback-only,
and never expose arbitrary native methods to untrusted scripts.

See [API/errors/security](docs/API.md), [wire/native compatibility](docs/PROTOCOL.md),
and [package validation/release](docs/RELEASING.md). The
[matching playground README](https://github.com/dotnet/maui/blob/653b2573265ad2c29140911e4bab4112975d5185/src/Controls/samples/Controls.Sample.HybridWebApp/README.md)
documents the consumer and staging. In the current source bundle the independently
packable SDK is at `JavaScript/` beside `Native/`, `Web/` and `Tests/`; source
commands here use that relocated layout. Refresh source links to the matching
release commit/tag before publication.
