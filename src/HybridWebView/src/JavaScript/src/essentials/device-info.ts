import { connect, enumValue, text } from '../core.js'

/** Native device classification. */
export type DeviceType = 'unknown' | 'physical' | 'virtual'
/** Convenience composition of native device properties; not an atomic native snapshot. */
export interface DeviceInfo {
  /** Exact native platform identifier, including custom platforms. */
  platform: string
  /** Native OS version string. */
  osVersion: string
  /** Native device model. */
  model: string
}
/** Reads the native model. @returns Native model string. */
export async function getModel(): Promise<string> { return (await connect()).invoke('deviceInfo.model', {}, text) }
/** Reads the native manufacturer. @returns Native manufacturer string. */
export async function getManufacturer(): Promise<string> { return (await connect()).invoke('deviceInfo.manufacturer', {}, text) }
/** Reads the native device name. @returns Native device name. */
export async function getName(): Promise<string> { return (await connect()).invoke('deviceInfo.name', {}, text) }
/** Reads the native OS version string. @returns Native version string. */
export async function getVersionString(): Promise<string> { return (await connect()).invoke('deviceInfo.versionString', {}, text) }
/** Reads the native Version. @returns System.Version in its standard string representation. */
export async function getVersion(): Promise<string> { return (await connect()).invoke('deviceInfo.version', {}, text) }
/** Reads the native platform. @returns Exact DevicePlatform identifier, not a closed enum or user-agent value. */
export async function getPlatform(): Promise<string> { return (await connect()).invoke('deviceInfo.platform', {}, text) }
/** Reads the native idiom. @returns Exact DeviceIdiom identifier, including custom idioms. */
export async function getIdiom(): Promise<string> { return (await connect()).invoke('deviceInfo.idiom', {}, text) }
/** Reads the native device classification. @returns Native physical/virtual/unknown state. */
export async function getDeviceType(): Promise<DeviceType> {
  return (await connect()).invoke('deviceInfo.deviceType', {}, value => enumValue(value, ['unknown', 'physical', 'virtual']))
}
/**
 * Composes direct annotated IDeviceInfo property reads in JavaScript.
 * @returns Native platform, OS version and model without browser emulation.
 * @throws `NativeError` for native errors or malformed transport results.
 * @remarks Uses deviceInfo.platform, deviceInfo.versionString and deviceInfo.model independently.
 */
export async function getDeviceInfo(): Promise<DeviceInfo> {
  const [platform, osVersion, model] = await Promise.all([getPlatform(), getVersionString(), getModel()])
  return { platform, osVersion, model }
}
