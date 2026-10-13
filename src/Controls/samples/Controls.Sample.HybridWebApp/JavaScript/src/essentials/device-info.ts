import { connect, object, text } from '../core.js'

/** Supported native device metadata; this preview does not expose every MAUI Essentials DeviceInfo property. */
export interface DeviceInfo {
  /** Native platform name as reported by the host; not a fixed JavaScript enum or user-agent value. */
  platform: string
  /** Native operating-system version string. */
  osVersion: string
  /** Device model reported by the OS; availability and representation are platform-specific. */
  model: string
}

/**
 * Reads the native device platform, operating-system version and model.
 * @returns Validated device metadata from the native host.
 * @throws `NativeError` when native is unavailable, the host rejects the call, or the result is invalid.
 * @remarks
 * Requires a compatible protocol-1 host with the `device.info` provider registered.
 * The independently registered command returns only DeviceInfo fields;
 * process architecture, PID and startup diagnostics are deliberately excluded.
 * No user-agent or browser emulation exists. The shared connection owns startup
 * and disposal; calls follow this document's native session lifetime.
 * @example
 * ```ts
 * import { getDeviceInfo } from '@maui/api/essentials/device-info'
 * const device = await getDeviceInfo()
 * console.log(device.platform, device.osVersion, device.model)
 * ```
 */
export async function getDeviceInfo(): Promise<DeviceInfo> {
  return (await connect()).invoke('device.info', {}, value => {
    const v = object(value)
    return { platform: text(v.platform), osVersion: text(v.osVersion), model: text(v.model) }
  })
}
