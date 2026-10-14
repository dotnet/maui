<script lang="ts">
  import { onDestroy } from 'svelte'
  import { currentWindow } from '@maui/api/ui/windows'
  import type { WindowCapabilities, WindowPosition, WindowSize } from '@maui/api/ui/windows'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session }: FeatureProps = $props()
  let own: ReturnType<typeof currentWindow>
  const windowHandle = () => own ??= currentWindow()
  let title = $state('My MAUI window')
  let x = $state(80), y = $state(80), width = $state(1000), height = $state(760)
  let id = $state<string>()
  let nativeTitle = $state<string>()
  let size = $state<WindowSize>()
  let position = $state<WindowPosition>()
  let capabilities = $state<WindowCapabilities>()
  let observationState = $state('Waiting for the native window.')
  let sizeEvents = $state(0), positionEvents = $state(0)
  let lastEvent = $state('No geometry event received yet.')
  let live = true, started = false, readVersion = 0
  const releases: (() => Promise<void>)[] = []

  function failed(label: string, error: unknown): void {
    if (live && !session.disposed) {
      observationState = error instanceof Error ? error.message : 'Native window observation failed.'
      session.writeLog(label, 'Error', observationState)
    }
  }
  async function retain(release: () => Promise<void>): Promise<void> {
    if (live && !session.disposed) releases.push(release)
    else await release()
  }
  async function read(): Promise<unknown> {
    const own = windowHandle()
    const version = ++readVersion, sizes = sizeEvents, positions = positionEvents
    const support = await own.capabilities()
    if (!live || session.disposed) return
    const [actualId, actualTitle, actualSize, actualPosition] = await Promise.all([
      own.id(), own.title(), own.size(), support.position ? own.position() : Promise.resolve(undefined),
    ])
    if (!live || session.disposed || version !== readVersion) return
    id = actualId; nativeTitle = actualTitle; capabilities = support
    if (sizeEvents === sizes) size = actualSize
    if (positionEvents === positions) position = actualPosition
    session.windowCapabilities = support
    session.refresh()
    observationState = 'Reading the actual native window. Resize it to see live events.'
    return { id, title: actualTitle, size: actualSize, position: actualPosition ?? 'unsupported', capabilities: support }
  }
  async function observe(): Promise<void> {
    const own = windowHandle()
    try {
      const support = await own.capabilities()
      if (!live || session.disposed) return
      capabilities = support
      session.windowCapabilities = support
      session.refresh()
      await retain(await own.onSizeChanged(value => {
        if (!live || session.disposed) return
        size = value; sizeEvents++
        lastEvent = `Size changed · ${new Date().toLocaleTimeString()}`
        session.writeLog('Window size changed', 'Event', value)
      }))
      if (!live || session.disposed) return
      if (support.positionChanged) {
        await retain(await own.onPositionChanged(value => {
          if (!live || session.disposed) return
          position = value; positionEvents++
          lastEvent = `Position changed · ${new Date().toLocaleTimeString()}`
          session.writeLog('Window position changed', 'Event', value)
        }))
      }
      if (live && !session.disposed) await read()
    } catch (error) { failed('Window observation', error) }
  }
  $effect(() => {
    if ($session.client.available && !$session.connecting && !started) {
      started = true
      void observe()
    }
  })
  onDestroy(() => {
    live = false
    for (const release of releases) void release().catch(error => {
      if (!session.disposed) session.writeLog('Window observation cleanup', 'Error',
        error instanceof Error ? error.message : 'Unsubscribe failed.')
    })
  })
</script>

<section class="window-live" aria-labelledby="window-live-title">
  <h3 id="window-live-title">Current native window</h3>
  <p id="window-observation-state" class="input-help">{observationState}</p>
  <dl class="window-values">
    <div><dt>ID</dt><dd id="window-live-id">{id ?? 'Not read yet'}</dd></div>
    <div><dt>Title</dt><dd id="window-live-title-value">{nativeTitle ?? 'Not read yet'}</dd></div>
    <div><dt>Size · native logical units</dt><dd id="window-live-size" aria-live="polite">{size ? `${size.width} × ${size.height}` : 'Not read yet'}</dd></div>
    <div><dt>Position · native logical units</dt><dd id="window-live-position" aria-live="polite">{position ? `${position.x}, ${position.y}` : capabilities?.position === false ? 'Unsupported on this host' : 'Not read yet'}</dd></div>
    <div><dt>Native events</dt><dd id="window-live-events">{sizeEvents} size · {positionEvents} position</dd></div>
  </dl>
  <p id="window-last-event" class="secondary">{lastEvent}</p>
  {#if capabilities}
    <p id="window-capabilities" class="input-help">
      Resize: {capabilities.resize ? 'supported' : 'unsupported'} ·
      Position: {capabilities.position ? 'supported' : 'unsupported'} ·
      Center: {capabilities.center ? 'supported' : 'unsupported'} ·
      Move events: {capabilities.positionChanged ? 'supported' : 'unsupported'}
    </p>
  {/if}
</section>
<div class="actions">
  <Action {session} action="window-info" title="Read current window" primary onclick={read} />
</div>
<p class="input-help">These are actual OS values, not CSS viewport dimensions or requested bounds.
  Size events are observed while this page is open. Native centering is supported on Mac Catalyst
  and Windows. Move-event observation remains Windows-only; other hosts do not emulate it.</p>
<label for="window-title">Native window title</label>
<input id="window-title" maxlength="160" bind:value={title} />
<div class="actions">
  <Action {session} action="title" title="Set window title" onclick={async () => {
    const value = title
    await windowHandle().setTitle(value)
    if (live && !session.disposed) return read()
  }} />
</div>
<div class="work-inputs">
  <div><label for="window-width">Width</label><input id="window-width" type="number" min="320" max="4096" bind:value={width} /></div>
  <div><label for="window-height">Height</label><input id="window-height" type="number" min="240" max="4096" bind:value={height} /></div>
</div>
<div class="actions">
  <Action {session} action="bounds" title="Request native size" disabled={!capabilities?.resize}
    onclick={() => windowHandle().setSize({ width, height })} />
</div>
<div class="work-inputs">
  <div><label for="window-x">X</label><input id="window-x" type="number" min="-10000" max="10000" bind:value={x} /></div>
  <div><label for="window-y">Y</label><input id="window-y" type="number" min="-10000" max="10000" bind:value={y} /></div>
</div>
<div class="actions">
  <Action {session} action="position" title="Request native position" disabled={!capabilities?.position}
    onclick={() => windowHandle().setPosition({ x, y })} />
  <Action {session} action="center" title="Center on current display" disabled={!capabilities?.center}
    onclick={() => windowHandle().center()} />
</div>
<p class="input-help">The OS can constrain requested dimensions. Setting a size or position does not
  fabricate a live event; the readouts change only on a native callback or an explicit read.</p>
