<script lang="ts">
  import { getBatteryInfo, onBatteryInfoChanged, onEnergySaverStatusChanged } from '@maui/api/essentials/battery'
  import { onDestroy } from 'svelte'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let charge = $state<number>()
  let listening = $state(false)
  let saving = $state(false)
  let changes = $state(0)
  let releaseInfo: (() => Promise<void>) | undefined
  let releaseSaver: (() => Promise<void>) | undefined
  let live = true
  onDestroy(() => {
    live = false
    for (const release of [releaseInfo, releaseSaver])
      void release?.().catch(error => { if (!session.disposed) session.writeLog('Battery listener cleanup', 'Error', String(error)) })
  })
  async function listenInfo() {
    const release = await onBatteryInfoChanged(value => {
      if (!live) return
      charge = value.chargeLevel; changes++
      session.writeLog('Native battery change', 'Event', value)
    })
    if (!live) { await release(); return { discarded: true } }
    releaseInfo = release; listening = true
    return { listening: true }
  }
  async function listenSaver() {
    const release = await onEnergySaverStatusChanged(value => {
      if (!live) return
      changes++
      session.writeLog('Native energy saver change', 'Event', value)
    })
    if (!live) { await release(); return { discarded: true } }
    releaseSaver = release; saving = true
    return { listening: true }
  }
  async function stop() {
    await releaseInfo?.(); await releaseSaver?.()
    releaseInfo = undefined; releaseSaver = undefined
    if (live) { listening = false; saving = false }
    return { listening: false }
  }
  async function read() {
    const result = await getBatteryInfo()
    if (live) charge = result.chargeLevel
    return result
  }
</script>

<div class="actions">
  <Action {session} action="battery" title="Refresh native power" primary
    onclick={read} />
  <Action {session} action="battery-listen" title="Listen for battery changes" disabled={listening} onclick={listenInfo} />
  <Action {session} action="energy-saver-listen" title="Listen for energy saver changes" disabled={saving} onclick={listenSaver} />
  <Action {session} action="battery-stop" title="Stop power listeners" disabled={!listening && !saving} onclick={stop} />
</div>
<p id="battery-events" role="status">{changes} actual native changes received</p>
{#if charge !== undefined}
  <p id="battery-charge" class="input-help">Battery charge:
    {charge < 0 ? 'Not reported by this device' : new Intl.NumberFormat(undefined, { style: 'percent', maximumFractionDigits: 1 }).format(charge)}
  </p>
{/if}
<p class="input-help">Charge is a fraction from zero to one; -1 means no battery is reported.
  State, power source and energy-saver status come directly from the native OS. Simulators
  or unsupported devices may report Unknown or reject the read. Listeners start only on explicit clicks;
  leaving this page removes only its own callbacks. Convenience reads compose four native properties,
  while events carry the actual native event arguments, not a synthesized snapshot.
  Mac Catalyst reads the Mac's internal capacity through IOKit rather than UIKit's
  misleading battery value. iOS can report a single-precision float, so JSON may show
  binary floating-point digits. Only the percentage above is formatted.</p>
