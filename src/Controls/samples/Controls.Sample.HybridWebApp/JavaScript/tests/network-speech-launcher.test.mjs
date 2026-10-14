import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, connect, resetBridge } from '@maui/api/core'
import { getConnectivity, onConnectivityChanged } from '@maui/api/essentials/connectivity'
import { getLocales, speak } from '@maui/api/essentials/text-to-speech'
import { openUrl } from '@maui/api/essentials/launcher'
import { mock } from './bridge.mjs'

const ready = { commands: ['connectivity.info', 'speech.locales', 'speech.speak', 'launcher.openUrl'],
  interactiveCommands: ['speech.speak', 'launcher.openUrl'], interactiveTimeoutMilliseconds: 300000,
  preferenceNamespace: 'native.', maximumTextLength: 64 }
const locale = { id: 'native', language: 'en-US', country: '', name: 'Native voice' }
afterEach(() => resetBridge())

test('new service modules import without native calls and never fall back in a browser', async () => {
  const host = mock(() => ready)
  configureBridge(host.bridge)
  for (const path of ['connectivity', 'text-to-speech', 'launcher']) await import(`@maui/api/essentials/${path}`)
  assert.equal(host.requests.length, 0)
  resetBridge()
  for (const action of [getConnectivity, () => onConnectivityChanged(() => {}), getLocales,
    () => speak('Hello'), () => openUrl('https://dot.net/maui')])
    await assert.rejects(action, error => error.code === 'native_unavailable')
})

test('fresh reads and native change listeners share one client and detach only after the last unsubscribe', async () => {
  let snapshot = { networkAccess: 'Internet', connectionProfiles: ['WiFi'] }
  const host = mock(request => request.command === 'core.connect' ? ready :
    request.command === 'connectivity.info' ? snapshot : { accepted: true })
  configureBridge(host.bridge)
  const client = await connect()
  assert.deepEqual(await getConnectivity(), snapshot)
  snapshot = { networkAccess: 'None', connectionProfiles: [] }
  assert.deepEqual(await getConnectivity(), snapshot)
  const first = [], second = []
  const release1 = await onConnectivityChanged(value => first.push(value))
  const release2 = await onConnectivityChanged(value => second.push(value))
  assert.equal(first.length, 0, 'subscription must not invent an initial event')
  assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  const emit = (value, session = client.session) => client.receive(JSON.stringify({
    version: 1, session, id: crypto.randomUUID(), topic: 'connectivity.changed', data: value,
  }))
  emit({ networkAccess: 'Local', connectionProfiles: ['Ethernet'] })
  assert.equal(first.length, 1); assert.equal(second.length, 1)
  await release1(); await release1()
  assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 0)
  emit(snapshot)
  assert.equal(first.length, 1); assert.equal(second.length, 2)
  await release2()
  assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
  emit(snapshot)
  assert.equal(second.length, 2)
  const release3 = await onConnectivityChanged(value => first.push(value))
  emit(snapshot, crypto.randomUUID())
  assert.equal(first.length, 1)
  client.dispose(); emit(snapshot)
  assert.equal(first.length, 1)
  await release3()
  assert.ok(host.requests.every(request => request.session === client.session))
})

test('native network events validate output and a throwing callback cannot stop other listeners', async t => {
  const warnings = []
  t.mock.method(console, 'warn', (...args) => warnings.push(args))
  const host = mock(request => request.command === 'core.connect' ? ready : { accepted: true })
  configureBridge(host.bridge)
  const client = await connect(), values = []
  const release1 = await onConnectivityChanged(() => { throw new Error('Consumer callback failed') })
  const release2 = await onConnectivityChanged(value => values.push(value))
  const emit = data => client.receive(JSON.stringify({ version: 1, session: client.session,
    id: crypto.randomUUID(), topic: 'connectivity.changed', data }))
  emit({ networkAccess: null, connectionProfiles: [] })
  emit({ networkAccess: 'Internet', connectionProfiles: [42] })
  assert.deepEqual(values, [])
  emit({ networkAccess: 'Internet', connectionProfiles: ['Cellular'] })
  assert.equal(values.length, 1)
  assert.equal(warnings.length, 5)
  await release1(); await release2()
})

test('speech uses real locales/options and operation-specific cancellation through the shared client', async () => {
  let finish
  const host = mock(request => {
    if (request.command === 'core.connect') return ready
    if (request.command === 'speech.locales') return { locales: [locale] }
    if (request.command === 'speech.speak') return new Promise(resolve => { finish = resolve })
    if (request.command === 'connectivity.info') return { networkAccess: 'Local', connectionProfiles: [] }
    return { accepted: true }
  })
  const invoke = host.bridge.InvokeDotNet
  host.bridge.InvokeDotNet = async (...args) => {
    const envelope = JSON.parse(await invoke(...args))
    if (envelope.result?.cancelled) {
      envelope.status = 'cancelled'; envelope.result = null
      envelope.error = { code: 'cancelled', message: 'Native speech cancelled.' }
    }
    return JSON.stringify(envelope)
  }
  configureBridge(host.bridge)
  assert.deepEqual(await getLocales(), [locale])
  const operation = await speak('你好 👋', { locale, pitch: 1.5, volume: 0.25, rate: 0.8 })
  const cancelled = assert.rejects(operation.result, error => error.code === 'cancelled')
  assert.equal(await operation.cancel(), true)
  const request = host.requests.find(request => request.command === 'speech.speak')
  assert.deepEqual(request.args, { text: '你好 👋', options: { pitch: 1.5, volume: 0.25, rate: 0.8, locale } })
  assert.equal(host.requests.find(request => request.command === 'core.cancel').args.requestId, operation.id)
  finish({ cancelled: true })
  await cancelled
  assert.equal((await getConnectivity()).networkAccess, 'Local', 'speech cancellation does not disconnect other features')
  assert.deepEqual(await getLocales(), [locale])
})

test('speech completion and default options are not a fake global stop API', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready :
    request.command === 'speech.locales' ? { locales: [] } : { accepted: true })
  configureBridge(host.bridge)
  assert.deepEqual(await getLocales(), [])
  const operation = await speak('Hello')
  assert.equal(await operation.result, undefined)
  assert.deepEqual(host.requests.find(request => request.command === 'speech.speak').args, { text: 'Hello', options: {} })
  assert.deepEqual(Object.keys(await import('@maui/api/essentials/text-to-speech')).sort(), ['getLocales', 'speak'])
})

test('invalid speech input and host text budget reject before native speech', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : { accepted: true })
  configureBridge(host.bridge)
  for (const [text, options] of [['', {}], ['  ', {}], ['a'.repeat(65), {}], ['Hello', { pitch: -1 }],
    ['Hello', { volume: 2 }], ['Hello', { rate: 0 }], ['Hello', { pitch: NaN }], ['Hello', { rate: Infinity }],
    ['Hello', { stop: true }], ['Hello', { locale: { ...locale, id: null } }]])
    await assert.rejects(() => speak(text, options), error => error.code === 'invalid_arguments')
  assert.equal(host.requests.filter(request => request.command === 'speech.speak').length, 0)
})

test('external URL calls the native launcher only and rejects unsafe addresses before invoking', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : { accepted: true })
  configureBridge(host.bridge)
  for (const url of ['http://example.com', 'file:///private/file', 'javascript:alert(1)', 'mailto:a@example.com',
    'https://user:password@example.com', 'https://127.0.0.1', 'https://0x7f000001', 'https://[::1]',
    'https://10.1.2.3', 'https://localhost', 'https://foo.local', 'https://host.internal', 'https://example.com:9443',
    'https://example.com./', 'https://example.com\\@localhost', ' https://example.com', 'https://example.com/' + 'a'.repeat(2048)])
    await assert.rejects(() => openUrl(url), error => error.code === 'invalid_url')
  assert.equal(host.requests.length, 0)
  await openUrl('https://learn.microsoft.com/dotnet/maui/')
  assert.deepEqual(host.requests.map(request => request.command), ['core.connect', 'launcher.openUrl'])
  assert.deepEqual(host.requests[1].args, { url: 'https://learn.microsoft.com/dotnet/maui/' })
})

test('malformed native feature outputs are explicit errors and launch refusal is not success', async () => {
  let output
  const host = mock(request => request.command === 'core.connect' ? ready : output)
  configureBridge(host.bridge)
  for (const [value, call] of [
    [{ networkAccess: null, connectionProfiles: [] }, getConnectivity],
    [{ networkAccess: 'Internet', connectionProfiles: 'WiFi' }, getConnectivity],
    [{ locales: null }, getLocales], [{ locales: [{ ...locale, name: null }] }, getLocales],
    [{ accepted: 'yes' }, async () => (await speak('Hello')).result],
    [{ accepted: false }, async () => (await speak('Hello')).result],
    [{ accepted: 'yes' }, () => openUrl('https://dot.net/maui')],
  ]) {
    output = value
    await assert.rejects(call, error => error.code === 'invalid_response')
  }
  output = { accepted: false }
  await assert.rejects(() => openUrl('https://dot.net/maui'), error => error.code === 'launch_failed')
})
