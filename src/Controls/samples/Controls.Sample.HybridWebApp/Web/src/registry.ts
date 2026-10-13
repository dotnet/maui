import type { Component } from 'svelte'
import type { Experiment } from './catalog.ts'
import type { FeatureProps } from './session.ts'
import Identity from './features/Identity.svelte'
import Clipboard from './features/Clipboard.svelte'
import Preferences from './features/Preferences.svelte'
import Commands from './features/Commands.svelte'
import Errors from './features/Errors.svelte'
import Policy from './features/Policy.svelte'
import Lifetime from './features/Lifetime.svelte'
import Events from './features/Events.svelte'
import Work from './features/Work.svelte'
import Development from './features/Development.svelte'

export const components: Record<Experiment, Component<FeatureProps>> = {
  identity: Identity,
  clipboard: Clipboard,
  preferences: Preferences,
  echo: Commands,
  errors: Errors,
  policy: Policy,
  lifetime: Lifetime,
  events: Events,
  work: Work,
  development: Development,
}
