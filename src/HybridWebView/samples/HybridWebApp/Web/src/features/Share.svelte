<script lang="ts">
  import { onDestroy } from 'svelte'
  import { share, stageFile, releaseFile } from '@maui/api/essentials/share'
  import type { StagedFile } from '@maui/api/essentials/share'
  import Action from '../components/Action.svelte'
  import { createPdf } from '../browser-pdf.ts'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let message = $state('A PDF created by JavaScript, shared by native MAUI.')
  let text = $state('Hello from the native MAUI share sheet.')
  let staged = $state.raw<StagedFile | null>(null)
  let notes = $state.raw<StagedFile | null>(null)
  let previewUrl = $state<string | null>(null)
  let live = true
  async function prepare() {
    const pdf = createPdf(message)
    if (staged) { await releaseFile(staged); staged = null }
    const file = await stageFile('browser-document.pdf', 'application/pdf', pdf)
    if (!live) { await releaseFile(file); return { staged: false } }
    if (previewUrl) URL.revokeObjectURL(previewUrl)
    previewUrl = URL.createObjectURL(new Blob([pdf.slice().buffer as ArrayBuffer], { type: 'application/pdf' }))
    staged = file
    return { staged: true, fileName: file.fileName, size: file.size }
  }
  async function sharePdf() {
    if (!staged) throw new Error('Generate and stage a PDF first.')
    await share({ file: staged, title: 'Share browser-produced PDF' })
    return { nativeRequestCompleted: true, consumedByAnotherApp: 'not known' }
  }
  async function shareMultiple() {
    if (!staged) throw new Error('Generate and stage a PDF first.')
    notes ??= await stageFile('notes.txt', 'text/plain', new TextEncoder().encode('A second browser-produced document.'))
    await share({ files: [staged, notes], title: 'Share two staged documents' })
    return { nativeRequestCompleted: true, fileCount: 2 }
  }
  onDestroy(() => {
    live = false
    if (previewUrl) URL.revokeObjectURL(previewUrl)
    for (const file of [staged, notes])
      if (file) void releaseFile(file).catch(error => {
        if (!session.disposed) session.writeLog('Release staged share authority', 'Error', String(error))
      })
  })
</script>

<section class="guide-section">
  <h3>Native text sharing</h3>
  <label for="share-text">Text · sent only on an explicit click</label>
  <textarea id="share-text" maxlength="4096" bind:value={text}></textarea>
  <Action {session} action="share-text" title="Open native text share sheet" onclick={async () => {
    await share({ text, title: 'Share from MAUI' })
    return { nativeRequestCompleted: true }
  }} />
</section>
<section class="guide-section">
  <h3>Browser bytes → native share</h3>
  <label for="pdf-message">PDF text · 1–160 printable ASCII characters</label>
  <input id="pdf-message" maxlength="160" bind:value={message} />
  <p class="input-help">Change the text and recreate this small one-page document. This is not a general
    PDF editor. Bytes travel over the raw binary bridge to an app-owned cache; no browser path or
    filesystem writer is accepted. The separate JSON share action uses the longer native interaction budget.</p>
  <div class="actions">
    <Action {session} action="share-stage-pdf" title="Create and stage browser PDF" primary onclick={prepare} />
    <Action {session} action="share-pdf" title="Open native PDF share sheet" disabled={!staged} onclick={sharePdf} />
    <Action {session} action="share-multiple" title="Share PDF and text document" disabled={!staged} onclick={shareMultiple} />
  </div>
  {#if previewUrl}
    <p><a href={previewUrl} download="browser-document.pdf">Download browser PDF preview</a> · {staged?.size} staged bytes</p>
  {/if}
  <p class="input-help">A browser download is not OS sharing. Native request completion, particularly on
    Android, can mean only that a chooser launched. Shared files are retained for 24 hours and pruned
    on subsequent calls/startup; handles retire on reload. The retained app cache is bounded, not permanent storage.</p>
</section>
