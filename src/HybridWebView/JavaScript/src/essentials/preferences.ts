import { connect, nullableText, object, text } from '../core.js'
import type { NativeClient } from '../core.js'

function preference(value: unknown): { key: string; value: string | null } {
  const v = object(value)
  return { key: text(v.key), value: nullableText(v.value) }
}
export const preferences = (client?: NativeClient) => ({
  get: async (key: string) => (client ?? await connect()).invoke('preferences.read', { key }, preference),
  set: async (key: string, value: string) => (client ?? await connect()).invoke('preferences.write', { key, value }, preference),
  remove: async (key: string) => (client ?? await connect()).invoke('preferences.remove', { key }, preference),
})
