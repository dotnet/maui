<script lang="ts">
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
</script>

<section class="guide-section">
  <h3>Web-first. Native when you need it.</h3>
  <p>This interface is Svelte running inside a MAUI HybridWebView. Its buttons call small,
    typed native providers through <code>@maui/api</code>. The operating system does the
    real work: dialogs, clipboard, windows and file selection.</p>
  <div class="guide-links">
    <a href="#/showcase">Try it hands-on</a>
    <a href="#/identity">Inspect the native host</a>
    <a href="#/file-picker">Choose a text file</a>
  </div>
</section>
<section class="guide-section">
  <h3>Explore without losing your place</h3>
  <p>Search or expand a navigation group for detailed experiments. Each page has a shareable
    internal fragment route. Reloading the frontend keeps your route and the native process;
    another playground window has its own providers and counter.</p>
  <p>Internal routes are not OS deep links. Custom-scheme activation and app actions are
    planned for a later milestone.</p>
</section>
<section class="guide-section">
  <h3>A clear boundary for websites</h3>
  <p><a href="#/remote-webview">Open a hosted website</a> in a second, ordinary native
    desktop browsing surface anchored to a slot within the web app. It is not a
    DOM element or iframe, and receives no playground native bridge or development inspector.</p>
  {#if $session.nativeUnavailable}
    <p>You're exploring in a browser. Navigation and these guides still work, but native
      actions are disabled. Launch the MAUI sample to try the real APIs.</p>
  {/if}
</section>
