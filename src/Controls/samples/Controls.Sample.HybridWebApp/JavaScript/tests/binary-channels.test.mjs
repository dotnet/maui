import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { NativeClient, NativeError, Channel, binaryBlob, invokeBinary, configureBridge, resetBridge, number } from '@maui/api/core'
import { mock } from './bridge.mjs'

afterEach(() => resetBridge())
const ready = { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 }
const tick = () => new Promise(resolve => setImmediate(resolve))
function deferred() {
  let resolve, reject
  const promise = new Promise((yes, no) => { resolve = yes; reject = no })
  return { promise, resolve, reject }
}

test('whole binary transfer is exact for empty, all values, large Blob, sliced view and 4 MiB, with bounded metadata only', async () => {
  const host = mock(() => ready, (_, payload) =>
    new Uint8Array(payload.buffer ?? payload, payload.byteOffset ?? 0, payload.byteLength))
  configureBridge(host.bridge)
  for (const length of [0, 256, 262144, 4194304]) {
    const bytes = Uint8Array.from({ length }, (_, index) => index % 256)
    assert.deepEqual(await invokeBinary('bytes.echo', [7], new Blob([bytes], { type: 'application/octet-stream' })), bytes)
    const sent = host.binaries.at(-1)
    assert.equal(sent.request.args[0], 7)
    assert.ok(JSON.stringify(sent.request).length < 100000)
    assert.ok(Object.values(sent.options.headers).every(value => value.length <= 240))
  }
  const bytes = Uint8Array.of(99, 0, 128, 255, 99)
  assert.deepEqual(await invokeBinary('bytes.echo', {}, bytes.subarray(1, 4)), Uint8Array.of(0, 128, 255))
  const blob = binaryBlob(await invokeBinary('bytes.echo', {}, Uint8Array.of(37, 80, 68, 70)), 'application/pdf')
  assert.equal(blob.type, 'application/pdf')
  assert.equal(await blob.text(), '%PDF')
})

test('missing opt-in never falls back to JSON, binary limit/metadata and real Core errors are explicit', async () => {
  let client = new NativeClient(mock(() => ready).bridge)
  await client.connect()
  await assert.rejects(() => client.invokeBinary('bytes.echo'), error => error.code === 'binary_unavailable')
  client.dispose()
  const host = mock(() => ready, () => new Uint8Array())
  client = new NativeClient(host.bridge)
  await client.connect()
  await assert.rejects(() => client.invokeBinary('bytes.echo', {}, new Uint8Array(4194305)), error => error.code === 'binary_limit')
  await assert.rejects(() => client.invokeBinary('bytes.echo', { large: 'x'.repeat(2880) }), error => error.code === 'invalid_arguments')
  assert.equal(host.binaries.length, 0)
  host.bridge.InvokeDotNetBinary = async () => {
    const error = new Error(JSON.stringify({ code: 'permission_denied', message: 'Denied by host.' }))
    error.dotNetErrorType = 'System.InvalidOperationException'
    throw error
  }
  await assert.rejects(() => client.invokeBinary('bytes.echo'), error => error instanceof NativeError && error.code === 'permission_denied')
  client.dispose()
})

test('three binary owners reserve a serialized fourth Core slot for channel fetches', async () => {
  const owners = deferred()
  let active = 0, peak = 0, fetches = 0
  const host = mock(() => ready, async request => {
    active++
    peak = Math.max(peak, active)
    try {
      if (request.command !== 'core.channel.fetch') await owners.promise
      else {
        await tick()
        fetches++
      }
      return Uint8Array.of(128)
    } finally { active-- }
  })
  const client = new NativeClient(host.bridge)
  await client.connect()
  const work = Array.from({ length: 3 }, () => client.invokeBinary('bytes.wait'))
  await tick()
  await assert.rejects(() => client.invokeBinary('bytes.wait'), error => error.code === 'busy')
  const results = await Promise.all(Array.from({ length: 8 }, () => client.invokeBinary('core.channel.fetch')))
  assert.equal(fetches, 8)
  assert.equal(peak, 4)
  assert.ok(results.every(bytes => bytes[0] === 128))
  owners.resolve()
  await Promise.all(work)
  client.dispose()
})

test('heavily escaped structured args fit actual Core metadata budgets without a policy expansion', async () => {
  const host = mock(() => ready, (_, bytes, options) => {
    const metadata = JSON.stringify({ MethodName: 'hybrid.invoke', Length: bytes.byteLength, Headers: options.headers })
    assert.ok(metadata.length <= 8192)
    assert.ok(encodeURIComponent(metadata).length <= 16384)
    assert.ok(Object.keys(options.headers).length <= 10)
    for (const value of Object.values(options.headers)) {
      assert.ok(value.length <= 240)
      assert.match(value, /^[\x20-\x7e]+$/)
    }
    return new Uint8Array()
  })
  const client = new NativeClient(host.bridge)
  await client.connect()
  for (const label of ['\\'.repeat(1000), '"'.repeat(1000), '你好'.repeat(150)]) {
    await client.invokeBinary('bytes.echo', { label })
    assert.equal(host.binaries.at(-1).request.args.label, label)
  }
  client.dispose()
})

test('channel binding limit and failed serialization retire partial bindings before invoking native', async () => {
  const host = mock(() => ready)
  const client = new NativeClient(host.bridge)
  await client.connect()
  const channels = Array.from({ length: 5 }, () => new Channel(number, () => {}))
  await assert.rejects(() => client.invoke('chunks', { a: channels[0], b: channels[1], c: channels[2], d: channels[3], e: channels[4] }, number),
    error => error.code === 'channel_limit')
  for (const channel of channels.slice(0, 4))
    await assert.rejects(channel.completion, error => error.code === 'channel_limit')
  assert.equal(host.requests.length, 1)
  await channels[4].unsubscribe()
  client.dispose()
})

function channelHost({ fetchGate, messages = 3, rawBytes } = {}) {
  let client
  const pending = new Map(), stored = new Map(), events = []
  const host = mock(async request => {
    if (request.command === 'core.connect') return ready
    if (request.command === 'core.channel.ack') {
      const key = `${request.args.channelId}:${request.args.sequence}`, waiter = pending.get(key)
      assert.ok(waiter)
      pending.delete(key)
      if (request.args.accepted) waiter.resolve()
      else waiter.reject(new Error('consumer_error'))
      return { accepted: true }
    }
    if (request.command === 'core.channel.close' || request.command === 'core.cancel') {
      for (const waiter of pending.values()) waiter.reject(new Error('cancelled'))
      pending.clear(); stored.clear()
      return { accepted: true }
    }
    const channel = request.args.channel
    for (let i = 1; i <= messages; i++) {
      const waiter = deferred(), dataId = crypto.randomUUID()
      pending.set(`${channel}:${i}`, waiter)
      const binary = i % 2 === 1
      if (binary) stored.set(dataId, rawBytes ?? Uint8Array.of(i, 128, 255))
      const event = { version: 1, session: request.session, id: request.id, topic: 'core.channel',
        data: { channelId: channel, sequence: i, kind: binary ? 'bytes' : 'json', value: i, dataId: binary ? dataId : null } }
      events.push(event)
      client.receive(JSON.stringify(event))
      await waiter.promise
    }
    client.receive(JSON.stringify({ version: 1, session: request.session, id: request.id, topic: 'core.channel',
      data: { channelId: channel, sequence: messages + 1, kind: 'complete' } }))
    return 1
  }, async request => {
    assert.equal(request.command, 'core.channel.fetch')
    if (fetchGate) await fetchGate.promise
    const bytes = stored.get(request.args.dataId)
    assert.ok(bytes, 'Native byte data fetched only once')
    stored.delete(request.args.dataId)
    return bytes
  })
  client = new NativeClient(host.bridge)
  return { host, client, events }
}

test('mixed ordered binary/JSON messages await async processing before sending consumption ack', async () => {
  const { host, client } = channelHost()
  await client.connect()
  const gate = deferred(), values = []
  const channel = new Channel(value => value instanceof Uint8Array ? value[0] : number(value), async value => {
    values.push(value)
    if (value === 1) await gate.promise
    await tick()
  })
  const work = client.invoke('chunks', { channel }, number)
  await tick()
  assert.deepEqual(values, [1])
  assert.equal(host.requests.filter(request => request.command === 'core.channel.ack').length, 0)
  gate.resolve()
  assert.equal(await work, 1)
  await channel.completion
  assert.deepEqual(values, [1, 2, 3])
  assert.deepEqual(host.requests.filter(request => request.command === 'core.channel.ack').map(request => request.args.sequence), [1, 2, 3])
  assert.equal(host.binaries.length, 2)
  assert.ok(host.binaries.every(item => item.bytes.byteLength === 0))
  await assert.rejects(() => client.invoke('chunks', { channel }, number), error => error.code === 'invalid_channel')
  client.dispose()
})

test('consumer throw negatively acknowledges raw data and never deadlocks the owner', async () => {
  const { client, host } = channelHost()
  await client.connect()
  const channel = Channel.bytes(async () => { await tick(); throw new Error('application callback failed') })
  const work = client.invoke('chunks', { channel }, number)
  await assert.rejects(channel.completion, error => error.code === 'consumer_error')
  await assert.rejects(work)
  assert.equal(host.requests.find(request => request.command === 'core.channel.ack').args.accepted, false)
  client.dispose()
})

test('a malformed native raw channel chunk is rejected before consumer delivery', async () => {
  const { client } = channelHost({ rawBytes: new Uint8Array(65537), messages: 1 })
  await client.connect()
  let callbacks = 0
  const channel = Channel.bytes(() => { callbacks++ })
  const work = client.invoke('chunks', { channel }, number)
  await assert.rejects(channel.completion, error => error.code === 'invalid_response')
  await assert.rejects(work)
  assert.equal(callbacks, 0)
  client.dispose()
})

test('unsubscribe during async consumption releases native data and suppresses late acknowledgement', async () => {
  const { client, host } = channelHost()
  await client.connect()
  const gate = deferred()
  const channel = Channel.bytes(() => gate.promise)
  const work = client.invoke('chunks', { channel }, number)
  await tick()
  await channel.unsubscribe()
  await assert.rejects(channel.completion, error => error.code === 'cancelled')
  await assert.rejects(work)
  gate.resolve()
  await tick()
  assert.equal(host.requests.filter(request => request.command === 'core.channel.ack').length, 0)
  client.dispose()
})

test('dispose/reload while raw fetch is pending cannot deliver old bytes or ack into a replacement client', async () => {
  const gate = deferred(), { client, host, events } = channelHost({ fetchGate: gate })
  await client.connect()
  let callbacks = 0
  const channel = Channel.bytes(() => { callbacks++ })
  const work = client.invoke('chunks', { channel }, number)
  await tick()
  client.dispose()
  await assert.rejects(work, error => error.code === 'cancelled')
  await assert.rejects(channel.completion, error => error.code === 'cancelled')
  const other = new NativeClient(mock(() => ready).bridge)
  await other.connect()
  other.receive(JSON.stringify(events[0]))
  gate.resolve()
  await tick()
  assert.equal(callbacks, 0)
  assert.equal(host.requests.filter(request => request.command === 'core.channel.ack').length, 0)
  other.dispose()
})

test('wrong-owner/stale events are ignored; replay and excess in-flight messages explicitly reject and cancel', async () => {
  for (const invalid of ['replay', 'extra']) {
    const { client, host, events } = channelHost()
    await client.connect()
    const gate = deferred(), seen = []
    const channel = Channel.bytes(value => { seen.push(value); return gate.promise })
    const work = client.invoke('chunks', { channel }, number)
    await tick()
    client.receive(JSON.stringify({ ...events[0], id: crypto.randomUUID() }))
    client.receive(JSON.stringify({ ...events[0], session: crypto.randomUUID() }))
    assert.equal(seen.length, 1)
    const event = structuredClone(events[0])
    if (invalid === 'extra') event.data.sequence = 2
    client.receive(JSON.stringify(event))
    await assert.rejects(channel.completion, error => error.code === 'invalid_response')
    await assert.rejects(work)
    gate.resolve()
    assert.ok(host.requests.some(request => request.command === 'core.channel.ack' || request.command === 'core.cancel'))
    client.dispose()
  }
})

test('binary AbortSignal and document disposal reject promptly and signal the actual Core primitive', async () => {
  for (const kind of ['abort', 'dispose']) {
    const pending = deferred()
    const host = mock(() => ready, () => pending.promise)
    const client = new NativeClient(host.bridge), controller = new AbortController()
    await client.connect()
    const work = client.invokeBinary('bytes.wait', {}, new Uint8Array(), { signal: controller.signal })
    await tick()
    if (kind === 'abort') controller.abort()
    else client.dispose()
    await assert.rejects(work, error => error.code === 'cancelled')
    assert.equal(host.binaries[0].options.signal.aborted, true)
    pending.resolve(Uint8Array.of(99))
    client.dispose()
  }
})
