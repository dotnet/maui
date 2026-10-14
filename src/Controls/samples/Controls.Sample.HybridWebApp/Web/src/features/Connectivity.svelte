<script lang="ts">
  import { onDestroy } from 'svelte'
  import { getConnectivity, onConnectivityChanged } from '@maui/api/essentials/connectivity'
  import type { ConnectivityInfo } from '@maui/api/essentials/connectivity'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let snapshot = $state<ConnectivityInfo>()
  let listening = $state(false)
  let changes = $state(0)
  let live = true
  let release: (() => Promise<void>) | undefined
  onDestroy(() => {
    live = false
    void release?.().catch(error => {
      if (!session.disposed) session.writeLog('Network listener cleanup', 'Error', String(error))
    })
  })
  async function read() {
    const value = await getConnectivity()
    if (live) snapshot = value
    return value
  }
  async function subscribe() {
    const unsubscribe = await onConnectivityChanged(value => {
      if (!live) return
      snapshot = value; changes++
      session.writeLog('Native network change', 'Event', value)
    })
    if (!live) { await unsubscribe(); return { discarded: true } }
    release = unsubscribe; listening = true
    return { listening: true }
  }
  async function unsubscribe() {
    await release?.()
    release = undefined
    if (live) listening = false
    return { listening: false }
  }
</script>

<div class="actions">
  <Action {session} action="connectivity-read" title="Refresh native network" primary onclick={read} />
  <Action {session} action="connectivity-subscribe" title="Listen for network changes" disabled={listening} onclick={subscribe} />
  <Action {session} action="connectivity-unsubscribe" title="Stop listening" disabled={!listening} onclick={unsubscribe} />
</div>
{#if snapshot}
  <p id="network-access">Native network access: {snapshot.networkAccess}</p>
  <p id="network-profiles">Connection profiles: {snapshot.connectionProfiles.join(', ') || 'None reported'}</p>
{/if}
<p id="network-listening" role="status">{listening ? 'Listening for actual OS network changes' : 'Not listening'} · {changes} changes received</p>
<p class="input-help">Refresh reads current native conditions. Listening does not manufacture an initial event:
  change Wi-Fi or another network connection to observe the OS notification. The last unsubscribe
  detaches the native source; leaving this page removes only its own listener.</p>
<p class="input-help">Even “Internet” does not guarantee internet or endpoint reachability. No HTTP probe,
  navigator.onLine, browser event or mock network value is used. Missing platform configuration,
  permission denial and unsupported native APIs appear as explicit errors.</p>
