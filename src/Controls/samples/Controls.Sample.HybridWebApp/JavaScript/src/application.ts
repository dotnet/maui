import { connect, enumValue, NativeError, object, subscribeShared } from './core.js'
import { Window } from './ui/windows.js'
import type { WindowOptions } from './ui/windows.js'
import { acquiredWindows, windowIdentity } from './internal/window-ownership.js'

/** Actual native effective application theme; not a per-window/remote website setting. */
export type AppTheme = 'unspecified' | 'light' | 'dark'
/** Native Application.UserAppTheme preference; unspecified restores OS following. */
export type AppThemePreference = AppTheme
function decodeTheme(value: unknown): AppTheme {
  const v = object(value)
  enumValue(v.preference, ['unspecified', 'light', 'dark'])
  return enumValue(v.effective, ['unspecified', 'light', 'dark'])
}
/**
 * Enumerates caller-authorized app-owned window resources.
 * @returns Real typed handles, not information snapshots or global cross-window hub authority.
 */
export async function windows(): Promise<Window[]> {
  const client = await connect()
  const listed = await client.invoke('window.list', {}, value => {
    const entries = object(value).windows
    if (!Array.isArray(entries)) throw new NativeError('invalid_response', 'Native windows must be a bounded app-owned list.')
    return entries.map(windowIdentity)
  })
  return listed.map(owner => {
    const handle = new Window(owner.id)
    acquiredWindows.set(handle, { client, owner })
    return handle
  })
}
/**
 * Looks up an app-owned native window by immutable developer ID.
 * @param id - Native registry identity, not a title, route or private lease token.
 * @returns Caller-authorized resource, or null if closed/unavailable.
 */
export async function window(id: string): Promise<Window | null> {
  const handle = new Window(id)
  try { await handle.id(); return handle }
  catch (error) { if (error instanceof NativeError && error.code === 'invalid_window') return null; throw error }
}
/**
 * Opens an isolated trusted-bundle native window with frontend-owned location/options.
 * @param options - Optional developer ID, bundled URL, native title/desktop dimensions.
 * @returns Native resource retaining its private generation binding; duplicate IDs reject.
 * @remarks Remote URLs never receive the trusted bridge through this API.
 */
export async function openWindow(options: WindowOptions = {}): Promise<Window> {
  const { url = 'index.html', ...native } = options
  if (url.length < 1 || url.length > 4096 || /^[a-z][a-z0-9+.-]*:/i.test(url) || url.startsWith('//') || url.includes('\\'))
    throw new NativeError('invalid_location', 'Use a trusted bundle-relative frontend URL, never a remote/special-scheme destination.')
  const client = await connect()
  const owner = await client.invoke('window.open', { url, options: native }, windowIdentity)
  const handle = new Window(owner.id)
  acquiredWindows.set(handle, { client, owner })
  return handle
}
/** Reads actual Application.RequestedTheme. @returns Native unspecified/light/dark state without inventing a fallback. */
export async function getTheme(): Promise<AppTheme> { return (await connect()).invoke('application.theme', {}, decodeTheme) }
/**
 * Sets application-level native theme preference.
 * @param preference - Unspecified/light/dark; unspecified restores OS following.
 * @returns Native setting acceptance; web CSS updates through actual theme events.
 */
export async function setTheme(preference: AppThemePreference): Promise<void> {
  enumValue(preference, ['unspecified', 'light', 'dark'], 'invalid_arguments')
  await (await connect()).invoke('application.setTheme', { theme: preference }, decodeTheme)
}
/**
 * Observes actual native effective theme changes, including OS-originated changes.
 * @param listener - Receives native unspecified/light/dark state.
 * @returns Idempotent unsubscribe; multiple listeners share one native handler.
 */
export async function onThemeChanged(listener: (theme: AppTheme) => void): Promise<() => Promise<void>> {
  return subscribeShared(await connect(), 'application.themeChanged', decodeTheme, listener)
}
