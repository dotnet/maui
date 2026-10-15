import { connect, enumValue, text } from '../core.js'
import type { AppTheme } from '../application.js'

/** Native application packaging model. */
export type AppPackagingModel = 'packaged' | 'unpackaged'
/** Native application layout direction. */
export type LayoutDirection = 'unknown' | 'leftToRight' | 'rightToLeft'
/** Convenience composition of native application properties; not an atomic native snapshot. */
export interface AppInfo {
  /** Native display name. */
  appName: string
  /** Native version string. */
  version: string
  /** Native build identifier. */
  build: string
}
/** Reads IAppInfo.PackageName. @returns Native package identifier. */
export async function getPackageName(): Promise<string> { return (await connect()).invoke('appInfo.packageName', {}, text) }
/** Reads IAppInfo.Name. @returns Native display name, not the HTML document title. */
export async function getName(): Promise<string> { return (await connect()).invoke('appInfo.name', {}, text) }
/** Reads IAppInfo.VersionString. @returns Native version string. */
export async function getVersionString(): Promise<string> { return (await connect()).invoke('appInfo.versionString', {}, text) }
/** Reads IAppInfo.Version. @returns Native System.Version in its standard string representation. */
export async function getVersion(): Promise<string> { return (await connect()).invoke('appInfo.version', {}, text) }
/** Reads IAppInfo.BuildString. @returns Native build identifier. */
export async function getBuildString(): Promise<string> { return (await connect()).invoke('appInfo.buildString', {}, text) }
/** Reads the OS-requested app theme, without Application.UserAppTheme overrides. @returns Native theme. */
export async function getRequestedTheme(): Promise<AppTheme> {
  return (await connect()).invoke('appInfo.requestedTheme', {}, value => enumValue(value, ['unspecified', 'light', 'dark']))
}
/** Reads native packaging. @returns Native packaged/unpackaged model. */
export async function getPackagingModel(): Promise<AppPackagingModel> {
  return (await connect()).invoke('appInfo.packagingModel', {}, value => enumValue(value, ['packaged', 'unpackaged']))
}
/** Reads the native requested layout direction. @returns Native layout direction. */
export async function getRequestedLayoutDirection(): Promise<LayoutDirection> {
  return (await connect()).invoke('appInfo.requestedLayoutDirection', {}, value => enumValue(value, ['unknown', 'leftToRight', 'rightToLeft']))
}
/**
 * Composes direct annotated IAppInfo property reads in JavaScript.
 * @returns Native name, version and build; no browser metadata fallback.
 * @throws `NativeError` for native errors or malformed transport results.
 * @remarks Uses appInfo.name, appInfo.versionString and appInfo.buildString; these are independent native reads.
 */
export async function getAppInfo(): Promise<AppInfo> {
  const [appName, version, build] = await Promise.all([getName(), getVersionString(), getBuildString()])
  return { appName, version, build }
}
