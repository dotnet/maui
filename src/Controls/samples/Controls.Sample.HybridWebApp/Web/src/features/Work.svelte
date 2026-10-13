<script lang="ts">
  import { sample } from '../native/sample.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session, reload }: FeatureProps = $props()
  let steps = $state<number | undefined>(20)
  let delay = $state<number | undefined>(300)

  async function work(): Promise<unknown> {
    // Capture this form before awaiting the document-owned progress subscription.
    const capturedSteps = Number(steps)
    const capturedDelay = Number(delay)
    const api = sample(session.client)
    if (!session.unsubscribeProgress) {
      const unsubscribe = await api.onProgress((value, id) => {
        if (session.disposed || session.active?.id !== id) return
        session.writeEvent('Native operation.progress', { ...value, requestId: id }, id)
        if (session.selected.experiment === 'work') {
          session.progressMaximum = value.total
          session.progressValue = value.completed
          session.showResult('Loading', { ...value, requestId: id })
        }
      })
      if (session.disposed) {
        await unsubscribe()
        return
      }
      session.unsubscribeProgress = unsubscribe
    }
    if (session.disposed) return
    if (session.selected.experiment === 'work') session.progressValue = 0
    session.active = api.work(capturedSteps, capturedDelay)
    session.refresh()
    return session.active.result
  }

  async function cancel(): Promise<void> {
    if (!session.active || session.disposed || !session.client.available) return
    try {
      const accepted = await session.active.cancel()
      if (!session.disposed) session.writeLog('Cancellation requested', 'Loading', { accepted })
    } catch (error) {
      if (!session.disposed)
        session.writeLog('Cancel request', 'Error', error instanceof Error ? error.message : 'Cancel request failed.')
    }
  }
</script>

<div class="work-inputs">
  <div>
    <label for="steps">Steps · 1–50</label>
    <input id="steps" type="number" min="1" max="50" bind:value={steps} />
  </div>
  <div>
    <label for="delay">Delay per step · 100–500 ms</label>
    <input id="delay" type="number" min="100" max="500" step="100" bind:value={delay} />
  </div>
</div>
<p class="input-help">
  At most 15 seconds per operation. Progress comes from native code, not a JavaScript timer.
  Reload while running to test cleanup.
</p>
<div class="actions">
  <Action {session} action="work" title="Run native work" primary onclick={work} />
  <Action {session} action="cancel" title="Cancel operation" onclick={() => { void cancel() }} />
  <Action {session} action="reload" title="Reload frontend" onclick={reload} />
</div>
