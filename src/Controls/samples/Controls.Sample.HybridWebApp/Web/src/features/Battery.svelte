<script lang="ts">
  import { getBatteryInfo } from '@maui/api/essentials/battery'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
</script>

<div class="actions">
  <Action {session} action="battery" title="Refresh native power" primary
    onclick={getBatteryInfo} />
</div>
<p class="input-help">Charge is a fraction from zero to one; -1 means no battery is reported.
  State, power source and energy-saver status come directly from the native OS. Simulators
  or unsupported devices may report Unknown or reject the read. No background monitoring.</p>
