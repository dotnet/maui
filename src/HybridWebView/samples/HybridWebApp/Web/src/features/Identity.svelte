<script lang="ts">
  import { getIdentity } from '../native/sample.ts'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'

  let { session, reload }: FeatureProps = $props()

  async function read(): Promise<unknown> {
      const identity = await getIdentity(session.client)
      if (!session.disposed) {
        session.identityText = JSON.stringify(identity, null, 2)
        session.refresh()
      }
      return identity
  }
</script>

<p class="input-help">
  Compare <code>pid</code>, <code>startupId</code> and <code>startedAt</code>
  before and after frontend reload.
</p>
<div class="actions">
  <Action {session} action="run" title="Read native identity" primary onclick={read} />
  <Action {session} action="reload" title="Reload frontend" onclick={reload} />
</div>
