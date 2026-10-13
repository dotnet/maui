import { mount, unmount } from 'svelte'
import App from './App.svelte'
import HostStatus from './components/HostStatus.svelte'
import type { PlaygroundSession } from './session.ts'

export function mountShell(document: Document, session: PlaygroundSession,
  reload = () => document.defaultView?.location.reload(),
  compact = () => document.defaultView?.matchMedia('(max-width: 760px)').matches ?? false) {
  const target = document.querySelector('#app')
  const header = document.querySelector('#connection')
  if (!target || !header) throw new Error('Missing playground mount targets.')
  session.select(session.selected)
  const app = mount(App, { target, props: { session, reload, compact } })
  const host = mount(HostStatus, { target: header, props: { session } })
  return {
    dispose: () => {
      void unmount(app)
      void unmount(host)
    },
  }
}
