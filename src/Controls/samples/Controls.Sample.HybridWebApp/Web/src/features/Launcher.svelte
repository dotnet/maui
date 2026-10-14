<script lang="ts">
  import { openUrl } from '@maui/api/essentials/launcher'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  let url = $state('https://learn.microsoft.com/dotnet/maui/')
  async function launch() {
    await openUrl(url)
    return { launched: true }
  }
</script>

<label for="launch-url">Public HTTPS website · opens outside this app</label>
<input id="launch-url" type="url" bind:value={url} />
<div class="actions">
  <Action {session} action="launch-url" title="Open with native system launcher" primary onclick={launch} />
</div>
<p class="input-help">Only this explicit click calls the OS URL launcher. A successful result means
  the system accepted the launch, not that the remote page loaded. The playground never navigates
  itself, opens an in-app browser or creates a native guest for this feature.</p>
<p class="input-help">Use an absolute public ASCII HTTPS address up to 2048 characters. Credentials,
  IP literals, local/internal names, custom ports, files, shell commands and custom schemes are
  rejected before native launch. This address policy is not a DNS or redirect firewall.
  Native unsupported APIs and launch refusal are explicit errors; there is no window.open fallback.</p>
