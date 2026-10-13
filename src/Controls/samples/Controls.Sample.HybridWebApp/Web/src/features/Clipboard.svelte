<script lang="ts">
  import { onDestroy } from 'svelte'
  import { clipboard } from '@maui/api/essentials/clipboard'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session }: FeatureProps = $props()
  let text = $state('Hello from MAUI — 你好 👋\n"Quotes", backslash \\ and a new line.')
  let mounted = true
  onDestroy(() => { mounted = false })

  function paste(): void {
    void session.run('Paste from native clipboard', async () => {
      const value = await clipboard(session.client).readText()
      if (mounted && !session.disposed && value !== null) text = value
      return { text: value }
    })
  }
</script>

<label for="clipboard-text">Text to copy · maximum 4096 characters</label>
<textarea id="clipboard-text" rows="4" maxlength="4096" bind:value={text}></textarea>
<div class="actions">
  <Action {session} action="copy" title="Copy to native clipboard" primary
    onclick={() => { void session.run('Copy to native clipboard', () => clipboard(session.client).writeText(text)) }} />
  <Action {session} action="paste" title="Paste from native clipboard" onclick={paste} />
</div>
