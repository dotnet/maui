import { connect, nullableText, object, text } from '../core.js'
import type { NativeClient } from '../core.js'

/** A validated text setting in the native host's owned namespace. */
export interface Preference {
  /** Logical key, excluding the host-owned namespace prefix. */
  key: string
  /** Stored text, or null when missing or removed. */
  value: string | null
}

/** Namespace-restricted native text settings; not secure storage or browser localStorage. */
export interface Preferences {
  /**
   * Reads a setting.
   * @param key - Logical key; the reference host accepts 1–48 ASCII letters, digits, underscores or hyphens.
   * @returns The validated key and stored value, with null for a missing setting.
   * @throws `NativeError` for unavailable native, invalid arguments, provider rejection or invalid output.
   * @remarks Requires the host's `preferences.read` provider; native validation is authoritative.
   */
  get(key: string): Promise<Preference>
  /**
   * Writes a text setting.
   * @param key - Logical key; the reference host accepts 1–48 ASCII letters, digits, underscores or hyphens.
   * @param value - Text to store; the reference host allows at most 4096 UTF-16 code units.
   * @returns The validated key and stored text.
   * @throws `NativeError` for unavailable native, invalid arguments, provider rejection or invalid output.
   * @remarks Requires `preferences.write`. Do not store credentials; there is no namespace override or secure-storage guarantee.
   */
  set(key: string, value: string): Promise<Preference>
  /**
   * Removes a setting from the host-owned namespace.
   * @param key - Logical key; the reference host accepts 1–48 ASCII letters, digits, underscores or hyphens.
   * @returns The validated key with a null value, including when it was already missing.
   * @throws `NativeError` for unavailable native, invalid arguments, provider rejection or invalid output.
   * @remarks Requires `preferences.remove`; no clear-all operation is exposed.
   */
  remove(key: string): Promise<Preference>
}

function preference(value: unknown): Preference {
  const v = object(value)
  return { key: text(v.key), value: nullableText(v.value) }
}
/**
 * Creates native preferences helpers without making a native call.
 * @param client - Exact connected client to use; omitted to lazily connect the shared document client on each operation.
 * @returns Text-setting operations scoped to the namespace chosen by the native host.
 * @remarks
 * Requires a compatible protocol-1 host with explicitly registered preferences providers.
 * The handshake reports the namespace and text limit; native validation remains authoritative.
 * An explicit client is neither connected, attached nor disposed by this helper.
 * Native settings can survive document reloads; requests are not replayed after disconnect.
 * @example
 * ```ts
 * import { preferences } from '@maui/api/essentials/preferences'
 * await preferences().set('theme', 'dark')
 * const theme = await preferences().get('theme')
 * ```
 */
export function preferences(client?: NativeClient): Preferences {
  return {
    get: async (key: string) => (client ?? await connect()).invoke('preferences.read', { key }, preference),
    set: async (key: string, value: string) => (client ?? await connect()).invoke('preferences.write', { key, value }, preference),
    remove: async (key: string) => (client ?? await connect()).invoke('preferences.remove', { key }, preference),
  }
}
