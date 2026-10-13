import { boolean, nullableText, object, text, NativeError } from '../core.js'
import type { NativeClient, Json } from '../core.js'
import type { Menu, MenuDefinition, MenuInfo, MenuItem, MenuPatch } from '../ui/menus.js'
interface MenuState {
  definitions: MenuDefinition[]
  onError: (error: unknown) => void
  installation?: Installed
}
export const menuStates = new WeakMap<Menu, MenuState>()
const publicMenus = new WeakMap<Installed, Menu>()
/** Owning-window menu installation created from TypeScript. */
interface MenuInstallation {
  /** Native source identity; isolates late clicks from replaced installations. */
  readonly sourceId: string
  /**
   * Atomically validates and applies one native item patch.
   * @param id - Owning installation's item identity.
   * @param patch - Native label/enabled/checked changes.
   */
  update(id: string, patch: MenuPatch): Promise<void>
  /**
   * Gets an installation-owned item handle; the ID is not global authority.
   * @param id - Item identity in this live menu.
   * @returns Stable item resource whose native getters/setters retain source/window/client ownership.
   */
  item(id: string): MenuItem
  /**
   * Updates a real native label.
   * @param id - This installation's item ID.
   * @param value - Bounded native label.
   */
  setText(id: string, value: string): Promise<void>
  /**
   * Enables/disables a native item/submenu.
   * @param id - This installation's item ID.
   * @param value - Actual native enabled state.
   */
  setEnabled(id: string, value: boolean): Promise<void>
  /**
   * Updates a native checked item, not a label-prefix emulation.
   * @param id - This installation's check item.
   * @param value - Actual native check state.
   */
  setChecked(id: string, value: boolean): Promise<void>
  /** Removes the native menu and JS callback subscription. @returns Cleanup completion. */
  dispose(): Promise<void>
}
/** TS-driven native menu creation and capabilities, not predefined C# action recipes. */
export interface WindowMenu {
  /** Reads real native capabilities. @returns Supported platform/check state and owning source identity. */
  getCapabilities(): Promise<MenuInfo>
  /**
   * Replaces the owning window's menu from JS descriptions and subscribes its JS callbacks.
   * @param menu - Validated Menu.create resource retaining its JS-only callbacks.
   * @returns The owning installation's update/disposal handle.
   */
  set(menu: Menu): Promise<Menu>
  /** Returns the current live document-owned installation. @returns Existing menu or null; no default C# menu is fabricated. */
  current(): Promise<Menu | null>
  /**
   * Updates the current owning installation.
   * @param id - Installation-local item identity.
   * @param patch - Text/enabled/checked changes; native validates platform support.
   */
  update(id: string, patch: MenuPatch): Promise<void>
  /** Removes this owning window's menu and callbacks. @returns Cleanup completion. */
  remove(): Promise<void>
}
function info(value: unknown): MenuInfo {
  const v = object(value)
  return { supported: boolean(v.supported), supportsChecked: boolean(v.supportsChecked), sourceId: nullableText(v.sourceId) }
}
interface Installed extends MenuInstallation {
  callbacks: Map<string, (checked: boolean) => void | Promise<void>>
  onError: (error: unknown) => void
  retired: boolean
}
interface Binding {
  active?: Installed
  release: () => Promise<void>
}
const bindings = new WeakMap<NativeClient, Binding>()
const queues = new WeakMap<NativeClient, Promise<void>>()
function serial<T>(client: NativeClient, action: () => Promise<T>): Promise<T> {
  const result = (queues.get(client) ?? Promise.resolve()).then(action)
  queues.set(client, result.then(() => {}, () => {}))
  return result
}
export function prepare(items: MenuDefinition[]): { tree: Json[]; callbacks: Map<string, (checked: boolean) => void | Promise<void>> } {
  const callbacks = new Map<string, (checked: boolean) => void | Promise<void>>()
  if (!Array.isArray(items) || items.length < 1 || items.length > 8 || items.some(item => item.kind !== 'submenu'))
    throw new NativeError('invalid_arguments', 'Install one to eight top-level submenus.')
  const serialize = (item: MenuDefinition, depth: number): Json => {
    if (!item || depth > 4 || callbacks.size >= 128 || typeof item.id !== 'string' ||
      !/^[a-zA-Z0-9-]{1,64}$/.test(item.id) || callbacks.has(item.id) ||
      typeof item.text !== 'string' || item.text.length > 160 ||
      !['submenu', 'item', 'check', 'separator'].includes(item.kind) ||
      (item.kind === 'submenu' ? !Array.isArray(item.items) : item.items !== undefined) ||
      (item.action !== undefined && typeof item.action !== 'function'))
      throw new NativeError('invalid_arguments', 'Use valid unique menu IDs/kinds, bounded labels and at most four levels/128 nodes.')
    if (item.shortcut && (typeof item.shortcut.key !== 'string' || item.shortcut.key.length < 1 ||
      item.shortcut.key.length > 64 || /[\u0000-\u001f\u007f]/.test(item.shortcut.key) ||
      !['item', 'check'].includes(item.kind)))
      throw new NativeError('invalid_arguments', 'Native accelerators require an actionable item and native key string.')
    const modifiers = item.shortcut?.modifiers ?? ['primary']
    if (modifiers.length > 5 || new Set(modifiers).size !== modifiers.length ||
      modifiers.some(value => !['primary', 'shift', 'ctrl', 'alt', 'cmd', 'windows'].includes(value)))
      throw new NativeError('invalid_arguments', 'Use unique supported accelerator modifiers.')
    callbacks.set(item.id, item.action ?? (() => {}))
    return { id: item.id, text: item.text, kind: item.kind, enabled: item.enabled ?? true,
      checked: item.checked ?? false, items: item.items?.map(child => serialize(child, depth + 1)) ?? null,
      shortcut: item.shortcut ? { key: item.shortcut.key, modifiers } : null }
  }
  return { tree: items.map(item => serialize(item, 0)), callbacks }
}

export function createWindowMenu(client: NativeClient, windowId: string): WindowMenu {
  const connected = async () => client
  let installedMenu: Installed | undefined
  return {
    getCapabilities: async () => (await connected()).invoke('menus.info', { windowId }, info),
    current: async () => {
      const c = await connected(), native = await c.invoke('menus.info', { windowId }, info), active = bindings.get(c)?.active
      return active && !active.retired && active.sourceId === native.sourceId ? publicMenus.get(active) ?? null : null
    },
    update: async (id, patch) => {
      if (!installedMenu) throw new NativeError('disconnected', 'Set this window menu before updating items.')
      await installedMenu.update(id, patch)
    },
    remove: async () => { await bindings.get(await connected())?.active?.dispose(); installedMenu = undefined },
    set: resource => serial(client, async () => {
      const state = menuStates.get(resource)
      if (!state) throw new NativeError('invalid_arguments', 'Attach a Menu.create resource, not an arbitrary object.')
      if (state.installation) throw new NativeError('invalid_menu', 'A menu resource may be attached once; create a fresh resource to replace it.')
      const { tree, callbacks } = prepare(state.definitions)
      const onError = state.onError
      const c = await connected()
      let binding = bindings.get(c)
      if (!binding) {
        const created: Binding = { release: async () => {} }
        created.release = await c.subscribe('menus.clicked', value => {
          const v = object(value)
          return { sourceId: text(v.sourceId), id: text(v.id), checked: boolean(v.checked) }
        }, event => {
          const active = created.active
          if (!active || active.retired || event.sourceId !== active.sourceId) return
          const action = active.callbacks.get(event.id)
          if (action) Promise.resolve().then(() => {
            if (created.active === active && !active.retired && c.available) return action(event.checked)
          }).catch(active.onError)
        })
        binding = created
        bindings.set(c, binding)
      }
      let source: string | null = null
      try {
        const installed = await c.invoke('menus.install', { windowId, items: tree }, info)
        source = installed.sourceId
        if (!source) throw new NativeError('invalid_response', 'Native menu installation has no identity.')
      } catch (error) {
        if (!binding.active) { await binding.release(); bindings.delete(c) }
        throw error
      }
      const sourceId = source
      const update = async (id: string, patch: MenuPatch) => {
        if (menu.retired) throw new NativeError('disconnected', 'This menu installation was removed.')
        await c.invoke('menus.update', { windowId, sourceId, id, ...patch }, info)
      }
      const menu: Installed = {
        sourceId, callbacks, onError, retired: false, update,
        item: id => {
          if (menu.retired || !callbacks.has(id)) throw new NativeError('invalid_menu', 'This menu item does not belong to the live installation.')
          const read = async () => {
            if (menu.retired) throw new NativeError('disconnected', 'This menu installation was replaced or removed.')
            return c.invoke('menus.item', { windowId, sourceId, id }, value => {
              const v = object(value)
              return { text: text(v.text), enabled: boolean(v.enabled), checked: v.checked === null ? null : boolean(v.checked) }
            })
          }
          return {
            id, text: async () => (await read()).text, enabled: async () => (await read()).enabled,
            checked: async () => {
              const value = (await read()).checked
              if (value === null) throw new NativeError('unsupported', 'This native menu item is not a checkbox.')
              return value
            },
            setText: value => update(id, { text: value }), setEnabled: value => update(id, { enabled: value }),
            setChecked: value => update(id, { checked: value }),
          }
        },
        setText: (id, value) => update(id, { text: value }),
        setEnabled: (id, value) => update(id, { enabled: value }),
        setChecked: (id, value) => update(id, { checked: value }),
        dispose: () => serial(c, async () => {
          if (menu.retired) return
          if (binding.active === menu) {
            if (c.available) await c.invoke('menus.clear', { windowId, sourceId }, info)
            binding.active = undefined
            await binding.release()
            bindings.delete(c)
          }
          menu.retired = true
          callbacks.clear()
        }),
      }
      if (binding.active) { binding.active.retired = true; binding.active.callbacks.clear() }
      binding.active = menu
      state.installation = menu
      publicMenus.set(menu, resource)
      installedMenu = menu
      return resource
    }),
  }
}
