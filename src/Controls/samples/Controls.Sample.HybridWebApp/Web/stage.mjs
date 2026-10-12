import { createHash } from 'node:crypto'
import { readdir, readFile, writeFile } from 'node:fs/promises'
import { dirname, join, relative } from 'node:path'
import { fileURLToPath } from 'node:url'

const root = dirname(fileURLToPath(import.meta.url))
async function files(directory) {
  const entries = await readdir(directory, { withFileTypes: true })
  return (await Promise.all(entries.map(entry => entry.isDirectory() ? files(join(directory, entry.name)) : [join(directory, entry.name)]))).flat().sort()
}
const xml = value => value.replaceAll('&', '&amp;').replaceAll('"', '&quot;').replaceAll('<', '&lt;')
const source = [...await files(join(root, 'src')), ...['index.html', 'package.json', 'package-lock.json', 'tsconfig.json', 'vite.config.ts', 'stage.mjs'].map(name => join(root, name))]
const assets = (await files(join(root, 'dist'))).filter(path => !path.endsWith('playground-assets.props'))
if (!assets.includes(join(root, 'dist', 'index.html'))) throw new Error('Build output is missing index.html. Run npm run stage.')
async function item(path, name, base) {
  const hash = createHash('sha256').update(await readFile(path)).digest('hex').toUpperCase()
  const file = xml(relative(base, path).replaceAll('\\', '/'))
  return `    <${name} Include="$(MSBuildThisFileDirectory)${file}" ExpectedHash="${hash}" />`
}
const manifest = [
  '<Project>', '  <ItemGroup>',
  ...await Promise.all(source.map(path => item(path, '_PlaygroundStageSource', join(root, 'dist')))),
  ...await Promise.all(assets.map(path => item(path, '_PlaygroundStageAsset', join(root, 'dist')))),
  '  </ItemGroup>', '</Project>', '',
].join('\n')
await writeFile(join(root, 'dist', 'playground-assets.props'), manifest)
console.log(`Staged ${assets.length} frontend assets; native builds validate source and output hashes. No npm runs in MSBuild.`)
