<script lang="ts">
  import { onDestroy } from 'svelte'
  import * as preferences from '@maui/api/essentials/preferences'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session }: FeatureProps = $props()
  let key = $state('example')
  let value = $state('A setting saved by the playground.')
  let confirmClear = $state(false)
  let mounted = true
  onDestroy(() => { mounted = false })

  async function read(): Promise<unknown> {
    const result = await preferences.get(key, '')
    if (mounted) value = result
    return { key, value: result }
  }
</script>

<label for="preference-key">Actual native preference key</label>
<input id="preference-key" bind:value={key} />
<label for="preference-value">Native string value</label>
<textarea id="preference-value" rows="3" bind:value></textarea>
<p class="input-help">
  Keys and values go unchanged to the default native preference store. Existing app settings
  are intentionally shared with native code. Do not put secrets here.
</p>
<div class="actions">
  <Action {session} action="save" title="Save setting" primary
    onclick={async () => { await preferences.set(key, value); return { key, value } }} />
  <Action {session} action="read" title="Read setting" onclick={read} />
  <Action {session} action="remove" title="Remove setting"
    onclick={async () => { await preferences.remove(key); return { key, value: null } }} />
</div>
<p class="input-help">Clear is destructive: it removes every setting in the default native store,
  including settings created outside this page. It is not limited to the key above.</p>
<label for="preference-confirm-clear">
  <input id="preference-confirm-clear" type="checkbox" bind:checked={confirmClear} />
  I intend to delete all default native preferences
</label>
<Action {session} action="clear" title="Clear entire native preference store" disabled={!confirmClear}
  onclick={async () => { await preferences.clear(); if (mounted) confirmClear = false; return { cleared: 'entire default native store' } }} />
