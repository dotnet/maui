import assert from 'node:assert/strict'
import { spawnSync } from 'node:child_process'
import { mkdir, readFile, readdir, rm, writeFile } from 'node:fs/promises'
import { join, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { Extractor, ExtractorConfig } from '@microsoft/api-extractor'
import { ApiDocumentedItem, ApiModel } from '@microsoft/api-extractor-model'

const root = fileURLToPath(new URL('../', import.meta.url))
const packageRoot = process.argv[2] ? resolve(process.argv[2]) : root
const manifest = JSON.parse(await readFile(join(packageRoot, 'package.json'), 'utf8'))
const destination = join(root, 'artifacts', packageRoot === root ? 'api-reference' : 'api-reference-packed')
await rm(destination, { recursive: true, force: true })
await mkdir(destination, { recursive: true })
const navigation = ['# @maui/api reference', '', 'Generated from shipped declarations and their TSDoc comments.', '',
  'Each folder documents one actual package export; features are not re-exported from the root.', '']
const toc = []
let documented = 0

function validateComments(item) {
  if (item instanceof ApiDocumentedItem) {
    const label = item.getScopedNameWithinPackage()
    const comment = item.tsdocComment
    assert.ok(comment?.summarySection.getChildNodes().length, `Missing declaration documentation: ${label}`)
    for (const parameter of item.parameters ?? []) {
      assert.ok(comment.params.tryGetBlockByName(parameter.name), `Missing @param ${parameter.name}: ${label}`)
    }
    documented++
  }
  for (const member of item.members) validateComments(member)
}

for (const [subpath, exported] of Object.entries(manifest.exports)) {
  const name = subpath === '.' ? manifest.name : `${manifest.name}${subpath.slice(1)}`
  const slug = subpath === '.' ? 'root' : subpath.slice(2)
  const input = join(destination, slug, 'input')
  const output = join(destination, slug, 'markdown')
  const modelPath = join(input, 'api.api.json')
  await mkdir(input, { recursive: true })
  const declaration = join(packageRoot, exported.types)
  const config = ExtractorConfig.prepare({
    configObjectFullPath: undefined,
    packageJsonFullPath: join(packageRoot, 'package.json'),
    configObject: {
      projectFolder: packageRoot,
      mainEntryPointFilePath: declaration,
      // Installed relative re-exports are classified as external by TypeScript; they still belong to this package.
      bundledPackages: [manifest.name],
      compiler: { overrideTsconfig: {
        compilerOptions: { target: 'ES2022', module: 'NodeNext', moduleResolution: 'NodeNext', lib: ['ES2022', 'DOM'], types: [], strict: true, skipLibCheck: false },
        files: [declaration],
      } },
      apiReport: { enabled: false },
      dtsRollup: { enabled: false },
      docModel: { enabled: true, apiJsonFilePath: modelPath },
      tsdocMetadata: { enabled: false },
      messages: {
        compilerMessageReporting: { default: { logLevel: 'error' } },
        extractorMessageReporting: {
          default: { logLevel: 'warning' },
          'ae-missing-release-tag': { logLevel: 'none' },
          // References to core types in feature signatures belong to another real public subpath.
          'ae-forgotten-export': { logLevel: 'none' },
        },
        tsdocMessageReporting: { default: { logLevel: 'warning' } },
      },
    },
  })
  const result = Extractor.invoke(config, { localBuild: true, showDiagnostics: false, showVerboseMessages: false })
  assert.ok(result.succeeded && result.errorCount === 0 && result.warningCount === 0, `API extraction failed for ${name}`)
  const model = new ApiModel()
  const pkg = model.loadPackage(modelPath)
  assert.ok(pkg.members.some(entry => entry.members.length > 0), `No exported declarations documented for ${name}`)
  for (const entry of pkg.members) for (const item of entry.members) validateComments(item)
  const documenter = spawnSync(process.execPath, [
    join(root, 'node_modules', '@microsoft', 'api-documenter', 'bin', 'api-documenter'),
    'markdown', '--input-folder', input, '--output-folder', output,
  ], { cwd: root, encoding: 'utf8' })
  if (documenter.error) throw documenter.error
  assert.equal(documenter.status, 0, documenter.stderr || documenter.stdout)
  for (const file of await readdir(output)) {
    if (!file.endsWith('.md')) continue
    const generated = await readFile(join(output, file), 'utf8')
    await writeFile(join(output, file), `Import path: \`${name}\`\n\n${generated}`)
  }
  navigation.push(`- [\`${name}\`](${slug}/markdown/index.md)`)
  toc.push(`- name: ${JSON.stringify(name)}\n  href: ${slug}/markdown/index.md`)
}

await writeFile(join(destination, 'index.md'), navigation.join('\n') + '\n')
await writeFile(join(destination, 'toc.yml'), toc.join('\n') + '\n')
console.log(`Generated ${Object.keys(manifest.exports).length} exact import-path references; validated TSDoc for ${documented} declarations and public members.`)
