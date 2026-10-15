import { connect, enumValue, NativeError, number, object } from '../core.js'

/** Native MAUI battery state names. */
export type BatteryState = 'unknown' | 'charging' | 'discharging' | 'full' | 'notCharging' | 'notPresent'
/** Native MAUI power source names; AC is encoded as ac. */
export type BatteryPowerSource = 'unknown' | 'battery' | 'ac' | 'usb' | 'wireless'
/** Native MAUI energy-saver state names. */
export type EnergySaverStatus = 'unknown' | 'on' | 'off'

/** Native read-only power snapshot; unsupported APIs reject rather than emulate battery values. */
export interface BatteryInfo {
  /** Charge fraction 0–1, or -1 when the OS reports no battery. */
  chargeLevel: number
  /** Native charging state, including platform-reported unknown. */
  state: BatteryState
  /** Native power source. */
  powerSource: BatteryPowerSource
  /** Native energy-saver status. */
  energySaverStatus: EnergySaverStatus
}

/**
 * Reads a fresh native battery/power snapshot.
 * @returns Native power data; -1 means no reported battery.
 * @throws `NativeError` for unsupported platforms, missing providers or malformed results.
 * @remarks Requires `battery.info`. No subscriptions, browser battery API or mock values are used.
 */
export async function getBatteryInfo(): Promise<BatteryInfo> {
  return (await connect()).invoke('battery.info', {}, value => {
    const v = object(value), chargeLevel = number(v.chargeLevel)
    if (chargeLevel !== -1 && (chargeLevel < 0 || chargeLevel > 1)) throw new NativeError('invalid_response', 'Invalid battery charge.')
    return { chargeLevel,
      state: enumValue(v.state, ['unknown', 'charging', 'discharging', 'full', 'notCharging', 'notPresent']),
      powerSource: enumValue(v.powerSource, ['unknown', 'battery', 'ac', 'usb', 'wireless']),
      energySaverStatus: enumValue(v.energySaverStatus, ['unknown', 'on', 'off']) }
  })
}
