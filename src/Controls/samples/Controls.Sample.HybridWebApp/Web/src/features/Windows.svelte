<script lang="ts">
  import { onDestroy } from 'svelte'
  import * as app from '@maui/api/application'
  import { currentWindow } from '@maui/api/ui/windows'
  import { navigationPages } from '../catalog.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let title = $state('My MAUI window — 你好 👋')
  let x = $state(80), y = $state(80), width = $state(1000), height = $state(760)
  let route = $state('showcase')
  let owned = $state<{ id: string; title: string; url: string; canClose: boolean }[]>([])
  let mounted = true
  onDestroy(() => { mounted = false })
  async function refresh(): Promise<unknown> {
      const own = currentWindow()
      const capabilities = await own.capabilities()
      session.windowCapabilities = capabilities
      session.refresh()
      const children = await app.windows()
      const id = await own.id()
      const listed = await Promise.all(children.map(async window => ({
        id: await window.id(), title: await window.title(), url: await window.url(), canClose: await window.id() !== id,
      })))
      if (mounted) owned = listed
      return { id: await own.id(), title: await own.title(), size: await own.size(),
        position: capabilities.position ? await own.position() : 'unsupported on mobile', capabilities, windows: listed }
  }
  async function close(id: string): Promise<unknown> {
      const child = await app.window(id)
      if (!child) throw new Error('The app-owned child window has closed.')
      const accepted = await child.close()
      if (mounted) owned = owned.filter(window => window.id !== id)
      return { accepted, id }
  }
</script>

<div class="actions">
  <Action {session} action="window-info" title="Read window and capabilities" primary onclick={refresh} />
</div>
<p class="input-help">Read first to check support. Mobile rejects desktop placement. Separate windows
  are available on Windows, Mac Catalyst, Android and iPad, not iPhone. Native OS availability
  can still reject an operation; no desktop result is emulated.</p>
<label for="window-title">Native window title</label>
<input id="window-title" maxlength="160" bind:value={title} />
<div class="actions">
  <Action {session} action="title" title="Set window title"
    onclick={() => currentWindow().setTitle(title)} />
</div>
<div class="work-inputs">
  <div><label for="window-x">X</label><input id="window-x" type="number" min="-10000" max="10000" bind:value={x} /></div>
  <div><label for="window-y">Y</label><input id="window-y" type="number" min="-10000" max="10000" bind:value={y} /></div>
  <div><label for="window-width">Width</label><input id="window-width" type="number" min="320" max="4096" bind:value={width} /></div>
  <div><label for="window-height">Height</label><input id="window-height" type="number" min="240" max="4096" bind:value={height} /></div>
</div>
<div class="actions">
  <Action {session} action="bounds" title="Request desktop placement"
    onclick={async () => {
      const bounds = { x, y, width, height }
      const own = currentWindow()
      await own.setSize({ width: bounds.width, height: bounds.height })
      await own.setPosition({ x: bounds.x, y: bounds.y })
    }} />
  <Action {session} action="center" title="Center on current display work area"
    onclick={() => currentWindow().center()} />
</div>
<p class="input-help">Center uses the actual owning display work area and current native size.
  The Windows adapter is implemented; Mac Catalyst/mobile reject rather than guess a primary
  monitor or fake a usable work area. Native position-change observation is likewise Windows-only
  until the Mac scene move callback is verified.</p>
<label for="window-route">Bundled page for a new window</label>
<select id="window-route" bind:value={route}>
  {#each navigationPages as page}<option value={page.id}>{page.title}</option>{/each}
</select>
<div class="actions">
  <Action {session} action="open-window" title="Open a new playground window"
    onclick={async () => {
      const child = await app.openWindow({ url: `index.html#/${route}`, title })
      return { id: await child.id() }
    }} />
</div>
<p class="input-help">Four app-owned playground windows maximum. Each playground
  window has its own hub, providers, counter and document. Process startup identity is shared.
  This is internal routing, not an OS deep link.</p>
{#if owned.length}
  <ul id="owned-windows">
    {#each owned as window (window.id)}
      <li>
        <span>{window.title} · {window.url} · <code>{window.id}</code></span>
        {#if window.canClose}
          <Action {session} action={`close-${window.id}`} title="Close child" onclick={() => close(window.id)} />
        {:else}<span class="secondary">This window</span>{/if}
      </li>
    {/each}
  </ul>
{/if}
