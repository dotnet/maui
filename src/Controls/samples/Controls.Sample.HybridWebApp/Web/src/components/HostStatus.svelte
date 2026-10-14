<script lang="ts">
  import type { PlaygroundSession } from '../session.ts'
  let { session, id = 'host-status' }: { session: PlaygroundSession; id?: string } = $props()
</script>

<div
  {id}
  class="host-status"
  role="status"
  data-state={$session.hostState}
  data-session={$session.hostSession}
  data-startup-id={$session.startupId}
>{$session.hostText}</div>
