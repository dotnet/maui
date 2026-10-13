<script lang="ts">
  import Navigation from './components/Navigation.svelte'
  import Experiment from './components/Experiment.svelte'
  import type { FeatureProps } from './session.ts'

  let { session, reload, compact }: FeatureProps & { compact: () => boolean } = $props()
</script>

<div class="workspace">
  <Navigation {session} {compact} />
  <Experiment {session} {reload} />
</div>
