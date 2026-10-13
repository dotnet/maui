import { connect, disconnect, NativeClient, NativeError } from '@maui/api'
import * as rootApi from '@maui/api'
import type { Bridge, Decoder, Json, Operation, Progress, Ready } from '@maui/api'
import { ack, object, progress, asciiJson } from '@maui/api/core'
import { clipboard } from '@maui/api/essentials/clipboard'
import type { ClipboardApi } from '@maui/api/essentials/clipboard'
import { preferences } from '@maui/api/essentials/preferences'
import type { Preference, Preferences } from '@maui/api/essentials/preferences'
import { getAppInfo } from '@maui/api/essentials/app-info'
import type { AppInfo } from '@maui/api/essentials/app-info'
import { getDeviceInfo } from '@maui/api/essentials/device-info'
import type { DeviceInfo } from '@maui/api/essentials/device-info'

function check(value: boolean): void { if (!value) throw new Error('Packed consumer assertion failed') }
const identity = { appName: 'Consumer', version: '1', build: '1', platform: 'test', osVersion: '1', model: 'test',
  architecture: 'arm64', pid: 1, startupId: 'startup', startedAt: '2026-10-12T00:00:00Z' }
const ready: Ready = { commands: ['clipboard.read', 'app.info', 'device.info'], preferenceNamespace: 'consumer.', maximumTextLength: 4096 }
const bridge: Bridge = {
  async InvokeDotNet(method, [wire]) {
    check(method === 'Invoke')
    const request = object(JSON.parse(wire!))
    const args = object(request.args)
    const result: Json = request.command === 'core.connect' ? { ...ready } : request.command === 'app.info'
      ? { appName: identity.appName, version: identity.version, build: identity.build }
      : request.command === 'device.info' ? { platform: identity.platform, osVersion: identity.osVersion, model: identity.model }
      : request.command === 'clipboard.read' ? { text: '你好 👋' } : request.command === 'preferences.read' ? { key: args.key as string, value: null }
      : request.command === 'custom.work' ? { completed: 1, total: 1 } : { accepted: true }
    return JSON.stringify({ version: 1, session: request.session, id: request.id, status: 'success', result, error: null })
  },
  SendRawMessage() {},
}
const client = new NativeClient(bridge)
try {
  check(Object.keys(rootApi).sort().join(',') === 'NativeClient,NativeError,connect,disconnect')
  const connected: Ready = await client.connect()
  check(connected.preferenceNamespace === 'consumer.')
  const nativeClipboard: ClipboardApi = clipboard(client)
  const text: string | null = await nativeClipboard.readText()
  check(text === '你好 👋')
  const accepted: boolean = await nativeClipboard.writeText('é')
  check(accepted)
  const nativePreferences: Preferences = preferences(client)
  const setting: Preference = await nativePreferences.get('key')
  check(setting.value === null)
  const info: AppInfo = await getAppInfo(client)
  check(info.appName === 'Consumer')
  check(Object.keys(info).sort().join(',') === 'appName,build,version')
  const device: DeviceInfo = await getDeviceInfo(client)
  check(device.platform === 'test')
  check(Object.keys(device).sort().join(',') === 'model,osVersion,platform')
  const decode: Decoder<boolean> = ack
  check(await client.invoke('custom.command', {}, decode))
  const operation: Operation<Progress> = client.start('custom.work', {}, progress)
  check((await operation.result).completed === 1)
  check(await operation.cancel())
  check(/^[\x00-\x7f]*$/.test(asciiJson({ text: '你好 👋' })))
  if (false) {
    // @ts-expect-error The published signature accepts text, not numbers.
    void clipboard(client).writeText(42)
    // @ts-expect-error Features are not re-exported by the infrastructure root.
    void rootApi.clipboard
    // @ts-expect-error Feature types belong to their Essentials subpaths.
    const rootInfo: import('@maui/api').AppInfo = info
    void rootInfo
    // @ts-expect-error Process diagnostics are not Essentials AppInfo.
    void info.pid
    // @ts-expect-error Process architecture is not DeviceInfo.
    void device.architecture
    // @ts-expect-error The unpublished combined identity path was intentionally removed.
    await import('@maui/api/app/info')
  }
} finally { client.dispose() }
for (const path of ['@maui/api/app/info', '@maui/api/dist/core.js', '@maui/api/src/core.ts']) {
  try {
    await import(path)
    throw new Error(`Private or removed package path resolved: ${path}`)
  } catch (error) {
    check(error instanceof Error && 'code' in error && error.code === 'ERR_PACKAGE_PATH_NOT_EXPORTED')
  }
}
try {
  await connect()
  throw new Error('A plain Node consumer must not emulate native APIs')
} catch (error) {
  check(error instanceof NativeError && error.code === 'native_unavailable')
} finally { disconnect() }
console.log('Packed ESM root/subpaths and TypeScript declarations passed without the SDK source tree.')
