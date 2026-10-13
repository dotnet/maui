import './style.css'
import { PlaygroundSession } from './session.ts'
import { mountShell } from './shell.ts'
import { createConnection } from './connection.ts'

const session = new PlaygroundSession()
const shell = mountShell(document, session)
const connection = createConnection(session)
void import('./details.ts').then(({ details }) => {
  if (!session.disposed) {
    session.moduleMessage = details
    session.refresh()
  }
})
void connection.start()

const onPageShow = (event: PageTransitionEvent) => { if (event.persisted) location.reload() }
window.addEventListener('pageshow', onPageShow)
const onPageHide = () => session.dispose()
window.addEventListener('pagehide', onPageHide, { once: true })
if (import.meta.hot) import.meta.hot.dispose(() => {
  window.removeEventListener('pageshow', onPageShow)
  window.removeEventListener('pagehide', onPageHide)
  shell.dispose()
  session.dispose()
})
