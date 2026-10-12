import { rm, writeFile } from 'node:fs/promises'
import { spawnSync } from 'node:child_process'
import { fileURLToPath } from 'node:url'
import { snapshot } from './inputs.mjs'

const root = fileURLToPath(new URL('../', import.meta.url))
await rm(new URL('../dist/', import.meta.url), { recursive: true, force: true })
const result = spawnSync(process.execPath, [fileURLToPath(new URL('../node_modules/typescript/bin/tsc', import.meta.url)), '-p', 'tsconfig.json'], {
  cwd: root,
  stdio: 'inherit',
})
if (result.error) throw result.error
if (result.status !== 0) process.exit(result.status ?? 1)
await writeFile(new URL('../dist/.build.json', import.meta.url), JSON.stringify(await snapshot(root), null, 2) + '\n')
