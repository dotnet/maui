import { connect, disconnect, NativeClient, NativeError, clipboard as rootClipboard } from '@maui/api'
import type { Bridge, Decoder, Json, Operation, Progress, Ready, Identity } from '@maui/api'
import { ack, object, progress, asciiJson } from '@maui/api/core'
import { clipboard } from '@maui/api/essentials/clipboard'
import { preferences } from '@maui/api/essentials/preferences'
import { getInfo } from '@maui/api/app/info'

function check(value: boolean): void { if (!value) throw new Error('Packed consumer assertion failed') }
const identity: Identity = { appName: 'Consumer', version: '1', build: '1', platform: 'test', osVersion: '1', model: 'test',
  architecture: 'arm64', pid: 1, startupId: 'startup', startedAt: '2026-10-12T00:00:00Z' }
const ready: Ready = { commands: ['clipboard.read', 'app.info'], preferenceNamespace: 'consumer.', maximumTextLength: 4096 }
const bridge: Bridge = {
  async InvokeDotNet(method, [wire]) {
    check(method === 'Invoke')
    const request = object(JSON.parse(wire!))
    const args = object(request.args)
    const result: Json = request.command === 'core.connect' ? { ...ready } : request.command === 'app.info' ? { ...identity }
      : request.command === 'clipboard.read' ? { text: '你好 👋' } : request.command === 'preferences.read' ? { key: args.key as string, value: null }
      : request.command === 'custom.work' ? { completed: 1, total: 1 } : { accepted: true }
    return JSON.stringify({ version: 1, session: request.session, id: request.id, status: 'success', result, error: null })
  },
  SendRawMessage() {},
}
const client = new NativeClient(bridge)
try {
  const connected: Ready = await client.connect()
  check(connected.preferenceNamespace === 'consumer.')
  const text: string | null = await clipboard(client).readText()
  check(text === '你好 👋')
  const accepted: boolean = await rootClipboard(client).writeText('é')
  check(accepted)
  const setting: { key: string; value: string | null } = await preferences(client).get('key')
  check(setting.value === null)
  const info: Identity = await getInfo(client)
  check(info.appName === 'Consumer')
  const decode: Decoder<boolean> = ack
  check(await client.invoke('custom.command', {}, decode))
  const operation: Operation<Progress> = client.start('custom.work', {}, progress)
  check((await operation.result).completed === 1)
  check(await operation.cancel())
  check(/^[\x00-\x7f]*$/.test(asciiJson({ text: '你好 👋' })))
  if (false) {
    // @ts-expect-error The published signature accepts text, not numbers.
    void clipboard(client).writeText(42)
  }
} finally { client.dispose() }
try {
  await connect()
  throw new Error('A plain Node consumer must not emulate native APIs')
} catch (error) {
  check(error instanceof NativeError && error.code === 'native_unavailable')
} finally { disconnect() }
console.log('Packed ESM root/subpaths and TypeScript declarations passed without the SDK source tree.')
