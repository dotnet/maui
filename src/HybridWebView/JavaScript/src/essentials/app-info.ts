import { connect, object, text } from '../core.js'

/** Native application metadata, matching the supported subset of MAUI Essentials AppInfo. */
export interface AppInfo {
  /** Display name reported by the native application, not the HTML document title. */
  appName: string
  /** Native application version string, not the npm SDK version. */
  version: string
  /** Native application build identifier; its format is platform-specific. */
  build: string
}

/**
 * Reads the native application's name, version and build.
 * @returns Validated application metadata from the native host.
 * @throws `NativeError` when native is unavailable, the host rejects the call, or the result is invalid.
 * @remarks
 * Requires a compatible protocol-1 host with the `app.info` provider registered.
 * The independently registered command returns only AppInfo fields;
 * device and process diagnostics are not returned. No browser/package metadata fallback exists.
 * The document's shared connection owns startup and disposal. Shared-client
 * calls follow the document session lifetime; no automatic operation replay occurs.
 * @example
 * ```ts
 * import { getAppInfo } from '@maui/api/essentials/app-info'
 * const app = await getAppInfo()
 * console.log(app.appName, app.version, app.build)
 * ```
 */
export async function getAppInfo(): Promise<AppInfo> {
  return (await connect()).invoke('app.info', {}, value => {
    const v = object(value)
    return { appName: text(v.appName), version: text(v.version), build: text(v.build) }
  })
}
