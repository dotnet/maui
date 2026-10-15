import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, resetBridge, connect, enumValue } from '@maui/api/core'
import { getBatteryInfo } from '@maui/api/essentials/battery'
import { getDisplayInfo } from '@maui/api/essentials/display'
import { getConnectivity } from '@maui/api/essentials/connectivity'
import * as preferences from '@maui/api/essentials/preferences'
import * as app from '@maui/api/application'
import * as dialogs from '@maui/api/ui/dialogs'
import { createBrowserView, supportsBrowserView } from '@maui/api/ui/remote-webview'
import { Menu } from '@maui/api/ui/menus'
import { mock } from './bridge.mjs'

afterEach(() => resetBridge())
const ready = { commands: [], maximumTextLength: 4096 }
const battery = { chargeLevel: 0.5, state: 'charging', powerSource: 'ac', energySaverStatus: 'off' }
const display = { width: 1080, height: 1920, density: 3, orientation: 'portrait', rotation: 'rotation0', refreshRate: 60 }
const network = { networkAccess: 'internet', connectionProfiles: ['wiFi'] }
const browser = { supported: true, open: true, id: 'guest', url: 'https://dot.net/maui',
  status: 'loaded', error: null, navigationResult: 'success', canGoBack: false, canGoForward: false }

test('shared enum validation accepts only declared exact names', () => {
  assert.equal(enumValue('notCharging', ['unknown', 'notCharging']), 'notCharging')
  for (const value of [1, '1', 'NotCharging', 'missing', null, [], {}])
    assert.throws(() => enumValue(value, ['unknown', 'notCharging']), error => error.code === 'invalid_response')
})

test('all native snapshot enum fields reject unknown, numeric and legacy-cased values', async () => {
  let output
  const host = mock(request => request.command === 'core.connect' ? ready : output)
  configureBridge(host.bridge)
  for (const [base, fields, read] of [
    [battery, ['state', 'powerSource', 'energySaverStatus'], getBatteryInfo],
    [display, ['orientation', 'rotation'], getDisplayInfo],
    [network, ['networkAccess'], getConnectivity],
    [browser, ['status', 'navigationResult'], supportsBrowserView],
    [{ effective: 'light', preference: 'unspecified' }, ['effective', 'preference'], app.getTheme],
    [{ kind: 'string', text: '' }, ['kind'], () => preferences.get('key', '')],
  ]) {
    output = base
    await read()
    for (const field of fields) for (const invalid of [999, '999', 'Unknown', 'missing']) {
      output = { ...base, [field]: invalid }
      await assert.rejects(read, error => error.code === 'invalid_response')
    }
  }
  for (const profile of [1, 'WiFi', 'notAProfile']) {
    output = { ...network, connectionProfiles: [profile] }
    await assert.rejects(getConnectivity, error => error.code === 'invalid_response')
  }
})

test('JavaScript bypass inputs fail before bridge invocation and native remains authoritative', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : null)
  configureBridge(host.bridge)
  for (const call of [
    () => app.setTheme('system'),
    () => app.setTheme(1),
    () => dialogs.alert('message', { flowDirection: 'invalid' }),
    () => dialogs.confirm('message', { flowDirection: 1 }),
    () => dialogs.actionSheet({ flowDirection: 'LeftToRight' }),
    () => dialogs.prompt('message', { keyboard: 'invalid' }),
    ...[{}, [], 0, { name: 'invalid' }, { name: 'Text' }, { name: 'text', flags: [] },
      { flags: null }, { flags: [1] }, { flags: ['all', 'spellcheck'] }, { flags: ['none', 'capitalizeNone'] },
      { flags: ['spellcheck', 'spellcheck'] }, { flags: ['Spellcheck'] }, { flags: ['unknown'] },
      { name: 'text', extra: true }].map(keyboard => () => dialogs.prompt('message', { keyboard })),
    () => preferences.set('value', 1, { numberType: 'invalid' }),
  ]) await assert.rejects(call, error => error.code === 'invalid_arguments')
  assert.equal(host.requests.length, 0)
  for (const flags of [[], ['none'], ['all'], ['spellcheck', 'suggestions']])
    assert.equal(await dialogs.prompt('message', { keyboard: { flags } }), null)
  for (const name of ['default', 'plain', 'chat', 'text', 'email', 'numeric', 'telephone', 'url', 'date', 'time', 'password'])
    assert.equal(await dialogs.prompt('message', { keyboard: { name } }), null)
  for (const kind of [1, 'invalid']) assert.throws(() => Menu.create([
    { id: 'root', text: 'Root', kind: 'submenu', items: [{ id: 'item', text: 'Item', kind }] },
  ]), error => error.code === 'invalid_arguments')
})

test('native browser navigation keeps closed status and typed completion separate from free error text', async () => {
  let snapshot = browser
  const host = mock(request => request.command === 'core.connect' ? ready :
    request.command === 'core.subscribe' || request.command === 'core.unsubscribe' ? { accepted: true } : snapshot)
  configureBridge(host.bridge)
  const view = await createBrowserView(browser.url)
  snapshot = { ...browser, status: 'failed', navigationResult: 'timeout' }
  assert.deepEqual(await view.navigation(), { status: 'failed', navigationResult: 'timeout', error: null, canGoBack: false, canGoForward: false })
  const received = []
  const client = await connect()
  const release = await view.onNavigation(value => received.push(value))
  snapshot = { ...browser, status: 'blocked', navigationResult: null, error: 'Only public HTTPS navigation is allowed.' }
  client.receive(JSON.stringify({ version: 1, session: client.session, id: crypto.randomUUID(), topic: 'remote.changed', data: snapshot }))
  assert.deepEqual(received, [{ status: 'blocked', navigationResult: null, error: snapshot.error, canGoBack: false, canGoForward: false }])
  await release()
})

test('unspecified theme is an actual native value, not a synthetic system preference', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : { effective: 'unspecified', preference: 'unspecified' })
  configureBridge(host.bridge)
  assert.equal(await app.getTheme(), 'unspecified')
  await app.setTheme('unspecified')
  assert.deepEqual(host.requests.find(request => request.command === 'application.setTheme').args, { theme: 'unspecified' })
})
