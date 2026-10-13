<script lang="ts">
  import { parseEcho } from '../arguments.ts'
  import { sample } from '../native/sample.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session }: FeatureProps = $props()
  let input = $state(JSON.stringify({
    text: 'Unicode: café 你好 👋\n"quotes" \\ slash',
    number: 42.5, flag: true, items: ['first', '第二'],
  }, null, 2))

  function echo(): void {
    void session.run('Async native DTO echo', async () => {
      return sample(session.client).echo(parseEcho(input))
    })
  }

  function subscribe(): void {
    void session.run('Subscribe to native changes', async () => {
      if (!session.unsubscribeCounter) {
        const unsubscribe = await sample(session.client).onCounter((counter, id) => {
          session.writeEvent('Native service.changed', { counter, requestId: id }, id)
        })
        if (session.disposed) {
          await unsubscribe()
          return
        }
        session.unsubscribeCounter = unsubscribe
      }
      return { subscribed: true, topic: 'service.changed' }
    })
  }

  function unsubscribe(): void {
    void session.run('Unsubscribe from native changes', async () => {
      await session.unsubscribeCounter?.()
      session.unsubscribeCounter = undefined
      return { subscribed: false, topic: 'service.changed' }
    })
  }
</script>

<label for="echo-input">Command arguments · JSON DTO</label>
<textarea id="echo-input" rows="8" spellcheck="false" class="code-input" bind:value={input}></textarea>
<p class="input-help">
  Use text or null, a finite number, a boolean and up to 16 strings. The response is asynchronous.
  Successful calls increment the native counter.
</p>
<div class="actions">
  <Action {session} action="echo" title="Run native echo" primary onclick={echo} />
  <Action {session} action="subscribe" title="Subscribe to changes" onclick={subscribe} />
  <Action {session} action="unsubscribe" title="Unsubscribe" onclick={unsubscribe} />
</div>
