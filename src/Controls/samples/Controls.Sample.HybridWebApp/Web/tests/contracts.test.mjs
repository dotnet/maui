import { test } from 'node:test'
import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { NativeClient, NativeError, configureBridge, connect, resetBridge } from '@maui/api/core'
import { getIdentity, sample } from '../src/native/sample.ts'
import { features, availableFeatures, navigationPages } from '../src/catalog.ts'

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
  assert.deepEqual(await sample(client).echo({ text: null, items: [], number: 0, flag: false }), { text: null, items: [], number: 0, flag: false, counter: 1 })
  client.dispose()
})

test('playground reload identity composes installed Essentials helpers with app-owned diagnostics', async () => {
  const identity = { appName: 'Native', version: '1', build: '2', platform: 'Android', osVersion: '16', model: 'Device',
    architecture: 'arm64', pid: 123, startupId: 'native-startup', startedAt: '2026-10-12T00:00:00Z' }
  const host = mock(request => {
    if (request.command === 'core.connect') return { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 }
    if (request.command === 'app.info') return { appName: identity.appName, version: identity.version, build: identity.build }
    if (request.command === 'device.info') return { platform: identity.platform, osVersion: identity.osVersion, model: identity.model }
    assert.equal(request.command, 'sample.diagnostics')
    return { architecture: identity.architecture, pid: identity.pid, startupId: identity.startupId, startedAt: identity.startedAt }
  })
  configureBridge(host.bridge)
  const client = await connect()
  try {
    assert.deepEqual(await getIdentity(client), identity)
    assert.deepEqual(await getIdentity(client), identity, 'the host owns process identity across repeated reads')
    assert.deepEqual(host.requests.map(request => request.command), ['core.connect', 'sample.diagnostics', 'app.info', 'device.info', 'sample.diagnostics', 'app.info', 'device.info'])
    assert.ok(host.requests.every(request => request.session === client.session))
    const source = await readFile(new URL('../src/native/sample.ts', import.meta.url), 'utf8')
    assert.ok(source.includes("from '@maui/api/essentials/app-info'"))
    assert.ok(source.includes("from '@maui/api/essentials/device-info'"))
  } finally { resetBridge() }
})

test('app-owned diagnostics reject malformed native fields instead of inventing reload identity', async () => {
  const identity = { appName: 'Native', version: '1', build: '2', platform: 'Android', osVersion: '16', model: 'Device',
    architecture: 'arm64', pid: 123, startupId: 'native-startup', startedAt: '2026-10-12T00:00:00Z' }
  let output = identity
  configureBridge(mock(request => request.command === 'core.connect'
    ? { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 } : output).bridge)
  const client = await connect()
  try {
    for (const [field, value] of [['architecture', null], ['pid', '123'], ['pid', null], ['startupId', 42], ['startedAt', {}]]) {
      output = { ...identity, [field]: value }
      await assert.rejects(() => getIdentity(client), error => error.code === 'invalid_response')
    }
  } finally { resetBridge() }
})

test('catalog has all 121 families with 19 P0, and later families never claim implementation', () => {
  assert.equal(features.length, 121)
  assert.equal(new Set(features.map(feature => feature.id)).size, 121)
  assert.deepEqual(['P0', 'P1', 'P2', 'P3'].map(priority => features.filter(feature => feature.priority === priority).length), [19, 29, 47, 26])
  assert.ok(features.filter(feature => !feature.experiment).every(feature => feature.description.startsWith('Not implemented')))
})

test('explorer shows implemented experiments with descriptions, without a priority filter', async () => {
  assert.equal(availableFeatures.length, 22)
  assert.ok(availableFeatures.every(feature => feature.experiment && feature.description))
  assert.ok(availableFeatures.every(feature => features.includes(feature)))
  assert.equal(new Set(availableFeatures.map(feature => feature.experiment)).size, availableFeatures.length)
  const html = await readFile(new URL('../index.html', import.meta.url), 'utf8')
  assert.ok(!html.includes('id="priority"'))
  const browser = features.find(feature => feature.id === 'browser')
  assert.ok(browser.description.includes('Only implemented experiments are shown'))
  assert.ok(!browser.description.includes('priority'))
  assert.ok(!availableFeatures.some(feature => feature.id === 'browser'), 'catalog explanation is not a duplicate developer page')
})

test('implemented routes are owned by frontend configuration rather than a C# feature catalog', async () => {
  assert.equal(navigationPages.length, 24)
  assert.ok(!navigationPages.some(page => page.id.startsWith('planned-')))
  const connection = await readFile(new URL('../src/connection.ts', import.meta.url), 'utf8')
  assert.doesNotMatch(connection, /registerWindowRoutes|window\.routes/)
})

test('duplicate family bookmarks resolve to unique canonical experiments', async () => {
  const { readRoute } = await import('../src/routes.ts')
  for (const [alias, canonical] of [['registration', 'arguments'], ['modules', 'arguments'], ['hub', 'arguments'],
    ['cleanup', 'work'], ['readiness', 'identity'], ['inspection', 'identity'],
    ['platform', 'policy'], ['browser', 'vite'], ['offline', 'vite'], ['action-sheet', 'dialogs']]) {
    assert.equal(readRoute(`#/${alias}`).page.id, canonical)
    assert.equal(readRoute(`#/${alias}`).error, undefined)
    assert.ok(!navigationPages.some(page => page.id === alias))
  }
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
  host.bridge.InvokeDotNet = mock(() => ({ completed: 3, total: 1 })).bridge.InvokeDotNet
  await assert.rejects(() => sample(client).work(1, 100).result, NativeError)
  client.dispose()
})
