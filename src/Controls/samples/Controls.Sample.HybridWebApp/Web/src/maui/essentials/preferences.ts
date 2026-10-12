import { nullableText, object, text } from '../core.ts'
import type { NativeClient } from '../core.ts'

function preference(value: unknown): { key: string; value: string | null } {
  const v = object(value)
  return { key: text(v.key), value: nullableText(v.value) }
}
export const preferences = (client: NativeClient) => ({
  get: (key: string) => client.invoke('preferences.read', { key }, preference),
  set: (key: string, value: string) => client.invoke('preferences.write', { key, value }, preference),
  remove: (key: string) => client.invoke('preferences.remove', { key }, preference),
})
