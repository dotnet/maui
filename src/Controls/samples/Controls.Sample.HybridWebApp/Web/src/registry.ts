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
import Welcome from './features/Welcome.svelte'
import Showcase from './features/Showcase.svelte'
import Windows from './features/Windows.svelte'
import MultiWindow from './features/MultiWindow.svelte'
import Dialogs from './features/Dialogs.svelte'
import FilePicker from './features/FilePicker.svelte'
import RemoteWebView from './features/RemoteWebView.svelte'
import Battery from './features/Battery.svelte'
import Display from './features/Display.svelte'
import NativeMenus from './features/NativeMenus.svelte'
import Theme from './features/Theme.svelte'
import Connectivity from './features/Connectivity.svelte'
import TextToSpeech from './features/TextToSpeech.svelte'
import Launcher from './features/Launcher.svelte'
import Share from './features/Share.svelte'
import Geolocation from './features/Geolocation.svelte'

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
  welcome: Welcome,
  showcase: Showcase,
  windows: Windows,
  multiWindow: MultiWindow,
  dialogs: Dialogs,
  filePicker: FilePicker,
  remoteWebView: RemoteWebView,
  battery: Battery,
  display: Display,
  menus: NativeMenus,
  theme: Theme,
  connectivity: Connectivity,
  textToSpeech: TextToSpeech,
  launcher: Launcher,
  share: Share,
  geolocation: Geolocation,
}
