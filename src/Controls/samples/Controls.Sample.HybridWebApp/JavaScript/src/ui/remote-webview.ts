import { boolean, connect, enumValue, NativeError, nullableText, object, subscribeShared } from '../core.js'

/** SDK actions forwarded to the owning native guest; no remote script execution. */
export type BrowserAction = 'back' | 'forward' | 'reload' | 'focus' | 'hide' | 'close'
/** Closed native guest lifecycle/navigation state, separate from error text. */
export type BrowserStatus = 'closed' | 'loading' | 'loaded' | 'failed' | 'blocked'
/** Actual MAUI navigation completion result, or null before completion. */
export type WebNavigationResult = 'success' | 'cancel' | 'timeout' | 'failure'

/** Measured CSS slot/viewport clipping geometry; native maps to actual host DPI/layout. */
export interface BrowserBounds {
  /** Full slot left. */
  x: number
  /** Full slot top. */
  y: number
  /** Full slot width. */
  width: number
  /** Full slot height. */
  height: number
  /** Visible intersection left. */
  clipX: number
  /** Visible intersection top. */
  clipY: number
  /** Visible intersection width. */
  clipWidth: number
  /** Visible intersection height. */
  clipHeight: number
  /** CSS viewport width. */
  viewportWidth: number
  /** CSS viewport height. */
  viewportHeight: number
  /** False while hidden/obscured/outside clipping viewport. */
  visible: boolean
}
/** Native navigation feedback only, never a remote DOM readback. */
export interface BrowserNavigation {
  /** Closed/loading/loaded/failed/blocked feedback, never combined with error text. */
  status: BrowserStatus
  /** Native policy error detail, null when absent; not a status discriminator. */
  error: string | null
  /** Actual native completion result, null before navigation completes. */
  navigationResult: WebNavigationResult | null
  /** Current native back-history availability. */
  canGoBack: boolean
  /** Current native forward-history availability. */
  canGoForward: boolean
}
/** A scoped ordinary native browsing surface, not an iframe or DOM element. */
export interface BrowserView {
  /** Owning native guest identity. */
  readonly id: string
  /** Reads actual current native top-level URL. @returns Current public HTTPS URL. */
  url(): Promise<string>
  /**
   * Navigates the same ordinary native guest.
   * @param url - Bounded public HTTPS website; native rejects unsafe/local targets.
   * @returns Native navigation acceptance, not network success.
   */
  navigate(url: string): Promise<void>
  /** Reads actual native navigation feedback/history. @returns Current navigation state. */
  navigation(): Promise<BrowserNavigation>
  /** Goes back when native history allows. @returns Native acceptance. */
  goBack(): Promise<void>
  /** Goes forward when native history allows. @returns Native acceptance. */
  goForward(): Promise<void>
  /** Reloads only this ordinary native guest. @returns Native acceptance. */
  reload(): Promise<void>
  /** Requests actual native guest focus. @returns Acceptance; native may reject focus. */
  focus(): Promise<void>
  /** Hides this guest before DOM overlays. @returns Native hide completion. */
  hide(): Promise<void>
  /** Closes this guest and native handlers. @returns Native close completion. */
  close(): Promise<void>
  /**
   * Applies freshly measured/clipped geometry; visible controls native exposure.
   * @param bounds - Full CSS slot, intersection and viewport coordinates.
   * @returns Native geometry acceptance.
   */
  setBounds(bounds: BrowserBounds): Promise<void>
  /**
   * Subscribes to this resource's actual native navigation changes.
   * @param listener - Receives bounded navigation feedback, not script/DOM results.
   * @returns Idempotent unsubscribe.
   */
  onNavigation(listener: (state: BrowserNavigation) => void): Promise<() => Promise<void>>
}
interface Snapshot extends BrowserNavigation { supported: boolean; open: boolean; id: string | null; url: string | null }
function decode(value: unknown): Snapshot {
  const v = object(value)
  return { supported: boolean(v.supported), open: boolean(v.open), id: nullableText(v.id), url: nullableText(v.url),
    status: enumValue(v.status, ['closed', 'loading', 'loaded', 'failed', 'blocked']),
    error: nullableText(v.error),
    navigationResult: v.navigationResult === null ? null : enumValue<WebNavigationResult>(v.navigationResult, ['success', 'cancel', 'timeout', 'failure']),
    canGoBack: boolean(v.canGoBack), canGoForward: boolean(v.canGoForward) }
}
function validateUrl(url: string): void {
  let parsed: URL
  try { parsed = new URL(url) } catch { throw new NativeError('invalid_url', 'Use an absolute HTTPS website.') }
  if (url.length > 2048 || parsed.protocol !== 'https:' || parsed.username || parsed.password || parsed.port)
    throw new NativeError('invalid_url', 'Use bounded public HTTPS without credentials/custom ports.')
}
/**
 * Checks whether this host supports the desktop native guest proof.
 * @returns True on supported desktop hosts; no iframe fallback.
 */
export async function supportsBrowserView(): Promise<boolean> {
  return ((await (await connect()).invoke('remote.info', {}, decode))).supported
}
/**
 * Creates an initially hidden ordinary native guest owned by this document/window.
 * @param url - Bounded public HTTPS website.
 * @returns Behavioral browser resource; supply measured bounds to show it.
 * @throws `NativeError` for unsupported mobile hosts, invalid destinations or native failure.
 * @remarks No SDK, invoke target, message bridge or inspector attaches to remote
 * content. Close on component teardown and hide before custom DOM overlays.
 * Native validates top-level navigation; this is not a DNS/subresource firewall.
 */
export async function createBrowserView(url: string): Promise<BrowserView> {
  validateUrl(url)
  const c = await connect(), initial = await c.invoke('remote.open', { url }, decode)
  if (!initial.id) throw new NativeError('invalid_response', 'Native guest creation did not return an identity.')
  const id = initial.id
  const read = async () => {
    const value = await c.invoke('remote.info', {}, decode)
    if (value.id !== id) throw new NativeError('invalid_view', 'This browser resource was closed or replaced.')
    return value
  }
  const control = async (action: BrowserAction) => { await c.invoke('remote.control', { id, action }, decode) }
  return {
    id,
    url: async () => { const value = await read(); if (!value.url) throw new NativeError('invalid_response', 'Native guest URL unavailable.'); return value.url },
    navigate: async url => { validateUrl(url); await c.invoke('remote.navigate', { id, url }, decode) },
    navigation: async () => { const v = await read(); return { status: v.status, error: v.error, navigationResult: v.navigationResult, canGoBack: v.canGoBack, canGoForward: v.canGoForward } },
    goBack: () => control('back'), goForward: () => control('forward'), reload: () => control('reload'), focus: () => control('focus'),
    hide: () => control('hide'), close: () => control('close'),
    setBounds: async bounds => {
      if (Object.values(bounds).some(value => typeof value === 'number' && !Number.isFinite(value))) throw new NativeError('invalid_arguments', 'Geometry must be finite.')
      await c.invoke('remote.bounds', { id, ...bounds }, decode)
    },
    onNavigation: listener => subscribeShared(c, 'remote.changed', decode, value => {
      if (value.id === id) listener({ status: value.status, error: value.error, navigationResult: value.navigationResult, canGoBack: value.canGoBack, canGoForward: value.canGoForward })
    }),
  }
}
