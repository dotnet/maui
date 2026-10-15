import * as root from '@maui/api'
import { NativeError, Channel, binaryBlob, invokeBinary, connect, disconnect } from '@maui/api'
import type { Bridge, Json } from '@maui/api'
import { configureBridge, resetBridge } from '@maui/api/core'
import { getAppInfo } from '@maui/api/essentials/app-info'
import { getDeviceInfo } from '@maui/api/essentials/device-info'
import { getText, setText, hasText } from '@maui/api/essentials/clipboard'
import * as preferences from '@maui/api/essentials/preferences'
import { getBatteryInfo } from '@maui/api/essentials/battery'
import { getDisplayInfo } from '@maui/api/essentials/display'
import { getConnectivity, onConnectivityChanged } from '@maui/api/essentials/connectivity'
import { getLocales, speak } from '@maui/api/essentials/text-to-speech'
import type { SpeechLocale, SpeechOptions } from '@maui/api/essentials/text-to-speech'
import { openUrl } from '@maui/api/essentials/launcher'
import { getLocation } from '@maui/api/essentials/geolocation'
import { stageFile, share, releaseFile } from '@maui/api/essentials/share'
import { pickFile, pickMultipleFiles } from '@maui/api/essentials/file-picker'
import { alert, confirm, prompt, actionSheet } from '@maui/api/ui/dialogs'
import { currentWindow, getCurrentFlowDirection } from '@maui/api/ui/windows'
import type { Window, WindowSize, WindowPosition } from '@maui/api/ui/windows'
import { Menu } from '@maui/api/ui/menus'
import type { MenuDefinition } from '@maui/api/ui/menus'
import * as app from '@maui/api/application'
import { createBrowserView, supportsBrowserView } from '@maui/api/ui/remote-webview'
import type { BatteryState } from '@maui/api/essentials/battery'
import type { NetworkAccess } from '@maui/api/essentials/connectivity'
import type { BrowserStatus } from '@maui/api/ui/remote-webview'
import type { FlowDirection } from '@maui/api/ui/dialogs'

function closedEnumTypeProof(): void {
  // @ts-expect-error Native enums are closed and use exact camelCase names.
  const battery: BatteryState = 'Charging'
  // @ts-expect-error Unknown network values cannot be public result types.
  const network: NetworkAccess = 'arbitrary'
  // @ts-expect-error Error detail is not part of the browser status enum.
  const browser: BrowserStatus = 'failed: Timeout'
  // @ts-expect-error FlowDirection is not a numeric enum on the wire.
  const direction: FlowDirection = 1
  void battery; void network; void browser; void direction
}
void closedEnumTypeProof

function check(value: boolean): void { if (!value) throw new Error('Packed API consumer assertion failed') }
let menuSource: string | null = null
const snapshot = { id: 'main', windowLease: 'private-lease', initialLocation: 'index.html#/start',
  title: 'Native', x: 80, y: 90, width: 800, height: 600, canResize: true, canPosition: true,
  canOpenWindows: true, canObservePosition: true, canCenter: true }
const bridge: Bridge = {
  async InvokeDotNetBinary(method, input, options) {
    check(method === 'hybrid.invoke')
    const metadata = Object.entries(options?.headers ?? {}).sort(([a], [b]) => a.localeCompare(b)).map(([, value]) => value).join('')
    const request = JSON.parse(metadata) as { command: string; args: Record<string, Json> }
    if (request.command === 'share.stage')
      return new TextEncoder().encode(JSON.stringify({ id: '00000000-0000-0000-0000-000000000001',
        fileName: request.args.fileName, contentType: request.args.contentType, size: input.byteLength })).buffer
    return input instanceof ArrayBuffer ? input.slice(0) : Uint8Array.from(new Uint8Array(input.buffer, input.byteOffset, input.byteLength)).buffer
  },
  async InvokeDotNet(_, [wire]) {
    const request = JSON.parse(wire!) as { command: string; args: Record<string, Json>; id: string; session: string }
    if (request.command === 'menus.install') menuSource = 'menu'
    if (request.command === 'menus.clear') menuSource = null
    const results: Record<string, Json> = {
      'core.connect': { commands: [], maximumTextLength: 4096 },
      'appInfo.name': 'Consumer', 'appInfo.versionString': '1', 'appInfo.buildString': '2',
      'deviceInfo.platform': 'test', 'deviceInfo.versionString': '1', 'deviceInfo.model': 'test',
      'clipboard.read': '你好 👋', 'clipboard.hasText': true, 'clipboard.write': null,
      'preferences.get': request.args.defaultValue ?? { kind: 'string', text: '' },
      'battery.chargeLevel': -1, 'battery.state': 'unknown', 'battery.powerSource': 'ac', 'battery.energySaverStatus': 'off',
      'display.mainDisplayInfo': { width: 1000, height: 700, density: 2, orientation: 'landscape', rotation: 'rotation0', refreshRate: 60 },
      'connectivity.networkAccess': 'internet', 'connectivity.connectionProfiles': ['wiFi'],
      'launcher.open': true,
      'speech.locales': [{ id: 'voice', language: 'en-US', country: '', name: 'Native' }],
      'speech.speak': null,
      'filePicker.pick': null, 'filePicker.pickMultiple': { files: [] },
      'dialogs.prompt': null, 'dialogs.actionSheet': 'Cancel', 'dialogs.alert': null, 'dialogs.confirm': true,
      'geolocation.location': null, 'window.flowDirection': 'matchParent',
      'window.info': { ...snapshot, id: request.args.windowId ?? snapshot.id, windowLease: request.args.windowLease ?? snapshot.windowLease },
      'window.title': { ...snapshot, title: 'Updated' },
      'window.open': { id: 'child', windowLease: 'child-lease' },
      'window.find': { window: { ...snapshot, id: 'child', windowLease: 'child-lease' } },
      'window.list': { windows: [{ id: 'main', title: 'Native', url: 'index.html', canClose: false, windowLease: 'private-lease' }] },
      'menus.info': { supported: true, supportsChecked: true, sourceId: menuSource },
      'menus.install': { supported: true, supportsChecked: true, sourceId: menuSource },
      'menus.update': { supported: true, supportsChecked: true, sourceId: menuSource },
      'menus.clear': { supported: true, supportsChecked: true, sourceId: menuSource },
      'menus.item': { text: 'Native label', enabled: true, checked: null, kind: 'item' },
      'application.requestedTheme': 'dark', 'application.userAppTheme': 'unspecified',
      'application.setUserAppTheme': null,
    }
    const result = request.command.startsWith('remote.')
      ? { supported: true, open: true, id: 'guest', url: 'https://dot.net/maui', status: 'loaded', error: null, navigationResult: 'success', canGoBack: false, canGoForward: false }
      : Object.hasOwn(results, request.command) ? results[request.command] : { accepted: true }
    return JSON.stringify({ version: 1, session: request.session, id: request.id, status: 'success', result, error: null })
  }, SendRawMessage() {},
}
configureBridge(bridge)
const own: Window = currentWindow()
try {
  const client = await connect()
  check(Object.keys(root).sort().join(',') === 'Channel,NativeClient,NativeError,binaryBlob,connect,disconnect,invoke,invokeBinary')
  const binary = await invokeBinary('consumer.echo', [7], new Blob([Uint8Array.of(0, 128, 255).buffer]))
  check(binary.length === 3 && binary[1] === 128)
  check(binaryBlob(binary, 'application/octet-stream').size === 3)
  const channel: Channel<Uint8Array> = Channel.bytes(async bytes => { check(bytes.length <= 65536) })
  await channel.unsubscribe()
  check((await getAppInfo()).appName === 'Consumer')
  check((await getDeviceInfo()).platform === 'test')
  check(await getText() === '你好 👋'); await setText('é'); check(await hasText())
  check(await preferences.get('key', true))
  const enabled: boolean = await preferences.get('enabled', true)
  const savedTheme: string = await preferences.get('theme', 'system')
  const savedDate: Date = await preferences.get('date', new Date())
  void enabled; void savedTheme; void savedDate
  await preferences.set('date', new Date('2026-10-13T00:00:00Z'))
  check(await preferences.contains('date')); await preferences.remove('date'); await preferences.clear()
  check((await getBatteryInfo()).chargeLevel === -1); check((await getDisplayInfo()).density === 2)
  check((await getConnectivity()).networkAccess === 'internet')
  const stopNetwork = await onConnectivityChanged(value => check(Array.isArray(value.connectionProfiles)))
  await stopNetwork()
  const locales: SpeechLocale[] = await getLocales()
  const speechOptions: SpeechOptions = { locale: locales[0]!, pitch: 1, volume: 0.5, rate: 1 }
  const speech = await speak('Hello from the packed SDK', speechOptions)
  await speech.result
  await (await speak('', { locale: { id: null, name: null, country: null, language: 'en' }, pitch: null, volume: null, rate: null })).result
  await (await speak(' ', null)).result
  await openUrl('https://dot.net/maui')
  check(await pickFile({ pickerTitle: 'Pick from TypeScript' }) === null); check((await pickMultipleFiles()).length === 0)
  await alert('Native message'); check(await confirm('Confirm native'))
  const text: string | null = await prompt('Message', { title: 'Title', accept: 'Choose', cancel: 'Later', keyboard: { name: 'email' }, maxLength: 10000 })
  check(text === null)
  await prompt('Custom flags', { keyboard: { flags: ['spellcheck', 'suggestions'] } })
  await prompt('Native default', { keyboard: null })
  check(await actionSheet({ buttons: ['Choice'], cancel: 'Cancel' }) === 'Cancel')
  check(await getCurrentFlowDirection() === 'matchParent')
  const location = await getLocation({ desiredAccuracy: 'medium', timeout: '00:00:20' })
  check(await location.result === null)
  const staged = await stageFile('browser.pdf', 'application/pdf', Uint8Array.of(37, 80, 68, 70))
  await share({ file: staged, title: 'Native PDF' })
  await share({ text: 'Native text' })
  await releaseFile(staged)
  check(await own.id() === 'main'); check(await own.title() === 'Native')
  const size: WindowSize = await own.size(), position: WindowPosition = await own.position()
  await own.setTitle('Unicode 👋'); await own.setSize(size); await own.setPosition(position)
  const child: Window = await app.openWindow({ id: 'settings', url: 'index.html#/settings', title: 'Settings' })
  check(await child.id() === 'child')
  await child.close()
  check((await app.windows()).length === 1)
  check(await app.getTheme() === 'dark'); await app.setTheme('unspecified')
  const definitions: MenuDefinition[] = [{ id: 'file', text: 'File', kind: 'submenu', items: [
    { id: 'click', text: 'JS action', kind: 'item', shortcut: { key: 'K', modifiers: ['primary', 'shift'] }, action: () => {} },
  ] }]
  const menu = await own.setMenu(Menu.create(definitions))
  check(menu !== null && await own.menu() !== null)
  const item = menu!.item('click')
  check(await item.text() === 'Native label'); check(await item.enabled())
  await item.setText('Renamed'); await item.setEnabled(false); await own.setMenu(null)
  check(await supportsBrowserView())
  const browser = await createBrowserView('https://dot.net/maui')
  check((await browser.navigation()).status === 'loaded'); check(await browser.url() === 'https://dot.net/maui')
  await browser.navigate('https://example.com'); await browser.reload(); await browser.hide(); await browser.close()
  if (false) {
    // @ts-expect-error Keyboard is an object value, not a native kind enum.
    await prompt('Message', { keyboard: 'text' })
    // @ts-expect-error Keyboard has exactly one name or flags representation.
    await prompt('Message', { keyboard: { name: 'text', flags: [] } })
    // @ts-expect-error Wrapper-level keyboard flags no longer exist.
    await prompt('Message', { keyboardFlags: ['spellcheck'] })
    // @ts-expect-error Feature resources do not pollute the infrastructure root.
    void root.currentWindow
    // @ts-expect-error Features never accept a NativeClient argument.
    await pickFile(client)
    // @ts-expect-error Dialogs use message/options, not a client factory.
    await prompt('Message', client)
    // @ts-expect-error No redundant application factory exists in this unpublished preview.
    void app.getApplication
    // @ts-expect-error Menu attachment belongs to Window.setMenu, not a public controller factory.
    void (await import('@maui/api/ui/menus')).createWindowMenu
    // @ts-expect-error Private native generation binding is not a package entrypoint.
    await import('@maui/api/internal/window-ownership')
    // @ts-expect-error A default literal does not narrow a stored preference to that literal.
    const literal: true = await preferences.get('enabled', true)
    void literal
    // @ts-expect-error Current acquisition returns the same Window type, not a CurrentWindow class.
    const obsolete: import('@maui/api/ui/windows').CurrentWindow = own
    void obsolete
  }
} finally { resetBridge() }
try { await connect(); throw new Error('Plain Node must not emulate native APIs') }
catch (error) { check(error instanceof NativeError && error.code === 'native_unavailable') }
finally { disconnect() }
console.log('Packed flat service modules, Window/Menu/Item resources and client-free declarations passed.')
