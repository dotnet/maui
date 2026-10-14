import assert from 'node:assert/strict'
import { spawnSync } from 'node:child_process'
import { copyFile, lstat, mkdir, readFile, rm, writeFile } from 'node:fs/promises'
import { join } from 'node:path'
import { fileURLToPath } from 'node:url'
import { ApiModel } from '@microsoft/api-extractor-model'

const root = fileURLToPath(new URL('../', import.meta.url))
const manifest = JSON.parse(await readFile(join(root, 'package.json'), 'utf8'))
const artifacts = join(root, 'artifacts')
const consumer = join(artifacts, 'packed-consumer')
const archive = join(artifacts, `${manifest.name.replace('@', '').replace('/', '-')}-${manifest.version}.tgz`)
function run(command, args, cwd = root, capture = false) {
  const result = spawnSync(command, args, { cwd, encoding: 'utf8', stdio: capture ? 'pipe' : 'inherit' })
  if (result.error) throw result.error
  if (result.status !== 0) throw new Error(`${command} ${args.join(' ')} failed (${result.status}).\n${result.stderr ?? ''}`)
  return result.stdout
}
function npm(args, cwd = root) {
  assert.ok(process.env.npm_execpath, 'Run this check with npm run pack-check.')
  run(process.execPath, [process.env.npm_execpath, ...args, '--no-audit', '--no-fund'], cwd)
}

await mkdir(artifacts, { recursive: true })
npm(['pack', '--pack-destination', artifacts])
const entries = run('tar', ['-tf', archive], root, true).trim().split('\n').sort()
assert.deepEqual(entries, [...manifest.files, 'package.json'].map(path => `package/${path}`).sort(), 'The tarball must contain exactly the public allowlist.')
for (const path of entries.filter(path => path.endsWith('.js') || path.endsWith('.d.ts') || path.endsWith('package.json'))) {
  const content = run('tar', ['-xOf', archive, path], root, true)
  assert.doesNotMatch(content, /\/Users\/|\/home\/|[A-Z]:\\Users\\|sourceMappingURL|_authToken|["'`]file:|\.\/src\//, `Unexpected private/source metadata in ${path}`)
}
await rm(consumer, { recursive: true, force: true })
await mkdir(consumer, { recursive: true })
await writeFile(join(consumer, 'package.json'), JSON.stringify({
  name: 'maui-api-packed-consumer', private: true, type: 'module', dependencies: { [manifest.name]: `file:../${archive.split(/[\\/]/).at(-1)}` },
}, null, 2) + '\n')
npm(['install', '--package-lock-only', '--offline'], consumer)
npm(['ci', '--offline'], consumer)
const installed = join(consumer, 'node_modules', '@maui', 'api')
assert.equal((await lstat(installed)).isSymbolicLink(), false, 'The consumer must use the tarball, not a source symlink.')
await assert.rejects(lstat(join(installed, 'src')), { code: 'ENOENT' })
run(process.execPath, [join(root, 'scripts', 'docs.mjs'), installed])
function publicSurface(item) {
  return {
    kind: item.kind,
    name: item.displayName,
    signature: item.excerpt?.text,
    comment: item.tsdocComment?.emitAsTsdoc(),
    members: item.members.map(publicSurface),
  }
}
for (const subpath of Object.keys(manifest.exports)) {
  const slug = subpath === '.' ? 'root' : subpath.slice(2)
  const local = new ApiModel().loadPackage(join(artifacts, 'api-reference', slug, 'input', 'api.api.json'))
  const packed = new ApiModel().loadPackage(join(artifacts, 'api-reference-packed', slug, 'input', 'api.api.json'))
  assert.deepEqual(publicSurface(packed), publicSurface(local), `Packed API signatures, exports, members and TSDoc differ for ${subpath}.`)
}
for (const file of ['consumer.ts', 'tsconfig.json']) {
  await copyFile(join(root, 'tests', 'fixtures', file), join(consumer, file))
}
run(process.execPath, [join(root, 'node_modules', 'typescript', 'bin', 'tsc'), '-p', join(consumer, 'tsconfig.json')])
run(process.execPath, [join(consumer, 'output', 'consumer.js')], consumer)
console.log(`Verified ${entries.length} tarball files, clean offline install, public ESM exports, and identical declaration API/TSDoc models for ${Object.keys(manifest.exports).length} entrypoints.`)
