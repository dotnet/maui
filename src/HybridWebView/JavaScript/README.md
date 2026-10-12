# .NET MAUI JavaScript API

`@maui/api` is an app-independent, experimental JavaScript/TypeScript SDK in the
.NET MAUI project. It provides typed native commands, document-scoped events
and small feature modules for app information, clipboard and preferences.

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
import { clipboard } from '@maui/api/essentials/clipboard'
import { preferences } from '@maui/api/essentials/preferences'
import { getInfo } from '@maui/api/app/info'

// Each feature lazily connects the shared document client on its first call.
const identity = await getInfo()
await clipboard().writeText(`Hello from ${identity.appName} — 你好 👋`)
const text = await clipboard().readText() // string | null, from the native OS
await preferences().set('theme', 'dark') // host-owned namespace and restrictions
```

No view object is needed in ordinary application code. A root import is also
available: `import { connect, disconnect, clipboard, getInfo } from '@maui/api'`.
Importing modules or creating a feature helper performs no native calls. An
ordinary browser/Node process has no native host: asynchronous feature calls
reject with `NativeError` code `native_unavailable`; they never fall back to web
clipboard/storage or fake identity. Handle that rejection in your UI.

## Matching native host required

**Stock .NET MAUI 11 JavaScript invocation does not implement this SDK's command
hub protocol automatically.** Installing this npm artifact alone does not add
providers, permissions or native registrations to an app. A compatible host must
implement version 1 of [the protocol](docs/PROTOCOL.md), including
`Task<string> Invoke(string request)`, session commands and explicit providers.

The current source-backed reference host lives in
[`Controls.Sample.HybridWebApp/Native`](https://github.com/dotnet/maui/tree/9ad591d8f6122d3ab96adcb1cbeda63a4c9b3773/src/Controls/samples/Controls.Sample.HybridWebApp/Native):

- [`MainPage.cs`](https://github.com/dotnet/maui/blob/9ad591d8f6122d3ab96adcb1cbeda63a4c9b3773/src/Controls/samples/Controls.Sample.HybridWebApp/Native/MainPage.cs)
  registers `SetInvokeJavaScriptTarget<IHybridAppTarget>(hub, PlaygroundJsonContext.Default)`,
  handles raw document messages and sends session-checked events.
- [`Contracts.cs`](https://github.com/dotnet/maui/blob/9ad591d8f6122d3ab96adcb1cbeda63a4c9b3773/src/Controls/samples/Controls.Sample.HybridWebApp/Native/Playground/Contracts.cs),
  [`HybridAppTargetObject.cs`](https://github.com/dotnet/maui/blob/9ad591d8f6122d3ab96adcb1cbeda63a4c9b3773/src/Controls/samples/Controls.Sample.HybridWebApp/Native/Playground/HybridAppTargetObject.cs)
  and [`CommandRegistry.cs`](https://github.com/dotnet/maui/blob/9ad591d8f6122d3ab96adcb1cbeda63a4c9b3773/src/Controls/samples/Controls.Sample.HybridWebApp/Native/Playground/CommandRegistry.cs)
  define the versioned, source-generated JSON contracts and bounded allowlist.
- [`Providers.cs`](https://github.com/dotnet/maui/blob/9ad591d8f6122d3ab96adcb1cbeda63a4c9b3773/src/Controls/samples/Controls.Sample.HybridWebApp/Native/Playground/Providers.cs)
  registers identity/clipboard/preferences;
  [`MauiPlaygroundPlatform.cs`](https://github.com/dotnet/maui/blob/9ad591d8f6122d3ab96adcb1cbeda63a4c9b3773/src/Controls/samples/Controls.Sample.HybridWebApp/Native/Playground/MauiPlaygroundPlatform.cs)
  implements OS calls.

No companion NuGet SDK or integration template is shipped by this extraction.
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

For custom transports/tests, construct `new NativeClient(bridge)`, attach it with
`attachToDocument()` in a browser, and `await client.connect()` before calling
commands. Pass it explicitly to helpers, e.g. `clipboard(client)`. Always call
`client.dispose()`. Injection never replaces production APIs with silent mocks.

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
[playground README](https://github.com/dotnet/maui/blob/b9809fe83750572310c7a871c36a0fc6aaac8395/src/Controls/samples/Controls.Sample.HybridWebApp/README.md)
documents this repository's consumer and explicit asset staging.
