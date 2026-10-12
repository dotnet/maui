import { ack, connect, nullableText, object } from '../core.js'
import type { NativeClient } from '../core.js'

export const clipboard = (client?: NativeClient) => ({
  writeText: async (text: string) => (client ?? await connect()).invoke('clipboard.write', { text }, ack),
  readText: async () => (client ?? await connect()).invoke('clipboard.read', {}, value => nullableText(object(value).text)),
})
