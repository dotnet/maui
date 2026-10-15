import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, resetBridge } from '@maui/api/core'
import { getText, hasText, setText } from '@maui/api/essentials/clipboard'
import { alert, confirm, actionSheet } from '@maui/api/ui/dialogs'
import { getLocation } from '@maui/api/essentials/geolocation'
import { share, stageFile, releaseFile } from '@maui/api/essentials/share'
import { getCurrentFlowDirection } from '@maui/api/ui/windows'
import { mock } from './bridge.mjs'
afterEach(() => resetBridge())
const ready = { commands: ['dialogs.alert', 'dialogs.confirm', 'dialogs.actionSheet', 'share.request', 'geolocation.location'],
  maximumTextLength: 4096,
  interactiveCommands: ['dialogs.alert', 'dialogs.confirm', 'dialogs.actionSheet', 'share.request', 'geolocation.location'],
  interactiveTimeoutMilliseconds: 300000 }

test('curated clipboard/getter/Core/dialog calls decode actual native scalar and void results', async () => {
  const host = mock(request => {
    switch (request.command) {
      case 'core.connect': return ready
      case 'clipboard.read': return 'Native scalar text'
      case 'clipboard.hasText': return true
      case 'clipboard.write':
      case 'dialogs.alert': return null
      case 'dialogs.confirm': return false
      case 'dialogs.actionSheet': return 'Choice'
      case 'window.flowDirection': return 'rightToLeft'
    }
  })
  configureBridge(host.bridge)
  assert.equal(await getText(), 'Native scalar text')
  assert.equal(await hasText(), true)
  assert.equal(await setText('Native'), undefined)
  assert.equal(await alert('Message'), undefined)
  assert.equal(await confirm('Message'), false)
  assert.equal(await actionSheet({ buttons: ['Choice'] }), 'Choice')
  assert.equal(await getCurrentFlowDirection(), 'rightToLeft')
  assert.deepEqual(host.requests.find(request => request.command === 'dialogs.alert').args,
    { title: '', message: 'Message', cancel: 'OK', flowDirection: 'matchParent' })
  assert.deepEqual(host.requests.find(request => request.command === 'dialogs.actionSheet').args,
    { title: '', cancel: null, destruction: null, buttons: ['Choice'], flowDirection: 'matchParent' })
})

test('browser bytes are staged by raw binary before separate interactive native union sharing', async () => {
  const file = { id: '00000000-0000-0000-0000-000000000001', fileName: 'browser.pdf', contentType: 'application/pdf', size: 4 }
  const host = mock(request => request.command === 'core.connect' ? ready : { accepted: true },
    (request, bytes) => {
      assert.equal(request.command, 'share.stage')
      assert.deepEqual([...new Uint8Array(bytes)], [37, 80, 68, 70])
      return new TextEncoder().encode(JSON.stringify(file))
    })
  configureBridge(host.bridge)
  const staged = await stageFile('browser.pdf', 'application/pdf', Uint8Array.of(37, 80, 68, 70))
  assert.deepEqual(staged, file)
  await share({ file: staged, title: 'Native PDF' })
  await share({ text: 'Native text', title: 'Native text' })
  assert.deepEqual(host.requests.find(request => request.command === 'share.request').args,
    { request: { file: { id: file.id }, title: 'Native PDF' } })
  assert.equal(host.binaries.length, 1)
  await assert.rejects(() => share({ file: { ...staged } }), error => error.code === 'invalid_resource')
  await assert.rejects(() => share({ text: 'Ambiguous', file: staged }), error => error.code === 'invalid_arguments')
  await assert.rejects(() => share({ files: [staged, staged] }), error => error.code === 'invalid_arguments')
  await assert.rejects(() => share({}), error => error.code === 'invalid_arguments')
  await assert.rejects(() => share({ text: 'known', unknown: 1 }), error => error.code === 'invalid_arguments')
  await releaseFile(staged)
  await assert.rejects(() => share({ file: staged }), error => error.code === 'invalid_resource')
  await assert.rejects(() => stageFile('../escape.pdf', 'application/pdf', new Uint8Array()), error => error.code === 'invalid_arguments')
  resetBridge()
  await assert.rejects(() => share({ file: staged }), error => error.code === 'invalid_resource')
})

test('staging rejects malformed metadata and size mismatches rather than producing usable authority', async () => {
  let bytes = Uint8Array.of(255)
  const host = mock(() => ready, () => bytes)
  configureBridge(host.bridge)
  await assert.rejects(() => stageFile('file.pdf', 'application/pdf', Uint8Array.of(1)), error => error.code === 'invalid_response')
  bytes = new TextEncoder().encode(JSON.stringify({ id: '00000000-0000-0000-0000-000000000001',
    fileName: 'file.pdf', contentType: 'application/pdf', size: 2 }))
  await assert.rejects(() => stageFile('file.pdf', 'application/pdf', Uint8Array.of(1)), error => error.code === 'invalid_response')
})

test('location uses real request fields, strict native result decoding, explicit null and bounded timeout', async () => {
  let result = { latitude: 51.5, longitude: -0.12, timestamp: '2026-10-14T00:00:00Z',
    altitude: null, accuracy: 8, verticalAccuracy: null, reducedAccuracy: false,
    speed: null, course: null, isFromMockProvider: false, altitudeReferenceSystem: 'unspecified' }
  const host = mock(request => request.command === 'core.connect' ? ready : result)
  configureBridge(host.bridge)
  const pending = await getLocation({ timeout: '00:00:20', desiredAccuracy: 'high' })
  assert.deepEqual(await pending.result, result)
  assert.deepEqual(host.requests.find(request => request.command === 'geolocation.location').args,
    { request: { timeout: '00:00:20', desiredAccuracy: 'high', requestFullAccuracy: false } })
  const nativeLocation = result
  for (const altitudeReferenceSystem of ['unspecified', 'terrain', 'ellipsoid', 'geoid', 'surface']) {
    result = { ...nativeLocation, altitudeReferenceSystem }
    assert.equal((await (await getLocation({ timeout: '00:00:20' })).result).altitudeReferenceSystem, altitudeReferenceSystem)
  }
  for (const altitudeReferenceSystem of [0, 1, 2, 3, 4, '0', 'Unspecified', 'Terrain', 'unknown', null]) {
    result = { ...nativeLocation, altitudeReferenceSystem }
    await assert.rejects((await getLocation({ timeout: '00:00:20' })).result, error => error.code === 'invalid_response')
  }
  result = null
  assert.equal(await (await getLocation({ timeout: '00:00:20' })).result, null)
  result = { latitude: 400 }
  await assert.rejects((await getLocation({ timeout: '00:00:20' })).result, error => error.code === 'invalid_response')
  for (const timeout of ['00:00:00', '00:02:01', 'not-time', '-00:00:20'])
    await assert.rejects(() => getLocation({ timeout }), error => error.code === 'invalid_arguments')
})
