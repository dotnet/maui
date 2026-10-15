<script lang="ts">
  import { getIdentity } from '../native/sample.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session, reload }: FeatureProps = $props()
</script>

<p class="input-help">
  Live-edit markers remain in the HTML title and host-details disclosure.
  Edit <code>Web/src/message.ts</code>, <code>details.ts</code>, <code>style.css</code> or <code>index.html</code>.
  Experiment UI lives in <code>Web/src/features/*.svelte</code>.
  For offline: <code>npm --prefix Web run stage</code>, then build native.
</p>
<div class="actions">
  <Action {session} action="identity" title="Read native startup identity" primary
    onclick={() => getIdentity(session.client)} />
  <Action {session} action="reload" title="Reload frontend" onclick={reload} />
</div>
