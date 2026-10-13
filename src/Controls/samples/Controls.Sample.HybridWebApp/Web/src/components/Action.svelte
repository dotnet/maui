<script lang="ts">
  import type { PlaygroundSession } from '../session.ts'

  let { session, action, title, primary = false, onclick }: {
    session: PlaygroundSession
    action: string
    title: string
    primary?: boolean
    onclick: () => void
  } = $props()
</script>

<button
  type="button"
  data-action={action}
  class:primary
  disabled={$session.isDisabled(action)}
  onclick={() => { if (!session.isDisabled(action)) onclick() }}
>{title}</button>
