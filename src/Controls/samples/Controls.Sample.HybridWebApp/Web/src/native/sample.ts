import { ack, boolean, nullableText, number, object, progress, strings } from '@maui/api/core'
import type { NativeClient, Decoder } from '@maui/api/core'

export interface EchoInput { text: string | null; number: number; flag: boolean; items: string[] }
export interface EchoResult extends EchoInput { counter: number }
export const counter: Decoder<number> = value => number(object(value).counter)
export const sample = (client: NativeClient) => ({
  echo: (input: EchoInput) => client.invoke('sample.echo', { ...input }, value => {
    const v = object(value)
    return { text: nullableText(v.text), number: number(v.number), flag: boolean(v.flag), items: strings(v.items), counter: number(v.counter) }
  }),
  getCounter: () => client.invoke('sample.counter', {}, counter),
  fail: () => client.invoke('sample.fail', {}, ack),
  disabled: () => client.invoke('sample.disabled', {}, ack),
  missing: () => client.invoke('sample.notRegistered', {}, ack),
  invalid: () => client.invoke('sample.echo', { text: 42 }, ack),
  work: (steps: number, delayMs: number) => client.start('sample.work', { steps, delayMs }, progress),
  onCounter: (listener: (value: number, id: string) => void) => client.subscribe('service.changed', counter, listener),
  onProgress: (listener: (value: { completed: number; total: number }, id: string) => void) => client.subscribe('operation.progress', progress, listener),
})
