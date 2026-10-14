import { ack, connect, documentConnectionGeneration, enumValue, NativeError, number, object, subscribeShared } from '../core.js'
import type { NativeClient } from '../core.js'
import { createWindowMenu } from '../internal/menu-installation.js'
import type { WindowMenu } from '../internal/menu-installation.js'
import type { Menu, MenuInfo } from './menus.js'
import { acquiredWindows, windowSnapshot as snapshot } from '../internal/window-ownership.js'
import type { WindowSnapshot } from '../internal/window-ownership.js'

/** Actual native window dimensions in logical units, not CSS pixels. */
export interface WindowSize {
  /** Native logical width. */
  width: number
  /** Native logical height. */
  height: number
}
/** Actual desktop native coordinates in logical units. */
export interface WindowPosition {
  /** Native X coordinate. */
  x: number
  /** Native Y coordinate. */
  y: number
}
/** Application-owned window creation options, selected entirely by the frontend. */
export interface WindowOptions {
  /** Optional app-unique immutable developer ID; native registry rejects duplicates. */
  id?: string
  /** Trusted bundle-relative URL/query/fragment; defaults to index.html, never a remote bridge destination. */
  url?: string
  /** Native title; independent from developer ID. */
  title?: string
  /** Finite positive native desktop width; OS constraints apply. */
  width?: number
  /** Finite positive native desktop height; OS constraints apply. */
  height?: number
}
/** Explicit native operation support, not a mega-information object or permission grant. */
export interface WindowCapabilities {
  /** Native desktop size updates. */
  resize: boolean
  /** Native desktop position updates. */
  position: boolean
  /** Separate app-owned windows; iPhone is excluded. */
  multiWindow: boolean
  /** Native desktop activation. */
  activate: boolean
  /** Actual current-display work-area centering; Windows adapter only in this proof. */
  center: boolean
  /** Verified actual OS-origin move events; Windows only in this proof. */
  positionChanged: boolean
}
interface Binding { client: NativeClient; owner: WindowSnapshot; callerId: string; menu: WindowMenu }

/** Reads the calling window's actual Core IWindow getter through its curated native contract. */
export async function getCurrentFlowDirection(): Promise<'matchParent' | 'leftToRight' | 'rightToLeft'> {
  return (await connect()).invoke('window.flowDirection', {}, value => enumValue(value, ['matchParent', 'leftToRight', 'rightToLeft']))
}

/** One behavioral native window resource, independent of whether acquired as current/listed/opened. */
export class Window {
  private readonly generation = documentConnectionGeneration()
  private binding: Promise<Binding> | undefined
  /**
   * Creates a local lazy window handle without native calls or a fabricated ID.
   * @param requestedId - Optional existing app-owned ID; omitted to bind the calling window on first invocation.
   * @remarks Construction is not authority. Native lookup verifies ownership and
   * a private generation lease prevents stale handles from targeting a reopened ID.
   */
  constructor(private readonly requestedId?: string) {}
  private bound(): Promise<Binding> {
    if (this.generation !== documentConnectionGeneration())
      return Promise.reject(new NativeError('disconnected', 'This window belongs to a retired document connection.'))
    return this.binding ??= (async () => {
      const acquired = acquiredWindows.get(this)
      const client = acquired?.client ?? await connect()
      const caller = await client.invoke('window.info', {}, snapshot)
      const owner = acquired ? await client.invoke('window.info', { windowId: acquired.owner.id, windowLease: acquired.owner.windowLease }, snapshot)
        : (this.requestedId && this.requestedId !== caller.id
        ? await client.invoke('window.find', { id: this.requestedId }, value => {
          const found = object(value).window
          if (found === null) throw new NativeError('invalid_window', 'This window is closed or unavailable to the calling view.')
          return snapshot(found)
        }) : caller)
      return { client, callerId: caller.id, owner, menu: createWindowMenu(client, owner.id) }
    })()
  }
  private async read(): Promise<WindowSnapshot> {
    const { client, owner } = await this.bound()
    const actual = await client.invoke('window.info', { windowId: owner.id, windowLease: owner.windowLease }, snapshot)
    if (actual.id !== owner.id || actual.windowLease !== owner.windowLease)
      throw new NativeError('invalid_window', 'This window generation has closed or changed.')
    return actual
  }
  /** Awaits the native-authoritative immutable developer ID. @returns Real ID, never a placeholder. */
  async id(): Promise<string> { return (await this.bound()).owner.id }
  /** Reads actual native title. @returns Current native title, independent from ID. */
  async title(): Promise<string> { return (await this.read()).title }
  /**
   * Updates only this native title.
   * @param value - Native title without control characters; transport policy applies.
   * @returns Native acceptance.
   */
  async setTitle(value: string): Promise<void> {
    const { client, owner } = await this.bound()
    await client.invoke('window.title', { windowId: owner.id, windowLease: owner.windowLease, title: value }, snapshot)
  }
  /** Reads actual native logical size. @returns Current dimensions; unavailable values reject. */
  async size(): Promise<WindowSize> {
    const v = await this.read()
    if (v.width === null || v.height === null) throw new NativeError('unsupported', 'Native window size is unavailable.')
    return { width: v.width, height: v.height }
  }
  /**
   * Requests this desktop window's logical dimensions.
   * @param size - Finite positive dimensions; native OS constraints apply.
   * @returns Native acceptance; mobile rejects explicitly.
   */
  async setSize(size: WindowSize): Promise<void> {
    const { client, owner } = await this.bound()
    await client.invoke('window.size', { windowId: owner.id, windowLease: owner.windowLease, ...size }, ack)
  }
  /** Reads actual desktop logical coordinates. @returns Current coordinates; mobile/unavailable adapters reject. */
  async position(): Promise<WindowPosition> {
    const v = await this.read()
    if (!v.canPosition || v.x === null || v.y === null) throw new NativeError('unsupported', 'Native position is unsupported.')
    return { x: v.x, y: v.y }
  }
  /**
   * Requests this native desktop position.
   * @param position - Finite logical coordinates; native OS constraints apply.
   * @returns Native acceptance.
   */
  async setPosition(position: WindowPosition): Promise<void> {
    const { client, owner } = await this.bound()
    await client.invoke('window.position', { windowId: owner.id, windowLease: owner.windowLease, ...position }, ack)
  }
  /** Reads this window's frontend-owned bundled entry URL. @returns Native location, not an app route identity. */
  async url(): Promise<string> { return (await this.read()).initialLocation }
  /** Reads this window's JS-owned installed menu. @returns Menu resource or null, never a predefined C# menu. */
  async menu(): Promise<Menu | null> { return (await this.bound()).menu.current() }
  /**
   * Attaches/replaces/removes this owning window's native menu.
   * @param value - Menu.create resource, or null to remove.
   * @returns Installed native/JS menu resource or null.
   * @remarks Menus on another view require its own JS realm/client; IDs do not grant cross-window callbacks.
   */
  async setMenu(value: Menu | null): Promise<Menu | null> {
    const binding = await this.bound()
    if (binding.owner.id !== binding.callerId) throw new NativeError('unsupported', 'Author this menu from that window’s own frontend realm.')
    if (value === null) { await binding.menu.remove(); return null }
    return binding.menu.set(value)
  }
  /** Reads secondary native menu support. @returns Renderer capabilities, not menu recipes. */
  async menuCapabilities(): Promise<MenuInfo> { return (await this.bound()).menu.getCapabilities() }
  /** Reads explicit operation support. @returns Native capabilities, never authorization or a broad information snapshot. */
  async capabilities(): Promise<WindowCapabilities> {
    const v = await this.read()
    return { resize: v.canResize, position: v.canPosition, multiWindow: v.canOpenWindows,
      activate: v.canPosition, center: v.canCenter, positionChanged: v.canObservePosition }
  }
  /** Closes this actual owning/caller-authorized native generation. @returns Native acceptance; owning destruction retires its client. */
  async close(): Promise<boolean> {
    const { client, owner, callerId } = await this.bound()
    return owner.id === callerId ? client.invoke('window.closeCurrent', {}, ack)
      : client.invoke('window.close', { id: owner.id, windowLease: owner.windowLease }, ack)
  }
  /** Activates this attached desktop window. @returns Native acceptance; mobile rejects. */
  async activate(): Promise<void> { await this.control('window.activate') }
  /** Centers this window using actual current display work area, size and density. @returns Native acceptance; unsupported adapters never approximate the primary screen. */
  async center(): Promise<void> { await this.control('window.center') }
  private async control(command: string): Promise<void> {
    const { client, owner } = await this.bound()
    await client.invoke(command, { windowId: owner.id, windowLease: owner.windowLease }, ack)
  }
  /**
   * Observes actual paired/coalesced native dimensions.
   * @param listener - Receives actual width/height after native frame updates.
   * @returns Unsubscribe; multiple listeners share one native source.
   */
  async onSizeChanged(listener: (size: WindowSize) => void): Promise<() => Promise<void>> {
    const { client, owner, callerId } = await this.bound()
    if (owner.id !== callerId) throw new NativeError('unsupported', 'Observe geometry from that window’s own frontend.')
    return subscribeShared(client, 'window.geometryChanged', object, event => {
      if (event.windowId === owner.id && event.size !== null) {
        const v = object(event.size); listener({ width: number(v.width), height: number(v.height) })
      }
    })
  }
  /**
   * Observes verified actual native moves, not events fabricated from SDK setters.
   * @param listener - Receives a coalesced X/Y pair.
   * @returns Unsubscribe; currently Windows only.
   */
  async onPositionChanged(listener: (position: WindowPosition) => void): Promise<() => Promise<void>> {
    const { client, owner, callerId } = await this.bound()
    if (owner.id !== callerId || !(await this.read()).canObservePosition)
      throw new NativeError('unsupported', 'Actual native move events are verified on the owning Windows frontend only.')
    return subscribeShared(client, 'window.geometryChanged', object, event => {
      if (event.windowId === owner.id && event.position !== null) {
        const v = object(event.position); listener({ x: number(v.x), y: number(v.y) })
      }
    })
  }
}
/**
 * Gets a synchronous local document-bound handle without identity fabrication or native calls.
 * @returns The same Window resource type used by app open/list/lookup.
 */
export function currentWindow(): Window { return new Window() }
