<script lang="ts">
  import { sample } from '../native/sample.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
</script>

<p class="input-help">
  Expected result: <code>intentional_error</code>. Read the native counter afterward to verify recovery.
</p>
<div class="actions">
  <Action {session} action="fail" title="Run deliberate failure" primary
    onclick={() => { void session.run('Intentional native failure', () => sample(session.client).fail()) }} />
  <Action {session} action="counter" title="Verify bridge still works"
    onclick={() => { void session.run('Read counter after error', () => sample(session.client).getCounter()) }} />
</div>
