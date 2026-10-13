<script lang="ts">
  import { components } from '../registry.ts'
  import Result from './Result.svelte'
  import OperationLog from './OperationLog.svelte'
  import HostDetails from './HostDetails.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session, reload }: FeatureProps = $props()
  let feature = $derived($session.selected)
</script>

<main id="experiment" tabindex="-1">
  <div class="experiment-heading">
    <div>
      <p id="feature-meta" class="secondary">{feature.category}</p>
      <h2 id="feature-title">{feature.title}</h2>
    </div>
    <a id="api-link" href={feature.docs} target="_blank" rel="noopener noreferrer">API documentation ↗</a>
  </div>
  <p id="feature-description" class="instructions">{feature.description}</p>
  <div id="native-unavailable" class="notice" hidden={!$session.nativeUnavailable}>
    Native host unavailable. Open this playground in the MAUI app to run native experiments.
    No clipboard, preferences or device values are emulated here.
  </div>
  <section id="controls" aria-label="Experiment inputs">
    {#key $session.selectionVersion}
      {#if feature.experiment}
        {@const FeatureComponent = components[feature.experiment]}
        <FeatureComponent {session} {reload} />
      {/if}
    {/key}
  </section>
  <Result {session} />
  <OperationLog {session} />
  <HostDetails {session} />
</main>
