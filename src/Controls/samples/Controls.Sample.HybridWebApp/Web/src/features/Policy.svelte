<script lang="ts">
  import { sample } from '../native/sample.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
</script>

<p class="input-help">
  These calls should reject with distinct codes: <code>unknown_command</code>,
  <code>disabled_command</code> and <code>invalid_arguments</code>. None requests an OS permission.
</p>
<div class="actions">
  <Action {session} action="missing" title="Try missing command" primary
    onclick={() => { void session.run('Unknown native command', () => sample(session.client).missing()) }} />
  <Action {session} action="disabled" title="Try disabled command"
    onclick={() => { void session.run('Disabled native command', () => sample(session.client).disabled()) }} />
  <Action {session} action="invalid" title="Send invalid DTO"
    onclick={() => { void session.run('Invalid native arguments', () => sample(session.client).invalid()) }} />
</div>
