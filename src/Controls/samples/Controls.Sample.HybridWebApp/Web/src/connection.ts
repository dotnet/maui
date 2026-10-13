import { NativeClient, loadNativeBridge } from '@maui/api/core'
import { getIdentity } from './native/sample.ts'
import type { PlaygroundSession } from './session.ts'

export function createConnection(session: PlaygroundSession, loadBridge = loadNativeBridge) {
  let startup: Promise<void> | undefined

  async function bootstrap(): Promise<void> {
    if (session.disposed) return
    session.connecting = true
    session.refresh()
    try {
      const bridge = await loadBridge()
      if (session.disposed) return
      session.setClient(new NativeClient(bridge))
      if (!bridge) {
        session.hostText = 'Native host unavailable · browser-only'
        session.hostState = 'unavailable'
        session.nativeUnavailable = true
        session.connecting = false
        session.refresh()
        return
      }
      session.client.attachToDocument()
      const ready = await session.client.connect()
      if (session.disposed) return
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
      session.hostText = 'Native connection failed · reload to retry'
      session.hostState = 'error'
      session.nativeUnavailable = true
      session.connecting = false
      session.showResult('Error', error instanceof Error ? error.message : 'Bridge failed.')
    }
  }

  return { start: () => startup ??= bootstrap() }
}
