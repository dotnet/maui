<script lang="ts">
  import { onDestroy } from 'svelte'
  import * as preferences from '@maui/api/essentials/preferences'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session }: FeatureProps = $props()
  let key = $state('example')
  let value = $state('A setting saved by the playground.')
  let mounted = true
  onDestroy(() => { mounted = false })

  async function read(): Promise<unknown> {
    const result = await preferences.get(key, '')
    if (mounted) value = result
    return { key, value: result }
  }
</script>

<label for="preference-key">Key · 1–48 letters, digits, underscores or hyphens</label>
<input id="preference-key" maxlength="48" pattern={'[a-zA-Z0-9_-]{1,48}'} bind:value={key} />
<label for="preference-value">Value · maximum 4096 characters</label>
<textarea id="preference-value" rows="3" maxlength="4096" bind:value></textarea>
<p class="input-help">
  Native keys use <code>maui.playground.v1.</code>. No other app settings are accessible.
</p>
<div class="actions">
  <Action {session} action="save" title="Save setting" primary
    onclick={async () => { await preferences.set(key, value); return { key, value } }} />
  <Action {session} action="read" title="Read setting" onclick={read} />
  <Action {session} action="remove" title="Remove setting"
    onclick={async () => { await preferences.remove(key); return { key, value: null } }} />
</div>
