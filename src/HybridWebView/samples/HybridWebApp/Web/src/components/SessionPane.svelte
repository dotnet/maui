<script lang="ts">
  import OperationLog from './OperationLog.svelte'
  import HostDetails from './HostDetails.svelte'
  import HostStatus from './HostStatus.svelte'
  import type { PlaygroundSession } from '../session.ts'
  let { session, reload }: { session: PlaygroundSession; reload: () => void } = $props()
</script>

<section class="session-pane" aria-label="Session diagnostics">
  <HostStatus {session} id="session-host-status" />
  <button id="frontend-refresh" class="refresh-button" title="Reload frontend, not the native app" onclick={reload}>
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M20 10a8 8 0 1 0-1 7M20 4v6h-6" /></svg>
    Reload frontend
  </button>
  <p class="secondary">Reloads only this document. Native service state and startup identity remain.</p>
  <section class="session-operation" aria-labelledby="operation-title">
    <h3 id="operation-title">Current operation</h3>
    <p id="current-operation" class="input-help" role="status">
      {$session.busy ? `${$session.activeLabel}…` : 'No native operation running.'}
    </p>
    <progress id="progress" max={$session.progressMaximum} value={$session.progressValue}
      aria-label="Native work progress" hidden={!$session.active}></progress>
    {#if $session.active}
      <p class="secondary">{$session.progressValue} / {$session.progressMaximum} steps completed</p>
      <button id="session-cancel" onclick={() => session.cancelOperation()}
        disabled={$session.isDisabled('cancel')}>Cancel operation</button>
    {/if}
  </section>
  <OperationLog {session} />
  <HostDetails {session} />
</section>
