<script lang="ts">
  import { onMount, onDestroy } from 'svelte'
  import { createBrowserView, supportsBrowserView } from '@maui/api/ui/remote-webview'
  import type { BrowserView, BrowserNavigation } from '@maui/api/ui/remote-webview'
  import { measureBrowserSlot } from '../browser-geometry.ts'
  import Action from './Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let url = $state('https://dot.net/maui')
  let supported = $state<boolean>()
  let browser = $state<BrowserView>()
  let navigation = $state<BrowserNavigation>()
  let slot: HTMLElement
  let live = true, sending = false, lastSent = 0
  let release: (() => Promise<void>) | undefined
  let cleanup: (() => void) | undefined
  async function place(): Promise<void> {
    if (!live || sending || !browser || !slot || performance.now() - lastSent < 100) return
    sending = true; lastSent = performance.now()
    try { await browser.setBounds(measureBrowserSlot(slot)) }
    catch (error) { if (live && !session.disposed) session.showResult('Error', error instanceof Error ? error.message : 'Native geometry failed.') }
    finally { sending = false }
  }
  async function read(): Promise<unknown> {
      const result = await supportsBrowserView()
      if (live) supported = result
      return { supported: result }
  }
  async function load(): Promise<unknown> {
    const address = url
      if (browser) {
        await browser.navigate(address)
        return { id: browser.id, url: await browser.url() }
      }
      const created = await createBrowserView(address)
      if (!live) { await created.close(); return { closed: true } }
      browser = created; session.browser = created
      navigation = await created.navigation()
      release = await created.onNavigation(value => { if (live) navigation = value })
      await place()
      return { id: created.id, url: await created.url() }
  }
  async function run(action: (view: BrowserView) => Promise<void>): Promise<unknown> {
    const own = browser
    if (!own) return
    await action(own)
    return { accepted: true }
  }
  async function close(): Promise<unknown> {
    const own = browser
    if (!own) return
      await own.close(); await release?.()
      if (live) { browser = undefined; navigation = undefined }
      if (session.browser === own) session.browser = undefined
      return { closed: true }
  }
  onMount(() => {
    supported = session.browserSupported
    const schedule = () => { void place() }
    const resize = typeof ResizeObserver === 'undefined' ? undefined : new ResizeObserver(schedule)
    resize?.observe(slot)
    const overlays = new MutationObserver(schedule)
    overlays.observe(document.body, { subtree: true, attributes: true, attributeFilter: ['open', 'aria-modal', 'data-browser-obscured'] })
    window.addEventListener('scroll', schedule, true); window.addEventListener('resize', schedule)
    document.addEventListener('visibilitychange', schedule)
    const pulse = setInterval(schedule, 250)
    cleanup = () => {
      resize?.disconnect(); overlays.disconnect(); clearInterval(pulse)
      window.removeEventListener('scroll', schedule, true); window.removeEventListener('resize', schedule)
      document.removeEventListener('visibilitychange', schedule)
    }
  })
  onDestroy(() => {
    live = false; cleanup?.()
    const failed = (error: unknown) => { if (!session.disposed) session.writeLog('Browser cleanup', 'Error', error instanceof Error ? error.message : 'Cleanup failed.') }
    if (browser && session.client.available) void browser.close().catch(failed)
    if (release) void release().catch(failed)
    if (session.browser === browser) session.browser = undefined
  })
</script>

<div class="browser-panel">
  <label for="remote-url">Public HTTPS website</label>
  <input id="remote-url" type="url" maxlength="2048" bind:value={url} />
  <div class="actions browser-toolbar">
    <Action {session} action="browser-info" title="Check native support" onclick={read} />
    <Action {session} action="remote" title="Load website" primary disabled={!supported} onclick={load} />
    <Action {session} action="browser-back" title="Back" disabled={!navigation?.canGoBack} onclick={() => run(view => view.goBack())} />
    <Action {session} action="browser-forward" title="Forward" disabled={!navigation?.canGoForward} onclick={() => run(view => view.goForward())} />
    <Action {session} action="browser-reload" title="Reload" disabled={!browser} onclick={() => run(view => view.reload())} />
    <Action {session} action="browser-focus" title="Focus website" disabled={!browser} onclick={() => run(view => view.focus())} />
    <Action {session} action="browser-close" title="Close browser" disabled={!browser} onclick={close} />
  </div>
  <p id="browser-status" class="input-help" aria-live="polite">
    {supported === false ? 'Native guests are unsupported on this mobile host; no iframe fallback.' : navigation?.status ?? 'Check native support before loading a website.'}
  </p>
  <div id="browser-slot" class="browser-slot" bind:this={slot} aria-label="Native browser surface position">
    <p>The ordinary native browser surface appears here on supported desktop hosts.</p>
  </div>
</div>
