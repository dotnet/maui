import { ack, boolean, nullableText, number, object, progress, strings, text } from '@maui/api/core'
import type { NativeClient, Decoder } from '@maui/api/core'
import { getAppInfo } from '@maui/api/essentials/app-info'
import type { AppInfo } from '@maui/api/essentials/app-info'
import { getDeviceInfo } from '@maui/api/essentials/device-info'
import type { DeviceInfo } from '@maui/api/essentials/device-info'

/** Playground-only process diagnostics; these are not MAUI Essentials metadata. */
export interface Diagnostics {
  /** Native process architecture, not a device architecture promise. */
  architecture: string
  /** Native process ID used to prove frontend reload does not restart the host. */
  pid: number
  /** Native startup UUID, stable until the host process restarts. */
  startupId: string
  /** Native process startup timestamp reported by the host. */
  startedAt: string
}
/** Existing playground UI shape composed from Essentials metadata and app-owned diagnostics. */
export interface Identity extends AppInfo, DeviceInfo, Diagnostics {}

/**
 * Reads the playground's reload identity while exercising the installed SDK helpers.
 * @param client - Exact connected document client shared by all three native reads.
 * @returns App/device metadata and the app-owned process diagnostics.
 * @remarks Protocol 1's existing app.info command supplies all three subsets; no native contracts change.
 */
export async function getIdentity(client: NativeClient): Promise<Identity> {
  const [app, device, diagnostics] = await Promise.all([
    getAppInfo(client),
    getDeviceInfo(client),
    client.invoke('app.info', {}, value => {
      const v = object(value)
      return { architecture: text(v.architecture), pid: number(v.pid), startupId: text(v.startupId), startedAt: text(v.startedAt) }
    }),
  ])
  return { ...app, ...device, ...diagnostics }
}

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
