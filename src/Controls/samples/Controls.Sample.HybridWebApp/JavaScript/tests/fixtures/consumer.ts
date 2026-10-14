import * as root from '@maui/api'
import { NativeError, connect, disconnect } from '@maui/api'
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
import { pickFile, pickMultipleFiles } from '@maui/api/essentials/file-picker'
import { alert, confirm, prompt, actionSheet } from '@maui/api/ui/dialogs'
import { currentWindow } from '@maui/api/ui/windows'
import type { Window, WindowSize, WindowPosition } from '@maui/api/ui/windows'
import { Menu } from '@maui/api/ui/menus'
import type { MenuDefinition } from '@maui/api/ui/menus'
import * as app from '@maui/api/application'
import { createBrowserView, supportsBrowserView } from '@maui/api/ui/remote-webview'

function check(value: boolean): void { if (!value) throw new Error('Packed API consumer assertion failed') }
let menuSource: string | null = null
const snapshot = { id: 'main', windowLease: 'private-lease', initialLocation: 'index.html#/start',
  title: 'Native', x: 80, y: 90, width: 800, height: 600, canResize: true, canPosition: true,
  canOpenWindows: true, canObservePosition: true, canCenter: true }
const bridge: Bridge = {
  async InvokeDotNet(_, [wire]) {
    const request = JSON.parse(wire!) as { command: string; args: Record<string, Json>; id: string; session: string }
    if (request.command === 'menus.install') menuSource = 'menu'
    if (request.command === 'menus.clear') menuSource = null
    const results: Record<string, Json> = {
      'core.connect': { commands: [], preferenceNamespace: 'consumer.', maximumTextLength: 4096 },
      'app.info': { appName: 'Consumer', version: '1', build: '2' },
      'device.info': { platform: 'test', osVersion: '1', model: 'test' },
      'clipboard.read': { text: '你好 👋' }, 'clipboard.hasText': { hasText: true },
      'preferences.get': request.args.defaultValue ?? { kind: 'string', text: '' },
      'battery.info': { chargeLevel: -1, state: 'Unknown', powerSource: 'AC', energySaverStatus: 'Off' },
      'display.info': { width: 1000, height: 700, density: 2, orientation: 'Landscape', rotation: 'Rotation0', refreshRate: 60 },
      'connectivity.info': { networkAccess: 'Internet', connectionProfiles: ['WiFi'] },
      'speech.locales': { locales: [{ id: 'voice', language: 'en-US', country: '', name: 'Native' }] },
      'filePicker.pick': { file: null }, 'filePicker.pickMultiple': { files: [] },
      'dialogs.prompt': { cancelled: true, text: null }, 'dialogs.actionSheet': { selection: 'Cancel' },
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
      'application.theme': { effective: 'dark', preference: 'system' },
      'application.setTheme': { effective: 'light', preference: request.args.theme ?? 'system' },
    }
    const result = request.command.startsWith('remote.')
      ? { supported: true, open: true, id: 'guest', url: 'https://dot.net/maui', status: 'loaded', canGoBack: false, canGoForward: false }
      : results[request.command] ?? { accepted: true }
    return JSON.stringify({ version: 1, session: request.session, id: request.id, status: 'success', result, error: null })
  }, SendRawMessage() {},
}
configureBridge(bridge)
const own: Window = currentWindow()
try {
  const client = await connect()
  check(Object.keys(root).sort().join(',') === 'NativeClient,NativeError,connect,disconnect,invoke')
  check((await getAppInfo()).appName === 'Consumer')
  check((await getDeviceInfo()).platform === 'test')
  check(await getText() === '你好 👋'); check(await setText('é')); check(await hasText())
  check(await preferences.get('key', true))
  const enabled: boolean = await preferences.get('enabled', true)
  const savedTheme: string = await preferences.get('theme', 'system')
  const savedDate: Date = await preferences.get('date', new Date())
  void enabled; void savedTheme; void savedDate
  await preferences.set('date', new Date('2026-10-13T00:00:00Z'))
  check(await preferences.contains('date')); await preferences.remove('date'); await preferences.clear()
  check((await getBatteryInfo()).chargeLevel === -1); check((await getDisplayInfo()).density === 2)
  check((await getConnectivity()).networkAccess === 'Internet')
  const stopNetwork = await onConnectivityChanged(value => check(Array.isArray(value.connectionProfiles)))
  await stopNetwork()
  const locales: SpeechLocale[] = await getLocales()
  const speechOptions: SpeechOptions = { locale: locales[0]!, pitch: 1, volume: 0.5, rate: 1 }
  const speech = await speak('Hello from the packed SDK', speechOptions)
  await speech.result
  await openUrl('https://dot.net/maui')
  check(await pickFile({ pickerTitle: 'Pick from TypeScript' }) === null); check((await pickMultipleFiles()).length === 0)
  check(await alert('Native message')); check(await confirm('Confirm native'))
  check((await prompt('Message', { title: 'Title', accept: 'Choose', cancel: 'Later', keyboard: 'email', maxLength: 10000 })).cancelled)
  check(await actionSheet({ buttons: ['Choice'], cancel: 'Cancel' }) === 'Cancel')
  check(await own.id() === 'main'); check(await own.title() === 'Native')
  const size: WindowSize = await own.size(), position: WindowPosition = await own.position()
  await own.setTitle('Unicode 👋'); await own.setSize(size); await own.setPosition(position)
  const child: Window = await app.openWindow({ id: 'settings', url: 'index.html#/settings', title: 'Settings' })
  check(await child.id() === 'child')
  await child.close()
  check((await app.windows()).length === 1)
  check(await app.getTheme() === 'dark'); await app.setTheme('system')
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
