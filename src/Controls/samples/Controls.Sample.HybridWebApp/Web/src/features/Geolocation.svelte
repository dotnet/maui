<script lang="ts">
  import { onDestroy } from 'svelte'
  import { getLocation } from '@maui/api/essentials/geolocation'
  import type { GeolocationAccuracy, Location } from '@maui/api/essentials/geolocation'
  import type { Operation } from '@maui/api/core'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let accuracy = $state<GeolocationAccuracy>('medium')
  let precise = $state(false)
  let result = $state<Location | null>(null)
  let pending = $state<Operation<Location | null> | null>(null)
  let live = true
  onDestroy(() => {
    live = false
    void pending?.cancel().catch(error => {
      if (!session.disposed) session.writeLog('Location cancellation', 'Error', String(error))
    })
  })
  async function locate() {
    const operation = await getLocation({ desiredAccuracy: accuracy, timeout: '00:00:20', requestFullAccuracy: precise })
    pending = operation
    if (!live) void operation.cancel().catch(error => {
      if (!session.disposed) session.writeLog('Location cancellation', 'Error', String(error))
    })
    else { session.active = operation; session.refresh() }
    try {
      const location = await operation.result
      if (live) result = location
      return { receivedNativeLocation: location !== null, coordinatesShownOnlyInThisPanel: true }
    } finally { if (pending === operation) pending = null }
  }
</script>

<label for="location-accuracy">Actual native GeolocationRequest accuracy</label>
<select id="location-accuracy" bind:value={accuracy}>
  {#each ['default', 'lowest', 'low', 'medium', 'high', 'best'] as value}
    <option value={value}>{value}</option>
  {/each}
</select>
<label><input type="checkbox" bind:checked={precise} /> Explicitly request temporary full accuracy on supported Apple platforms</label>
<div class="actions">
  <Action {session} action="geolocation-read" title="Request actual native location" primary disabled={pending !== null} onclick={locate} />
  <Action {session} action="cancel" title="Cancel this location request" disabled={!pending} onclick={() => pending?.cancel()} />
</div>
{#if result}
  <p data-result="native-location">Latitude {result.latitude} · Longitude {result.longitude} · Accuracy {result.accuracy ?? 'unavailable'} m</p>
{/if}
<p class="input-help">Coordinates are shown here only after your click; they are not included in the operation
  log. OS permission denial, disabled location settings and unsupported native APIs are explicit errors.
  No browser geolocation, fake coordinates or location fallback is used. The request has a native 20-second timeout.</p>
