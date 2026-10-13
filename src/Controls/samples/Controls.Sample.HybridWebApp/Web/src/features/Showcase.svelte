<script lang="ts">
  import * as app from '@maui/api/application'
  import { currentWindow } from '@maui/api/ui/windows'
  import { prompt } from '@maui/api/ui/dialogs'
  import { setText } from '@maui/api/essentials/clipboard'
  import { getBatteryInfo } from '@maui/api/essentials/battery'
  import { getDisplayInfo } from '@maui/api/essentials/display'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let title = $state('My MAUI playground — 你好 👋')
</script>

<section class="guide-section">
  <h3>Make this window yours</h3>
  <p>Change the actual title of this native window, not the HTML document title.</p>
  <label for="showcase-title">Window title</label>
  <input id="showcase-title" maxlength="160" bind:value={title} />
  <div class="actions">
    <Action {session} action="title" title="Change my window title" primary
      onclick={() => currentWindow().setTitle(title)} />
    <Action {session} action="copy" title="Copy title to clipboard"
      onclick={() => setText(title)} />
  </div>
  <p class="input-help"><a href="#/windows">Window details and desktop placement</a></p>
</section>
<section class="guide-section">
  <h3>Ask the operating system</h3>
  <p>A native prompt appears over this calling page. Cancel is a normal result; you have
    up to five minutes, and reloading cancels the frontend wait.</p>
  <div class="actions">
    <Action {session} action="prompt" title="Show a native prompt"
      onclick={() => prompt('What would you like to build?', {
        title: 'Hello from MAUI', accept: 'Share idea', cancel: 'Later', placeholder: 'Your idea',
      })} />
    <Action {session} action="device" title="Read power and display"
      onclick={async () => ({
        battery: await getBatteryInfo(), display: await getDisplayInfo(),
      })} />
  </div>
  <p class="input-help"><a href="#/dialogs">All native dialogs</a> · <a href="#/file-picker">Choose a file</a></p>
</section>
<section class="guide-section">
  <h3>Open something separate</h3>
  <p>A new playground window gets an independent native scope.
    The <a href="#/remote-webview">desktop browser pane</a> instead hosts a separate native
    browsing surface in a measured slot within this web app, without an iframe or native bridge.</p>
  <div class="actions">
    <Action {session} action="open-window" title="Open hands-on in a new window"
      onclick={async () => ({
        id: await (await app.openWindow({ url: 'index.html#/showcase' })).id(),
      })} />
  </div>
  <p class="input-help"><a href="#/multi-window">List and close child windows</a> · <a href="#/menus">Try the real desktop menu bar</a></p>
</section>
