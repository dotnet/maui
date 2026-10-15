<script lang="ts">
  import { sample } from '../native/sample.ts'
  import { openWindow } from '@maui/api/application'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session, reload }: FeatureProps = $props()
</script>

<p class="input-help">
  Increment the native counter, read it, reload, then read it again.
  The service instance survives a frontend-only reload.
</p>
<div class="actions">
  <Action {session} action="increment" title="Increment native counter" primary
    onclick={() => sample(session.client).echo({ text: null, number: 0, flag: false, items: [] })} />
  <Action {session} action="counter" title="Read native counter"
    onclick={() => sample(session.client).getCounter()} />
  <Action {session} action="reload" title="Reload frontend" onclick={reload} />
</div>
<section class="guide-section">
  <h3>Local target versus shared business target</h3>
  <p>Open another window and compare instance IDs. The local target differs per view; the explicitly
    supplied shared target spans views. Each view still owns its separate hub, document session,
    subscriptions and resources. Reload preserves targets but retires document authority.</p>
  <div class="actions">
    <Action {session} action="local-target" title="Increment this view's local target"
      onclick={() => sample(session.client).localCounter(1)} />
    <Action {session} action="shared-target" title="Increment shared business target"
      onclick={() => sample(session.client).sharedCounter(1)} />
    <Action {session} action="read-targets" title="Read both target identities"
      onclick={async () => ({ local: await sample(session.client).localCounter(), shared: await sample(session.client).sharedCounter() })} />
    <Action {session} action="targets-window" title="Compare in a child window"
      onclick={async () => ({ id: await (await openWindow({ url: 'index.html#/lifetime' })).id() })} />
  </div>
</section>
