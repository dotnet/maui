<script lang="ts">
  import { getBatteryInfo } from '@maui/api/essentials/battery'
  import { onDestroy } from 'svelte'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let charge = $state<number>()
  let live = true
  onDestroy(() => { live = false })
  async function read() {
    const result = await getBatteryInfo()
    if (live) charge = result.chargeLevel
    return result
  }
</script>

<div class="actions">
  <Action {session} action="battery" title="Refresh native power" primary
    onclick={read} />
</div>
{#if charge !== undefined}
  <p id="battery-charge" class="input-help">Battery charge:
    {charge < 0 ? 'Not reported by this device' : new Intl.NumberFormat(undefined, { style: 'percent', maximumFractionDigits: 1 }).format(charge)}
  </p>
{/if}
<p class="input-help">Charge is a fraction from zero to one; -1 means no battery is reported.
  State, power source and energy-saver status come directly from the native OS. Simulators
  or unsupported devices may report Unknown or reject the read. No background monitoring.
  Mac Catalyst reads the Mac's internal capacity through IOKit rather than UIKit's
  misleading battery value. iOS can report a single-precision float, so JSON may show
  binary floating-point digits. Only the percentage above is formatted.</p>
