export type Json = null | boolean | number | string | Json[] | { [key: string]: Json }
export type Decoder<T> = (value: unknown) => T
export interface Bridge {
  InvokeDotNet(method: string, args: string[]): Promise<unknown>
  SendRawMessage(message: string): void
}
declare global {
  interface Window { HybridWebView?: Bridge }
}

export class NativeError extends Error {
  readonly code: string
  constructor(code: string, message: string) { super(message); this.code = code; this.name = 'NativeError' }
}
function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}
export function object(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) throw new NativeError('invalid_response', 'Expected an object.')
  return value
}
export function text(value: unknown): string {
  if (typeof value !== 'string') throw new NativeError('invalid_response', 'Expected a string.')
  return value
}
export function number(value: unknown): number {
  if (typeof value !== 'number' || !Number.isFinite(value)) throw new NativeError('invalid_response', 'Expected a finite number.')
  return value
}
export function boolean(value: unknown): boolean {
  if (typeof value !== 'boolean') throw new NativeError('invalid_response', 'Expected a boolean.')
  return value
}
export function nullableText(value: unknown): string | null { return value === null ? null : text(value) }
export function strings(value: unknown): string[] {
  if (!Array.isArray(value)) throw new NativeError('invalid_response', 'Expected a string array.')
  return value.map(text)
}
export function ack(value: unknown): boolean { return boolean(object(value).accepted) }
export function asciiJson(value: Json): string {
  // InvokeDotNet serializes this string twice into a Fetch ByteString header.
  // Escape UTF-16 code units (including both surrogate halves), not Unicode code points.
  return JSON.stringify(value).replace(/[\u007f-\uffff]/g, unit => `\\u${unit.charCodeAt(0).toString(16).padStart(4, '0')}`)
}
export function isNativeOrigin(href: string): boolean {
  const url = new URL(href)
  return (url.protocol === 'app:' || url.protocol === 'https:') && url.hostname === '0.0.0.1' && !url.port
}
export async function loadNativeBridge(): Promise<Bridge | undefined> {
  if (typeof window === 'undefined' || typeof document === 'undefined' || !isNativeOrigin(window.location.href)) return undefined
  if (!window.HybridWebView) {
    await new Promise<void>((resolve, reject) => {
      const script = document.createElement('script')
      script.src = '/_framework/hybridwebview.js'
      const timeout = setTimeout(() => reject(new NativeError('transport_error', 'Native bridge script did not load. Reload to retry.')), 5000)
      script.onload = () => { clearTimeout(timeout); resolve() }
      script.onerror = () => { clearTimeout(timeout); reject(new NativeError('transport_error', 'Native bridge script failed to load.')) }
      document.head.append(script)
    })
  }
  if (!window.HybridWebView) throw new NativeError('transport_error', 'The native bridge was not initialized.')
  return window.HybridWebView
}

export interface Ready { commands: string[]; preferenceNamespace: string; maximumTextLength: number }
export interface Operation<T> { id: string; result: Promise<T>; cancel(): Promise<boolean> }
export interface Progress { completed: number; total: number }
export const progress: Decoder<Progress> = value => {
  const v = object(value)
  const completed = number(v.completed), total = number(v.total)
  if (!Number.isInteger(total) || !Number.isInteger(completed) || total < 1 || completed < 0 || completed > total) throw new NativeError('invalid_response', 'Invalid native progress.')
  return { completed, total }
}
type Listener = (value: unknown, id: string) => void

export class NativeClient {
  readonly session = crypto.randomUUID()
  private disposed = false
  private readonly listeners = new Map<string, Listener>()
  private readonly aborts = new Map<string, () => void>()
  private readonly notices: (() => void)[] = []
  private readonly bridge: Bridge | undefined
  constructor(bridge: Bridge | undefined) { this.bridge = bridge }
  get available(): boolean { return !!this.bridge && !this.disposed }
  async connect(): Promise<Ready> {
    return this.invoke('core.connect', {}, value => {
      const v = object(value)
      return { commands: strings(v.commands), preferenceNamespace: text(v.preferenceNamespace), maximumTextLength: number(v.maximumTextLength) }
    })
  }
  invoke<T>(command: string, args: Json, decode: Decoder<T>): Promise<T> { return this.start(command, args, decode).result }
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
        this.bridge?.SendRawMessage(asciiJson({ session: this.session, kind: 'disconnect' }))
      }, 23000)
    })
    try {
      const wire = await Promise.race([this.bridge.InvokeDotNet('Invoke', [request]), obsolete])
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
  receive(raw: unknown): void {
    if (this.disposed || typeof raw !== 'string' || raw.length > 100000) return
    try {
      const event = object(JSON.parse(raw))
      if (event.version !== 1 || event.session !== this.session) return
      this.listeners.get(text(event.topic))?.(event.data, text(event.id))
    } catch (error) { console.warn('Rejected malformed native event', error) }
  }
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
  dispose(): void {
    if (this.disposed) return
    this.disposed = true
    try { this.bridge?.SendRawMessage(asciiJson({ session: this.session, kind: 'disconnect' })) }
    catch { /* The transport may already be gone; local cleanup must still complete. */ }
    this.listeners.clear()
    for (const abort of this.aborts.values()) abort()
    this.aborts.clear()
    for (const cleanup of this.notices) cleanup()
    this.notices.length = 0
  }
}

let defaultClient: NativeClient | undefined
let defaultConnection: Promise<NativeClient> | undefined

/** Connects the document's shared client lazily; rejects when no matching native host is available. */
export function connect(): Promise<NativeClient> {
  if (defaultClient?.available && !defaultConnection) return Promise.resolve(defaultClient)
  if (defaultConnection) return defaultConnection
  const connection = loadNativeBridge().then(async bridge => {
    if (defaultConnection !== connection) throw new NativeError('cancelled', 'The frontend document disconnected.')
    if (!bridge) throw new NativeError('native_unavailable', 'Native host unavailable. Launch a matching MAUI app; browser mode does not emulate native APIs.')
    const client = new NativeClient(bridge)
    defaultClient = client
    try {
      client.attachToDocument()
      await client.connect()
      if (!client.available || defaultConnection !== connection) throw new NativeError('cancelled', 'The frontend document disconnected.')
      return client
    } catch (error) {
      client.dispose()
      if (defaultClient === client) defaultClient = undefined
      throw error
    }
  })
  defaultConnection = connection
  const clear = () => { if (defaultConnection === connection) defaultConnection = undefined }
  void connection.then(clear, clear)
  return connection
}

/** Disposes the shared client and cancels startup; a later connect creates a new session. */
export function disconnect(): void {
  const client = defaultClient
  defaultClient = undefined
  defaultConnection = undefined
  client?.dispose()
}
