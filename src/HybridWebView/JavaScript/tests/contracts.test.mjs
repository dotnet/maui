import { test } from 'node:test'
import assert from 'node:assert/strict'
import { NativeClient, NativeError, asciiJson, object, ack, progress, isNativeOrigin, loadNativeBridge, connect, disconnect } from '@maui/api/core'
import { clipboard } from '@maui/api/essentials/clipboard'
import { preferences } from '@maui/api/essentials/preferences'
import { getInfo } from '@maui/api/app/info'
import * as api from '@maui/api'
import { mock } from './bridge.mjs'

const ready = { commands: ['app.info', 'clipboard.read'], preferenceNamespace: 'host-owned.', maximumTextLength: 4096 }
const identity = { appName: 'Host', version: '1', build: '1', platform: 'test', osVersion: '1', model: 'test', architecture: 'arm64', pid: 1, startupId: 'startup', startedAt: '2026-10-12T00:00:00Z' }

test('built public root and subpaths share the same API without eager browser access', async () => {
  assert.equal(api.NativeClient, NativeClient)
  assert.equal(api.clipboard, clipboard)
  assert.equal(api.preferences, preferences)
  assert.equal(api.getInfo, getInfo)
  assert.equal(api.connect, connect)
  assert.equal(api.disconnect, disconnect)
  assert.equal(await loadNativeBridge(), undefined)
})

test('Unicode, escapes, null, arrays and DTOs survive ByteString headers and both framework serializations', async () => {
  const dto = { text: '你好 👋 café\n"quote"\\\0', number: 12.5, flag: true, items: ['日本語', '\r\n', '\\'] }
  const host = mock(request => request.args)
  const client = new NativeClient(host.bridge)
  try {
    assert.deepEqual(await client.invoke('custom.echo', dto, object), dto)
    assert.deepEqual(JSON.parse(asciiJson({ text: null, items: [], number: 0, flag: false })), { text: null, items: [], number: 0, flag: false })
  } finally { client.dispose() }
})

test('browser origin and unavailable transport never emulate native APIs', async () => {
  assert.equal(isNativeOrigin('http://127.0.0.1:5173/'), false)
  assert.equal(isNativeOrigin('https://example.com/'), false)
  assert.equal(isNativeOrigin('app://0.0.0.1/index.html'), true)
  assert.equal(isNativeOrigin('https://0.0.0.1/index.html'), true)
  const client = new NativeClient(undefined)
  try {
    for (const action of [() => client.connect(), () => clipboard(client).readText(), () => preferences(client).get('example'), () => getInfo(client),
      () => connect(), () => clipboard().readText(), () => preferences().get('example'), () => getInfo()]) {
      await assert.rejects(action, error => error instanceof NativeError && error.code === 'native_unavailable')
    }
  } finally { client.dispose(); disconnect() }
})

test('typed clipboard, preferences and identity helpers validate outputs and dispatch only provider commands', async () => {
  const host = mock(request => {
    if (request.command === 'clipboard.read') return { text: null }
    if (request.command === 'clipboard.write') return { accepted: true }
    if (request.command === 'app.info') return identity
    return { key: request.args.key, value: request.command === 'preferences.remove' ? null : request.args.value ?? 'é' }
  })
  const client = new NativeClient(host.bridge)
  try {
    assert.equal(await clipboard(client).readText(), null)
    assert.equal(await clipboard(client).writeText('你好 👋'), true)
    assert.deepEqual(await preferences(client).get('key'), { key: 'key', value: 'é' })
    assert.deepEqual(await preferences(client).set('key', '你好'), { key: 'key', value: '你好' })
    assert.deepEqual(await preferences(client).remove('key'), { key: 'key', value: null })
    assert.deepEqual(await getInfo(client), identity)
    assert.deepEqual(host.requests.map(request => request.command), ['clipboard.read', 'clipboard.write', 'preferences.read', 'preferences.write', 'preferences.remove', 'app.info'])
    host.bridge.InvokeDotNet = mock(() => ({ text: 42, key: 42, value: false })).bridge.InvokeDotNet
    for (const action of [() => clipboard(client).readText(), () => clipboard(client).writeText('text'), () => preferences(client).get('key'), () => getInfo(client)]) {
      await assert.rejects(action, error => error.code === 'invalid_response')
    }
  } finally { client.dispose() }
})

test('native error and cancellation are not success-shaped', async () => {
  for (const [status, code] of [['error', 'permission_denied'], ['error', 'invalid_arguments'], ['error', 'unknown_command'], ['error', 'disabled_command'], ['cancelled', 'cancelled']]) {
    const host = mock(() => null)
    host.bridge.InvokeDotNet = async (_, [wire]) => {
      const request = JSON.parse(wire)
      return JSON.stringify({ version: 1, session: request.session, id: request.id, status, result: null, error: { code, message: 'Expected native rejection' } })
    }
    const client = new NativeClient(host.bridge)
    try { await assert.rejects(() => clipboard(client).readText(), error => error.code === code) }
    finally { client.dispose() }
  }
})

test('malformed envelopes, response identity/version and transport failures are rejected', async () => {
  const host = mock(() => ({ accepted: true }))
  const client = new NativeClient(host.bridge)
  try {
    for (const output of [
      true, '{bad json', JSON.stringify({ version: 2, session: client.session, id: 'wrong', status: 'success', result: true, error: null }),
      JSON.stringify({ version: 1, session: 'obsolete', id: 'wrong', status: 'success', result: true, error: null }),
    ]) {
      host.bridge.InvokeDotNet = async () => output
      await assert.rejects(() => client.invoke('custom.command', {}, ack), error => error instanceof NativeError)
    }
    host.bridge.InvokeDotNet = async (_, [wire]) => {
      const request = JSON.parse(wire)
      return JSON.stringify({ version: 1, session: request.session, id: request.id, status: 'success', result: { accepted: true }, error: { code: 'bad', message: 'bad' } })
    }
    await assert.rejects(() => client.invoke('custom.command', {}, ack), error => error.code === 'invalid_response')
    host.bridge.InvokeDotNet = async () => { throw new TypeError('Fetch failed') }
    await assert.rejects(() => client.invoke('custom.command', {}, ack), error => error.code === 'transport_error')
  } finally { client.dispose() }
})

test('events are session-scoped, validated, unsubscribed and not multiplied', async () => {
  const host = mock(() => ({ accepted: true }))
  const client = new NativeClient(host.bridge)
  try {
    const events = []
    const unsubscribe = await client.subscribe('custom.changed', ack, (value, id) => events.push({ value, id }))
    await assert.rejects(() => client.subscribe('custom.changed', ack, () => {}), error => error.code === 'duplicate_subscription')
    client.receive(JSON.stringify({ version: 1, session: 'stale', id: 'old', topic: 'custom.changed', data: { accepted: true } }))
    client.receive(JSON.stringify({ version: 1, session: client.session, id: 'new', topic: 'custom.changed', data: { accepted: true } }))
    assert.deepEqual(events, [{ value: true, id: 'new' }])
    await unsubscribe()
    await unsubscribe()
    client.receive(JSON.stringify({ version: 1, session: client.session, id: 'later', topic: 'custom.changed', data: { accepted: true } }))
    assert.equal(events.length, 1)
    assert.deepEqual(host.requests.map(request => request.command), ['core.subscribe', 'core.unsubscribe'])
  } finally { client.dispose() }
})

test('operation cancellation uses correlated request ID and validates progress', async () => {
  const host = mock(request => request.command === 'core.cancel' ? { accepted: true } : { completed: 1, total: 1 })
  const client = new NativeClient(host.bridge)
  try {
    const operation = client.start('custom.work', {}, progress)
    assert.deepEqual(await operation.result, { completed: 1, total: 1 })
    assert.equal(await operation.cancel(), true)
    assert.equal(host.requests[1].args.requestId, operation.id)
    assert.throws(() => progress({ completed: 3, total: 1 }), NativeError)
    assert.throws(() => object(null), NativeError)
    assert.throws(() => ack({ accepted: 'yes' }), NativeError)
  } finally { client.dispose() }
})

test('oversized ASCII-escaped requests are rejected before reaching native', async () => {
  const host = mock(() => ({ accepted: true }))
  const client = new NativeClient(host.bridge)
  try {
    await assert.rejects(() => client.invoke('custom.command', { text: '你'.repeat(17000) }, ack), error => error.code === 'invalid_arguments')
    assert.equal(host.requests.length, 0)
  } finally { client.dispose() }
})

test('disconnect rejects pending calls and emits a single cleanup message even if transport is gone', async () => {
  let complete
  const host = mock(() => new Promise(resolve => { complete = resolve }))
  const client = new NativeClient(host.bridge)
  const result = client.invoke('custom.command', {}, ack)
  client.dispose()
  client.dispose()
  await assert.rejects(() => result, error => error.code === 'cancelled')
  assert.deepEqual(host.messages, [{ session: client.session, kind: 'disconnect' }])
  complete({ accepted: true })
  await assert.rejects(() => client.connect(), error => error.code === 'native_unavailable')
  const lost = new NativeClient({ ...host.bridge, SendRawMessage() { throw new Error('Disconnected transport') } })
  assert.doesNotThrow(() => lost.dispose())
  assert.equal(lost.available, false)
})

test('default feature calls share lazy startup, and teardown creates a new session', async () => {
  const original = globalThis.window
  const originalDocument = globalThis.document
  const target = new EventTarget()
  let additions = 0, removals = 0
  const host = mock(request => request.command === 'core.connect' ? ready : request.command === 'app.info' ? identity
    : request.command === 'clipboard.read' ? { text: 'native' } : { accepted: true })
  globalThis.window = {
    location: { href: 'app://0.0.0.1/index.html' }, HybridWebView: host.bridge,
    addEventListener(...args) { additions++; target.addEventListener(...args) },
    removeEventListener(...args) { removals++; target.removeEventListener(...args) },
  }
  globalThis.document = {}
  try {
    disconnect()
    assert.equal(host.requests.length, 0)
    assert.deepEqual(await Promise.all([clipboard().readText(), getInfo()]), ['native', identity])
    const client = await connect()
    assert.equal(await connect(), client)
    assert.equal(host.requests.filter(request => request.command === 'core.connect').length, 1)
    assert.equal(additions, 2)
    let count = 0
    await client.subscribe('custom.changed', ack, () => count++)
    target.dispatchEvent(new CustomEvent('HybridWebViewMessageReceived', { detail: null }))
    const message = JSON.stringify({ version: 1, session: client.session, id: 'event', topic: 'custom.changed', data: { accepted: true } })
    target.dispatchEvent(new CustomEvent('HybridWebViewMessageReceived', { detail: { message } }))
    assert.equal(count, 1)
    target.dispatchEvent(new Event('pagehide'))
    assert.equal(removals, 2)
    assert.equal(client.available, false)
    target.dispatchEvent(new CustomEvent('HybridWebViewMessageReceived', { detail: { message } }))
    assert.equal(count, 1)
    const next = await connect()
    assert.notEqual(next.session, client.session)
    disconnect()
    disconnect()
    assert.equal(removals, 4)
    assert.equal(host.messages.filter(message => message.kind === 'disconnect').length, 2)
  } finally {
    disconnect()
    globalThis.window = original
    globalThis.document = originalDocument
  }
})

test('default startup failures are retriable and disconnect cancels an in-flight handshake', async () => {
  const original = globalThis.window, originalDocument = globalThis.document
  const host = mock(() => ready)
  let fail = true, complete
  const invoke = host.bridge.InvokeDotNet
  host.bridge.InvokeDotNet = async (...args) => {
    if (fail) throw new Error('Host not ready')
    return invoke(...args)
  }
  globalThis.window = { location: { href: 'https://0.0.0.1/' }, HybridWebView: host.bridge, addEventListener() {}, removeEventListener() {} }
  globalThis.document = {}
  try {
    disconnect()
    await assert.rejects(() => connect(), error => error.code === 'transport_error')
    fail = false
    const client = await connect()
    assert.equal(client.available, true)
    disconnect()
    host.bridge.InvokeDotNet = (...args) => new Promise(resolve => { complete = () => resolve(invoke(...args)) })
    const pending = connect()
    await Promise.resolve()
    disconnect()
    await assert.rejects(() => pending, error => error.code === 'cancelled')
    complete()
    const beforeStart = connect()
    disconnect()
    await assert.rejects(() => beforeStart, error => error.code === 'cancelled')
  } finally {
    disconnect()
    globalThis.window = original
    globalThis.document = originalDocument
  }
})

test('framework bootstrap loads only at native origins and reports script failures', async () => {
  const original = globalThis.window, originalDocument = globalThis.document
  const host = mock(() => ready)
  let scripts = 0, fail = false
  globalThis.window = { location: { href: 'http://127.0.0.1:5173/' }, HybridWebView: host.bridge }
  globalThis.document = {
    createElement(name) { assert.equal(name, 'script'); return {} },
    head: {
      append(script) {
        scripts++
        assert.equal(script.src, '/_framework/hybridwebview.js')
        queueMicrotask(() => {
          if (fail) script.onerror()
          else { globalThis.window.HybridWebView = host.bridge; script.onload() }
        })
      },
    },
  }
  try {
    assert.equal(await loadNativeBridge(), undefined)
    assert.equal(scripts, 0)
    globalThis.window.location.href = 'app://0.0.0.1/'
    delete globalThis.window.HybridWebView
    assert.equal(await loadNativeBridge(), host.bridge)
    assert.equal(scripts, 1)
    fail = true
    delete globalThis.window.HybridWebView
    await assert.rejects(() => loadNativeBridge(), error => error.code === 'transport_error')
    assert.equal(scripts, 2)
  } finally {
    globalThis.window = original
    globalThis.document = originalDocument
  }
})

test('document heartbeats and the 23-second frontend deadline use the session identity', async context => {
  context.mock.timers.enable({ apis: ['setTimeout', 'setInterval'] })
  const original = globalThis.window
  let complete
  const host = mock(() => new Promise(resolve => { complete = resolve }))
  globalThis.window = { addEventListener() {}, removeEventListener() {} }
  const client = new NativeClient(host.bridge)
  try {
    client.attachToDocument()
    const pending = client.invoke('custom.command', {}, ack)
    const rejected = assert.rejects(() => pending, error => error.code === 'transport_timeout')
    context.mock.timers.tick(2000)
    assert.deepEqual(host.messages, [{ session: client.session, kind: 'heartbeat' }])
    context.mock.timers.tick(21000)
    await rejected
    assert.ok(host.messages.some(message => message.session === client.session && message.kind === 'disconnect'))
    client.dispose()
    const count = host.messages.length
    context.mock.timers.tick(2000)
    assert.equal(host.messages.length, count)
    complete({ accepted: true })
  } finally {
    client.dispose()
    globalThis.window = original
    context.mock.timers.reset()
  }
})
