import { ack, nullableText, object } from '../core.ts'
import type { NativeClient } from '../core.ts'

export const clipboard = (client: NativeClient) => ({
  writeText: (text: string) => client.invoke('clipboard.write', { text }, ack),
  readText: () => client.invoke('clipboard.read', {}, value => nullableText(object(value).text)),
})
