import { Menu } from '@maui/api/ui/menus'
import type { MenuDefinition } from '@maui/api/ui/menus'
import * as app from '@maui/api/application'
import { currentWindow } from '@maui/api/ui/windows'
import { alert } from '@maui/api/ui/dialogs'
import type { PlaygroundSession } from '../session.ts'
import { presentMenuActions } from '../action.ts'

/** All sample recipes/callbacks are TypeScript; native receives only descriptions. */
export async function installPlaygroundMenu(session: PlaygroundSession, requireSupport = false): Promise<void> {
  const own = currentWindow()
  session.menuWindow = own
  const capabilities = await own.menuCapabilities()
  session.menuSupportsChecked = capabilities.supportsChecked
  if (!capabilities.supported) {
    if (requireSupport) throw new Error('Native menu bars are unsupported on this mobile host.')
    return
  }
  const items: MenuDefinition[] = [
    { id: 'file', text: 'File', kind: 'submenu', items: [
      { id: 'new-window', text: 'New playground window', kind: 'item', shortcut: { key: 'N', modifiers: ['primary'] },
        action: async () => { await app.openWindow({ url: `index.html#/${session.selected.id}` }) } },
      { id: 'close-window', text: 'Close this window', kind: 'item', shortcut: { key: 'W', modifiers: ['primary'] },
        action: async () => { await own.close() } },
    ] },
    { id: 'navigate', text: 'Navigate', kind: 'submenu', items: [
      { id: 'showcase', text: 'Try it hands-on', kind: 'item', action: () => session.navigateNative('showcase') },
      { id: 'clipboard', text: 'Text clipboard', kind: 'item', action: () => session.navigateNative('clipboard') },
    ] },
    { id: 'view', text: 'View', kind: 'submenu', items: [
      { id: 'reload-frontend', text: 'Reload frontend', kind: 'item', shortcut: { key: 'R', modifiers: ['primary', 'shift'] }, action: () => session.reloadFrontend?.() },
      { id: 'reload-browser', text: 'Reload browser guest', kind: 'item', action: async () => {
        if (!session.browser) throw new Error('Open the browser pane first.')
        await session.browser.reload()
      } },
      { id: 'view-separator', text: '', kind: 'separator' },
      ...(capabilities.supportsChecked ? [{
        id: 'check-example', text: 'Checked menu example', kind: 'check' as const, checked: true,
        action: (checked: boolean) => session.showResult('Success', { nativeMenuChecked: checked }),
      }] : []),
    ] },
    { id: 'help', text: 'Help', kind: 'submenu', items: [
      { id: 'about', text: 'About this playground', kind: 'item', action: async () => {
        await alert(`Native window ${await own.id()}\n${await own.title()}\nMenus and actions are authored in TypeScript.`,
          { title: '.NET MAUI Playground', cancel: 'Close' })
      } },
    ] },
  ]
  const menu = Menu.create(presentMenuActions(session, items), error => {
    if (!session.disposed) session.showResult('Error', error instanceof Error ? error.message : 'Menu callback failed.')
  })
  session.menu = await own.setMenu(menu) ?? undefined
}
