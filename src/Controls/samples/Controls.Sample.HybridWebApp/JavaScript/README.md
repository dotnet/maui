# .NET MAUI JavaScript API

`@maui/api` is an app-independent, experimental JavaScript/TypeScript SDK in the
.NET MAUI project. It provides typed native commands, document-scoped events
and separate Essentials / MAUI UI feature modules for app/device information,
clipboard, preferences, battery/display, native connectivity events, cancellable
text-to-speech, external HTTPS launching, selected-file/read-stream resources, page-owned
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
await set('theme', 'dark') // host-owned namespace and restrictions
```

No view object is needed in ordinary application code. The root contains only
infrastructure/lifecycle APIs and core types:
`import { connect, disconnect, NativeClient, NativeError } from '@maui/api'`.
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
[`Controls.Sample.HybridWebApp`](https://github.com/dotnet/maui/tree/370c152eb668c9911c94c6d3eb7e01e13a48bb10/src/Controls/samples/Controls.Sample.HybridWebApp).
`AddHybridApp` registers the product infrastructure and implemented Essentials/UI
providers; the host adds sample or external providers explicitly.
The single WIP managed package now depends on Controls.Core/Core for reusable UI
providers. Its earlier Essentials-only boundary has intentionally changed; it
still has no sample dependency and no second integration NuGet.

- [`MainPage.cs`](https://github.com/dotnet/maui/blob/370c152eb668c9911c94c6d3eb7e01e13a48bb10/src/Controls/samples/Controls.Sample.HybridWebApp/Native/MainPage.cs)
  attaches `SetInvokeJavaScriptTarget<IHybridAppTarget>(hub, ProtocolJsonContext.Default)`,
  handles raw document messages and sends session-checked events.
- [`Managed`](https://github.com/dotnet/maui/tree/370c152eb668c9911c94c6d3eb7e01e13a48bb10/src/Controls/samples/Controls.Sample.HybridWebApp/Managed)
  owns protocol/control metadata, the bounded hub, registry and DI registration.
- [`Managed/Essentials`](https://github.com/dotnet/maui/tree/370c152eb668c9911c94c6d3eb7e01e13a48bb10/src/Controls/samples/Controls.Sample.HybridWebApp/Managed/Essentials)
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
[matching playground README](https://github.com/dotnet/maui/blob/370c152eb668c9911c94c6d3eb7e01e13a48bb10/src/Controls/samples/Controls.Sample.HybridWebApp/README.md)
documents the consumer and staging. In the current source bundle the independently
packable SDK is at `JavaScript/` beside `Native/`, `Web/` and `Tests/`; source
commands here use that relocated layout. Refresh source links to the matching
release commit/tag before publication.
