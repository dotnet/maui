# Public API and operational behavior

All subpaths resolve to built `.js` and `.d.ts` files, never original `.ts`.
There are no wildcard/private-source exports.

| Import | Exports |
| --- | --- |
| `@maui/api` | `connect`, `disconnect`, `NativeClient`, `NativeError`, `clipboard`, `preferences`, `getInfo`; types `Bridge`, `Json`, `Decoder`, `Ready`, `Operation`, `Progress`, `Identity` |
| `@maui/api/core` | Transport/lifetime API above; `loadNativeBridge`, `isNativeOrigin`, `asciiJson`; validators `object`, `text`, `number`, `boolean`, `nullableText`, `strings`, `ack`, `progress` |
| `@maui/api/app/info` | `getInfo(client?)`, `Identity` |
| `@maui/api/essentials/clipboard` | `clipboard(client?)` |
| `@maui/api/essentials/preferences` | `preferences(client?)` |

Optional `client` means use the lazy shared connection. Supplying a client uses
that exact instance: the helper does not connect/dispose it or attach listeners.

## Features

`getInfo()` resolves an `Identity`: appName, version, build, platform, osVersion,
model, architecture, numeric pid, startupId and startedAt, all from the host.
`clipboard().writeText(text)` resolves the native boolean acknowledgement;
`readText()` resolves `string | null`. No web clipboard fallback exists.
`preferences().get(key)`, `.set(key, value)` and `.remove(key)` resolve
`{ key: string; value: string | null }`; remove/missing value is null.
This API handles only text settings. It does not expose a namespace override,
clear-all operation or secure storage. Host metadata supplies the namespace;
there is no sample namespace embedded in this library.

Each helper validates result fields at runtime, including nullability and
finite numbers. Inputs are typed but **native validation is authoritative**:
TypeScript is not a security boundary and JavaScript callers can supply invalid
values. The reference provider bounds text to 4096 UTF-16 code units and keys
to 1–48 ASCII letters/digits/underscore/hyphen. Other hosts must define their
limits consistently with their registration/contracts.

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
