import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, resetBridge, connect } from '@maui/api/core'
import { currentWindow } from '@maui/api/ui/windows'
import * as app from '@maui/api/application'
import { prompt } from '@maui/api/ui/dialogs'
import { createBrowserView, supportsBrowserView } from '@maui/api/ui/remote-webview'
import { pickFile } from '@maui/api/essentials/file-picker'
import { getBatteryInfo } from '@maui/api/essentials/battery'
import { getDisplayInfo } from '@maui/api/essentials/display'
import { mock } from './bridge.mjs'
afterEach(() => resetBridge())
const ready = { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 }
const base = { id: 'main', windowLease: 'private', initialLocation: 'index.html', title: 'Native',
  x: 80, y: 90, width: 800, height: 600, canResize: true, canPosition: true, canOpenWindows: true }

test('public modules expose direct services and synchronous Window acquisition without client/factory aliases', async () => {
  assert.deepEqual(Object.keys(await import('@maui/api/ui/windows')), ['Window', 'currentWindow'])
  assert.deepEqual(Object.keys(await import('@maui/api/ui/remote-webview')), ['createBrowserView', 'supportsBrowserView'])
  assert.deepEqual(Object.keys(await import('@maui/api/ui/dialogs')), ['actionSheet', 'alert', 'confirm', 'prompt'])
  assert.deepEqual(Object.keys(await import('@maui/api/essentials/file-picker')), ['pickFile', 'pickMultipleFiles'])
  assert.deepEqual(Object.keys(await import('@maui/api/ui/menus')), ['Menu'])
  assert.deepEqual(Object.keys(await import('@maui/api/application')), ['getTheme', 'onThemeChanged', 'openWindow', 'setTheme', 'window', 'windows'])
})
test('Window is local/lazy, getters reread native and app creates caller-ID/generation-bound resources', async () => {
  let title = 'Native'
  const host = mock(request => {
    if (request.command === 'core.connect') return ready
    if (request.command === 'window.info') return { ...base, title, id: request.args.windowId ?? base.id,
      windowLease: request.args.windowLease ?? base.windowLease }
    if (request.command === 'window.title') return { ...base, title: title = request.args.title }
    if (request.command === 'window.open') return { id: request.args.options.id ?? 'child', windowLease: 'child-lease' }
    if (request.command === 'window.find') return { window: { ...base, id: request.args.id, windowLease: 'child-lease' } }
    if (request.command === 'window.list') return { windows: [{ id: 'main', title, url: 'index.html', canClose: false, windowLease: 'private' }] }
    return { accepted: true }
  })
  configureBridge(host.bridge)
  const own = currentWindow()
  assert.equal(host.requests.length, 0, 'local handle construction never handshakes or fabricates identity')
  assert.equal(await own.id(), 'main')
  assert.equal(await own.title(), 'Native')
  title = 'OS changed title'; assert.equal(await own.title(), title)
  await own.setTitle('你好 👋')
  assert.deepEqual(await own.size(), { width: 800, height: 600 })
  await own.setSize({ width: 100, height: 6000 }); await own.setPosition({ x: 20000, y: -20000 })
  const child = await app.openWindow({ id: 'settings', url: 'index.html#/settings', title: 'Settings' })
  assert.equal(await child.id(), 'settings')
  await child.close()
  assert.deepEqual(host.requests.find(request => request.command === 'window.close').args, { id: 'settings', windowLease: 'child-lease' })
  assert.equal((await app.windows()).length, 1)
  assert.equal(host.requests.filter(request => request.command === 'core.connect').length, 1)
  await assert.rejects(() => app.openWindow({ url: 'https://example.com' }), error => error.code === 'invalid_location')
  resetBridge()
  await assert.rejects(() => own.title(), error => error.code === 'disconnected')
})
test('open/list capture private generations before lazy binding and never acquire a reopened ID', async () => {
  let lease = 'first'
  const host = mock(request => {
    if (request.command === 'core.connect') return ready
    if (request.command === 'window.open') return { id: 'settings', windowLease: lease }
    if (request.command === 'window.list') return { windows: [{ id: 'settings', windowLease: lease }] }
    if (request.command === 'window.info') {
      if (request.args.windowId === 'settings' && request.args.windowLease !== lease)
        return { error: { code: 'invalid_window', message: 'Retired native generation.' } }
      return { ...base, id: request.args.windowId ?? 'main', windowLease: request.args.windowLease ?? 'private' }
    }
    return { accepted: true }
  })
  const invoke = host.bridge.InvokeDotNet
  host.bridge.InvokeDotNet = async (...args) => {
    const response = JSON.parse(await invoke(...args))
    return JSON.stringify(response.result?.error
      ? { ...response, status: 'error', result: null, error: response.result.error } : response)
  }
  configureBridge(host.bridge)
  const opened = await app.openWindow({ id: 'settings' })
  const [listed] = await app.windows()
  lease = 'replacement'
  await assert.rejects(() => opened.setTitle('stale'), error => error.code === 'invalid_window')
  await assert.rejects(() => listed.setTitle('stale'), error => error.code === 'invalid_window')
  assert.equal(host.requests.filter(request => request.command === 'window.find').length, 0)
  assert.equal(host.requests.filter(request => request.command === 'window.title').length, 0)
})
test('browser resources capture the private shared client and use behavior rather than arbitrary native script strings', async () => {
  let address = 'https://dot.net/maui'
  const host = mock(request => {
    if (request.command === 'core.connect') return ready
    if (request.command === 'remote.navigate') address = request.args.url
    return { supported: true, open: true, id: 'guest', url: address, status: 'loaded', canGoBack: true, canGoForward: false }
  })
  configureBridge(host.bridge)
  assert.equal(await supportsBrowserView(), true)
  const view = await createBrowserView(address)
  assert.equal(await view.url(), address)
  await view.navigate('https://example.com'); assert.equal(await view.url(), 'https://example.com')
  await view.goBack(); await view.goForward(); await view.reload(); await view.hide(); await view.close()
  await assert.rejects(() => view.navigate('http://dot.net/maui'), error => error.code === 'invalid_url')
})
test('browser navigation listeners share one native source and detach after the last listener', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready
    : request.command.startsWith('core.') ? { accepted: true }
    : { supported: true, open: true, id: 'guest', url: 'https://dot.net/maui',
      status: 'loaded', canGoBack: true, canGoForward: false })
  configureBridge(host.bridge)
  const view = await createBrowserView('https://dot.net/maui')
  const client = await connect()
  const received = []
  const first = await view.onNavigation(value => received.push(['first', value.status]))
  const second = await view.onNavigation(value => received.push(['second', value.status]))
  const emit = id => client.receive(JSON.stringify({ version: 1, session: client.session,
    id: crypto.randomUUID(), topic: 'remote.changed',
    data: { supported: true, open: true, id, url: 'https://dot.net/maui',
      status: 'loaded', canGoBack: true, canGoForward: false } }))
  assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  emit('foreign'); assert.deepEqual(received, [])
  emit('guest'); assert.deepEqual(received, [['first', 'loaded'], ['second', 'loaded']])
  await first()
  assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 0)
  await second()
  assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
  emit('guest'); assert.equal(received.length, 2)
})
test('direct picker/dialog calls and coherent device readouts use one hidden connection', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready
    : request.command === 'filePicker.pick' ? { file: null }
    : request.command === 'dialogs.prompt' ? { cancelled: true, text: null }
    : request.command === 'battery.info' ? { chargeLevel: -1, state: 'Unknown', powerSource: 'AC', energySaverStatus: 'Off' }
    : { width: 1080, height: 1920, density: 3, orientation: 'Portrait', rotation: 'Rotation0', refreshRate: 60 })
  configureBridge(host.bridge)
  assert.equal(await pickFile(), null)
  assert.deepEqual(await prompt('Message', { title: 'Title', accept: 'Choose', cancel: 'Later', keyboard: 'email', maxLength: 10000 }),
    { cancelled: true, text: null })
  assert.equal((await getBatteryInfo()).chargeLevel, -1)
  assert.equal((await getDisplayInfo()).density, 3)
  assert.equal(host.requests.filter(request => request.command === 'core.connect').length, 1)
})
test('interactive waits outlive 23 seconds but remain bounded and document disposal cancels immediately', async context => {
  context.mock.timers.enable({ apis: ['setTimeout'] })
  let finish
  const host = mock(request => request.command === 'core.connect'
    ? { ...ready, commands: ['dialogs.prompt'], interactiveCommands: ['dialogs.prompt'], interactiveTimeoutMilliseconds: 300000 }
    : new Promise(resolve => { finish = resolve }))
  configureBridge(host.bridge)
  const client = await connect()
  const pending = prompt('Message')
  await Promise.resolve()
  context.mock.timers.tick(24000)
  finish({ cancelled: false, text: 'After 23 seconds' })
  assert.equal((await pending).text, 'After 23 seconds')
  const second = prompt('Message')
  await Promise.resolve()
  const cancelled = assert.rejects(() => second, error => error.code === 'cancelled')
  client.dispose(); await cancelled
  finish({ cancelled: true, text: null })
  context.mock.timers.reset()
})
