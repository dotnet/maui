import { createHash } from 'node:crypto'
import { readdir, readFile } from 'node:fs/promises'
import { join, relative } from 'node:path'

export async function files(directory) {
  const entries = await readdir(directory, { withFileTypes: true })
  return (await Promise.all(entries.map(entry => entry.isDirectory() ? files(join(directory, entry.name)) : [join(directory, entry.name)]))).flat().sort()
}

export async function snapshot(root) {
  const inputs = [...await files(join(root, 'src')), ...await files(join(root, 'scripts')),
    ...await files(join(root, 'dist')), ...['package.json', 'package-lock.json', 'tsconfig.json'].map(name => join(root, name))]
    .filter(path => path !== join(root, 'dist', '.build.json')).sort()
  return Object.fromEntries(await Promise.all(inputs.map(async path => [
    relative(root, path).replaceAll('\\', '/'),
    createHash('sha256').update(await readFile(path)).digest('hex'),
  ])))
}
