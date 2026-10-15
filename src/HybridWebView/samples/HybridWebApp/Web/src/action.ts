import type { PlaygroundSession } from './session.ts'
import type { MenuDefinition } from '@maui/api/ui/menus'

/** Sample presentation only; SDK business handlers remain ordinary async functions. */
export function presentAction(session: PlaygroundSession, label: string, action: () => unknown | Promise<unknown>): Promise<void> {
  return session.run(label, async () => action())
}

/** Native callback presentation belongs outside copyable TypeScript menu recipes. */
export function presentMenuActions(session: PlaygroundSession, items: MenuDefinition[]): MenuDefinition[] {
  return items.map(item => ({
    ...item,
    items: item.items ? presentMenuActions(session, item.items) : undefined,
    action: item.action ? checked => presentAction(session, item.text, () => item.action!(checked)) : undefined,
  }))
}
