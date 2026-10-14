<script lang="ts">
  import type { PlaygroundSession } from '../session.ts'
  let { session }: { session: PlaygroundSession } = $props()
</script>

<section class="result-section" aria-labelledby="result-title">
  <div class="section-heading">
    <h3 id="result-title">Result</h3>
    <span id="result-state" data-state={$session.resultStatus.toLowerCase()} role="status">{$session.resultStatus}</span>
  </div>
  <div id="result-json" tabindex="0" role="textbox" aria-readonly="true" aria-multiline="true"
    aria-label="Native operation result"><pre>{$session.resultText}</pre></div>
</section>
