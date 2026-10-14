import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, connect, resetBridge } from '@maui/api/core'
import { currentWindow } from '@maui/api/ui/windows'
import { Menu } from '@maui/api/ui/menus'
import { mock } from './bridge.mjs'
afterEach(() => resetBridge())

const windowInfo = { id: 'own-window', title: 'Own', x: 0, y: 0, width: 800, height: 600,
  canResize: true, canPosition: true, canOpenWindows: true, initialLocation: 'index.html#/welcome', windowLease: 'private' }

test('window-owned JS menu definitions/actions, native clicks, updates and replacements retain client/source identities', async () => {
  let source = null, index = 0
  let rejectInstall = false
  const host = mock(request => {
    if (request.command === 'window.info') return windowInfo
    if (request.command === 'core.connect') return { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 }
    if (request.command === 'core.subscribe' || request.command === 'core.unsubscribe') return { accepted: true }
    if (request.command === 'menus.install' && rejectInstall) throw new Error('OS rejected replacement')
    if (request.command === 'menus.install') source = `source-${++index}`
    if (request.command === 'menus.clear') source = null
    return { supported: true, supportsChecked: true, sourceId: source }
  })
  configureBridge(host.bridge)
  const client = await connect()
  try {
    const own = currentWindow(), clicks = []
    const definitions = [{ id: 'file', kind: 'submenu', text: 'File', items: [
      { id: 'open', kind: 'item', text: 'Open', shortcut: { key: 'Enter', modifiers: ['primary', 'shift'] }, action: () => clicks.push('open') },
      { id: 'sep', kind: 'separator', text: '' },
      { id: 'check', kind: 'check', text: 'Checked', checked: true, action: value => clicks.push(value) },
      { id: 'sub', kind: 'submenu', text: 'Nested', items: [{ id: 'nested', kind: 'item', text: 'Nested item' }] },
    ] }]
    const installed = await own.setMenu(Menu.create(definitions))
    const request = host.requests.find(request => request.command === 'menus.install')
    assert.equal(request.args.windowId, await own.id())
    assert.equal(request.args.items[0].items[0].action, undefined, 'functions are never native DTOs')
    assert.deepEqual(request.args.items[0].items[0].shortcut, { key: 'Enter', modifiers: ['primary', 'shift'] })
    const event = (sourceId, id, checked = false, session = client.session) => client.receive(JSON.stringify({
      version: 1, session, id: crypto.randomUUID(), topic: 'menus.clicked', data: { sourceId, id, checked },
    }))
    event(installed.sourceId, 'open')
    event(installed.sourceId, 'check', false)
    event('foreign', 'open')
    event(installed.sourceId, 'open', false, 'foreign-document')
    await Promise.resolve(); await Promise.resolve()
    assert.deepEqual(clicks, ['open', false])
    const beforeInvalid = host.requests.length
    assert.throws(() => Menu.create([definitions[0], definitions[0]]), error => error.code === 'invalid_arguments')
    assert.equal(host.requests.length, beforeInvalid, 'invalid definitions must not clear, unsubscribe or install')
    rejectInstall = true
    await assert.rejects(() => own.setMenu(Menu.create(definitions)), error => error.code === 'transport_error')
    rejectInstall = false
    event(installed.sourceId, 'open')
    await Promise.resolve(); await Promise.resolve()
    assert.deepEqual(clicks, ['open', false, 'open'], 'failed replacement preserves prior callbacks')
    await installed.update('open', { text: 'Renamed', enabled: false })
    await installed.item('check').setChecked(false)
    assert.ok(host.requests.filter(request => request.command === 'menus.update').every(request =>
      request.args.windowId === windowInfo.id && request.args.sourceId === installed.sourceId))
    const oldItem = installed.item('open')
    const replacement = await own.setMenu(Menu.create(definitions))
    await assert.rejects(() => oldItem.setText('stale'), error => error.code === 'disconnected')
    event(installed.sourceId, 'open')
    event(replacement.sourceId, 'open')
    await Promise.resolve(); await Promise.resolve()
    assert.deepEqual(clicks, ['open', false, 'open', 'open'])
    await own.setMenu(null)
    event(replacement.sourceId, 'open')
    await Promise.resolve()
    assert.equal(clicks.length, 4)
    assert.equal(host.requests.at(-1).command, 'core.unsubscribe')
    assert.equal(host.requests.filter(request => request.command === 'menus.clear').length, 1)
    assert.equal(await own.menu(), null)
  } finally { resetBridge() }
})

test('concurrent installations share one source listener and item handles retain native patches and callbacks', async () => {
  let source = null, index = 0
  const item = { text: 'Native', enabled: true, checked: null }
  const host = mock(request => {
    if (request.command === 'core.connect') return { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 }
    if (request.command === 'window.info') return windowInfo
    if (request.command === 'core.subscribe' || request.command === 'core.unsubscribe') return { accepted: true }
    if (request.command === 'menus.install') source = `source-${++index}`
    if (request.command === 'menus.item') return item
    if (request.command === 'menus.update') Object.assign(item, Object.fromEntries(
      Object.entries(request.args).filter(([key]) => ['text', 'enabled', 'checked'].includes(key))))
    return { supported: true, supportsChecked: true, sourceId: source }
  })
  configureBridge(host.bridge)
  const client = await connect()
  const definitions = () => [{ id: 'file', kind: 'submenu', text: 'File', items: [{ id: 'click', kind: 'item', text: 'JS', action: () => {} }] }]
  const first = currentWindow(), second = currentWindow()
  await Promise.all([first.setMenu(Menu.create(definitions())), second.setMenu(Menu.create(definitions()))])
  assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  const installed = await second.menu(), nativeItem = installed.item('click')
  assert.equal(await nativeItem.text(), 'Native')
  assert.equal(await nativeItem.enabled(), true)
  await nativeItem.setText('Renamed')
  await nativeItem.setEnabled(false)
  assert.equal(await nativeItem.text(), 'Renamed')
  assert.equal(await nativeItem.enabled(), false)
  await assert.rejects(() => nativeItem.checked(), error => error.code === 'unsupported')
  assert.deepEqual(host.requests.filter(request => request.command === 'menus.update').map(request => request.args), [
    { windowId: windowInfo.id, sourceId: installed.sourceId, id: 'click', text: 'Renamed' },
    { windowId: windowInfo.id, sourceId: installed.sourceId, id: 'click', enabled: false },
  ])
  await second.setMenu(null)
  assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
  assert.equal(client.available, true)
})

test('repeated label/check setters and native clicks retain the installation, shortcut and JS callback', async () => {
  // Transport double: native snapshot invalidation is covered by managed menu tests.
  let source = null, items = new Map()
  const host = mock(request => {
    if (request.command === 'core.connect') return { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 }
    if (request.command === 'window.info') return windowInfo
    if (request.command === 'core.subscribe' || request.command === 'core.unsubscribe') return { accepted: true }
    if (request.command === 'menus.install') {
      source = 'native-source'
      const collect = nodes => nodes.forEach(node => { items.set(node.id, structuredClone(node)); if (node.items) collect(node.items) })
      collect(request.args.items)
    }
    if (request.command === 'menus.item' || request.command === 'menus.update' || request.command === 'menus.clear') {
      assert.equal(request.args.windowId, windowInfo.id)
      assert.equal(request.args.sourceId, source)
    }
    if (request.command === 'menus.item') return items.get(request.args.id)
    if (request.command === 'menus.update') {
      const item = items.get(request.args.id)
      for (const key of ['text', 'enabled', 'checked']) if (key in request.args) item[key] = request.args[key]
    }
    if (request.command === 'menus.clear') source = null
    return { supported: true, supportsChecked: true, sourceId: source }
  })
  configureBridge(host.bridge)
  const client = await connect(), clicks = []
  const own = currentWindow()
  const installed = await own.setMenu(Menu.create([{ id: 'view', text: 'View', kind: 'submenu', items: [{
    id: 'check', text: 'Checked item', kind: 'check', checked: true,
    shortcut: { key: 'k', modifiers: ['primary', 'shift'] }, action: checked => clicks.push(checked),
  }] }]))
  const item = installed.item('check'), sourceId = installed.sourceId
  const shortcut = structuredClone(items.get('check').shortcut)
  for (const checked of [false, true, false]) {
    await item.setText(checked ? 'On' : 'Off')
    await item.setChecked(checked)
    assert.equal(await item.text(), checked ? 'On' : 'Off')
    assert.equal(await item.checked(), checked)
    assert.equal(installed.sourceId, sourceId)
    assert.equal(await own.menu(), installed)
  }
  const event = (checked, session = client.session) => client.receive(JSON.stringify({
    version: 1, session, id: crypto.randomUUID(), topic: 'menus.clicked', data: { sourceId, id: 'check', checked },
  }))
  items.get('check').checked = true
  event(true)
  event(false, 'retired-document')
  await Promise.resolve(); await Promise.resolve()
  assert.deepEqual(clicks, [true])
  assert.equal(await item.checked(), true)
  await item.setChecked(!await item.checked())
  assert.equal(await item.checked(), false, 'web controls toggle the native state, not a stale local flag')
  await installed.update('view', { text: 'Renamed menu' })
  assert.equal(await installed.item('view').text(), 'Renamed menu')
  assert.deepEqual(items.get('check').shortcut, shortcut)
  assert.equal(host.requests.filter(request => request.command === 'menus.install').length, 1)
  assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  assert.ok(host.requests.filter(request => request.command === 'menus.update').every(request =>
    request.args.windowId === windowInfo.id && request.args.sourceId === sourceId))
  await installed.dispose()
  await assert.rejects(() => item.setText('retired'), error => error.code === 'disconnected')
  event(false)
  await Promise.resolve(); await Promise.resolve()
  assert.deepEqual(clicks, [true])
})
