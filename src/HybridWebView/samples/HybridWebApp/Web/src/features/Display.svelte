<script lang="ts">
  import { getDisplayInfo } from '@maui/api/essentials/display'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
</script>

<div class="actions">
  <Action {session} action="display" title="Refresh native display" primary
    onclick={getDisplayInfo} />
</div>
<p class="input-help">These are main-display pixels, density, orientation, rotation and refresh rate,
  not the browser viewport. Rotate the device and refresh to read the actual native result.
  This read-only provider never changes keep-screen-awake or subscribes to sensors.</p>
