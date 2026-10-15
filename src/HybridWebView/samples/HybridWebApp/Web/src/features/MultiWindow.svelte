<script lang="ts">
  import { onDestroy } from 'svelte'
  import * as app from '@maui/api/application'
  import { currentWindow } from '@maui/api/ui/windows'
  import { navigationPages } from '../catalog.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let own: ReturnType<typeof currentWindow>
  let developerId = $state('')
  let title = $state('New playground window')
  let url = $state('index.html#/showcase')
  let route = $state('showcase')
  let customSize = $state(false)
  let width = $state(1000), height = $state(760)
  let listed = $state<{ id: string; title: string; url: string; current: boolean; canClose: boolean }[]>([])
  let inspected = $state(false)
  let live = true, version = 0
  onDestroy(() => { live = false })

  async function refresh(): Promise<unknown> {
    own ??= currentWindow()
    const request = ++version
    const capabilities = await own.capabilities()
    if (!live || session.disposed) return
    session.windowCapabilities = capabilities; session.refresh()
    const [windows, caller] = await Promise.all([app.windows(), own.id()])
    if (!live || session.disposed) return
    const values = await Promise.all(windows.map(async window => {
      const [id, title, url] = await Promise.all([window.id(), window.title(), window.url()])
      return { id, title, url, current: id === caller, canClose: id !== caller && session.childWindows.has(id) }
    }))
    if (!live || session.disposed || request !== version) return
    listed = values; inspected = true
    return { capabilities, windows: values }
  }
  async function open(): Promise<unknown> {
    const options = { url, title, ...(developerId.trim() ? { id: developerId.trim() } : {}),
      ...(customSize && session.windowCapabilities?.resize ? { width, height } : {}) }
    const child = await app.openWindow(options)
    const id = await child.id()
    if (!session.disposed) session.childWindows.set(id, child)
    if (live && !session.disposed) await refresh()
    return { id, url: options.url }
  }
  async function close(id: string): Promise<unknown> {
    const child = session.childWindows.get(id)
    if (!child) throw new Error('Only a child opened by this document can be closed here.')
    const accepted = await child.close()
    if (accepted) {
      session.childWindows.delete(id)
      if (live) listed = listed.filter(window => window.id !== id)
    }
    return { accepted, id }
  }
</script>

<div class="actions">
  <Action {session} action="window-list" title="Read support and list windows" primary onclick={refresh} />
</div>
<p class="input-help">Read first to check separate-window support. Windows, Mac Catalyst, Android
  and iPad may support it; iPhone does not. Native availability can still reject a request.</p>
<label for="child-route">Bundled page</label>
<select id="child-route" bind:value={route} onchange={() => { url = `index.html#/${route}` }}>
  {#each navigationPages as page}<option value={page.id}>{page.title}</option>{/each}
</select>
<label for="child-url">Trusted bundled URL</label>
<input id="child-url" maxlength="4096" bind:value={url} />
<p class="input-help">Bundle-relative URL, query and internal route only. A remote URL cannot receive the native bridge.</p>
<label for="child-id">Developer ID · optional</label>
<input id="child-id" maxlength="160" placeholder="Let native assign an ID" bind:value={developerId} />
<label for="child-title">New window title</label>
<input id="child-title" maxlength="160" bind:value={title} />
<label class="checkbox-label"><input id="child-custom-size" type="checkbox" bind:checked={customSize}
  disabled={!$session.windowCapabilities?.resize} />Request desktop size</label>
{#if customSize && $session.windowCapabilities?.resize}
  <div class="work-inputs">
    <div><label for="child-width">Width</label><input id="child-width" type="number" min="320" max="4096" bind:value={width} /></div>
    <div><label for="child-height">Height</label><input id="child-height" type="number" min="240" max="4096" bind:value={height} /></div>
  </div>
{/if}
<div class="actions">
  <Action {session} action="open-window" title="Open a new playground window" onclick={open} />
</div>
<p class="input-help">Four app-owned playground windows maximum. Each has its own native providers,
  document and subscriptions; process startup identity is shared. Internal routes are not OS deep links.
  This page closes only children opened by this document, using their original resource handles.</p>
{#if inspected}
  <section class="window-list" aria-labelledby="owned-windows-title">
    <h3 id="owned-windows-title">Caller-authorized windows</h3>
    <ul id="owned-windows">
      {#each listed as window (window.id)}
        <li>
          <strong>{window.title}</strong><span>{window.url}</span><code>{window.id}</code>
          {#if window.canClose}
            <Action {session} action={`close-${window.id}`} title="Close child" onclick={() => close(window.id)} />
          {:else}<span class="secondary">{window.current ? 'This window' : 'Not opened by this document'}</span>{/if}
        </li>
      {:else}<li>No caller-authorized windows were reported. Read again to refresh the native list.</li>{/each}
    </ul>
  </section>
{/if}
