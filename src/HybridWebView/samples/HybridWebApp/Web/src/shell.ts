import { mount, unmount } from 'svelte'
import App from './App.svelte'
import type { PlaygroundSession } from './session.ts'
import { readRoute } from './routes.ts'

export function mountShell(document: Document, session: PlaygroundSession,
  reload = () => document.defaultView?.location.reload(),
  compact = () => document.defaultView?.matchMedia('(max-width: 1000px)').matches ?? false) {
  const target = document.querySelector('#app')
  if (!target) throw new Error('Missing playground mount target.')
  const window = document.defaultView
  const navigate = (page: { id: string }) => {
    if (window && window.location.hash !== `#/${page.id}`) window.location.hash = `/${page.id}`
  }
  const changed = () => {
    const route = readRoute(window?.location.hash ?? '')
    if (session.selected.id !== route.page.id) session.select(route.page)
    if (route.error) session.showResult('Error', { code: 'invalid_route', message: route.error })
    document.querySelector('#experiment')?.scrollTo?.(0, 0)
  }
  session.select(readRoute(window?.location.hash ?? '').page)
  changed()
  session.navigate = navigate
  session.reloadFrontend = reload
  const initialLocation = (url: string) => {
    if (!window || window.location.hash) return
    const location = new URL(url, window.location.href)
    window.history.replaceState(null, '', location.search + location.hash)
    changed()
  }
  session.initialLocation = initialLocation
  window?.addEventListener('hashchange', changed)
  const app = mount(App, { target, props: { session, reload, compact } })
  return {
    dispose: () => {
      window?.removeEventListener('hashchange', changed)
      if (session.navigate === navigate) session.navigate = undefined
      if (session.reloadFrontend === reload) session.reloadFrontend = undefined
      if (session.initialLocation === initialLocation) session.initialLocation = undefined
      void unmount(app)
    },
  }
}
