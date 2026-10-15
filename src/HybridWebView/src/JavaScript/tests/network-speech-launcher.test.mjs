import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, connect, resetBridge } from '@maui/api/core'
import { getConnectivity, onConnectivityChanged } from '@maui/api/essentials/connectivity'
import { getLocales, speak } from '@maui/api/essentials/text-to-speech'
import { canOpen, openUrl, tryOpen } from '@maui/api/essentials/launcher'
import { mock, nativeProperty } from './bridge.mjs'

const ready = { commands: ['connectivity.networkAccess', 'connectivity.connectionProfiles', 'speech.locales', 'speech.speak', 'launcher.open', 'launcher.canOpen', 'launcher.tryOpen'],
  interactiveCommands: ['speech.speak', 'launcher.open', 'launcher.tryOpen'], interactiveTimeoutMilliseconds: 300000,
  maximumTextLength: 64 }
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
  let snapshot = { networkAccess: 'internet', connectionProfiles: ['wiFi'] }
  const host = mock(request => request.command === 'core.connect' ? ready :
    request.command.startsWith('connectivity.') ? nativeProperty(request.command, snapshot) : { accepted: true })
  configureBridge(host.bridge)
  const client = await connect()
  assert.deepEqual(await getConnectivity(), snapshot)
  snapshot = { networkAccess: 'none', connectionProfiles: [] }
  assert.deepEqual(await getConnectivity(), snapshot)
  const first = [], second = []
  const release1 = await onConnectivityChanged(value => first.push(value))
  const release2 = await onConnectivityChanged(value => second.push(value))
  assert.equal(first.length, 0, 'subscription must not invent an initial event')
  assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  const emit = (value, session = client.session) => client.receive(JSON.stringify({
    version: 1, session, id: crypto.randomUUID(), topic: 'connectivity.changed', data: value,
  }))
  emit({ networkAccess: 'local', connectionProfiles: ['ethernet'] })
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
  emit({ networkAccess: 'internet', connectionProfiles: [42] })
  assert.deepEqual(values, [])
  emit({ networkAccess: 'internet', connectionProfiles: ['cellular'] })
  assert.equal(values.length, 1)
  assert.equal(warnings.length, 5)
  await release1(); await release2()
})

test('speech uses real locales/options and operation-specific cancellation through the shared client', async () => {
  let finish
  const host = mock(request => {
    if (request.command === 'core.connect') return ready
    if (request.command === 'speech.locales') return [locale]
    if (request.command === 'speech.speak') return new Promise(resolve => { finish = resolve })
    if (request.command.startsWith('connectivity.')) return nativeProperty(request.command, { networkAccess: 'local', connectionProfiles: [] })
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
  assert.equal((await getConnectivity()).networkAccess, 'local', 'speech cancellation does not disconnect other features')
  assert.deepEqual(await getLocales(), [locale])
})

test('speech completion and default options are not a fake global stop API', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready :
    request.command === 'speech.locales' ? [] : null)
  configureBridge(host.bridge)
  assert.deepEqual(await getLocales(), [])
  const operation = await speak('Hello')
  assert.equal(await operation.result, undefined)
  assert.deepEqual(host.requests.find(request => request.command === 'speech.speak').args, { text: 'Hello', options: null })
  assert.deepEqual(Object.keys(await import('@maui/api/essentials/text-to-speech')).sort(), ['getLocales', 'speak'])
})

test('only invalid speech JSON contracts reject before native speech', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : { accepted: true })
  configureBridge(host.bridge)
  for (const [text, options] of [[null, {}], [42, {}], ['Hello', { pitch: NaN }], ['Hello', { rate: Infinity }],
    ['Hello', { volume: 1e300 }], ['Hello', { pitch: '1' }], ['Hello', []], ['Hello', { stop: true }],
    ['Hello', { locale: { ...locale, id: 42 } }], ['Hello', { locale: { ...locale, extra: true } }]])
    await assert.rejects(() => speak(text, options), error => error.code === 'invalid_arguments')
  assert.equal(host.requests.filter(request => request.command === 'speech.speak').length, 0)
})

test('speech passes empty whitespace long text and out-of-range representable options without policy validation', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : null)
  configureBridge(host.bridge)
  const options = { pitch: -5, volume: 3, rate: 0, locale: { id: ' voice ', language: '', country: '', name: '語'.repeat(1024) } }
  for (const text of ['', ' \t\n', '文'.repeat(8192)]) {
    assert.equal(await (await speak(text, options)).result, undefined)
    assert.deepEqual(host.requests.at(-1).args, { text, options })
  }
  const nullable = { pitch: null, volume: null, rate: null, locale: null }
  await (await speak('', nullable)).result
  assert.deepEqual(host.requests.at(-1).args, { text: '', options: nullable })
  await (await speak('', null)).result
  assert.deepEqual(host.requests.at(-1).args, { text: '', options: null })
  const nullableLocale = { locale: { id: null, language: 'en', country: null, name: null } }
  await (await speak('', nullableLocale)).result
  assert.deepEqual(host.requests.at(-1).args, { text: '', options: nullableLocale })
})

test('native speech validation errors reach the caller instead of being intercepted by the SDK', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : null)
  const invoke = host.bridge.InvokeDotNet
  host.bridge.InvokeDotNet = async (...args) => {
    const envelope = JSON.parse(await invoke(...args))
    if (host.requests.at(-1).command === 'speech.speak') {
      envelope.status = 'error'
      envelope.error = { code: 'native_error', message: 'Destination rejected speech.' }
    }
    return JSON.stringify(envelope)
  }
  configureBridge(host.bridge)
  const speech = await speak('', { pitch: -100 })
  await assert.rejects(speech.result, error => error.code === 'native_error')
  assert.deepEqual(host.requests.at(-1).args, { text: '', options: { pitch: -100 } })
})

test('URI launcher forwards platform schemes and native boolean results without product URL policy', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : false)
  configureBridge(host.bridge)
  for (const url of ['http://example.com', 'file:///private/file', 'javascript:alert(1)', 'mailto:a@example.com',
    'https://user:password@example.com', 'https://127.0.0.1', 'https://0x7f000001', 'https://[::1]',
    'https://10.1.2.3', 'https://localhost', 'https://foo.local', 'https://host.internal', 'https://example.com:9443',
    'https://example.com./', 'https://example.com\\@localhost', ' https://example.com', 'https://example.com/' + 'a'.repeat(2048)])
    assert.equal(await openUrl(url), false)
  assert.equal(await canOpen('mailto:user@example.com'), false)
  assert.equal(await tryOpen('custom:action'), false)
  assert.deepEqual(host.requests.at(-1).args, { uri: 'custom:action' })
  assert.equal(host.requests.at(-1).command, 'launcher.tryOpen')
  assert.equal(host.requests.filter(request => request.command === 'core.connect').length, 1)
})

test('malformed native feature outputs are explicit errors and launch refusal is not success', async () => {
  let output
  const host = mock(request => request.command === 'core.connect' ? ready : nativeProperty(request.command, output))
  configureBridge(host.bridge)
  for (const [value, call] of [
    [{ networkAccess: null, connectionProfiles: [] }, getConnectivity],
    [{ networkAccess: 'internet', connectionProfiles: 'wiFi' }, getConnectivity],
    [null, getLocales], [[{ ...locale, name: null }], getLocales], [{ locales: [locale] }, getLocales],
    [{ accepted: 'yes' }, async () => (await speak('Hello')).result],
    [{ accepted: false }, async () => (await speak('Hello')).result],
    [{ accepted: 'yes' }, () => openUrl('https://dot.net/maui')],
  ]) {
    output = value
    await assert.rejects(call, error => error.code === 'invalid_response')
  }
  output = false
  assert.equal(await openUrl('https://dot.net/maui'), false)
})
