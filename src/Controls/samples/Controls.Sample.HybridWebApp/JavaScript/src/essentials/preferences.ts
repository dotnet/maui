import { ack, connect, NativeError, object, text, boolean, number } from '../core.js'
import type { Json } from '../core.js'
/** Supported MAUI primitive preference values; SecureStorage is a different API. */
export type PreferenceValue = string | boolean | number | Date
/** Native preference container/number-storage options; host namespaces remain authoritative. */
export interface PreferenceOptions {
  /** Optional shared store name, isolated inside the host-owned namespace. */
  sharedName?: string
  /** Native numeric primitive representation; defaults to double. */
  numberType?: 'int' | 'long' | 'float' | 'double'
}
function encode(value: PreferenceValue, options: PreferenceOptions): Json {
  if (typeof value === 'string') return { kind: 'string', text: value }
  if (typeof value === 'boolean') return { kind: 'boolean', boolean: value }
  if (value instanceof Date) {
    if (!Number.isFinite(value.getTime())) throw new NativeError('invalid_arguments', 'Preference Date must be valid.')
    return { kind: 'dateTime', text: value.toISOString() }
  }
  if (typeof value !== 'number' || !Number.isFinite(value)) throw new NativeError('invalid_arguments', 'Use a supported native primitive preference.')
  const kind = options.numberType ?? 'double'
  if (kind === 'int' && (!Number.isInteger(value) || value < -2147483648 || value > 2147483647) ||
    kind === 'long' && !Number.isSafeInteger(value) || kind === 'float' && !Number.isFinite(Math.fround(value)))
    throw new NativeError('invalid_arguments', 'Numeric value does not fit the requested native primitive.')
  return kind === 'int' || kind === 'long' ? { kind, integer: value } : { kind, number: value }
}
function decode(value: unknown): PreferenceValue {
  const v = object(value), kind = text(v.kind)
  switch (kind) {
    case 'string': return text(v.text)
    case 'boolean': return boolean(v.boolean)
    case 'double': case 'float': return number(v.number)
    case 'int': case 'long': {
      const value = number(v.integer)
      if (!Number.isSafeInteger(value)) throw new NativeError('invalid_response', 'Native integer cannot be represented safely in JavaScript.')
      return value
    }
    case 'dateTime': {
      const result = new Date(text(v.text))
      if (!Number.isFinite(result.getTime())) throw new NativeError('invalid_response', 'Native date preference is invalid.')
      return result
    }
    default: throw new NativeError('invalid_response', 'Unknown native preference primitive.')
  }
}
/**
 * Reads a native preference with a typed native default.
 * @typeParam T - Supported primitive matching the stored native type.
 * @param key - Host-namespaced logical preference key.
 * @param defaultValue - Value returned by MAUI when absent.
 * @param options - Optional shared container/numeric native representation.
 * @returns Actual native primitive, including defaults when absent.
 */
export async function get<T extends PreferenceValue>(key: string, defaultValue: T, options: PreferenceOptions = {}): Promise<T extends Date ? Date : T extends string ? string : T extends boolean ? boolean : number> {
  return (await connect()).invoke('preferences.get', { key, defaultValue: encode(defaultValue, options), sharedName: options.sharedName ?? null }, value => {
    const result = decode(value)
    if (typeof result !== typeof defaultValue || (result instanceof Date) !== (defaultValue instanceof Date))
      throw new NativeError('invalid_response', 'Native preference does not match the requested primitive type.')
    return result as T extends Date ? Date : T extends string ? string : T extends boolean ? boolean : number
  })
}
/**
 * Writes a supported native primitive.
 * @param key - Logical host-owned key.
 * @param value - String/boolean/numeric/Date value, not a JSON object or secret-storage claim.
 * @param options - Shared container/numeric representation.
 * @returns Native completion.
 */
export async function set(key: string, value: PreferenceValue, options: PreferenceOptions = {}): Promise<void> {
  await (await connect()).invoke('preferences.set', { key, value: encode(value, options), sharedName: options.sharedName ?? null }, ack)
}
/**
 * Tests native key existence.
 * @param key - Logical host-namespaced key.
 * @param options - Optional shared preference container.
 * @returns Native existence flag.
 */
export async function contains(key: string, options: PreferenceOptions = {}): Promise<boolean> {
  return (await connect()).invoke('preferences.contains', { key, sharedName: options.sharedName ?? null }, ack)
}
/**
 * Removes a native key in the host-owned namespace.
 * @param key - Logical key.
 * @param options - Optional shared container.
 * @returns Native completion.
 */
export async function remove(key: string, options: PreferenceOptions = {}): Promise<void> {
  await (await connect()).invoke('preferences.delete', { key, sharedName: options.sharedName ?? null }, ack)
}
/**
 * Clears only tracked keys owned by this host namespace/container, never arbitrary native preference stores.
 * @param options - Optional shared preference container.
 * @returns Native completion.
 * @remarks The bounded persistent key index preserves namespace isolation across native restarts.
 */
export async function clear(options: PreferenceOptions = {}): Promise<void> {
  await (await connect()).invoke('preferences.clear', { sharedName: options.sharedName ?? null }, ack)
}
