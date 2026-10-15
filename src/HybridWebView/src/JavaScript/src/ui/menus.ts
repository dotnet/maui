import { NativeError } from '../core.js'
import { menuStates, prepare } from '../internal/menu-installation.js'

/** SDK descriptor kind selecting a concrete native menu class, not an open string. */
export type MenuKind = 'submenu' | 'item' | 'check' | 'separator'
/** TypeScript menu node; functions stay in JS and are never serialized/evaluated by native. */
export interface MenuDefinition {
  /** Unique installation-local ID, 1–64 ASCII letters/digits/hyphens. */
  id: string
  /** Native label, at most 160 characters. */
  text: string
  /** Actual native submenu/item/check/separator kind. */
  kind: MenuKind
  /** Whether native enables the item; defaults true. */
  enabled?: boolean
  /** Native check state; requires capability supportsChecked. */
  checked?: boolean
  /** Native key string and platform-validated modifiers. */
  shortcut?: MenuShortcut
  /** Children of a submenu only; at most four nesting levels/128 total nodes. */
  items?: MenuDefinition[]
  /**
   * Runs in this JS document on an actual native click, never in C#.
   * @param checked - Native updated check state.
   */
  action?: (checked: boolean) => void | Promise<void>
}
/** MAUI keyboard accelerator options; primary means Cmd on Mac and Ctrl on Windows. */
export interface MenuShortcut {
  /** Native key string, including named keys where the platform recognizes them. */
  key: string
  /** Explicit modifier combination; defaults to primary. Cmd/Windows are platform-specific. */
  modifiers?: MenuModifier[]
}
/** Modifier flags supported by MAUI, plus the portable primary convenience. */
export type MenuModifier = 'primary' | 'shift' | 'ctrl' | 'alt' | 'cmd' | 'windows'
/** Actual native renderer capabilities and current installation identity. */
export interface MenuInfo {
  /** Real desktop native menu support, false on mobile. */
  supported: boolean
  /** Native checks supported on Mac Catalyst; Windows MAUI toggle items currently unsupported. */
  supportsChecked: boolean
  /** Current owning installation UUID, null when absent. */
  sourceId: string | null
}
/** Bounded native item changes; action callbacks are unaffected. */
export interface MenuPatch {
  /** Replacement native label. */
  text?: string
  /** Native enabled state. */
  enabled?: boolean
  /** Native check state on a supported check item. */
  checked?: boolean
}
/** Native item resource, not a plain object whose mutation secretly performs IPC. */
export interface MenuItem {
  /** Stable installation-local identity. */
  readonly id: string
  /** Reads actual native text. @returns Current native label. */
  text(): Promise<string>
  /**
   * Patches only this native label.
   * @param value - Replacement native label.
   * @returns Native acceptance.
   */
  setText(value: string): Promise<void>
  /** Reads actual native enabled state. @returns Current native enabled flag. */
  enabled(): Promise<boolean>
  /**
   * Patches only this native enabled flag.
   * @param value - Native enabled state.
   * @returns Native acceptance.
   */
  setEnabled(value: boolean): Promise<void>
  /** Reads actual check state. @returns Native checkbox flag; other kinds/adapters reject. */
  checked(): Promise<boolean>
  /**
   * Patches only this actual native check item.
   * @param value - Native checked state.
   * @returns Native acceptance.
   */
  setChecked(value: boolean): Promise<void>
}
/** A JS-created menu resource; attachment uses Window.setMenu, not hidden property IPC. */
export class Menu {
  private constructor(items: MenuDefinition[], onError: (error: unknown) => void) {
    menuStates.set(this, { definitions: items, onError })
  }
  /**
   * Creates a validated JS menu resource with JS-only action callbacks.
   * @param items - Bounded submenu/item/separator/check definitions.
   * @param onError - Async action failure reporter.
   * @returns A menu ready for explicit attachment to one native window.
   */
  static create(items: MenuDefinition[], onError: (error: unknown) => void = error => console.error('Native menu callback failed', error)): Menu {
    prepare(items)
    return new Menu(items, onError)
  }
  /** Native installation identity, null until explicitly attached. */
  get sourceId(): string | null { return menuStates.get(this)?.installation?.sourceId ?? null }
  /**
   * Gets a stable item handle owned by this menu resource.
   * @param id - Item identity.
   * @returns Native async getter/setter handle; stale or unattached installations reject.
   */
  item(id: string): MenuItem { return this.installed().item(id) }
  /**
   * Applies one explicit native item patch.
   * @param id - Item identity.
   * @param patch - Bounded native label/enabled/checked changes.
   * @returns Native update acceptance.
   */
  update(id: string, patch: MenuPatch): Promise<void> { return this.installed().update(id, patch) }
  /** Removes native attachment and action listeners. @returns Cleanup completion. */
  async dispose(): Promise<void> { await menuStates.get(this)?.installation?.dispose() }
  private installed() {
    const installed = menuStates.get(this)?.installation
    if (!installed || installed.retired) throw new NativeError('disconnected', 'This menu is unattached, replaced or disposed.')
    return installed
  }
}
