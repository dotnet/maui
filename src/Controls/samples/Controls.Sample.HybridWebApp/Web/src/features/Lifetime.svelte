<script lang="ts">
  import { sample } from '../native/sample.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session, reload }: FeatureProps = $props()
</script>

<p class="input-help">
  Increment the native counter, read it, reload, then read it again.
  The service instance survives a frontend-only reload.
</p>
<div class="actions">
  <Action {session} action="increment" title="Increment native counter" primary
    onclick={() => { void session.run('Increment service counter', () =>
      sample(session.client).echo({ text: null, number: 0, flag: false, items: [] })) }} />
  <Action {session} action="counter" title="Read native counter"
    onclick={() => { void session.run('Read service counter', () => sample(session.client).getCounter()) }} />
  <Action {session} action="reload" title="Reload frontend" onclick={reload} />
</div>
