<script lang="ts">
  import BrowserPanel from '../components/BrowserPanel.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session, reload }: FeatureProps = $props()
</script>

<p class="input-help">This desktop proof places an actual ordinary native browser surface over a
  measured slot in this web app's layout—not an iframe, DOM WebView element, separate window
  or native grid row. The HTML toolbar remains in Svelte. Mac Catalyst/Windows are supported;
  mobile embedding is explicitly unsupported for this proof.</p>
<p class="input-help">Use HTTPS explicitly. The default site redirects to Microsoft's MAUI page,
  whose framing policy blocks ordinary iframes. No framing/security headers are bypassed.
  The remote guest receives no SDK, invoke target, messages, command hub or inspector.</p>
<BrowserPanel {session} {reload} />
<p class="input-help">Native compositing is not HTML z-order. The guest is clipped to its scroll
  viewport, hidden during DOM dialogs/popovers or while the document is hidden, and closed
  when you leave this feature or reload. Custom DOM overlays must mark
  <code>data-browser-obscured="true"</code> before showing; geometry is not a DNS/subresource firewall.</p>
