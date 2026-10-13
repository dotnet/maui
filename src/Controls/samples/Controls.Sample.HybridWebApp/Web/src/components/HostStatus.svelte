<script lang="ts">
  import type { PlaygroundSession } from '../session.ts'
  let { session }: { session: PlaygroundSession } = $props()
</script>

<div
  id="host-status"
  class="host-status"
  role="status"
  data-state={$session.hostState}
  data-session={$session.hostSession}
  data-startup-id={$session.startupId}
>{$session.hostText}</div>
