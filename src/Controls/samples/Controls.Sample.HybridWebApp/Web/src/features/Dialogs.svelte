<script lang="ts">
  import { alert, confirm, prompt } from '@maui/api/ui/dialogs'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let title = $state('Native MAUI dialog — 你好 👋')
  let message = $state('This OS dialog belongs to the calling playground window.')
  let accept = $state('Continue')
  let cancel = $state('Not now')
  let placeholder = $state('Type your answer')
  let initialValue = $state('Hello from TypeScript')
  let maxLength = $state(4096)
</script>

<label for="dialog-title">Dialog title</label>
<input id="dialog-title" maxlength="160" bind:value={title} />
<label for="dialog-message">Message</label>
<textarea id="dialog-message" rows="3" maxlength="4096" bind:value={message}></textarea>
<div class="work-inputs">
  <div><label for="dialog-accept">Accept label</label><input id="dialog-accept" bind:value={accept} /></div>
  <div><label for="dialog-cancel">Cancel label</label><input id="dialog-cancel" bind:value={cancel} /></div>
  <div><label for="prompt-placeholder">Prompt placeholder</label><input id="prompt-placeholder" bind:value={placeholder} /></div>
  <div><label for="prompt-initial">Prompt initial value</label><input id="prompt-initial" bind:value={initialValue} /></div>
  <div><label for="prompt-maxlength">Prompt max length · -1 unspecified</label><input id="prompt-maxlength" type="number" min="-1" bind:value={maxLength} /></div>
</div>
<div class="actions">
  <Action {session} action="alert" title="Show native alert" primary
    onclick={() => alert(message, { title, cancel })} />
  <Action {session} action="confirm" title="Ask for confirmation"
    onclick={() => confirm(message, { title, accept, cancel })} />
  <Action {session} action="prompt" title="Ask for text"
    onclick={() => prompt(message, {
      title, accept, cancel, placeholder, initialValue, maxLength, keyboard: 'text',
    })} />
</div>
<p class="input-help">Confirm returns false on Cancel. Prompt returns cancelled with null text.
  OS interaction has a five-minute budget rather than the normal twenty seconds.
  Reload or window destruction cancels the wait immediately. Stock MAUI provides no
  public force-dismiss API; dismiss any remaining OS dialog yourself.</p>
