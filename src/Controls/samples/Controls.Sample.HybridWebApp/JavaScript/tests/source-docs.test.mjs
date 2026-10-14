import { test } from 'node:test'
import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { fileURLToPath } from 'node:url'
import { ApiModel } from '@microsoft/api-extractor-model'

function entry(slug) {
  const model = new ApiModel()
  return model.loadPackage(fileURLToPath(new URL(`../artifacts/api-reference/${slug}/input/api.api.json`, import.meta.url))).members[0]
}
function member(container, name) {
  const found = container.members.find(item => item.displayName === name)
  assert.ok(found, `Missing documented API: ${name}`)
  return found
}

test('emitted declarations document the real root and feature boundaries', () => {
  assert.deepEqual([...new Set(entry('root').members.map(item => item.displayName))].sort(),
    ['Bridge', 'Decoder', 'Json', 'NativeClient', 'NativeError', 'Operation', 'Progress', 'Ready', 'connect', 'disconnect', 'invoke'])
  const app = entry('essentials/app-info')
  assert.deepEqual(app.members.map(item => item.displayName), ['AppInfo', 'getAppInfo'])
  assert.deepEqual(member(app, 'AppInfo').members.map(item => item.displayName), ['appName', 'build', 'version'])
  const device = entry('essentials/device-info')
  assert.deepEqual(device.members.map(item => item.displayName), ['DeviceInfo', 'getDeviceInfo'])
  assert.deepEqual(member(device, 'DeviceInfo').members.map(item => item.displayName), ['model', 'osVersion', 'platform'])
  assert.deepEqual(entry('essentials/clipboard').members.map(item => item.displayName), ['getText', 'hasText', 'onContentChanged', 'setText'])
  assert.deepEqual(entry('essentials/preferences').members.map(item => item.displayName), ['clear', 'contains', 'get', 'PreferenceKind', 'PreferenceOptions', 'PreferenceValue', 'remove', 'set'])
})

test('TSDoc host requirements, session ownership, parameters, results and errors survive declaration emit', () => {
  for (const [slug, name] of [['essentials/app-info', 'getAppInfo'], ['essentials/device-info', 'getDeviceInfo']]) {
    const comment = member(entry(slug), name).tsdocComment
    assert.equal(comment.params.tryGetBlockByName('client'), undefined)
    assert.ok(comment.returnsBlock)
    assert.ok(comment.remarksBlock)
    const source = comment.emitAsTsdoc()
    assert.ok(source.includes(slug.endsWith('app-info') ? 'app.info' : 'device.info'))
    assert.ok(source.includes('@throws'))
    assert.ok(source.includes('native'))
  }
  const core = entry('core')
  for (const name of ['connect', 'disconnect']) {
    assert.ok(member(core, name).tsdocComment.emitAsTsdoc().includes('session'))
  }
  const client = member(core, 'NativeClient')
  const invoke = member(client, 'invoke').tsdocComment
  for (const name of ['command', 'args', 'decode']) assert.ok(invoke.params.tryGetBlockByName(name))
  for (const text of ['100000', '23-second', '@throws', '@returns']) assert.ok(invoke.emitAsTsdoc().includes(text))
  assert.ok(member(entry('essentials/clipboard'), 'setText').tsdocComment.params.tryGetBlockByName('text'))
  assert.ok(member(entry('essentials/clipboard'), 'getText').tsdocComment.returnsBlock)
  for (const name of ['get', 'set', 'remove'])
    assert.ok(member(entry('essentials/preferences'), name).tsdocComment.params.tryGetBlockByName('key'))
})

test('generated reference pages identify exact import paths and contain source-derived API details', async () => {
  const page = await readFile(new URL('../artifacts/api-reference/essentials/app-info/markdown/api.getappinfo.md', import.meta.url), 'utf8')
  assert.ok(page.includes('Import path: `@maui/api/essentials/app-info`'))
  assert.ok(!page.includes('client?:'))
  assert.ok(page.includes('## Exceptions'))
  assert.ok(page.includes('## Remarks'))
  assert.ok(page.includes('getAppInfo()'))
  const index = await readFile(new URL('../artifacts/api-reference/index.md', import.meta.url), 'utf8')
  const toc = await readFile(new URL('../artifacts/api-reference/toc.yml', import.meta.url), 'utf8')
  for (const path of ['@maui/api', '@maui/api/core', '@maui/api/essentials/app-info', '@maui/api/essentials/device-info', '@maui/api/essentials/clipboard', '@maui/api/essentials/preferences']) {
    assert.ok(index.includes(`\`${path}\``))
    assert.ok(toc.includes(JSON.stringify(path)))
  }
})
