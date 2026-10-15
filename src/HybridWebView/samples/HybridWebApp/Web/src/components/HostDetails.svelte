<script lang="ts">
  import { message } from '../message.ts'
  import type { PlaygroundSession } from '../session.ts'
  let { session }: { session: PlaygroundSession } = $props()
</script>

<details class="host-details">
  <summary>Native identity and live-edit targets</summary>
  <pre id="native-identity">{$session.identityText}</pre>
  <div id="style-target">
    <p id="typescript-message">TypeScript edit target: {message}</p>
    <p id="module-message">{$session.moduleMessage}</p>
  </div>
</details>
