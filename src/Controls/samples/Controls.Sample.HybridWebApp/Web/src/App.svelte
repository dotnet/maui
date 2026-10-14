<script lang="ts">
  import { onMount, tick, untrack } from 'svelte'
  import Navigation from './components/Navigation.svelte'
  import Experiment from './components/Experiment.svelte'
  import SessionPane from './components/SessionPane.svelte'
  import HostStatus from './components/HostStatus.svelte'
  import { clampSessionWidth, sessionWidthLimit, minimumSessionWidth } from './panes.ts'
  import type { FeatureProps } from './session.ts'

  let { session, reload, compact }: FeatureProps & { compact: () => boolean } = $props()
  let mobile = $state(untrack(() => compact()))
  let navigationOpen = $state(untrack(() => !mobile))
  let sessionOpen = $state(untrack(() => !mobile))
  let sessionWidth = $state(300)
  let maximumWidth = $state(440)
  let workspace: HTMLDivElement
  let navigationPane: HTMLDialogElement
  let diagnosticsPane: HTMLDialogElement
  let navigationToggle: HTMLButtonElement
  let sessionToggle: HTMLButtonElement
  let dragging = $state(false)
  let drag: { id: number; x: number; width: number } | undefined
  let live = true

  function clamp(): void {
    maximumWidth = sessionWidthLimit(workspace?.clientWidth || window.innerWidth, navigationOpen)
    sessionWidth = clampSessionWidth(sessionWidth, maximumWidth)
  }
  function close(pane: 'navigation' | 'session', returnFocus = true): void {
    if (pane === 'navigation') navigationOpen = false
    else sessionOpen = false
    drag = undefined; dragging = false
    if (mobile) {
      const dialog = pane === 'navigation' ? navigationPane : diagnosticsPane
      dialog.close()
      if (returnFocus) (pane === 'navigation' ? navigationToggle : sessionToggle).focus()
    } else if (returnFocus) (pane === 'navigation' ? navigationToggle : sessionToggle).focus()
    clamp()
  }
  function toggle(pane: 'navigation' | 'session'): void {
    if (pane === 'navigation') {
      if (navigationOpen) { close(pane); return }
      if (mobile && sessionOpen) close('session', false)
      navigationOpen = true
    } else {
      if (sessionOpen) { close(pane); return }
      if (mobile && navigationOpen) close('navigation', false)
      sessionOpen = true
    }
    clamp()
  }
  function selected(): void {
    if (mobile) close('navigation', false)
  }
  function outside(event: MouseEvent, pane: 'navigation' | 'session'): void {
    if (!mobile || event.target !== event.currentTarget) return
    const bounds = (event.currentTarget as HTMLElement).getBoundingClientRect()
    if (event.clientX < bounds.left || event.clientX > bounds.right ||
      event.clientY < bounds.top || event.clientY > bounds.bottom) close(pane)
  }
  function trap(event: KeyboardEvent, pane: 'navigation' | 'session'): void {
    if (!mobile) return
    if (event.key === 'Escape') { event.preventDefault(); close(pane); return }
    if (event.key !== 'Tab') return
    const dialog = event.currentTarget as HTMLElement
    const controls = [...dialog.querySelectorAll<HTMLElement>('button:not(:disabled), input, select, textarea, a[href], summary, [tabindex="0"]')]
      .filter(element => !element.closest('[hidden]'))
    const first = controls[0], last = controls.at(-1)
    if (!first || !last) { event.preventDefault(); dialog.focus(); return }
    if (event.shiftKey && (document.activeElement === first || document.activeElement === dialog)) {
      event.preventDefault(); last.focus()
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault(); first.focus()
    }
  }
  function resizeKey(event: KeyboardEvent): void {
    if (mobile) return
    const step = event.shiftKey ? 48 : 24
    const width = event.key === 'ArrowLeft' ? sessionWidth + step : event.key === 'ArrowRight' ? sessionWidth - step
      : event.key === 'Home' ? minimumSessionWidth : event.key === 'End' ? maximumWidth : undefined
    if (width !== undefined) { event.preventDefault(); sessionWidth = clampSessionWidth(width, maximumWidth) }
  }
  function startResize(event: PointerEvent): void {
    if (mobile || event.button !== 0) return
    event.preventDefault()
    const separator = event.currentTarget as HTMLElement
    separator.focus()
    drag = { id: event.pointerId, x: event.clientX, width: sessionWidth }
    dragging = true
  }
  $effect(() => {
    const compactMode = mobile
    const nav = navigationOpen, diagnostics = sessionOpen
    const synchronize = async (dialog: HTMLDialogElement, open: boolean, first: string) => {
      if (!dialog) return
      if (compactMode) {
        if (open && !dialog.open) {
          dialog.showModal()
          await tick()
          if (live && dialog.open) dialog.querySelector<HTMLElement>(first)?.focus()
        } else if (!open && dialog.open) dialog.close()
      } else {
        if (dialog.open) dialog.close()
        if (open) dialog.setAttribute('open', '')
      }
    }
    void synchronize(navigationPane, nav, '#search')
    void synchronize(diagnosticsPane, diagnostics, '.pane-close')
  })
  onMount(() => {
    const resized = () => {
      const next = compact()
      if (next !== mobile) {
        navigationPane.close(); diagnosticsPane.close()
        mobile = next
        navigationOpen = !next; sessionOpen = !next
        drag = undefined; dragging = false
        if (next) navigationToggle.focus()
      }
      clamp()
    }
    const move = (event: PointerEvent) => {
      if (drag && event.pointerId === drag.id)
        sessionWidth = clampSessionWidth(drag.width + drag.x - event.clientX, maximumWidth)
    }
    const end = (event: PointerEvent) => {
      if (drag?.id === event.pointerId) { drag = undefined; dragging = false }
    }
    const media = window.matchMedia?.('(max-width: 1000px)')
    media?.addEventListener('change', resized)
    window.addEventListener('resize', resized)
    window.addEventListener('pointermove', move)
    window.addEventListener('pointerup', end)
    window.addEventListener('pointercancel', end)
    clamp()
    return () => {
      live = false
      media?.removeEventListener('change', resized)
      window.removeEventListener('resize', resized)
      window.removeEventListener('pointermove', move)
      window.removeEventListener('pointerup', end)
      window.removeEventListener('pointercancel', end)
    }
  })
</script>

<header class="app-header">
  <div class="header-title">
    <button id="navigation-toggle" class="icon-button" aria-label="Toggle feature navigation" title="Feature navigation"
      aria-controls="navigation-pane" aria-expanded={navigationOpen} bind:this={navigationToggle}
      onclick={() => toggle('navigation')}>
      <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><rect x="3" y="4" width="18" height="16" rx="2" /><path d="M9 4v16M5.5 9h1M5.5 13h1" /></svg>
    </button>
    <h1 id="html-message">.NET MAUI <span>Playground</span></h1>
  </div>
  <div class="header-actions">
    <div id="connection" class="header-connection"><HostStatus {session} /></div>
    {#if $session.active}
      <button id="header-cancel" class="quiet" onclick={() => session.cancelOperation()}
        disabled={$session.isDisabled('cancel')}>Cancel operation</button>
    {/if}
    <button id="session-toggle" class="icon-button" aria-label="Toggle session diagnostics" title="Session diagnostics"
      aria-controls="session-pane" aria-expanded={sessionOpen} bind:this={sessionToggle}
      onclick={() => toggle('session')}>
      <svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><rect x="3" y="4" width="18" height="16" rx="2" /><path d="M15 4v16M17.5 9h1M17.5 13h1" /></svg>
    </button>
  </div>
</header>
<div class="workspace" class:mobile class:resizing={dragging}
  style={`--navigation-width: ${!mobile && navigationOpen ? 292 : 0}px; --session-width: ${!mobile && sessionOpen ? sessionWidth : 0}px; --separator-width: ${!mobile && sessionOpen ? 8 : 0}px`}
  bind:this={workspace}>
  <dialog id="navigation-pane" class="shell-pane navigation-pane" aria-label="Feature navigation"
    aria-modal={mobile && navigationOpen ? 'true' : undefined} data-browser-obscured={mobile && navigationOpen}
    bind:this={navigationPane} oncancel={event => { event.preventDefault(); close('navigation') }}
    onclick={event => outside(event, 'navigation')} onkeydown={event => trap(event, 'navigation')}>
    <div class="pane-heading"><h2>Features</h2><button class="icon-button pane-close" aria-label="Close feature navigation"
      onclick={() => close('navigation')}><svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="m6 6 12 12M18 6 6 18" /></svg></button></div>
    <Navigation {session} compact={() => mobile} onselect={selected} />
  </dialog>
  <Experiment {session} {reload} />
  <!-- svelte-ignore a11y_no_noninteractive_tabindex, a11y_no_noninteractive_element_interactions (ARIA window splitters are focusable separators with value and keyboard controls.) -->
  <div class="pane-separator" role="separator" tabindex={mobile || !sessionOpen ? -1 : 0}
    hidden={mobile || !sessionOpen} aria-label="Resize session pane" aria-orientation="vertical"
    aria-controls="session-pane" aria-valuemin={minimumSessionWidth} aria-valuemax={maximumWidth}
    aria-valuenow={Math.round(sessionWidth)} onpointerdown={startResize} onkeydown={resizeKey}></div>
  <dialog id="session-pane" class="shell-pane diagnostics-pane" aria-label="Session"
    aria-modal={mobile && sessionOpen ? 'true' : undefined} data-browser-obscured={mobile && sessionOpen}
    bind:this={diagnosticsPane} oncancel={event => { event.preventDefault(); close('session') }}
    onclick={event => outside(event, 'session')} onkeydown={event => trap(event, 'session')}>
    <div class="pane-heading"><h2>Session</h2><button class="icon-button pane-close" aria-label="Close session diagnostics"
      onclick={() => close('session')}><svg viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="m6 6 12 12M18 6 6 18" /></svg></button></div>
    <SessionPane {session} {reload} />
  </dialog>
</div>
