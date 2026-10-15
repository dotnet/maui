import { test } from 'node:test'
import assert from 'node:assert/strict'
import { readFile } from 'node:fs/promises'
import { fileURLToPath } from 'node:url'
import { ApiModel } from '@microsoft/api-extractor-model'

test('new Essentials export allowlist and source-derived API models cover every public member', async () => {
  const manifest = JSON.parse(await readFile(new URL('../package.json', import.meta.url), 'utf8'))
  const navigation = await readFile(new URL('../artifacts/api-reference/index.md', import.meta.url), 'utf8')
  for (const [module, exports] of [
    ['connectivity', ['ConnectionProfile', 'ConnectivityInfo', 'NetworkAccess', 'getConnectivity', 'onConnectivityChanged']],
    ['text-to-speech', ['getLocales', 'speak', 'SpeechLocale', 'SpeechOptions']],
    ['launcher', ['openUrl']],
  ]) {
    const path = `essentials/${module}`
    assert.deepEqual(manifest.exports[`./${path}`], { types: `./dist/${path}.d.ts`, import: `./dist/${path}.js` })
    for (const extension of ['js', 'd.ts']) assert.ok(manifest.files.includes(`dist/${path}.${extension}`))
    const entry = new ApiModel().loadPackage(fileURLToPath(new URL(
      `../artifacts/api-reference/${path}/input/api.api.json`, import.meta.url))).members[0]
    assert.deepEqual(entry.members.map(item => item.displayName).sort(), exports.sort())
    for (const item of entry.members) {
      assert.ok(item.tsdocComment?.summarySection.getChildNodes().length)
      for (const member of item.members) assert.ok(member.tsdocComment?.summarySection.getChildNodes().length)
      for (const parameter of item.parameters ?? []) {
        assert.notEqual(parameter.name, 'client')
        assert.ok(item.tsdocComment.params.tryGetBlockByName(parameter.name))
      }
    }
    assert.ok(navigation.includes(`@maui/api/${path}`))
  }
})
