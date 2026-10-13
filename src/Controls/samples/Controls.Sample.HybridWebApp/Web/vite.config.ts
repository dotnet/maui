import { defineConfig } from 'vitest/config'
import { svelte } from '@sveltejs/vite-plugin-svelte'

export default defineConfig(({ mode }) => ({
  plugins: [svelte({ configFile: false })],
  resolve: mode === 'test' ? { conditions: ['browser'] } : undefined,
  test: {
    include: ['tests/playground.test.mjs'],
    environment: 'jsdom',
  },
  server: {
    host: '127.0.0.1',
    port: 5173,
    strictPort: true,
    hmr: {
      protocol: 'ws',
      host: '127.0.0.1',
      clientPort: 5173,
    },
  },
}))
