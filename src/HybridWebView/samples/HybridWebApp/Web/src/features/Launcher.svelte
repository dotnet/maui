<script lang="ts">
  import { canOpen, openUrl, tryOpen } from '@maui/api/essentials/launcher'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let url = $state('https://learn.microsoft.com/dotnet/maui/')
  async function launch() {
    return { accepted: await openUrl(url) }
  }
</script>

<label for="launch-url">Native URI · opens outside this app</label>
<input id="launch-url" type="url" bind:value={url} />
<div class="actions">
  <Action {session} action="launch-url" title="Open with native system launcher" primary onclick={launch} />
  <Action {session} action="can-launch-url" title="Query native URI support" onclick={async () => ({ canOpen: await canOpen(url) })} />
  <Action {session} action="try-launch-url" title="Try native launch" onclick={async () => ({ accepted: await tryOpen(url) })} />
</div>
<p class="input-help">Only this explicit click calls the OS URL launcher. A successful result means
  the system accepted the launch, not that the remote page loaded. The playground never navigates
  itself, opens an in-app browser or creates a native guest for this feature.</p>
<p class="input-help">HTTPS and platform-supported custom schemes are passed to the native destination.
  The actual native boolean is shown, including false refusal; native errors stay errors.
  This is not the separate remote guest's HTTPS policy. No file-request overload,
  shell execution or window.open fallback is exposed.</p>
