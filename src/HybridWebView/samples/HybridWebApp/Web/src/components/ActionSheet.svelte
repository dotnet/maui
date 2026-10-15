<script lang="ts">
  import { actionSheet } from '@maui/api/ui/dialogs'
  import type { ActionSheetOptions } from '@maui/api/ui/dialogs'
  import Action from './Action.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session }: Pick<FeatureProps, 'session'> = $props()
  let title = $state('Choose an action')
  let choices = $state('Duplicate\nShare\nExport')
  let cancel = $state('Cancel')
  let destruction = $state('Delete')
  let flowDirection = $state<NonNullable<ActionSheetOptions['flowDirection']>>('matchParent')

  function show(): Promise<string | null> {
    return actionSheet({
      title,
      buttons: choices.split(/\r?\n/).filter(label => label.trim()),
      cancel: cancel.trim() ? cancel : null,
      destruction: destruction.trim() ? destruction : null,
      flowDirection,
    })
  }
</script>

<label for="action-sheet-title">Native sheet title</label>
<input id="action-sheet-title" maxlength="160" bind:value={title} />
<label for="action-sheet-buttons">Actions · one label per line</label>
<textarea id="action-sheet-buttons" rows="4" maxlength="4096" bind:value={choices}></textarea>
<p class="input-help">Blank lines are ignored. These are selectable labels, not destructive app operations.</p>
<div class="work-inputs">
  <div>
    <label for="action-sheet-cancel">Cancel label · blank to omit</label>
    <input id="action-sheet-cancel" maxlength="160" bind:value={cancel} />
  </div>
  <div>
    <label for="action-sheet-destruction">Destructive label · blank to omit</label>
    <input id="action-sheet-destruction" maxlength="160" bind:value={destruction} />
  </div>
</div>
<label for="action-sheet-direction">Native text direction</label>
<select id="action-sheet-direction" bind:value={flowDirection}>
  <option value="matchParent">Follow the native page</option>
  <option value="leftToRight">Left to right</option>
  <option value="rightToLeft">Right to left</option>
</select>
<div class="actions">
  <Action {session} action="action-sheet" title="Show native action sheet" primary onclick={show} />
</div>
<p class="input-help">The result is the selected native label, including Cancel, or null on dismissal.
  The sheet belongs to this window and uses the same five-minute interactive budget as other dialogs.</p>
