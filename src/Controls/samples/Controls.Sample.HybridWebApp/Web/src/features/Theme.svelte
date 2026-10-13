<script lang="ts">
  import * as app from '@maui/api/application'
  import type { AppThemePreference } from '@maui/api/application'
  import Action from '../components/Action.svelte'
  import type { FeatureProps } from '../session.ts'
  let { session }: FeatureProps = $props()
  async function set(preference: AppThemePreference): Promise<unknown> {
      await app.setTheme(preference)
      const effective = await app.getTheme()
      if (!session.disposed) {
        session.theme = effective
        document.documentElement.dataset.theme = effective
        session.refresh()
      }
      return { preference, effective }
  }
</script>
<p class="input-help">Theme belongs to the native application, not a window. TypeScript chooses
  Application.UserAppTheme and observes the actual effective RequestedThemeChanged event.
  The web interface applies its own CSS from that event; native theme alone cannot paint Svelte.
  System restores OS theme following. Remote websites are not forced to this application's CSS.</p>
<div class="actions">
  <Action {session} action="theme-system" title="Follow system" primary onclick={() => set('system')} />
  <Action {session} action="theme-light" title="Use light theme" onclick={() => set('light')} />
  <Action {session} action="theme-dark" title="Use dark theme" onclick={() => set('dark')} />
</div>
<p id="effective-theme" class="input-help">Effective native theme: {$session.theme ?? 'not read yet'}</p>
