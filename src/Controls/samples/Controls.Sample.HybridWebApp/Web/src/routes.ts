import { guidePages, navigationPages } from './catalog.ts'
import type { Feature } from './catalog.ts'

/** Internal frontend routing only; never an OS activation or native authorization boundary. */
export interface Route {
  page: Feature
  error?: string
}

export function readRoute(fragment: string): Route {
  if (!fragment) return { page: guidePages[0]! }
  if (!/^#\/[a-z0-9-]{1,64}$/.test(fragment))
    return { page: guidePages[0]!, error: 'This page address is malformed. Choose a page from navigation to recover.' }
  const page = navigationPages.find(page => page.id === fragment.slice(2))
  return page ? { page } : { page: guidePages[0]!, error: 'This page is not implemented. Choose a page from navigation to recover.' }
}
