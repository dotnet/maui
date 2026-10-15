<script lang="ts">
  import { onDestroy } from 'svelte'
  import { pickFile } from '@maui/api/essentials/file-picker'
  import type { PickedFile } from '@maui/api/essentials/file-picker'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let title = $state('Choose a file to preview')
  let text = $state<string>()
  let name = $state('')
  let truncated = $state(false)
  let cancelled = $state(false)
  let live = true
  let selected: PickedFile | undefined
  const previewBytes = 65536
  onDestroy(() => {
    live = false
    if (selected && session.client.available) void selected.dispose().catch(error => {
      if (!session.disposed) session.writeLog('Selected file cleanup', 'Error', error instanceof Error ? error.message : 'Cleanup failed.')
    })
  })
  async function pick(): Promise<unknown> {
    text = undefined; truncated = false; cancelled = false
    const pickerTitle = title
    await selected?.dispose(); selected = undefined
    const file = await pickFile({ pickerTitle })
    if (!file) { if (live) cancelled = true; return { cancelled: true } }
    if (!live) { await file.dispose(); return { discarded: true } }
    selected = file; name = file.name
    const reader = await file.openRead()
    try {
      const chunks: Uint8Array[] = []
      let total = 0, eof = false
      while (total < previewBytes && !eof) {
        const part = await reader.readBytes(Math.min(reader.maximumChunkBytes, previewBytes - total))
        chunks.push(part.bytes); total += part.bytes.length; eof = part.eof
        if (part.bytes.length === 0) break
      }
      const extra = eof ? undefined : await reader.readBytes(1)
      const limited = !!extra?.bytes.length
      const bytes = new Uint8Array(total)
      let offset = 0
      for (const chunk of chunks) { bytes.set(chunk, offset); offset += chunk.length }
      const value = new TextDecoder('utf-8', { fatal: true }).decode(bytes, { stream: limited })
      if (live && !session.disposed) { text = value; truncated = limited }
      return { name: file.name, contentType: file.contentType, previewBytes: total, truncated: limited }
    } finally { await reader.close() }
  }
</script>
<label for="picker-title">Native picker title · supplied from TypeScript</label>
<input id="picker-title" bind:value={title} />
<div class="actions"><Action {session} action="pick-file" title="Choose file and preview UTF-8" primary onclick={pick} /></div>
<p class="input-help">The SDK selects real files of any size/type without reading them first.
  This sample reads at most 64 KiB through the normal public reader API; that is a UI preview
  budget, not an SDK file-size limit. Other apps can use pickMultiple, platform type filters,
  binary readBytes, incremental UTF-8 readText and seek where the native stream supports it.</p>
<p class="input-help">No FullPath, arbitrary path or saved filesystem authority is exposed.
  Binary/non-UTF8 files remain valid SDK selections but cannot be shown as this text preview.
  File bodies stay out of the operation log. Folder/save/defaultPath are not native MAUI picker options.</p>
{#if cancelled}<p id="file-cancelled">No file selected.</p>{/if}
{#if text !== undefined}
  <h3>Selected preview · {name}</h3>
  {#if truncated}<p id="file-truncated" class="notice">Preview truncated at the sample's 64 KiB UI limit. The SDK can continue reading this file.</p>{/if}
  <pre id="file-preview" class="file-preview">{text}</pre>
{/if}
