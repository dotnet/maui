import { test } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, connect, resetBridge } from '@maui/api/core'
import * as app from '@maui/api/application'
import { onContentChanged } from '@maui/api/essentials/clipboard'
import { currentWindow } from '@maui/api/ui/windows'
import { mock } from './bridge.mjs'

test('empty native clipboard events decode null and share only document-local subscriptions', async () => {
  const host = mock(request => request.command === 'core.connect'
    ? { commands: [], maximumTextLength: 4096 } : { accepted: true })
  configureBridge(host.bridge)
  try {
    const client = await connect()
    let changed = 0
    const first = await onContentChanged(() => changed++)
    const second = await onContentChanged(() => changed++)
    const emit = data => client.receive(JSON.stringify({
      version: 1, session: client.session, id: crypto.randomUUID(), topic: 'clipboard.changed', data,
    }))
    emit({ accepted: true })
    assert.equal(changed, 0)
    emit(null)
    assert.equal(changed, 2)
    assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 1)
    await first()
    emit(null)
    assert.equal(changed, 3)
    await second()
    emit(null)
    assert.equal(changed, 3)
    assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
    assert.equal(host.requests.filter(request => request.command === 'clipboard.read').length, 0)
  } finally { resetBridge() }
})

test('native geometry and app theme events multiplex listeners and detach only after last unsubscribe', async () => {
  const host = mock(request => request.command === 'window.info'
    ? { id: 'own', windowLease: 'private', initialLocation: 'index.html', title: 'Native', x: 0, y: 0, width: 800, height: 600, canResize: true,
      canPosition: true, canOpenWindows: true, canObservePosition: true, canCenter: true }
    : request.command === 'core.connect' ? { commands: [], maximumTextLength: 4096 }
    : request.command === 'application.theme' ? { effective: 'dark', preference: 'unspecified' }
    : { accepted: true })
  configureBridge(host.bridge)
  const client = await connect()
  const emit = (topic, data, session = client.session) => client.receive(JSON.stringify({
    version: 1, session, id: crypto.randomUUID(), topic, data,
  }))
  try {
    const own = currentWindow()
    const sizes = [], positions = [], themes = []
    const release1 = await own.onSizeChanged(value => sizes.push(['first', value]))
    const release2 = await own.onSizeChanged(value => sizes.push(['second', value]))
    const release3 = await own.onPositionChanged(value => positions.push(value))
    assert.equal(host.requests.filter(request => request.command === 'core.subscribe' && request.args.topic === 'window.geometryChanged').length, 1)
    emit('window.geometryChanged', { windowId: 'own', size: { width: 1200, height: 700 }, position: { x: 120, y: 90 } })
    assert.equal(sizes.length, 2)
    assert.deepEqual(positions, [{ x: 120, y: 90 }])
    emit('window.geometryChanged', { windowId: 'foreign', size: { width: 1, height: 1 }, position: null })
    assert.equal(sizes.length, 2)
    await release1(); await release2()
    assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 0)
    await release3()
    assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
    const t1 = await app.onThemeChanged(value => themes.push(value))
    const t2 = await app.onThemeChanged(value => themes.push(value))
    emit('application.themeChanged', { effective: 'dark', preference: 'unspecified' })
    emit('application.themeChanged', { requestedTheme: 1 })
    assert.deepEqual(themes, [])
    emit('application.themeChanged', { requestedTheme: 'light' })
    assert.deepEqual(themes, ['light', 'light'])
    await t1(); await t2()
    emit('application.themeChanged', { requestedTheme: 'dark' })
    assert.equal(themes.length, 2)
    const resumed = await own.onSizeChanged(value => sizes.push(['resumed', value]))
    emit('window.geometryChanged', { windowId: 'own', size: { width: 900, height: 600 }, position: null }, 'retired')
    assert.equal(sizes.length, 2)
    client.dispose()
    emit('window.geometryChanged', { windowId: 'own', size: { width: 900, height: 600 }, position: null })
    assert.equal(sizes.length, 2)
    await resumed()
  } finally { resetBridge() }
})

test('remount waits for the retiring native unsubscribe and ignores late callbacks', async () => {
  let finish
  const host = mock(request => {
    if (request.command === 'core.connect') return { commands: [], maximumTextLength: 4096 }
    if (request.command === 'core.unsubscribe') return new Promise(resolve => { finish = resolve })
    return { accepted: true }
  })
  configureBridge(host.bridge)
  try {
    const client = await connect(), themes = []
    const emit = () => client.receive(JSON.stringify({ version: 1, session: client.session,
      id: crypto.randomUUID(), topic: 'application.themeChanged', data: { requestedTheme: 'dark' } }))
    const release = await app.onThemeChanged(theme => themes.push(theme))
    const stopping = release()
    const mounting = app.onThemeChanged(theme => themes.push(theme))
    await Promise.resolve(); await Promise.resolve()
    assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 1)
    emit(); assert.deepEqual(themes, [])
    finish({ accepted: true })
    await stopping
    const releaseNew = await mounting
    assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 2)
    emit(); assert.deepEqual(themes, ['dark'])
    const stopped = releaseNew()
    finish({ accepted: true })
    await stopped
    emit(); assert.deepEqual(themes, ['dark'])
  } finally { resetBridge() }
})
