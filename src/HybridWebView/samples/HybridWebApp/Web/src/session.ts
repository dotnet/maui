import { writable } from 'svelte/store'
import { NativeClient, NativeError } from '@maui/api/core'
import type { Operation } from '@maui/api/core'
import { guidePages } from './catalog.ts'
import type { Feature } from './catalog.ts'
import type { WindowCapabilities } from '@maui/api/ui/windows'
import type { BrowserView } from '@maui/api/ui/remote-webview'
import type { AppTheme } from '@maui/api/application'
import { navigationPages } from './catalog.ts'
import type { Menu } from '@maui/api/ui/menus'
import type { Window } from '@maui/api/ui/windows'

export interface LogEntry {
  id: number
  time: string
  label: string
  status: string
  detail: string
}

export interface FeatureProps {
  session: PlaygroundSession
  reload: () => void
}

const format = (value: unknown): string =>
  typeof value === 'string' ? value : JSON.stringify(value, null, 2) ?? ''

/** Document-owned native state; component unmount never disposes this session. */
export class PlaygroundSession {
  client = new NativeClient(undefined)
  busy = false
  activeLabel = ''
  connecting = false
  active: Operation<unknown> | undefined
  unsubscribeCounter: (() => Promise<void>) | undefined
  unsubscribeProgress: (() => Promise<void>) | undefined
  selected: Feature = guidePages[0]!
  navigate: ((page: Feature) => void) | undefined
  initialLocation: ((url: string) => void) | undefined
  windowCapabilities: WindowCapabilities | undefined
  readonly childWindows = new Map<string, Window>()
  browserSupported: boolean | undefined
  browser: BrowserView | undefined
  menu: Menu | undefined
  menuWindow: Window | undefined
  menuSupportsChecked = false
  theme: AppTheme | undefined
  themeRelease: (() => Promise<void>) | undefined
  reloadFrontend: (() => void) | undefined

  navigateNative(id: string): void {
    const page = navigationPages.find(page => page.id === id)
    if (!page) { this.showResult('Error', { code: 'invalid_route', message: 'Native menu requested an unknown internal page.' }); return }
    this.select(page)
  }
  selectionVersion = 0
  disposed = false
  resultStatus = 'Ready to run'
  resultText = 'Run an experiment to inspect its actual native result.'
  progressValue = 0
  progressMaximum = 1
  entries: LogEntry[] = []
  emptyLog = 'Native requests and events will appear here.'
  lastEventId: string | undefined
  identityText = 'No native identity received.'
  hostText = 'Connecting to native host…'
  hostState = 'loading'
  hostSession: string | undefined
  startupId: string | undefined
  nativeUnavailable = false
  moduleMessage = ''
  private logId = 0
  private readonly changes = writable<PlaygroundSession>(this)
  readonly subscribe = this.changes.subscribe

  setClient(client: NativeClient): void {
    this.client.dispose()
    if (this.disposed) client.dispose()
    this.client = client
    this.refresh()
  }

  select(feature: Feature): void {
    this.navigate?.(feature)
    this.selected = feature
    this.selectionVersion++
    this.showResult(this.busy ? 'Loading' : 'Ready to run', this.busy
      ? 'A native operation is running. Inspect Session or use Cancel operation in the header.'
      : 'Run an experiment to inspect its actual native result.')
  }

  writeLog(label: string, status: string, value: unknown): void {
    if (this.disposed) return
    this.entries = [{
      id: ++this.logId, time: new Date().toLocaleTimeString(), label, status, detail: format(value),
    }, ...this.entries].slice(0, 40)
    this.refresh()
  }

  writeEvent(label: string, value: unknown, id: string): void {
    if (this.disposed) return
    this.lastEventId = id
    this.writeLog(label, 'Event', value)
  }

  clearLog(): void {
    this.entries = []
    this.emptyLog = 'Log cleared. New native requests and events will appear here.'
    this.refresh()
  }

  showResult(status: string, value: unknown): void {
    if (this.disposed) return
    this.resultStatus = status
    this.resultText = format(value)
    this.refresh()
  }

  isDisabled(action: string): boolean {
    if (action === 'reload') return false
    if (this.disposed || this.connecting || !this.client.available) return true
    if (action === 'bounds' && !this.windowCapabilities?.resize) return true
    if (action === 'position' && !this.windowCapabilities?.position) return true
    if (action === 'open-window' && !this.windowCapabilities?.multiWindow) return true
    if (action === 'center' && !this.windowCapabilities?.center) return true
    if (action === 'cancel') return !this.active
    if (action === 'unsubscribe') return this.busy || !this.unsubscribeCounter
    if (action === 'subscribe') return this.busy || !!this.unsubscribeCounter
    return this.busy
  }

  refresh(): void {
    this.changes.set(this)
  }

  async cancelOperation(): Promise<void> {
    const operation = this.active
    if (!operation || this.disposed || !this.client.available) return
    try {
      const accepted = await operation.cancel()
      if (!this.disposed) this.writeLog('Cancellation requested', 'Loading', { accepted })
    } catch (error) {
      if (!this.disposed)
        this.writeLog('Cancel request', 'Error', error instanceof Error ? error.message : 'Cancel request failed.')
    }
  }

  async run(label: string, action: () => Promise<unknown>): Promise<void> {
    if (this.busy || this.disposed || this.connecting || !this.client.available) return
    this.busy = true
    this.activeLabel = label
    const version = this.selectionVersion
    this.showResult('Loading', `${label}…`)
    try {
      const value = await action()
      if (this.disposed) return
      this.writeLog(label, 'Success', value)
      if (version === this.selectionVersion) this.showResult('Success', value)
    } catch (error) {
      if (this.disposed) return
      const code = error instanceof NativeError ? error.code : 'input_error'
      const value = { code, message: error instanceof Error ? error.message : 'The operation failed.' }
      const status = code === 'cancelled' ? 'Cancelled' : 'Error'
      this.writeLog(label, status, value)
      if (version === this.selectionVersion) this.showResult(status, value)
    } finally {
      this.busy = false
      this.activeLabel = ''
      this.active = undefined
      if (!this.disposed) {
        if (version !== this.selectionVersion && this.resultStatus === 'Loading')
          this.showResult('Ready to run', 'The previous operation finished. Run this experiment to inspect its result.')
        this.refresh()
      }
    }
  }

  dispose(): void {
    if (this.disposed) return
    this.disposed = true
    this.client.dispose()
    this.active = undefined
    this.unsubscribeCounter = undefined
    this.unsubscribeProgress = undefined
    this.childWindows.clear()
    this.refresh()
  }
}
