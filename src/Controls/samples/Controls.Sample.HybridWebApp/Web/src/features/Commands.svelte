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
</script>

<label for="echo-input">Command arguments · JSON DTO</label>
<textarea id="echo-input" rows="8" spellcheck="false" class="code-input" bind:value={input}></textarea>
<p class="input-help">
  Use text or null, a finite number, a boolean and up to 16 strings. The response is asynchronous.
  Successful calls increment the native counter.
</p>
<div class="actions">
  <Action {session} action="echo" title="Run native echo" primary onclick={echo} />
  <Action {session} action="counter" title="Read native counter"
    onclick={() => { void session.run('Read native service counter', () => sample(session.client).getCounter()) }} />
</div>
