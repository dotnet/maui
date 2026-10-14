<script lang="ts">
  import { parseEcho } from '../arguments.ts'
  import { sample } from '../native/sample.ts'
  import Action from '../components/Action.svelte'
  import { Channel, binaryBlob, number, object } from '@maui/api/core'
  import type { FeatureProps } from '../session.ts'

  let { session }: FeatureProps = $props()
  let input = $state(JSON.stringify({
    text: 'Unicode: café 你好 👋\n"quotes" \\ slash',
    number: 42.5, flag: true, items: ['first', '第二'],
  }, null, 2))
  let documentBytes = $state<Uint8Array | undefined>()
  let streamProof = $state('Not started')

  async function echo(): Promise<unknown> {
    return sample(session.client).echo(parseEcho(input))
  }

  async function binaryRoundtrip(): Promise<unknown> {
    const original = Uint8Array.from({ length: 262144 }, (_, i) => i % 256)
    const result = await sample(session.client).binaryEcho(new Blob([original.buffer], { type: 'application/octet-stream' }))
    if (result.length !== original.length || !result.every((value, i) => value === original[i]))
      throw new Error('The native Blob roundtrip did not preserve every byte.')
    return { exact: true, bytes: result.length, all256Values: true, transport: 'Core binary' }
  }
  async function nativeDocument(): Promise<unknown> {
    documentBytes = await sample(session.client).binaryDocument()
    const blob = binaryBlob(documentBytes, 'application/pdf')
    const url = URL.createObjectURL(blob), link = document.createElement('a')
    try {
      link.href = url; link.download = 'native-binary-document.pdf'; link.click()
    } finally {
      // Browser download handoff, not an OS share. Release this document-owned object URL after consumption.
      setTimeout(() => URL.revokeObjectURL(url), 1000)
    }
    return { bytes: documentBytes.length, mimeType: blob.type, prefix: new TextDecoder().decode(documentBytes.subarray(0, 8)) }
  }
  async function nativeChunks(): Promise<unknown> {
    const seen: number[] = [], progress: number[] = []
    streamProof = 'Receiving with an async consumer…'
    const chunks = Channel.bytes(async bytes => {
      const index = seen.length
      await new Promise(resolve => setTimeout(resolve, 20))
      if (bytes.length !== 65536 || !bytes.every((value, i) => value === (index * 65536 + i) % 256))
        throw new Error('Native chunks were not exact and ordered.')
      seen.push(bytes.length)
      streamProof = `${seen.length} ordered chunks consumed`
    })
    const updates = new Channel(value => number(object(value).completed), async completed => {
      if (completed !== progress.length + 1 || completed !== seen.length) throw new Error('Progress arrived before byte consumption.')
      progress.push(completed)
    })
    const operation = sample(session.client).binaryChunks(chunks, updates)
    session.active = operation
    session.refresh()
    const result = await operation.result
    await Promise.all([chunks.completion, updates.completion])
    return { ...result, chunks: seen.length, bytes: seen.reduce((a, b) => a + b, 0), ordered: true, asyncConsumer: true }
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
    onclick={() => sample(session.client).getCounter()} />
</div>

<h3>Binary documents and ordered channels</h3>
<p class="input-help">
  Send a 256 KiB browser Blob without JSON/base64 expansion, load native-produced PDF bytes as a Blob,
  and consume four 64 KiB native chunks in order. Native waits for each async consumer acknowledgement.
  Channels end with their action; file handles remain read-only, and no OS share is claimed.
</p>
<div class="actions" data-testid="binary-experiment">
  <Action {session} action="binary-roundtrip" title="Roundtrip 256 KiB Blob" onclick={binaryRoundtrip} />
  <Action {session} action="binary-document" title="Download native PDF Blob" onclick={nativeDocument} />
  <Action {session} action="binary-chunks" title="Consume ordered native chunks" onclick={nativeChunks} />
  <Action {session} action="cancel" title="Cancel native chunks" onclick={() => session.cancelOperation()} />
</div>
<p data-testid="binary-stream-proof">{streamProof}</p>
{#if documentBytes}
  <p data-testid="binary-document-proof">Native byte document: {documentBytes.length} bytes (application/pdf)</p>
{/if}
