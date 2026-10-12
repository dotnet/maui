import { test } from 'node:test'
import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { NativeClient, NativeError, asciiJson, object, ack, progress, isNativeOrigin } from '../src/maui/core.ts'
import { sample } from '../src/maui/app/sample.ts'
import { clipboard } from '../src/maui/essentials/clipboard.ts'
import { preferences } from '../src/maui/essentials/preferences.ts'
import { getInfo } from '../src/maui/app/info.ts'
import { features, availableFeatures } from '../src/catalog.ts'

// Explicit test double only: never imported by the UI or used as a browser fallback.
function mock(handler) {
  const requests = [], messages = []
  return {
    requests, messages,
    bridge: {
      async InvokeDotNet(method, values) {
        assert.equal(method, 'Invoke')
        assert.equal(values.length, 1)
        const header = JSON.stringify({ MethodName: method, ParamValues: values.map(value => JSON.stringify(value)) })
        assert.match(header, /^[\x00-\x7f]*$/)
        new Headers({ 'X-Maui-Request-Body': header })
        const request = JSON.parse(values[0])
        requests.push(request)
        const output = await handler(request)
        return JSON.stringify({ version: 1, session: request.session, id: request.id, status: 'success', result: output, error: null })
      },
      SendRawMessage(message) { messages.push(JSON.parse(message)) },
    },
  }
}

test('Unicode, escapes, null, arrays and DTOs survive ByteString headers and both framework serializations', async () => {
  const dto = { text: '你好 👋 café\n"quote"\\\0', number: 12.5, flag: true, items: ['日本語', '\r\n', '\\'] }
  const host = mock(request => ({ ...request.args, counter: 1 }))
  const client = new NativeClient(host.bridge)
  assert.deepEqual(await sample(client).echo(dto), { ...dto, counter: 1 })
  assert.deepEqual(JSON.parse(asciiJson({ text: null, items: [], number: 0, flag: false })), { text: null, items: [], number: 0, flag: false })
  client.dispose()
})

test('catalog has all 121 families with 19 P0, and later families never claim implementation', () => {
  assert.equal(features.length, 121)
  assert.equal(new Set(features.map(feature => feature.id)).size, 121)
  assert.deepEqual(['P0', 'P1', 'P2', 'P3'].map(priority => features.filter(feature => feature.priority === priority).length), [19, 29, 47, 26])
  assert.ok(features.filter(feature => feature.priority !== 'P0').every(feature => !feature.experiment && feature.description.startsWith('Not implemented')))
})

test('explorer shows implemented experiments with descriptions, without a priority filter', async () => {
  assert.equal(availableFeatures.length, 19)
  assert.ok(availableFeatures.every(feature => feature.experiment && feature.description))
  assert.ok(availableFeatures.every(feature => features.includes(feature)))
  const html = await readFile(new URL('../index.html', import.meta.url), 'utf8')
  assert.ok(!html.includes('id="priority"'))
})

test('browser origin and unavailable transport never emulate native APIs', async () => {
  assert.equal(isNativeOrigin('http://127.0.0.1:5173/'), false)
  assert.equal(isNativeOrigin('https://example.com/'), false)
  assert.equal(isNativeOrigin('app://0.0.0.1/index.html'), true)
  assert.equal(isNativeOrigin('https://0.0.0.1/index.html'), true)
  const client = new NativeClient(undefined)
  for (const action of [() => client.connect(), () => clipboard(client).readText(), () => preferences(client).get('example'), () => getInfo(client)]) {
    await assert.rejects(action, error => error instanceof NativeError && error.code === 'native_unavailable')
  }
  client.dispose()
})

test('typed clipboard/preferences helpers validate values, including null', async () => {
  const host = mock(request => request.command === 'clipboard.read' ? { text: null } : { key: request.args.key, value: 'é' })
  const client = new NativeClient(host.bridge)
  assert.equal(await clipboard(client).readText(), null)
  assert.deepEqual(await preferences(client).get('key'), { key: 'key', value: 'é' })
  assert.equal(host.requests[1].command, 'preferences.read')
  client.dispose()
})

test('native error, intentional failure and cancelled are not success-shaped', async () => {
  for (const [status, code] of [['error', 'intentional_error'], ['error', 'invalid_arguments'], ['error', 'unknown_command'], ['error', 'disabled_command'], ['cancelled', 'cancelled']]) {
    const host = mock(() => null)
    host.bridge.InvokeDotNet = async (_, [wire]) => {
      const request = JSON.parse(wire)
      return JSON.stringify({ version: 1, session: request.session, id: request.id, status, result: null, error: { code, message: 'Expected native rejection' } })
    }
    const client = new NativeClient(host.bridge)
    await assert.rejects(() => sample(client).fail(), error => error.code === code)
    client.dispose()
  }
})

test('output shape, response identity and transport failures are rejected', async () => {
  const host = mock(() => ({ counter: 'not a number' }))
  const client = new NativeClient(host.bridge)
  await assert.rejects(() => sample(client).getCounter(), error => error.code === 'invalid_response')
  host.bridge.InvokeDotNet = async () => JSON.stringify({ version: 1, session: 'obsolete', id: 'wrong', status: 'success', result: true, error: null })
  await assert.rejects(() => sample(client).getCounter(), error => error.code === 'invalid_response')
  host.bridge.InvokeDotNet = async () => { throw new TypeError('Fetch failed') }
  await assert.rejects(() => sample(client).getCounter(), error => error.code === 'transport_error')
  client.dispose()
})

test('events are session-scoped, validated, unsubscribed and not multiplied', async () => {
  const host = mock(() => ({ accepted: true }))
  const client = new NativeClient(host.bridge)
  const events = []
  const unsubscribe = await sample(client).onCounter((counter, id) => events.push({ counter, id }))
  await assert.rejects(() => sample(client).onCounter(() => {}), error => error.code === 'duplicate_subscription')
  client.receive(JSON.stringify({ version: 1, session: 'stale', id: 'old', topic: 'service.changed', data: { counter: 999 } }))
  client.receive(JSON.stringify({ version: 1, session: client.session, id: 'new', topic: 'service.changed', data: { counter: 2 } }))
  assert.deepEqual(events, [{ counter: 2, id: 'new' }])
  await unsubscribe()
  await unsubscribe()
  client.receive(JSON.stringify({ version: 1, session: client.session, id: 'later', topic: 'service.changed', data: { counter: 3 } }))
  assert.equal(events.length, 1)
  assert.deepEqual(host.requests.map(request => request.command), ['core.subscribe', 'core.unsubscribe'])
  client.dispose()
})

test('operation cancellation uses correlated request ID and native progress result', async () => {
  const host = mock(request => request.command === 'core.cancel' ? { accepted: true } : { completed: 1, total: 1 })
  const client = new NativeClient(host.bridge)
  const operation = sample(client).work(1, 100)
  assert.deepEqual(await operation.result, { completed: 1, total: 1 })
  assert.equal(await operation.cancel(), true)
  assert.equal(host.requests[1].args.requestId, operation.id)
  assert.throws(() => progress({ completed: 3, total: 1 }), NativeError)
  assert.throws(() => object(null), NativeError)
  assert.throws(() => ack({ accepted: 'yes' }), NativeError)
  client.dispose()
})

test('disconnect rejects pending calls and emits a single cleanup message', async () => {
  let complete
  const host = mock(() => new Promise(resolve => { complete = resolve }))
  const client = new NativeClient(host.bridge)
  const result = sample(client).getCounter()
  client.dispose()
  client.dispose()
  await assert.rejects(() => result, error => error.code === 'cancelled')
  assert.deepEqual(host.messages, [{ session: client.session, kind: 'disconnect' }])
  complete({ counter: 7 })
  await assert.rejects(() => sample(client).getCounter(), error => error.code === 'native_unavailable')
})

test('document listeners attach once and pagehide removes listeners and subscriptions', async () => {
  const original = globalThis.window
  const target = new EventTarget()
  let additions = 0, removals = 0
  globalThis.window = {
    addEventListener(...args) { additions++; target.addEventListener(...args) },
    removeEventListener(...args) { removals++; target.removeEventListener(...args) },
  }
  const host = mock(() => ({ accepted: true }))
  const client = new NativeClient(host.bridge)
  try {
    let count = 0
    await sample(client).onCounter(() => count++)
    client.attachToDocument()
    client.attachToDocument()
    assert.equal(additions, 2)
    const message = JSON.stringify({ version: 1, session: client.session, id: 'event', topic: 'service.changed', data: { counter: 1 } })
    target.dispatchEvent(new CustomEvent('HybridWebViewMessageReceived', { detail: { message } }))
    assert.equal(count, 1)
    target.dispatchEvent(new Event('pagehide'))
    assert.equal(removals, 2)
    assert.equal(client.available, false)
    target.dispatchEvent(new CustomEvent('HybridWebViewMessageReceived', { detail: { message } }))
    assert.equal(count, 1)
    assert.equal(host.messages.filter(message => message.kind === 'disconnect').length, 1)
  } finally { client.dispose(); globalThis.window = original }
})
