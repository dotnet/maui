import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, resetBridge } from '@maui/api/core'
import * as preferences from '@maui/api/essentials/preferences'
import { mock } from './bridge.mjs'

afterEach(() => resetBridge())
const ready = { commands: [], maximumTextLength: 1 }

test('preferences pass native keys long strings and shared containers without namespaces or quotas', async () => {
  const value = '語'.repeat(8192)
  const host = mock(request => request.command === 'core.connect' ? ready :
    request.command === 'preferences.get' ? { kind: 'string', text: value } : { accepted: true })
  configureBridge(host.bridge)
  for (const key of ['', 'key with spaces', 'native.key', '你好 👋', 'a'.repeat(1024)]) {
    for (const sharedName of [undefined, '', 'native shared.語\n']) {
      const options = { sharedName }
      assert.equal(await preferences.get(key, '', options), value)
      assert.deepEqual(host.requests.at(-1).args, { key, defaultValue: { kind: 'string', text: '' }, sharedName: sharedName ?? null })
      await preferences.set(key, value, options)
      assert.deepEqual(host.requests.at(-1).args, { key, value: { kind: 'string', text: value }, sharedName: sharedName ?? null })
      assert.equal(await preferences.contains(key, options), true)
      assert.deepEqual(host.requests.at(-1).args, { key, sharedName: sharedName ?? null })
      await preferences.remove(key, options)
      assert.deepEqual(host.requests.at(-1).args, { key, sharedName: sharedName ?? null })
      await preferences.clear(options)
      assert.equal(host.requests.at(-1).command, 'preferences.clear')
      assert.deepEqual(host.requests.at(-1).args, { sharedName: sharedName ?? null })
    }
  }
})

test('preferences retain scalar representation safeguards without storage policy', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : { accepted: true })
  configureBridge(host.bridge)
  for (const [value, options] of [[NaN, {}], [Infinity, {}], [2 ** 53, { numberType: 'long' }],
    [2147483648, { numberType: 'int' }], [1e300, { numberType: 'float' }], [new Date(NaN), {}], [{}, {}]])
    await assert.rejects(() => preferences.set('native.key', value, options), error => error.code === 'invalid_arguments')
  assert.equal(host.requests.length, 0)
})

test('native preference failures are surfaced for each adapter operation', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : null)
  const invoke = host.bridge.InvokeDotNet
  host.bridge.InvokeDotNet = async (...args) => {
    const response = JSON.parse(await invoke(...args))
    if (host.requests.at(-1).command !== 'core.connect') {
      response.status = 'error'
      response.error = { code: 'native_error', message: 'Native preference store failed.' }
    }
    return JSON.stringify(response)
  }
  configureBridge(host.bridge)
  for (const call of [() => preferences.get('key', ''), () => preferences.set('key', 'value'),
    () => preferences.contains('key'), () => preferences.remove('key'), () => preferences.clear()])
    await assert.rejects(call, error => error.code === 'native_error')
})
