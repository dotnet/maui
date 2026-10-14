<script lang="ts">
  import { onDestroy } from 'svelte'
  import { getLocales, speak } from '@maui/api/essentials/text-to-speech'
  import type { SpeechLocale } from '@maui/api/essentials/text-to-speech'
  import type { Operation } from '@maui/api/core'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let text = $state('Hello from the .NET MAUI Playground.')
  let locales = $state<SpeechLocale[]>()
  let localeIndex = $state('')
  let pitch = $state(1)
  let volume = $state(0.5)
  let rate = $state(1)
  let current = $state<Operation<void>>()
  let live = true
  onDestroy(() => {
    live = false
    void current?.cancel().catch(error => {
      if (!session.disposed) session.writeLog('Speech cancellation', 'Error', String(error))
    })
  })
  async function loadLocales() {
    const values = await getLocales()
    if (live) { locales = values; localeIndex = '' }
    return values
  }
  async function say() {
    const operation = await speak(text, {
      pitch, volume, rate, locale: localeIndex === '' ? undefined : locales?.[Number(localeIndex)],
    })
    current = operation
    if (!live) void operation.cancel().catch(error => {
      if (!session.disposed) session.writeLog('Speech cancellation', 'Error', String(error))
    })
    else { session.active = operation; session.refresh() }
    try { await operation.result; return { completed: true } }
    finally { current = undefined }
  }
  async function cancel() {
    const operation = current
    if (operation) {
      try {
        const accepted = await operation.cancel()
        session.writeLog('Cancel this speech', 'Loading', { accepted })
      } catch (error) {
        session.writeLog('Cancel this speech', 'Error', error instanceof Error ? error.message : 'Native cancellation failed.')
      }
    }
  }
</script>

<label for="speech-text">Text to speak · no speech until you choose Speak</label>
<textarea id="speech-text" rows="3" bind:value={text}></textarea>
<div class="actions">
  <Action {session} action="speech-locales" title="Load native locales" onclick={loadLocales} />
</div>
<label for="speech-locale">Native voice locale</label>
<select id="speech-locale" bind:value={localeIndex}>
  <option value="">Native default</option>
  {#each locales ?? [] as locale, index}
    <option value={String(index)}>{locale.name || locale.language || locale.id || 'Unnamed native locale'} {locale.country}</option>
  {/each}
</select>
{#if locales}<p id="speech-locale-count" class="input-help">{locales.length} native locales reported. An empty list can mean no installed voices.</p>{/if}
<label for="speech-pitch">Pitch · 0–2</label>
<input id="speech-pitch" type="number" min="0" max="2" step="0.1" bind:value={pitch} />
<label for="speech-volume">Volume · 0–1</label>
<input id="speech-volume" type="number" min="0" max="1" step="0.1" bind:value={volume} />
<label for="speech-rate">Rate · 0.1–2 (1 is normal)</label>
<input id="speech-rate" type="number" min="0.1" max="2" step="0.1" bind:value={rate} />
<div class="actions">
  <Action {session} action="speech-speak" title="Speak through native engine" primary onclick={say} />
  <Action {session} action="cancel" title="Cancel this speech" disabled={!current} onclick={cancel} />
</div>
<p id="speech-status" role="status">{current ? 'Native speech in progress' : 'Ready for an explicit speech action'}</p>
<p class="input-help">The SDK uses actual native locales, SpeechOptions and SpeakAsync cancellation.
  This page cancels only its own speech operation, never another feature. Leaving the page
  requests that same cancellation; disconnect cancels document-owned work. There is no global stop API,
  browser speech synthesis or invented voice list.</p>
<p class="input-help">Text must fit the host-advertised UTF-16 budget (normally 4096 characters).
  Native speech has a bounded interactive deadline. Platform/engine failures are explicit;
  cancellation is cooperative and cannot undo audio already played.</p>
