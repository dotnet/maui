/** JSON-compatible arguments for the version-1 native command protocol. */
export type Json = null | boolean | number | string | Json[] | { [key: string]: Json }
/**
 * Validates untrusted native result or event data and returns its typed value.
 * @typeParam T - Validated result type.
 * @param value - Untrusted parsed JSON supplied by native.
 * @returns Validated application value.
 * @remarks Throw on invalid data, preferably with {@link NativeError} code `invalid_response`.
 */
export type Decoder<T> = (value: unknown) => T
/**
 * Narrow injectable transport for an explicitly registered protocol-1 native host.
 * @remarks
 * MAUI HybridWebView provides these operations, but an explicit implementation can
 * deliver the same wire contract without automatic MAUI-origin bootstrap or Controls.
 * The client/Essentials helpers require the protocol, not a particular UI framework.
 */
export interface Bridge {
  /**
   * Invokes a registered native forwarding method.
   * @param method - Forwarding method name; this SDK uses `Invoke`.
   * @param args - Serialized argument strings; the SDK supplies one ASCII-escaped protocol request.
   * @returns A native response; protocol 1 requires a JSON envelope string.
   * @throws Transport-specific errors when delivery fails.
   * @remarks Stock MAUI JavaScript invocation alone does not register this SDK's command hub.
   */
  InvokeDotNet(method: string, args: string[]): Promise<unknown>
  /**
   * Sends a best-effort document heartbeat or disconnect notice.
   * @param message - ASCII-escaped JSON notice containing a session UUID and kind.
   * @throws Transport-specific errors when delivery fails.
   * @remarks Delivery can fail; injected/headless hosts must implement lease management.
   */
  SendRawMessage(message: string): void
}
declare global {
  /** Framework transport global installed by MAUI HybridWebView bootstrap. */
  interface Window {
    /** Optional bridge installed only after the native framework script initializes. */
    HybridWebView?: Bridge
  }
}

/** Rejected native operation or SDK validation/transport failure, never a success-shaped result. */
export class NativeError extends Error {
  /** SDK or native error code; native provider codes are preserved without remapping. */
  readonly code: string
  /**
   * Creates a coded failure.
   * @param code - SDK or native error code, such as `native_unavailable` or `permission_denied`.
   * @param message - Human-readable detail; avoid including clipboard or preference contents.
   */
  constructor(code: string, message: string) { super(message); this.code = code; this.name = 'NativeError' }
}
function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}
/**
 * Validates a non-null, non-array object.
 * @param value - Untrusted parsed native value.
 * @returns The original record; its individual fields still require validation.
 * @throws {@link NativeError} with `invalid_response` when the value is not a record.
 */
export function object(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) throw new NativeError('invalid_response', 'Expected an object.')
  return value
}
/**
 * Validates text without coercion.
 * @param value - Untrusted native value.
 * @returns The original string.
 * @throws {@link NativeError} with `invalid_response` when the value is not a string.
 */
export function text(value: unknown): string {
  if (typeof value !== 'string') throw new NativeError('invalid_response', 'Expected a string.')
  return value
}
/**
 * Validates a finite number without coercion.
 * @param value - Untrusted native value.
 * @returns The original finite number; integrality/range require additional checks.
 * @throws {@link NativeError} with `invalid_response` for non-numbers, NaN or infinities.
 */
export function number(value: unknown): number {
  if (typeof value !== 'number' || !Number.isFinite(value)) throw new NativeError('invalid_response', 'Expected a finite number.')
  return value
}
/**
 * Validates a boolean without coercion.
 * @param value - Untrusted native value.
 * @returns The original boolean.
 * @throws {@link NativeError} with `invalid_response` when the value is not a boolean.
 */
export function boolean(value: unknown): boolean {
  if (typeof value !== 'boolean') throw new NativeError('invalid_response', 'Expected a boolean.')
  return value
}
/**
 * Validates text or explicit null, not undefined.
 * @param value - Untrusted native value.
 * @returns The original string or null.
 * @throws {@link NativeError} with `invalid_response` for all other values.
 */
export function nullableText(value: unknown): string | null { return value === null ? null : text(value) }
/**
 * Validates an array of strings.
 * @param value - Untrusted native value.
 * @returns A new array containing the validated strings.
 * @throws {@link NativeError} with `invalid_response` for a non-array or any non-string element.
 */
export function strings(value: unknown): string[] {
  if (!Array.isArray(value)) throw new NativeError('invalid_response', 'Expected a string array.')
  return value.map(text)
}
/**
 * Decodes the native acknowledgement shape.
 * @param value - Untrusted result with an `accepted` boolean property.
 * @returns Whether native accepted the request; false is a valid result.
 * @throws {@link NativeError} with `invalid_response` for malformed acknowledgement data.
 */
export function ack(value: unknown): boolean { return boolean(object(value).accepted) }
/**
 * Serializes JSON as ASCII while preserving Unicode UTF-16 code units.
 * @param value - JSON-compatible value; callers must not pass cycles, BigInt or other non-JSON data.
 * @returns JSON safe for the framework's twice-serialized Fetch ByteString header.
 * @remarks Escapes U+007F–U+FFFF, including both surrogate halves. Native requests are capped at 100000 escaped characters.
 * @throws A serialization error for non-JSON input; callers must supply JSON-compatible arguments.
 */
export function asciiJson(value: Json): string {
  // InvokeDotNet serializes this string twice into a Fetch ByteString header.
  // Escape UTF-16 code units (including both surrogate halves), not Unicode code points.
  return JSON.stringify(value).replace(/[\u007f-\uffff]/g, unit => `\\u${unit.charCodeAt(0).toString(16).padStart(4, '0')}`)
}
/**
 * Checks the exact production MAUI virtual origin recognized by automatic bootstrap.
 * @param href - Absolute document URL.
 * @returns True for `app://0.0.0.1` or `https://0.0.0.1` without a non-default port; false for ordinary browser/Vite origins.
 * @throws TypeError when the URL cannot be parsed.
 * @remarks This origin check is not authentication or an authorization boundary.
 */
export function isNativeOrigin(href: string): boolean {
  const url = new URL(href)
  return (url.protocol === 'app:' || url.protocol === 'https:') && url.hostname === '0.0.0.1' && !url.port
}
const pendingBridgeLoads = new WeakMap<Document, Promise<void>>()
/**
 * Loads the framework bridge lazily on a recognized native origin.
 * @returns The framework transport, or undefined in Node/SSR or an ordinary browser.
 * @throws {@link NativeError} with `transport_error` for script load failure, its five-second deadline or missing initialization.
 * @remarks
 * Loads `/_framework/hybridwebview.js`, not a native command provider. Concurrent loads
 * share one script per document independently of client disconnect. A timed-out script
 * remains shared until it finishes or errors; reload rather than inject a duplicate.
 */
export async function loadNativeBridge(): Promise<Bridge | undefined> {
  if (typeof window === 'undefined' || typeof document === 'undefined' || !isNativeOrigin(window.location.href)) return undefined
  if (!window.HybridWebView) {
    const currentDocument = document
    let bootstrap = pendingBridgeLoads.get(currentDocument)
    if (!bootstrap) {
      bootstrap = new Promise<void>((resolve, reject) => {
        const script = currentDocument.createElement('script')
        script.src = '/_framework/hybridwebview.js'
        // A timed-out script can still execute. Keep its load shared until it actually finishes.
        const timeout = setTimeout(() => reject(new NativeError('transport_error', 'Native bridge script did not load. Reload to retry.')), 5000)
        script.onload = () => { clearTimeout(timeout); pendingBridgeLoads.delete(currentDocument); resolve() }
        script.onerror = () => { clearTimeout(timeout); pendingBridgeLoads.delete(currentDocument); reject(new NativeError('transport_error', 'Native bridge script failed to load.')) }
        currentDocument.head.append(script)
      })
      pendingBridgeLoads.set(currentDocument, bootstrap)
    }
    await bootstrap
  }
  if (!window.HybridWebView) throw new NativeError('transport_error', 'The native bridge was not initialized.')
  return window.HybridWebView
}

/** Descriptive metadata returned by the native `core.connect` handshake; not an OS authorization grant. */
export interface Ready {
  /** Registered native command names; the reference host also lists disabled registrations. */
  commands: string[]
  /** Settings namespace chosen by the host; helpers do not allow overriding it. */
  preferenceNamespace: string
  /** Host-advertised text limit in UTF-16 code units; native input validation remains authoritative. */
  maximumTextLength: number
}
/**
 * Correlated native work and its cooperative cancellation handle.
 * @typeParam T - Validated native result.
 */
export interface Operation<T> {
  /** Unique request UUID within the document session. */
  id: string
  /** Validated completion value; rejects with NativeError for native, transport or decode failure. */
  result: Promise<T>
  /**
   * Requests cancellation using `core.cancel` and this operation's request UUID.
   * @returns The native acknowledgement, not proof that a completed OS side effect was undone.
   * @throws {@link NativeError} if the host cannot accept the cancellation request.
   * @remarks Cooperative only; needs a live session and an available host call slot.
   */
  cancel(): Promise<boolean>
}
/** Validated integer progress for a bounded operation. */
export interface Progress {
  /** Completed units, between zero and total inclusive. */
  completed: number
  /** Positive total number of units. */
  total: number
}
/**
 * Decodes bounded integer progress.
 * @param value - Untrusted native result/event containing completed and total.
 * @returns Validated progress with positive total and completed between zero and total.
 * @throws {@link NativeError} with `invalid_response` for malformed or out-of-range progress.
 */
export const progress: Decoder<Progress> = value => {
  const v = object(value)
  const completed = number(v.completed), total = number(v.total)
  if (!Number.isInteger(total) || !Number.isInteger(completed) || total < 1 || completed < 0 || completed > total) throw new NativeError('invalid_response', 'Invalid native progress.')
  return { completed, total }
}
type Listener = (value: unknown, id: string) => void
const handshakeCompletions = new WeakMap<NativeClient, Promise<void>>()

/**
 * Low-level client for one native WebView document session.
 * @remarks
 * A compatible, explicitly registered protocol-1 command host is required.
 * An injected Bridge does not use automatic origin detection/framework loading
 * and need not depend on MAUI Controls. Feature helpers use the exact supplied client.
 * Construction does not handshake or attach browser listeners. Call the instance's
 * connect method before commands and dispose at teardown. Only one session should
 * own a WebView document: a new native handshake retires the previous document.
 * No browser emulation, automatic reconnect or operation replay is performed.
 */
export class NativeClient {
  /** Document UUID generated on construction; identifies a session but does not authenticate it. */
  readonly session = crypto.randomUUID()
  private disposed = false
  private readonly listeners = new Map<string, Listener>()
  private readonly aborts = new Map<string, () => void>()
  private readonly notices: (() => void)[] = []
  private readonly bridge: Bridge | undefined
  /**
   * Creates an unconnected client without native calls.
   * @param bridge - Injected protocol transport, or undefined for an unavailable client; no origin check or fallback is inferred.
   * @remarks Requires crypto.randomUUID; the caller owns handshake, document attachment and disposal.
   * @example
   * ```ts
   * import { NativeClient } from '@maui/api'
   * import type { Bridge } from '@maui/api'
   * import { getAppInfo } from '@maui/api/essentials/app-info'
   * async function readApp(transport: Bridge) {
   *   const client = new NativeClient(transport)
   *   try {
   *     await client.connect()
   *     return await getAppInfo(client)
   *   } finally { client.dispose() }
   * }
   * ```
   */
  constructor(bridge: Bridge | undefined) { this.bridge = bridge }
  /** True when a transport exists and the client is not disposed; not proof of handshake, live lease, provider or OS permission. */
  get available(): boolean { return !!this.bridge && !this.disposed }
  /**
   * Establishes this session using `core.connect`.
   * @returns Host command/namespace/limit metadata, not a shared-client instance.
   * @throws {@link NativeError} for unavailable transport, host rejection or malformed metadata.
   * @remarks Unlike root connect, this method does not bootstrap or attach document cleanup.
   */
  async connect(): Promise<Ready> {
    return this.invoke('core.connect', {}, value => {
      const v = object(value)
      return { commands: strings(v.commands), preferenceNamespace: text(v.preferenceNamespace), maximumTextLength: number(v.maximumTextLength) }
    })
  }
  /**
   * Invokes one explicitly registered command and validates its completion value.
   * @typeParam T - Validated completion type.
   * @param command - Host-registered name; no arbitrary native member invocation is supported.
   * @param args - JSON-compatible arguments validated authoritatively by the host.
   * @param decode - Runtime result validator; TypeScript types alone do not validate native data.
   * @returns The decoded result, never an error-shaped success.
   * @throws {@link NativeError} for unavailable/disposed native, host rejection, bad envelopes, invalid output or transport failure.
   * @remarks
   * Requests are bounded to 100000 ASCII-escaped characters. Frontend waits have a
   * 23-second deadline and signal disconnect on timeout; the reference host's deadline
   * is 20 seconds with 16 simultaneous calls. No automatic replay or OS undo occurs.
   */
  invoke<T>(command: string, args: Json, decode: Decoder<T>): Promise<T> { return this.start(command, args, decode).result }
  /**
   * Starts correlated native work with a cooperative cancellation handle.
   * @typeParam T - Validated completion type.
   * @param command - Host-registered command name.
   * @param args - JSON-compatible, native-validated arguments.
   * @param decode - Runtime result validator.
   * @returns Request UUID, completion promise and cancellation method.
   * @remarks Same session/error/size/deadline behavior as invoke; cancellation is an additional native call, not Promise abortion or undo.
   */
  start<T>(command: string, args: Json, decode: Decoder<T>): Operation<T> {
    const id = crypto.randomUUID()
    const result = this.request(id, command, args, decode)
    return { id, result, cancel: () => this.invoke('core.cancel', { requestId: id }, ack) }
  }
  private async request<T>(id: string, command: string, args: Json, decode: Decoder<T>): Promise<T> {
    if (!this.available || !this.bridge) throw new NativeError('native_unavailable', 'Native host unavailable. Launch the MAUI app; browser mode does not emulate native APIs.')
    const request = asciiJson({ version: 1, session: this.session, id, command, args })
    if (request.length > 100000) throw new NativeError('invalid_arguments', 'Request exceeds the protocol size limit.')
    let timer: ReturnType<typeof setTimeout> | undefined
    const obsolete = new Promise<never>((_, reject) => {
      this.aborts.set(id, () => reject(new NativeError('cancelled', 'The frontend document disconnected.')))
      timer = setTimeout(() => {
        reject(new NativeError('transport_timeout', 'The native call timed out; reload to reconnect.'))
        this.sendDisconnect()
      }, 23000)
    })
    try {
      const transport = this.bridge.InvokeDotNet('Invoke', [request])
      if (command === 'core.connect') {
        // Cancelling the frontend wait cannot abort the framework's in-flight native handshake.
        const complete = () => { if (this.disposed) this.sendDisconnect() }
        handshakeCompletions.set(this, transport.then(complete, complete))
      }
      const wire = await Promise.race([transport, obsolete])
      if (this.disposed) throw new NativeError('cancelled', 'The frontend document disconnected.')
      if (typeof wire !== 'string') throw new NativeError('invalid_response', 'Native envelope must be a JSON string.')
      const response = object(JSON.parse(wire))
      if (response.version !== 1 || response.session !== this.session || response.id !== id) throw new NativeError('invalid_response', 'Native response identity/version mismatch.')
      if (response.status === 'success') {
        if (response.error !== null) throw new NativeError('invalid_response', 'Success envelope contained an error.')
        return decode(response.result)
      }
      if (response.status !== 'error' && response.status !== 'cancelled') throw new NativeError('invalid_response', 'Unknown native operation state.')
      const error = object(response.error)
      throw new NativeError(response.status === 'cancelled' ? 'cancelled' : text(error.code), text(error.message))
    } catch (error) {
      if (error instanceof NativeError) throw error
      throw new NativeError('transport_error', error instanceof Error ? error.message : 'Native transport failed.')
    } finally {
      clearTimeout(timer)
      this.aborts.delete(id)
    }
  }
  /**
   * Subscribes to one explicitly allowed native topic.
   * @typeParam T - Validated event type.
   * @param topic - Topic opted into by the host.
   * @param decode - Runtime event-data validator.
   * @param listener - Receives validated data and the originating request UUID.
   * @returns An async idempotent unsubscribe function; no native unsubscribe is sent after disposal.
   * @throws {@link NativeError} for a duplicate local topic, host rejection or transport failure.
   * @remarks One listener per topic per client. Failed startup removes the listener. Event decoding/listener failures are warned and rejected, not delivered as success.
   */
  async subscribe<T>(topic: string, decode: Decoder<T>, listener: (value: T, id: string) => void): Promise<() => Promise<void>> {
    if (this.listeners.has(topic)) throw new NativeError('duplicate_subscription', 'Unsubscribe the existing listener before subscribing again.')
    this.listeners.set(topic, (value, id) => listener(decode(value), id))
    try { await this.invoke('core.subscribe', { topic }, ack) }
    catch (error) { this.listeners.delete(topic); throw error }
    let stopped = false
    return async () => {
      if (stopped) return
      stopped = true
      this.listeners.delete(topic)
      if (this.available) await this.invoke('core.unsubscribe', { topic }, ack)
    }
  }
  /**
   * Accepts a raw native event from an injected transport or framework listener.
   * @param raw - Protocol-1 event JSON string.
   * @remarks
   * Ignores disposed clients, non-strings, input over 100000 characters, stale sessions
   * and versions. Malformed events or decoder/listener errors are warned rather than thrown.
   */
  receive(raw: unknown): void {
    if (this.disposed || typeof raw !== 'string' || raw.length > 100000) return
    try {
      const event = object(JSON.parse(raw))
      if (event.version !== 1 || event.session !== this.session) return
      this.listeners.get(text(event.topic))?.(event.data, text(event.id))
    } catch (error) { console.warn('Rejected malformed native event', error) }
  }
  /**
   * Idempotently attaches framework events, pagehide disposal and a two-second heartbeat.
   * @remarks No-op without window or for unavailable/disposed clients. Headless injected hosts must manage their own lease.
   */
  attachToDocument(): void {
    if (this.notices.length || !this.available || typeof window === 'undefined') return
    const receive = (event: Event) => {
      if (event instanceof CustomEvent && isRecord(event.detail)) this.receive(event.detail.message)
    }
    const disconnect = () => this.dispose()
    window.addEventListener('HybridWebViewMessageReceived', receive)
    window.addEventListener('pagehide', disconnect, { once: true })
    const heartbeat = setInterval(() => this.bridge?.SendRawMessage(asciiJson({ session: this.session, kind: 'heartbeat' })), 2000)
    this.notices.push(() => {
      window.removeEventListener('HybridWebViewMessageReceived', receive)
      window.removeEventListener('pagehide', disconnect)
      clearInterval(heartbeat)
    })
  }
  /**
   * Idempotently disposes this client and rejects pending frontend waits as cancelled.
   * @remarks
   * Sends best-effort disconnect, clears subscriptions/listeners and stops heartbeat.
   * Transport delivery failure is warned without preventing local cleanup. A disposed
   * client cannot be reused; already completed OS side effects are not undone.
   */
  dispose(): void {
    if (this.disposed) return
    this.disposed = true
    this.sendDisconnect()
    this.listeners.clear()
    for (const abort of this.aborts.values()) abort()
    this.aborts.clear()
    for (const cleanup of this.notices) cleanup()
    this.notices.length = 0
  }
  private sendDisconnect(): void {
    try { this.bridge?.SendRawMessage(asciiJson({ session: this.session, kind: 'disconnect' })) }
    catch (error) { console.warn('Native disconnect notice could not be delivered', error) }
  }
}

let defaultClient: NativeClient | undefined
let defaultConnection: Promise<NativeClient> | undefined
let cancelDefaultStartup: ((error: NativeError) => void) | undefined
let defaultHandshake: Promise<void> = Promise.resolve()

/**
 * Lazily connects the shared client for the current frontend document.
 * @returns The connected shared NativeClient; concurrent callers share the same startup.
 * @throws {@link NativeError} for unavailable native, failed bootstrap/handshake, cancelled startup or the 28-second overall deadline.
 * @remarks
 * Loads the bridge, attaches document cleanup and awaits `core.connect`. Requires an
 * explicitly registered protocol-1 host; importing this module alone does nothing.
 * Startup failures clear the failed default so a later call can retry. Replacement
 * startup waits for a cancelled handshake's underlying transport to settle and retires
 * any late session. The overall deadline includes bootstrap and predecessor waiting.
 * No automatic reconnect/replay occurs; reload after expired native leases or suspension.
 * @example
 * ```ts
 * import { connect, disconnect } from '@maui/api'
 * const client = await connect()
 * // Use the registered providers, then dispose during frontend teardown.
 * disconnect()
 * ```
 */
export function connect(): Promise<NativeClient> {
  if (defaultClient?.available && !defaultConnection) return Promise.resolve(defaultClient)
  if (defaultConnection) return defaultConnection
  const previousHandshake = defaultHandshake
  let cancel!: (error: NativeError) => void
  const cancelled = new Promise<never>((_, reject) => { cancel = reject })
  const startup = loadNativeBridge().then(async bridge => {
    await previousHandshake
    if (defaultConnection !== connection) throw new NativeError('cancelled', 'The frontend document disconnected.')
    if (!bridge) throw new NativeError('native_unavailable', 'Native host unavailable. Launch a matching MAUI app; browser mode does not emulate native APIs.')
    const client = new NativeClient(bridge)
    defaultClient = client
    try {
      client.attachToDocument()
      const handshake = client.connect()
      defaultHandshake = handshakeCompletions.get(client) ?? Promise.resolve()
      await handshake
      if (!client.available || defaultConnection !== connection) throw new NativeError('cancelled', 'The frontend document disconnected.')
      return client
    } catch (error) {
      client.dispose()
      if (defaultClient === client) defaultClient = undefined
      throw error
    }
  })
  const connection = Promise.race([startup, cancelled])
  defaultConnection = connection
  cancelDefaultStartup = cancel
  const timeout = setTimeout(() => {
    cancel(new NativeError('transport_timeout', 'Native connection startup did not finish. Reload to reconnect.'))
    if (defaultConnection === connection) {
      const client = defaultClient
      defaultClient = undefined
      client?.dispose()
    }
  }, 28000)
  const clear = () => {
    clearTimeout(timeout)
    if (defaultConnection === connection) { defaultConnection = undefined; cancelDefaultStartup = undefined }
  }
  void connection.then(clear, clear)
  return connection
}

/**
 * Cancels shared startup and disposes the default document client.
 * @remarks
 * Idempotent; does not own explicitly constructed clients. A later connect creates
 * a fresh session after any underlying old handshake settles. Pending frontend waits
 * are rejected as cancelled; completed OS side effects are not undone.
 */
export function disconnect(): void {
  const client = defaultClient
  cancelDefaultStartup?.(new NativeError('cancelled', 'The frontend document disconnected.'))
  cancelDefaultStartup = undefined
  defaultClient = undefined
  defaultConnection = undefined
  client?.dispose()
}
