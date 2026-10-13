<script lang="ts">
  import { installPlaygroundMenu } from '../native/menus.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let text = $state('Hands-on — renamed from TypeScript')
  let enabled = $state(true)
  let checked = $state(true)
  const missing = () => { throw new Error('Install the native menu first on a supported desktop host.') }
</script>
<p class="input-help">The real OS menu bar is authored in <code>Web/src/native/menus.ts</code>
  using the public TypeScript menu module. Its JS callbacks call the same window/dialog/browser
  modules as these pages. C# only renders validated descriptions and emits scoped item IDs.
  Mac uses Cmd accelerators, Windows Ctrl; no OS-owned application menu is replaced.</p>
<div class="actions">
  <Action {session} action="menus-install" title="Install / replace TypeScript menu" primary
    onclick={() => installPlaygroundMenu(session, true)} />
  <Action {session} action="menus-remove" title="Remove native menu" disabled={!$session.menu}
    onclick={async () => { await session.menuWindow?.setMenu(null); session.menu = undefined }} />
</div>
<label for="menu-text">New label for Navigate / hands-on</label>
<input id="menu-text" maxlength="160" bind:value={text} />
<div class="actions">
  <Action {session} action="menu-rename" title="Rename native item" disabled={!$session.menu}
    onclick={() => session.menu?.item('showcase').setText(text) ?? missing()} />
  <Action {session} action="menu-enable" title={enabled ? 'Disable hands-on item' : 'Enable hands-on item'} disabled={!$session.menu}
    onclick={async () => { const next = !enabled; await (session.menu?.item('showcase').setEnabled(next) ?? missing()); enabled = next }} />
  <Action {session} action="menu-check" title={checked ? 'Uncheck native item' : 'Check native item'} disabled={!$session.menu || !$session.menuSupportsChecked}
    onclick={async () => { const next = !checked; await (session.menu?.item('check-example').setChecked(next) ?? missing()); checked = next }} />
</div>
<p class="input-help">Actual native clicks run TypeScript callbacks. Replacement/removal clears
  old item listeners; frontend reload reinstalls this document's menu. Native checkbox state is
  supported on Mac Catalyst; the current MAUI Windows item lacks a toggle contract, so checked
  creation/update rejects explicitly there. Mobile menu bars are unsupported, never faked in HTML.</p>
