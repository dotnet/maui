import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { runInNewContext } from 'node:vm'
import { NativeClient, NativeError, asciiJson, object, ack, progress, isNativeOrigin, loadNativeBridge, connect, disconnect, configureBridge, resetBridge } from '@maui/api/core'
import { getText, setText } from '@maui/api/essentials/clipboard'
import * as preferences from '@maui/api/essentials/preferences'
import { getAppInfo } from '@maui/api/essentials/app-info'
import { getDeviceInfo } from '@maui/api/essentials/device-info'
import * as api from '@maui/api'
import { mock } from './bridge.mjs'

const ready = { commands: ['app.info', 'device.info', 'clipboard.read'], preferenceNamespace: 'host-owned.', maximumTextLength: 4096 }
const appInfo = { appName: 'Host', version: '1', build: '1' }
const deviceInfo = { platform: 'test', osVersion: '1', model: 'test' }
afterEach(() => resetBridge())

test('built root contains only lifecycle infrastructure without eager browser access', async () => {
  assert.equal(api.NativeClient, NativeClient)
  assert.equal(api.connect, connect)
  assert.equal(api.disconnect, disconnect)
  assert.deepEqual(Object.keys(api).sort(), ['NativeClient', 'NativeError', 'connect', 'disconnect', 'invoke'])
  assert.equal(await loadNativeBridge(), undefined)
})

test('direct invoke shares one connection and preserves named and positional action arguments', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : request.args)
  configureBridge(host.bridge)
  assert.deepEqual(await api.invoke('custom.echo', { value: 'Hello' }), { value: 'Hello' })
  assert.deepEqual(await api.invoke('custom.echo', ['theme', 'dark']), ['theme', 'dark'])
  assert.deepEqual(await api.invoke('custom.echo', {}, object), {})
  assert.equal(host.requests.filter(request => request.command === 'core.connect').length, 1)
  assert.equal(new Set(host.requests.map(request => request.session)).size, 1)
})

test('Essentials are explicit subpaths and removed/private paths are not exported', async () => {
  for (const path of ['@maui/api/app/info', '@maui/api/essentials', '@maui/api/src/core.ts', '@maui/api/dist/core.js']) {
    await assert.rejects(() => import(path), error => error.code === 'ERR_PACKAGE_PATH_NOT_EXPORTED')
  }
  assert.deepEqual(Object.keys(await import('@maui/api/essentials/app-info')), ['getAppInfo'])
  assert.deepEqual(Object.keys(await import('@maui/api/essentials/device-info')), ['getDeviceInfo'])
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
    for (const action of [() => client.connect(), () => getText(), () => preferences.get('example', ''), () => getAppInfo(), () => getDeviceInfo(), () => connect()]) {
      await assert.rejects(action, error => error instanceof NativeError && error.code === 'native_unavailable')
    }
  } finally { client.dispose(); disconnect() }
})

test('injected protocol transport supports Essentials without MAUI-origin bootstrap or UI framework dependencies', async () => {
  const originalWindow = globalThis.window, originalDocument = globalThis.document
  let scriptLoads = 0, listeners = 0
  globalThis.window = {
    location: { href: 'https://example.test/app' },
    addEventListener() { listeners++ },
    removeEventListener() {},
  }
  globalThis.document = { createElement() { scriptLoads++; throw new Error('No framework bootstrap is allowed') } }
  const host = mock(request => {
    if (request.command === 'core.connect') return ready
    if (request.command === 'app.info') return appInfo
    if (request.command === 'device.info') return deviceInfo
    if (request.command === 'clipboard.read') return { text: 'injected native transport' }
    if (request.command === 'clipboard.write' || request.command === 'preferences.set' || request.command === 'preferences.delete') return { accepted: true }
    return { kind: 'string', text: 'native setting' }
  })
  let client
  try {
    disconnect()
    assert.equal(await loadNativeBridge(), undefined)
    await assert.rejects(() => connect(), error => error instanceof NativeError && error.code === 'native_unavailable')
    configureBridge(host.bridge)
    client = await connect()
    assert.deepEqual(client.readiness, ready)
    assert.deepEqual(await getAppInfo(), appInfo)
    assert.deepEqual(await getDeviceInfo(), deviceInfo)
    assert.equal(await getText(), 'injected native transport')
    assert.equal(await setText('你好'), true)
    assert.equal(await preferences.get('key', ''), 'native setting')
    await preferences.set('key', 'value')
    await preferences.remove('key')
    assert.ok(host.requests.every(request => request.session === client.session))
    assert.equal(host.requests.filter(request => request.command === 'core.connect').length, 1)
    assert.equal(scriptLoads, 0)
    assert.equal(listeners, 2)
    disconnect()
    configureBridge(undefined)
    await assert.rejects(() => getAppInfo(), error => error instanceof NativeError && error.code === 'native_unavailable')
    assert.deepEqual(host.messages, [{ session: client.session, kind: 'disconnect' }])
  } finally {
    client?.dispose()
    disconnect()
    globalThis.window = originalWindow
    globalThis.document = originalDocument
  }
})

test('typed Essentials helpers validate outputs and dispatch only provider commands', async () => {
  const host = mock(request => {
    if (request.command === 'core.connect') return ready
    if (request.command === 'clipboard.read') return { text: null }
    if (request.command === 'clipboard.write') return { accepted: true }
    if (request.command === 'app.info') return appInfo
    if (request.command === 'device.info') return deviceInfo
    if (request.command === 'preferences.set' || request.command === 'preferences.delete') return { accepted: true }
    return { kind: 'string', text: 'é' }
  })
  configureBridge(host.bridge)
  const client = await connect()
  try {
    assert.equal(await getText(), null)
    assert.equal(await setText('你好 👋'), true)
    assert.equal(await preferences.get('key', ''), 'é')
    await preferences.set('key', '你好')
    await preferences.remove('key')
    assert.deepEqual(await getAppInfo(), appInfo)
    assert.deepEqual(await getDeviceInfo(), deviceInfo)
    assert.deepEqual(host.requests.map(request => request.command), ['core.connect', 'clipboard.read', 'clipboard.write', 'preferences.get', 'preferences.set', 'preferences.delete', 'app.info', 'device.info'])
    assert.ok(host.requests.every(request => request.session === client.session))
    host.bridge.InvokeDotNet = mock(() => ({ text: 42, key: 42, value: false })).bridge.InvokeDotNet
    for (const action of [() => getText(), () => setText('text'), () => preferences.get('key', ''), () => getAppInfo(), () => getDeviceInfo()]) {
      await assert.rejects(action, error => error.code === 'invalid_response')
    }
  } finally { client.dispose() }
})

test('AppInfo and DeviceInfo project only their own validated fields, never process diagnostics', async () => {
  let result = { ...appInfo, ...deviceInfo, architecture: false, pid: 'not a PID', startupId: null, startedAt: {} }
  const host = mock(request => request.command === 'core.connect' ? ready : result)
  configureBridge(host.bridge)
  const client = await connect()
  try {
    assert.deepEqual(await getAppInfo(), appInfo)
    assert.deepEqual(await getDeviceInfo(), deviceInfo)
    result = { ...appInfo }
    assert.deepEqual(await getAppInfo(), appInfo)
    await assert.rejects(() => getDeviceInfo(), error => error.code === 'invalid_response')
    result = { ...deviceInfo }
    assert.deepEqual(await getDeviceInfo(), deviceInfo)
    await assert.rejects(() => getAppInfo(), error => error.code === 'invalid_response')
    for (const field of Object.keys(appInfo)) {
      result = { ...appInfo, [field]: null }
      await assert.rejects(() => getAppInfo(), error => error.code === 'invalid_response')
    }
    for (const field of Object.keys(deviceInfo)) {
      result = { ...deviceInfo, [field]: 42 }
      await assert.rejects(() => getDeviceInfo(), error => error.code === 'invalid_response')
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
    try { await assert.rejects(() => client.invoke('clipboard.read', {}, value => value), error => error.code === code) }
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

test('disconnect rejects pending calls and emits a single cleanup message even if transport is gone', async context => {
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
  const warning = context.mock.method(console, 'warn', () => {})
  assert.doesNotThrow(() => lost.dispose())
  assert.equal(lost.available, false)
  assert.equal(warning.mock.callCount(), 1)
  assert.equal(warning.mock.calls[0].arguments[0], 'Native disconnect notice could not be delivered')
})

test('default feature calls share lazy startup, and teardown creates a new session', async () => {
  const original = globalThis.window
  const originalDocument = globalThis.document
  const target = new EventTarget()
  let additions = 0, removals = 0
  const host = mock(request => request.command === 'core.connect' ? ready : request.command === 'app.info' ? appInfo
    : request.command === 'device.info' ? deviceInfo
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
    assert.deepEqual(await Promise.all([getText(), getAppInfo(), getDeviceInfo()]), ['native', appInfo, deviceInfo])
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
    let started
    const handshakeStarted = new Promise(resolve => { started = resolve })
    host.bridge.InvokeDotNet = (...args) => new Promise(resolve => { complete = () => resolve(invoke(...args)); started() })
    const pending = connect()
    await handshakeStarted
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

test('a cancelled handshake finishes and retires before a replacement reaches native', async () => {
  const original = globalThis.window, originalDocument = globalThis.document
  let releaseFirst, firstStarted, session
  const firstArrival = new Promise(resolve => { releaseFirst = resolve })
  const started = new Promise(resolve => { firstStarted = resolve })
  const retired = new Set()
  let connections = 0
  const host = mock(async request => {
    if (request.command === 'core.connect') {
      if (++connections === 1) { firstStarted(); await firstArrival }
      if (retired.has(request.session)) throw new NativeError('stale_session', 'Retired document')
      if (session && session !== request.session) retired.add(session)
      session = request.session
      return ready
    }
    if (session !== request.session) throw new NativeError('stale_session', 'Disconnected document')
    return { text: 'replacement still owns native' }
  })
  const send = host.bridge.SendRawMessage
  host.bridge.SendRawMessage = wire => {
    send(wire)
    const notice = JSON.parse(wire)
    if (notice.kind === 'disconnect' && notice.session === session) {
      retired.add(session)
      session = undefined
    }
  }
  globalThis.window = { location: { href: 'app://0.0.0.1/' }, HybridWebView: host.bridge, addEventListener() {}, removeEventListener() {} }
  globalThis.document = {}
  let replacement
  try {
    disconnect()
    const first = connect()
    await started
    disconnect()
    await assert.rejects(() => first, error => error.code === 'cancelled')
    replacement = connect()
    await new Promise(resolve => setImmediate(resolve))
    assert.equal(connections, 1, 'replacement must not overtake the underlying handshake')
    releaseFirst()
    const client = await replacement
    assert.equal(session, client.session)
    assert.ok(retired.has(host.requests[0].session), 'late-established disposed session must be retired')
    assert.equal(await getText(), 'replacement still owns native')
  } finally {
    disconnect()
    releaseFirst()
    await replacement?.catch(() => {})
    globalThis.window = original
    globalThis.document = originalDocument
  }
})

test('disconnect during framework bootstrap does not initialize duplicate platform listeners', async () => {
  const framework = await readFile(new URL('./fixtures/hybridwebview.js', import.meta.url), 'utf8')
  const original = globalThis.window, originalDocument = globalThis.document
  const target = new EventTarget(), scripts = [], platformListeners = []
  const host = mock(request => request.command === 'core.connect' ? ready : { accepted: true })
  globalThis.window = {
    location: { href: 'https://0.0.0.1/' },
    addEventListener: target.addEventListener.bind(target), removeEventListener: target.removeEventListener.bind(target),
    dispatchEvent: target.dispatchEvent.bind(target),
    chrome: { webview: {
      addEventListener(name, listener) { assert.equal(name, 'message'); platformListeners.push(listener) },
      postMessage() {},
    } },
  }
  globalThis.document = { createElement() { return {} }, head: { append(script) { scripts.push(script) } } }
  const fetch = async (_, options) => {
    const body = JSON.parse(options.headers['X-Maui-Request-Body'])
    const result = await host.bridge.InvokeDotNet(body.MethodName, body.ParamValues.map(value => JSON.parse(value)))
    return { json: async () => ({ IsJson: false, Result: result }) }
  }
  let first, replacement
  try {
    disconnect()
    first = connect().catch(error => error)
    await new Promise(resolve => setImmediate(resolve))
    disconnect()
    replacement = connect()
    await new Promise(resolve => setImmediate(resolve))
    assert.equal(scripts.length, 1)
    runInNewContext(framework, { window: globalThis.window, CustomEvent, fetch, console })
    scripts[0].onload()
    assert.equal((await first).code, 'cancelled')
    const client = await replacement
    let received = 0
    await client.subscribe('custom.changed', ack, () => received++)
    const message = JSON.stringify({ version: 1, session: client.session, id: 'event', topic: 'custom.changed', data: { accepted: true } })
    for (const listener of platformListeners) listener({ data: encodeURIComponent(message) })
    assert.equal(platformListeners.length, 1)
    assert.equal(received, 1)
  } finally {
    disconnect()
    for (const script of scripts) script.onerror()
    await first
    await replacement?.catch(() => {})
    globalThis.window = original
    globalThis.document = originalDocument
  }
})

test('an unsettled cancelled handshake cannot leave replacement startup waiting forever', async context => {
  context.mock.timers.enable({ apis: ['setTimeout', 'setInterval'] })
  const original = globalThis.window, originalDocument = globalThis.document
  let complete, started, delayFirst = true
  const handshakeStarted = new Promise(resolve => { started = resolve })
  const host = mock(() => {
    if (!delayFirst) return ready
    delayFirst = false
    return new Promise(resolve => { complete = resolve; started() })
  })
  globalThis.window = { location: { href: 'app://0.0.0.1/' }, HybridWebView: host.bridge, addEventListener() {}, removeEventListener() {} }
  globalThis.document = {}
  try {
    disconnect()
    const first = connect()
    await handshakeStarted
    disconnect()
    await assert.rejects(() => first, error => error.code === 'cancelled')
    const replacement = connect()
    const rejected = assert.rejects(() => replacement, error => error.code === 'transport_timeout')
    context.mock.timers.tick(28000)
    await rejected
    assert.equal(host.requests.length, 1)
    complete(ready)
    assert.equal((await connect()).available, true)
  } finally {
    disconnect()
    complete?.(ready)
    globalThis.window = original
    globalThis.document = originalDocument
    context.mock.timers.reset()
  }
})

test('a timed-out framework script remains shared until it finishes or errors', async context => {
  context.mock.timers.enable({ apis: ['setTimeout'] })
  const original = globalThis.window, originalDocument = globalThis.document
  const host = mock(() => ready), scripts = []
  globalThis.window = { location: { href: 'app://0.0.0.1/' } }
  globalThis.document = { createElement() { return {} }, head: { append(script) { scripts.push(script) } } }
  try {
    const pending = loadNativeBridge()
    const rejected = assert.rejects(() => pending, error => error.code === 'transport_error')
    context.mock.timers.tick(5000)
    await rejected
    await assert.rejects(() => loadNativeBridge(), error => error.code === 'transport_error')
    assert.equal(scripts.length, 1)
    globalThis.window.HybridWebView = host.bridge
    scripts[0].onload()
    assert.equal(await loadNativeBridge(), host.bridge)
  } finally {
    for (const script of scripts) script.onerror()
    globalThis.window = original
    globalThis.document = originalDocument
    context.mock.timers.reset()
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
