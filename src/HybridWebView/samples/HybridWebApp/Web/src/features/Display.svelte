<script lang="ts">
  import { getDisplayInfo, getKeepScreenOn, setKeepScreenOn, onMainDisplayInfoChanged } from '@maui/api/essentials/display'
  import { onDestroy } from 'svelte'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let keepScreenOn = $state(false)
  let listening = $state(false)
  let changes = $state(0)
  let release: (() => Promise<void>) | undefined
  let live = true
  onDestroy(() => {
    live = false
    void release?.().catch(error => { if (!session.disposed) session.writeLog('Display listener cleanup', 'Error', String(error)) })
  })
  async function readAwake() {
    const value = await getKeepScreenOn()
    if (live) keepScreenOn = value
    return value
  }
  async function listen() {
    const unsubscribe = await onMainDisplayInfoChanged(value => {
      if (!live) return
      changes++
      session.writeLog('Native display change', 'Event', value)
    })
    if (!live) { await unsubscribe(); return { discarded: true } }
    release = unsubscribe; listening = true
    return { listening: true }
  }
  async function stop() {
    await release?.(); release = undefined
    if (live) listening = false
    return { listening: false }
  }
</script>

<label><input id="keep-screen-on" type="checkbox" bind:checked={keepScreenOn} /> Native keep-screen-on preference</label>
<div class="actions">
  <Action {session} action="display" title="Refresh native display" primary
    onclick={getDisplayInfo} />
  <Action {session} action="display-awake-read" title="Read keep-screen-on" onclick={readAwake} />
  <Action {session} action="display-awake-set" title="Apply keep-screen-on" onclick={() => setKeepScreenOn(keepScreenOn)} />
  <Action {session} action="display-listen" title="Listen for display changes" disabled={listening} onclick={listen} />
  <Action {session} action="display-stop" title="Stop display listener" disabled={!listening} onclick={stop} />
</div>
<p id="display-events" role="status">{changes} actual native display changes received</p>
<p class="input-help">These are main-display pixels, density, orientation, rotation and refresh rate,
  not the browser viewport. Rotate the device and refresh to read the actual native result.
  Native display reads, keep-screen-on get/set and change events use annotations directly.
  Only Apply changes the native preference; leaving the page detaches its own display listener
  without resetting the native preference or other callers' state.</p>
