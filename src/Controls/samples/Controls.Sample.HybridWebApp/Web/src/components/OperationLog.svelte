<script lang="ts">
  import type { PlaygroundSession } from '../session.ts'
  let { session }: { session: PlaygroundSession } = $props()
</script>

<section class="log-section" aria-labelledby="log-title">
  <div class="section-heading">
    <h3 id="log-title">Operation log</h3>
    <button id="clear-log" class="quiet" onclick={() => session.clearLog()}>Clear log</button>
  </div>
  <p class="secondary">Most recent 40 entries. Frontend reload clears this log, not native service state.</p>
  <ol id="operation-log" aria-live="polite" aria-relevant="additions" data-last-event-id={$session.lastEventId}>
    {#each $session.entries as entry (entry.id)}
      <li data-state={entry.status.toLowerCase()}>
        <time>{entry.time}</time>
        <strong>{entry.label}</strong>
        <span>{entry.status}</span>
        <pre>{entry.detail}</pre>
      </li>
    {:else}
      <li class="empty-log">{$session.emptyLog}</li>
    {/each}
  </ol>
</section>
