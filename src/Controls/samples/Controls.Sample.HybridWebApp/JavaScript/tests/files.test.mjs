import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, connect, resetBridge } from '@maui/api/core'
import { pickFile } from '@maui/api/essentials/file-picker'
import { mock } from './bridge.mjs'
afterEach(() => resetBridge())

test('single selection returns direct nullable metadata and rejects obsolete wrapper envelopes', async () => {
  let result = null
  const host = mock(request => request.command === 'core.connect'
    ? { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 } : result)
  configureBridge(host.bridge)
  assert.equal(await pickFile(), null)
  result = { id: 'selected', name: 'real.txt', contentType: 'text/plain' }
  assert.equal((await pickFile()).name, 'real.txt')
  for (const file of [null, result]) {
    result = { file }
    await assert.rejects(() => pickFile(), error => error.code === 'invalid_response')
  }
})

function hostFor(bytes, { partial = 3, seekable = true } = {}) {
  let offset = 0, readers = 0
  const positions = new Map()
  const host = mock(request => {
    switch (request.command) {
      case 'core.connect': return { commands: [], preferenceNamespace: 'test.', maximumTextLength: 4096 }
      case 'filePicker.pick': return { id: 'file', name: 'selected.bin', contentType: 'application/octet-stream' }
      case 'filePicker.pickMultiple': return { files: [{ id: 'file', name: 'selected.bin', contentType: 'application/octet-stream' }] }
      case 'file.openRead': {
        const id = `reader-${++readers}`; positions.set(id, 0)
        return { id, canSeek: seekable, maximumChunkBytes: 65536 }
      }
      case 'file.seek': positions.set(request.args.id, request.args.position); return { accepted: true }
      default: return { accepted: true }
    }
  }, request => {
    assert.equal(request.command, 'file.readBytes')
    {
        offset = positions.get(request.args.id)
        const chunk = bytes.subarray(offset, offset + Math.min(partial, request.args.count))
        offset += chunk.length; positions.set(request.args.id, offset)
        return Uint8Array.of(offset >= bytes.length ? 1 : 0, ...chunk)
      }
  })
  return host
}
test('picker supplies real options, selects before reading, and supports large binary partial reads and explicit resource release', async () => {
  const bytes = Uint8Array.from({ length: 200000 }, (_, index) => index % 256)
  const host = hostFor(bytes, { partial: 65000 })
  configureBridge(host.bridge)
  const client = await connect()
  try {
    const selected = await pickFile({ pickerTitle: 'Chosen from TS', fileTypes: { windows: ['.bin'], android: ['application/octet-stream'] } })
    assert.equal(host.requests.filter(request => request.command.startsWith('file.')).length, 0, 'selection does not open/read contents')
    assert.equal(selected.name, 'selected.bin')
    assert.equal('fullPath' in selected, false)
    const reader = await selected.openRead(), chunks = []
    for (;;) {
      const part = await reader.readBytes(65536)
      chunks.push(...part.bytes)
      if (part.eof) break
    }
    assert.deepEqual(Uint8Array.from(chunks), bytes)
    await reader.seek(199998)
    assert.deepEqual((await reader.readBytes(10)).bytes, bytes.subarray(199998))
    await reader.close()
    await assert.rejects(() => reader.readBytes(), error => error.code === 'invalid_file')
    await selected.dispose()
    await assert.rejects(() => selected.openRead(), error => error.code === 'invalid_file')
    assert.deepEqual(host.requests.find(request => request.command === 'filePicker.pick').args, { pickerTitle: 'Chosen from TS', fileTypes: { windows: ['.bin'], android: ['application/octet-stream'] } })
  } finally { client.dispose() }
})
test('readText incrementally decodes split UTF-8 sequences and binary/nonseekable streams stay genuine resources', async () => {
  const expected = '你好 👋 café'
  configureBridge(hostFor(new TextEncoder().encode(expected), { partial: 1 }).bridge)
  const client = await connect()
  try {
    const file = await pickFile()
    const reader = await file.openRead()
    let actual = ''
    for (;;) {
      const chunk = await reader.readText(1)
      actual += chunk.text
      if (chunk.eof) break
    }
    assert.equal(actual, expected)
    await assert.rejects(() => reader.readBytes(), error => error.code === 'invalid_arguments')
    await reader.close(); await file.dispose()
    await assert.rejects(() => pickFile({ defaultPath: 'private' }), error => error.code === 'unsupported')
  } finally { client.dispose() }
  configureBridge(hostFor(Uint8Array.of(0xff, 0x00), { seekable: false }).bridge)
  const binaryClient = await connect()
  try {
    const file = await pickFile(), reader = await file.openRead()
    await assert.rejects(() => reader.readText(), error => error.code === 'unsupported_encoding')
    await assert.rejects(() => reader.seek(0), error => error.code === 'unsupported')
    const binary = await file.openRead()
    assert.deepEqual((await binary.readBytes()).bytes, Uint8Array.of(0xff, 0x00))
    await reader.close(); await binary.close(); await file.dispose()
  } finally { binaryClient.dispose() }
})
test('concurrent readText, seek and close calls serialize without corrupting incremental UTF-8', async () => {
  const expected = '你好 👋'
  const bytes = new TextEncoder().encode(expected)
  const host = hostFor(bytes, { partial: 1 })
  configureBridge(host.bridge)
  const file = await pickFile(), reader = await file.openRead()
  const chunks = await Promise.all(Array.from(bytes, () => reader.readText(1)))
  assert.equal(chunks.map(chunk => chunk.text).join(''), expected)
  assert.equal(chunks.at(-1).eof, true)
  const first = reader.seek(0), second = reader.readBytes(1), third = reader.close()
  await first
  assert.deepEqual((await second).bytes, bytes.subarray(0, 1))
  await third
  assert.deepEqual(host.requests.slice(-3).map(request => request.command), ['file.seek', 'file.readBytes', 'file.closeReader'])
  await assert.rejects(() => reader.readText(), error => error.code === 'invalid_file')
  await file.dispose()
})
