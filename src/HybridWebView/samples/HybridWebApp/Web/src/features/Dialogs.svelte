<script lang="ts">
  import { alert, confirm, prompt } from '@maui/api/ui/dialogs'
  import Action from '../components/Action.svelte'
  import ActionSheet from '../components/ActionSheet.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let title = $state('Native MAUI dialog — 你好 👋')
  let message = $state('This OS dialog belongs to the calling playground window.')
  let accept = $state('Continue')
  let cancel = $state('Not now')
  let placeholder = $state('Type your answer')
  let initialValue = $state('Hello from TypeScript')
  let maxLength = $state(4096)
  let alertCancel = $state('Close')
  let promptAccept = $state('Continue')
  let promptCancel = $state('Not now')
</script>

<p class="input-help">Title and message apply to the alert, confirmation and prompt below.
  The action sheet has its own title and selectable labels.</p>
<label for="dialog-title">Dialog title</label>
<input id="dialog-title" bind:value={title} />
<label for="dialog-message">Message</label>
<textarea id="dialog-message" rows="3" bind:value={message}></textarea>
<section class="guide-section dialog-section" aria-labelledby="alert-heading">
  <h3 id="alert-heading">Alert</h3>
  <p class="input-help">One native acknowledgement button.</p>
  <label for="alert-cancel">Dismiss label</label>
  <input id="alert-cancel" bind:value={alertCancel} />
  <div class="actions">
    <Action {session} action="alert" title="Show native alert" primary
      onclick={() => alert(message, { title, cancel: alertCancel })} />
  </div>
</section>
<section class="guide-section dialog-section" aria-labelledby="confirm-heading">
  <h3 id="confirm-heading">Confirmation</h3>
  <p class="input-help">Returns true on acceptance or false on normal cancellation.</p>
  <div class="work-inputs">
    <div><label for="dialog-accept">Accept label</label><input id="dialog-accept" bind:value={accept} /></div>
    <div><label for="dialog-cancel">Cancel label</label><input id="dialog-cancel" bind:value={cancel} /></div>
  </div>
  <div class="actions">
    <Action {session} action="confirm" title="Ask for confirmation"
      onclick={() => confirm(message, { title, accept, cancel })} />
  </div>
</section>
<section class="guide-section dialog-section" aria-labelledby="prompt-heading">
  <h3 id="prompt-heading">Text prompt</h3>
  <p class="input-help">Returns the native text directly, or null on dismissal.</p>
  <div class="work-inputs">
    <div><label for="prompt-accept">Accept label</label><input id="prompt-accept" bind:value={promptAccept} /></div>
    <div><label for="prompt-cancel">Cancel label</label><input id="prompt-cancel" bind:value={promptCancel} /></div>
    <div><label for="prompt-placeholder">Placeholder</label><input id="prompt-placeholder" bind:value={placeholder} /></div>
    <div><label for="prompt-initial">Initial value</label><input id="prompt-initial" bind:value={initialValue} /></div>
    <div><label for="prompt-maxlength">Max length · -1 unspecified</label><input id="prompt-maxlength" type="number" bind:value={maxLength} /></div>
  </div>
  <div class="actions">
    <Action {session} action="prompt" title="Ask for text"
      onclick={() => prompt(message, {
        title, accept: promptAccept, cancel: promptCancel, placeholder, initialValue, maxLength, keyboard: { name: 'text' },
      })} />
  </div>
</section>
<section class="guide-section dialog-section" aria-labelledby="action-sheet-heading">
  <h3 id="action-sheet-heading">Action sheet</h3>
  <p class="input-help">Select a native action, destructive label or Cancel.</p>
  <ActionSheet {session} />
</section>
<p class="input-help">
  OS interaction has a five-minute budget rather than the normal twenty seconds.
  Reload or window destruction cancels the wait immediately. Stock MAUI provides no
  public force-dismiss API; dismiss any remaining OS dialog yourself.</p>
