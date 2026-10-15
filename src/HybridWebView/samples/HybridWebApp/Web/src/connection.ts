import { connect, configureBridge, loadNativeBridge, NativeError } from '@maui/api/core'
import { getIdentity } from './native/sample.ts'
import type { PlaygroundSession } from './session.ts'
import { supportsBrowserView } from '@maui/api/ui/remote-webview'
import { installPlaygroundMenu } from './native/menus.ts'
import * as app from '@maui/api/application'
import { currentWindow } from '@maui/api/ui/windows'

export function createConnection(session: PlaygroundSession, loadBridge = loadNativeBridge) {
  let startup: Promise<void> | undefined

  async function bootstrap(): Promise<void> {
    if (session.disposed) return
    session.connecting = true
    session.refresh()
    try {
      if (loadBridge !== loadNativeBridge) {
        const bridge = await loadBridge()
        if (session.disposed) return
        configureBridge(bridge)
      }
      if (session.disposed) return
      session.setClient(await connect())
      const ready = session.client.readiness
      if (!ready) throw new Error('The shared native connection has no handshake metadata.')
      if (session.disposed) return
      if (ready.initialLocation) session.initialLocation?.(ready.initialLocation)
      if (ready.commands.includes('window.info')) session.windowCapabilities = await currentWindow().capabilities()
      if (ready.commands.includes('remote.info')) session.browserSupported = await supportsBrowserView()
      if (ready.commands.includes('menus.info')) await installPlaygroundMenu(session)
      if (ready.commands.includes('application.requestedTheme')) {
        session.themeRelease = await app.onThemeChanged(theme => {
          if (session.disposed) return
          session.theme = theme
          document.documentElement.dataset.theme = theme
          session.refresh()
        })
        const theme = await app.getTheme()
        if (session.disposed) return
        session.theme = theme
        document.documentElement.dataset.theme = theme
      }
      const identity = await getIdentity(session.client)
      if (session.disposed) return
      session.identityText = JSON.stringify(identity, null, 2)
      session.hostText = `Native connected · ${identity.platform} · PID ${identity.pid}`
      session.hostState = 'ready'
      session.hostSession = session.client.session
      session.startupId = identity.startupId
      session.connecting = false
      session.writeLog('Native session connected', 'Success', { ...ready, identity })
    } catch (error) {
      if (session.disposed) return
      session.client.dispose()
      if (error instanceof NativeError && error.code === 'native_unavailable') {
        session.hostText = 'Native host unavailable · browser-only'
        session.hostState = 'unavailable'; session.nativeUnavailable = true; session.connecting = false
        session.refresh()
        return
      }
      session.hostText = 'Native connection failed · reload to retry'
      session.hostState = 'error'
      session.nativeUnavailable = true
      session.connecting = false
      const failure = { code: error instanceof NativeError ? error.code : 'connection_error',
        message: error instanceof Error ? error.message : 'Bridge failed.' }
      session.writeLog('Native connection failed', 'Error', failure)
      session.showResult('Error', failure)
    }
  }

  return { start: () => startup ??= bootstrap() }
}
