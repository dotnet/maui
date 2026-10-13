<script lang="ts">
  import type { PlaygroundSession } from '../session.ts'
  import { presentAction } from '../action.ts'

  let { session, action, title, primary = false, disabled = false, onclick }: {
    session: PlaygroundSession
    action: string
    title: string
    primary?: boolean
    disabled?: boolean
    onclick: () => unknown | Promise<unknown>
  } = $props()
</script>

<button
  type="button"
  data-action={action}
  class:primary
  disabled={disabled || $session.isDisabled(action)}
  onclick={() => {
    if (disabled || session.isDisabled(action)) return
    if (action === 'reload' || action === 'cancel') { void onclick(); return }
    void presentAction(session, title, onclick)
  }}
>{title}</button>
