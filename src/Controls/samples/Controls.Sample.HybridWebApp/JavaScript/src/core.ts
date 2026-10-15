/** JSON-compatible arguments for the version-1 native command protocol. */
export type Json = null | boolean | number | string | Json[] | { [key: string]: Json }
/** Named/positional JSON arguments with operation-owned channels at the top level. */
export type ActionArgs = Json | Record<string, Json | Channel<unknown>> | Array<Json | Channel<unknown>>
/** Whole-buffer binary input; Blob MIME is descriptive metadata, not inferred from raw bytes. */
export type BinaryInput = ArrayBuffer | ArrayBufferView | Blob
/** Cooperative Core transport cancellation for a binary operation. */
export interface BinaryOptions {
  /** Cancels transfer and the owning native operation. Completed side effects are not undone. */
  signal?: AbortSignal
}
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
 * The JavaScript client/helpers require the protocol; a native UI provider may
 * additionally require a concrete page/window supplied by its managed host.
 */
export interface Bridge {
  /**
   * Optional explicitly opted-in Core binary primitive. Success is raw bytes, never base64 JSON.
   * @param method - Approved native forwarding route.
   * @param payload - Separate whole-buffer bytes.
   * @param options - Bounded action metadata and cooperative cancellation.
   * @returns Raw native bytes; rejects genuine Core invocation failures.
   */
  InvokeDotNetBinary?(method: string, payload: ArrayBuffer | ArrayBufferView,
    options?: { headers?: Record<string, string>; signal?: AbortSignal }): Promise<ArrayBuffer>
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
 * Validates a closed string enum without coercion or unchecked casts.
 * @typeParam T - The declared enum-name union.
 * @param value - Untrusted wire value or JavaScript argument.
 * @param values - Exact allowed names, normally native camelCase.
 * @param code - Response validation by default; use invalid_arguments for input.
 * @returns The matching declared name.
 * @throws {@link NativeError} for unknown, numeric or incorrectly cased names.
 */
export function enumValue<T extends string>(value: unknown, values: readonly T[], code: 'invalid_response' | 'invalid_arguments' = 'invalid_response'): T {
  for (const candidate of values) if (value === candidate) return candidate
  throw new NativeError(code, 'Unknown enum name.')
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
  /** Host-advertised clipboard/share text limit in UTF-16 code units, not a speech or preference limit. */
  maximumTextLength: number
  /** Explicit OS-interactive commands allowed to outlive normal deadlines; omitted by older hosts. */
  interactiveCommands?: string[]
  /** Native OS-interactive budget, at most 300000 milliseconds; the client adds three transport seconds. */
  interactiveTimeoutMilliseconds?: number
  /** Validated bundle-relative entry URL; routing semantics are owned by the frontend. */
  initialLocation?: string
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
interface ChannelBinding {
  client: NativeClient
  owner: string
  sequence: number
  busy: boolean
}

/**
 * One typed, ordered native-to-JS channel supplied as an annotated action argument.
 * @typeParam T - Runtime-validated message type.
 * @remarks Single-use and operation-owned, with one native message in flight. Native
 * SendAsync waits for decoding, async consumption and an explicit ack. No background
 * survival after the action completes. Unsubscribe/consumer failure cancels delivery.
 */
export class Channel<T> {
  /** Scoped channel identity, not native method or filesystem authority. */
  readonly id = crypto.randomUUID()
  private binding?: ChannelBinding
  private used = false
  private stopped = false
  private readonly decodeValue: (value: unknown) => unknown
  private readonly consume: (value: unknown) => void | Promise<void>
  private resolve!: () => void
  private reject!: (error: NativeError) => void
  /** Completes when the native owner action ends; rejects on cancel/error/consumer failure. */
  readonly completion = new Promise<void>((resolve, reject) => { this.resolve = resolve; this.reject = reject })
  /**
   * Creates a channel without native calls.
   * @param decode - Validates each JSON message (or Uint8Array for raw messages).
   * @param consumer - Awaited consumer; throwing reports consumer_error without blocking native indefinitely.
   */
  constructor(decode: Decoder<T>, consumer: (value: T) => void | Promise<void>) {
    this.decodeValue = decode
    this.consume = value => consumer(value as T)
    // A caller may use only the owning action result; don't create unhandled rejection noise.
    void this.completion.catch(() => {})
  }
  /**
   * Creates a raw-byte channel; chunks are fetched over the approved binary path.
   * @param consumer - Awaited bounded-chunk consumer.
   * @returns Single-use operation-owned channel.
   */
  static bytes(consumer: (bytes: Uint8Array) => void | Promise<void>): Channel<Uint8Array> {
    return new Channel(value => {
      if (!(value instanceof Uint8Array)) throw new NativeError('invalid_response', 'Expected raw channel bytes.')
      return value
    }, consumer)
  }
  /** Stops this channel and releases native pending data. @returns Native close completion if bound. */
  async unsubscribe(): Promise<void> {
    if (this.stopped) return
    const binding = this.binding
    this.finish(new NativeError('cancelled', 'The channel was unsubscribed.'))
    if (binding?.client.available)
      await binding.client.invoke('core.channel.close', { channelId: this.id, ownerId: binding.owner }, ack)
  }
  /** @internal Binds once to the exact owning action/client. */
  bind(client: NativeClient, owner: string): void {
    if (this.used || this.stopped) throw new NativeError('invalid_channel', 'A channel may only belong to one action.')
    this.used = true
    this.binding = { client, owner, sequence: 0, busy: false }
  }
  /** @internal Invalidates callbacks and pending local bytes. */
  finish(error?: NativeError): void {
    if (this.stopped) return
    this.stopped = true
    this.binding = undefined
    if (error) this.reject(error)
    else this.resolve()
  }
  /** @internal Receives only the owning document's correlated native channel event. */
  async receive(value: unknown, owner: string): Promise<void> {
    const binding = this.binding
    if (!binding || this.stopped || binding.owner !== owner) return
    try {
      const message = object(value), sequence = number(message.sequence), kind = text(message.kind)
      if (!Number.isInteger(sequence) || sequence !== binding.sequence + 1)
        throw new NativeError('invalid_response', 'Channel message is out of order or replayed.')
      if (kind === 'complete' || kind === 'cancelled' || kind === 'error') {
        binding.sequence = sequence
        this.finish(kind === 'complete' ? undefined : new NativeError(kind === 'error' ? text(message.code) : 'cancelled', 'Native channel owner ended.'))
        return
      }
      if (binding.busy || (kind !== 'json' && kind !== 'bytes'))
        throw new NativeError('invalid_response', 'Channel message exceeded its one-in-flight bound.')
      binding.sequence = sequence
      binding.busy = true
      const data = kind === 'bytes'
        ? await binding.client.invokeBinary('core.channel.fetch', {
          channelId: this.id, ownerId: binding.owner, dataId: text(message.dataId), sequence,
        })
        : message.value
      if (this.binding !== binding || this.stopped || !binding.client.available) return
      if (kind === 'bytes' ? !(data instanceof Uint8Array) || data.length > 65536 : asciiJson(data as Json).length > 16384)
        throw new NativeError('invalid_response', 'Channel message exceeded its documented size bound.')
      await this.consume(this.decodeValue(data))
      if (this.binding !== binding || this.stopped || !binding.client.available) return
      // Release the local flight before the ack: native can synchronously emit its next message.
      binding.busy = false
      await binding.client.invoke('core.channel.ack', { channelId: this.id, ownerId: binding.owner, sequence, accepted: true }, ack)
    } catch (error) {
      if (this.binding !== binding || this.stopped) return
      const failure = error instanceof NativeError ? error : new NativeError('consumer_error', 'Channel consumer failed.')
      this.finish(failure)
      if (binding.client.available) {
        try {
          await binding.client.invoke('core.channel.ack', { channelId: this.id, ownerId: binding.owner, sequence: binding.sequence, accepted: false }, ack)
        } catch {
          // Malformed delivery may not have a valid acknowledgement; cancel the owning action instead.
          try { await binding.client.invoke('core.cancel', { requestId: binding.owner }, ack) } catch { /* Document teardown owns cleanup. */ }
        }
      }
    }
  }
}

/**
 * Creates a Blob from raw bytes with an explicit caller-known MIME type.
 * @param bytes - Binary invocation result.
 * @param mimeType - Known document type; raw bytes do not identify it.
 * @returns Browser Blob without a native OS share claim.
 */
export function binaryBlob(bytes: Uint8Array, mimeType: string): Blob {
  return new Blob([Uint8Array.from(bytes).buffer], { type: mimeType })
}

const handshakeCompletions = new WeakMap<NativeClient, Promise<void>>()
interface SharedTopic {
  listeners: Set<(value: unknown, id: string) => void>
  release: Promise<() => Promise<void>>
  stopping?: Promise<void>
}
const sharedTopics = new WeakMap<NativeClient, Map<string, SharedTopic>>()

/**
 * Multiplexes explicit resource listeners over one native document-topic subscription.
 * @typeParam T - Provider-owned validated event payload.
 * @param client - Exact owning connected client; no global window substitution.
 * @param topic - Explicit provider-registered topic.
 * @param decode - Per-listener runtime data validator.
 * @param listener - Receives validated native data and event identity.
 * @returns Idempotent unsubscribe; the last listener detaches the native source.
 * @throws `NativeError` when native subscription fails.
 * @remarks Multiple resource callbacks share one NativeClient listener. Decoders
 * remain provider-owned, and disposal/reset still retires the native document.
 */
export async function subscribeShared<T>(client: NativeClient, topic: string, decode: Decoder<T>,
  listener: (value: T, id: string) => void): Promise<() => Promise<void>> {
  let topics = sharedTopics.get(client)
  if (!topics) { topics = new Map(); sharedTopics.set(client, topics) }
  let entry = topics.get(topic)
  while (entry?.stopping) {
    try { await entry.stopping } catch { /* The retiring subscriber owns its cleanup error. */ }
    entry = topics.get(topic)
  }
  if (!entry) {
    const listeners = new Set<(value: unknown, id: string) => void>()
    entry = { listeners, release: client.subscribe(topic, value => value, (value, id) => {
      for (const callback of [...listeners]) {
        if (!listeners.has(callback)) continue
        try { callback(value, id) }
        catch (error) { console.warn('Rejected resource event callback', error) }
      }
    }) }
    topics.set(topic, entry)
  }
  const callback = (value: unknown, id: string) => listener(decode(value), id)
  entry.listeners.add(callback)
  let release: () => Promise<void>
  try { release = await entry.release }
  catch (error) {
    entry.listeners.delete(callback)
    if (topics.get(topic) === entry) topics.delete(topic)
    throw error
  }
  let stopped = false
  return async () => {
    if (stopped) return
    stopped = true
    entry.listeners.delete(callback)
    if (entry.listeners.size === 0 && topics.get(topic) === entry) {
      entry.stopping = release()
      try { await entry.stopping }
      finally { if (topics.get(topic) === entry) topics.delete(topic) }
    }
  }
}

/**
 * Low-level client for one native WebView document session.
 * @remarks
 * A compatible, explicitly registered protocol-1 command host is required.
 * An injected Bridge does not use automatic origin detection/framework loading
 * and need not depend on MAUI Controls. Ordinary feature functions use the one
 * shared document connection configured by core setup, not an explicit client parameter.
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
  private readonly channels = new Map<string, { owner: string; channel: Channel<unknown> }>()
  private binaryActive = 0
  private controlPending = 0
  private controlTail: Promise<void> = Promise.resolve()
  private readonly bridge: Bridge | undefined
  private interactive = new Set<string>()
  private interactiveTimeout = 300000
  private readyValue: Ready | undefined
  /** Last successful native handshake metadata; descriptive only, never a permission grant. */
  get readiness(): Ready | undefined { return this.readyValue }
  /**
   * Creates an unconnected client without native calls.
   * @param bridge - Injected protocol transport, or undefined for an unavailable client; no origin check or fallback is inferred.
   * @remarks Requires crypto.randomUUID; the caller owns handshake, document attachment and disposal.
   * @example
   * ```ts
   * import { NativeClient } from '@maui/api'
   * import type { Bridge } from '@maui/api'
   * import { object, text } from '@maui/api/core'
   * async function greet(transport: Bridge) {
   *   const client = new NativeClient(transport)
   *   try {
   *     await client.connect()
   *     return await client.invoke('custom.greet', {}, value => text(object(value).message))
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
    const ready = await this.invoke('core.connect', {}, value => {
      const v = object(value)
      const ready: Ready = { commands: strings(v.commands), maximumTextLength: number(v.maximumTextLength) }
      this.interactive = new Set()
      this.interactiveTimeout = 300000
      if (v.initialLocation !== undefined && v.initialLocation !== null) ready.initialLocation = text(v.initialLocation)
      if (v.interactiveCommands !== undefined && v.interactiveCommands !== null) {
        const commands = strings(v.interactiveCommands)
        const timeout = number(v.interactiveTimeoutMilliseconds)
        if (!Number.isInteger(timeout) || timeout < 1000 || timeout > 300000 || commands.some(name => !ready.commands.includes(name)))
          throw new NativeError('invalid_response', 'Invalid interactive command policy.')
        ready.interactiveCommands = commands
        ready.interactiveTimeoutMilliseconds = timeout
        this.interactive = new Set(commands)
        this.interactiveTimeout = timeout
      }
      return ready
    })
    this.readyValue = ready
    return ready
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
   * is 20 seconds with 16 ordinary calls and four reserved control calls. Explicit interactive metadata from
   * connect extends only those commands to at most five minutes plus three transport
   * seconds. Disposal still cancels immediately. No automatic replay or OS undo occurs.
   */
  invoke<T>(command: string, args: ActionArgs, decode: Decoder<T>): Promise<T> { return this.start(command, args, decode).result }
  /**
   * Starts correlated native work with a cooperative cancellation handle.
   * @typeParam T - Validated completion type.
   * @param command - Host-registered command name.
   * @param args - JSON-compatible, native-validated arguments.
   * @param decode - Runtime result validator.
   * @returns Request UUID, completion promise and cancellation method.
   * @remarks Same session/error/size/deadline behavior as invoke; cancellation is an additional native call, not Promise abortion or undo.
   */
  start<T>(command: string, args: ActionArgs, decode: Decoder<T>): Operation<T> {
    const id = crypto.randomUUID()
    const control = ['core.cancel', 'core.channel.ack', 'core.channel.close'].includes(command)
    const result = control
      ? this.scheduleControl(() => this.request(id, command, args, decode))
      : this.request(id, command, args, decode)
    return { id, result, cancel: () => this.invoke('core.cancel', { requestId: id }, ack) }
  }
  private scheduleControl<T>(work: () => Promise<T>): Promise<T> {
    if (this.controlPending >= 64) return Promise.reject(new NativeError('busy', 'The bounded channel control queue is full.'))
    this.controlPending++
    const result = this.controlTail.then(work)
    this.controlTail = result.then(() => {}, () => {})
    return result.finally(() => { this.controlPending-- })
  }
  private bindArgs(id: string, args: ActionArgs): Json {
    const bind = (value: Json | Channel<unknown>): Json => {
      if (!(value instanceof Channel)) return value
      if ([...this.channels.values()].filter(entry => entry.owner === id).length >= 4)
        throw new NativeError('channel_limit', 'At most four channels may belong to one action.')
      value.bind(this, id)
      this.channels.set(value.id, { owner: id, channel: value })
      return value.id
    }
    try {
      if (Array.isArray(args)) return args.map(bind)
      if (isRecord(args)) return Object.fromEntries(Object.entries(args).map(([key, value]) => [key, bind(value)]))
      return args
    } catch (error) {
      const failure = error instanceof NativeError ? error : new NativeError('invalid_arguments', 'Action argument binding failed.')
      this.finishChannels(id, failure)
      throw failure
    }
  }
  private finishChannels(id: string, error?: NativeError): void {
    for (const [key, entry] of this.channels) if (entry.owner === id) {
      this.channels.delete(key)
      entry.channel.finish(error)
    }
  }
  private async request<T>(id: string, command: string, args: ActionArgs, decode: Decoder<T>): Promise<T> {
    if (!this.available || !this.bridge) throw new NativeError('native_unavailable', 'Native host unavailable. Launch the MAUI app; browser mode does not emulate native APIs.')
    let failure: NativeError | undefined
    let timer: ReturnType<typeof setTimeout> | undefined
    const obsolete = new Promise<never>((_, reject) => {
      this.aborts.set(id, () => reject(new NativeError('cancelled', 'The frontend document disconnected.')))
      timer = setTimeout(() => {
        reject(new NativeError('transport_timeout', 'The native call timed out; reload to reconnect.'))
        this.sendDisconnect()
      }, this.interactive.has(command) ? this.interactiveTimeout + 3000 : 23000)
    })
    try {
      const request = asciiJson({ version: 1, session: this.session, id, command, args: this.bindArgs(id, args) })
      if (request.length > 100000) throw new NativeError('invalid_arguments', 'Request exceeds the protocol size limit.')
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
      failure = error instanceof NativeError ? error : new NativeError('transport_error', error instanceof Error ? error.message : 'Native transport failed.')
      throw failure
    } finally {
      clearTimeout(timer)
      this.aborts.delete(id)
      this.finishChannels(id, failure)
    }
  }
  /**
   * Invokes an approved byte-result action with separate bytes and bounded structured arguments.
   * @param command - Registered binary action route.
   * @param args - Named/positional JSON arguments and optional operation-owned channels.
   * @param payload - Separate Blob, buffer or view; defaults to empty input.
   * @param options - Cooperative transfer cancellation.
   * @returns Exact native bytes as Uint8Array.
   * @remarks 4 MiB input/result; 2400 ASCII metadata characters, at most ten
   * 240-character headers. Same hub session, 16-operation/replay/deadline policies;
   * Three binary actions may run concurrently; one Core transfer slot is reserved
   * for serialized channel data fetches. Core has a 30-second transfer lifetime.
   * Thus binary actions cannot promise the full five-minute interactive duration.
   */
  async invokeBinary(command: string, args: ActionArgs = {}, payload: BinaryInput = new Uint8Array(), options: BinaryOptions = {}): Promise<Uint8Array> {
    if (command === 'core.channel.fetch')
      return this.scheduleControl(() => this.binaryRequest(command, args, payload, options, false))
    if (this.binaryActive >= 3) throw new NativeError('busy', 'At most three binary actions may be active; channel data has a reserved transfer slot.')
    return this.binaryRequest(command, args, payload, options, true)
  }
  private async binaryRequest(command: string, args: ActionArgs, payload: BinaryInput, options: BinaryOptions, ordinary: boolean): Promise<Uint8Array> {
    if (!this.available || !this.bridge?.InvokeDotNetBinary)
      throw new NativeError('binary_unavailable', 'The host did not explicitly opt in to Core binary invocation.')
    if (ordinary) this.binaryActive++
    const id = crypto.randomUUID(), controller = new AbortController()
    let timer: ReturnType<typeof setTimeout> | undefined, failure: NativeError | undefined
    const abort = () => controller.abort()
    const obsolete = new Promise<never>((_, reject) => {
      this.aborts.set(id, () => { abort(); reject(new NativeError('cancelled', 'The binary operation was cancelled.')) })
      timer = setTimeout(() => { abort(); reject(new NativeError('transport_timeout', 'The binary action timed out.')); this.sendDisconnect() },
        this.interactive.has(command) ? Math.min(this.interactiveTimeout + 3000, 33000) : 23000)
      controller.signal.addEventListener('abort', () => reject(new NativeError('cancelled', 'The binary operation was cancelled.')), { once: true })
    })
    options.signal?.addEventListener('abort', abort, { once: true })
    if (options.signal?.aborted) abort()
    try {
      const work = async () => {
        try {
          const size = payload instanceof Blob ? payload.size : payload.byteLength
          if (size > 4 * 1024 * 1024) throw new NativeError('binary_limit', 'Binary input exceeds 4 MiB.')
          const bytes = payload instanceof Blob ? await payload.arrayBuffer() : payload
          if (controller.signal.aborted || this.disposed) throw new NativeError('cancelled', 'The binary operation was cancelled.')
          const request = asciiJson({ version: 1, session: this.session, id, command, args: this.bindArgs(id, args) })
          if (request.length > 2400) throw new NativeError('invalid_arguments', 'Binary action metadata exceeds 2400 ASCII characters.')
          const headers: Record<string, string> = {}
          for (let i = 0; i < request.length; i += 240)
            headers[`maui-request-${String(i / 240).padStart(2, '0')}`] = request.slice(i, i + 240)
          const result = await this.bridge!.InvokeDotNetBinary!('hybrid.invoke', bytes, { headers, signal: controller.signal })
          if (!(result instanceof ArrayBuffer) || result.byteLength > 4 * 1024 * 1024)
            throw new NativeError('invalid_response', 'Expected a bounded raw binary result.')
          return new Uint8Array(result)
        } finally {
          if (ordinary) this.binaryActive--
        }
      }
      const result = await Promise.race([work(), obsolete])
      if (this.disposed || controller.signal.aborted) throw new NativeError('cancelled', 'The binary operation was cancelled.')
      return result
    } catch (error) {
      failure = error instanceof NativeError ? error : new NativeError('transport_error', error instanceof Error ? error.message : 'Binary transport failed.')
      if (!(error instanceof NativeError) && error instanceof Error && 'dotNetErrorType' in error) {
        try { const value = object(JSON.parse(error.message)); failure = new NativeError(text(value.code), text(value.message)) } catch { /* Genuine unknown Core failure remains a transport error. */ }
      }
      throw failure
    } finally {
      clearTimeout(timer)
      options.signal?.removeEventListener('abort', abort)
      this.aborts.delete(id)
      this.finishChannels(id, failure)
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
      if (event.topic === 'core.channel') {
        const message = object(event.data)
        const entry = this.channels.get(text(message.channelId))
        if (entry) void entry.channel.receive(message, text(event.id))
        return
      }
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
    for (const entry of this.channels.values()) entry.channel.finish(new NativeError('cancelled', 'The frontend document disconnected.'))
    this.channels.clear()
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
let boundBridge: Bridge | undefined
let hasBoundBridge = false
let connectionGeneration = 0
/** Infrastructure generation for lazy resource bindings; changes when the shared document client is retired. */
export function documentConnectionGeneration(): number { return connectionGeneration }
let defaultConnection: Promise<NativeClient> | undefined
let cancelDefaultStartup: ((error: NativeError) => void) | undefined
let defaultHandshake: Promise<void> = Promise.resolve()

/**
 * Configures one explicit protocol transport before the document's shared feature connection.
 * @param bridge - Explicit host/test transport; undefined deliberately represents no native host.
 * @returns Unconnected low-level client for infrastructure/bootstrap inspection; ordinary features still use shared connect.
 * @remarks Infrastructure setup only. Retires the old shared client using normal
 * disconnect/handshake ordering; feature/resource APIs never accept clients.
 * A custom transport is not inferred on an ordinary browser and grants no native permissions.
 */
export function configureBridge(bridge: Bridge | undefined): NativeClient {
  disconnect()
  boundBridge = bridge
  hasBoundBridge = true
  defaultClient = new NativeClient(bridge)
  return defaultClient
}
/**
 * Clears explicit transport setup and retires the shared document connection.
 * @remarks Restores normal native-origin bootstrap; existing resources retain their retired client and cannot rebind.
 */
export function resetBridge(): void {
  disconnect()
  boundBridge = undefined
  hasBoundBridge = false
}

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
  if (defaultClient?.available && defaultClient.readiness && !defaultConnection) return Promise.resolve(defaultClient)
  if (defaultConnection) return defaultConnection
  const previousHandshake = defaultHandshake
  let cancel!: (error: NativeError) => void
  const cancelled = new Promise<never>((_, reject) => { cancel = reject })
  const startup = (hasBoundBridge ? Promise.resolve(boundBridge) : loadNativeBridge()).then(async bridge => {
    await previousHandshake
    if (defaultConnection !== connection) throw new NativeError('cancelled', 'The frontend document disconnected.')
    if (!bridge) throw new NativeError('native_unavailable', 'Native host unavailable. Launch a matching MAUI app; browser mode does not emulate native APIs.')
    const client = hasBoundBridge && defaultClient?.available && !defaultClient.readiness ? defaultClient : new NativeClient(bridge)
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
 * Invokes an opted-in native action through the shared document connection.
 * @param command - The host-registered action name, never an arbitrary native member.
 * @param args - Named JSON arguments or a positional array; defaults to an empty object.
 * @returns The native JSON result as unknown; validate it before use.
 * @throws {@link NativeError} for native absence, invalid arguments, host rejection or transport failure.
 * @remarks Attribute-only native actions accept named objects or positional arrays,
 * validated against their declared method contracts. Service, context and cancellation
 * parameters consume neither object keys nor array positions.
 */
export function invoke(command: string, args?: ActionArgs): Promise<unknown>
/**
 * Invokes an opted-in native action and validates its result.
 * @typeParam T - The type produced by the supplied runtime decoder.
 * @param command - The host-registered action name.
 * @param args - Named JSON arguments or a positional array.
 * @param decode - Runtime result validator; TypeScript types alone do not validate native data.
 * @returns The validated result from the shared native client.
 * @throws {@link NativeError} for invocation or result-validation failures.
 */
export function invoke<T>(command: string, args: ActionArgs, decode: Decoder<T>): Promise<T>
export async function invoke(command: string, args: ActionArgs = {}, decode: Decoder<unknown> = value => value): Promise<unknown> {
  return (await connect()).invoke(command, args, decode)
}

/**
 * Invokes a registered byte-result action on the shared document connection.
 * @param command - Approved binary action name.
 * @param args - Bounded named/positional arguments, including optional channels.
 * @param payload - Separate whole-buffer bytes or Blob, up to 4 MiB.
 * @param options - Cooperative transfer cancellation.
 * @returns Exact bytes; use binaryBlob with an explicitly known MIME type.
 * @throws NativeError for missing opt-in, invalid metadata/input, native failure or cancellation.
 */
export async function invokeBinary(command: string, args: ActionArgs = {}, payload: BinaryInput = new Uint8Array(), options: BinaryOptions = {}): Promise<Uint8Array> {
  return (await connect()).invokeBinary(command, args, payload, options)
}

/**
 * Cancels shared startup and disposes the default document client.
 * @remarks
 * Idempotent; does not own explicitly constructed clients. A later connect creates
 * a fresh session after any underlying old handshake settles. Pending frontend waits
 * are rejected as cancelled; completed OS side effects are not undone.
 */
export function disconnect(): void {
  connectionGeneration++
  const client = defaultClient
  cancelDefaultStartup?.(new NativeError('cancelled', 'The frontend document disconnected.'))
  cancelDefaultStartup = undefined
  defaultClient = undefined
  defaultConnection = undefined
  client?.dispose()
}
