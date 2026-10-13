import { connect, NativeError, number, object, text } from '../core.js'

/** Native read-only power snapshot; unsupported APIs reject rather than emulate battery values. */
export interface BatteryInfo {
  /** Charge fraction 0–1, or -1 when the OS reports no battery. */
  chargeLevel: number
  /** Native charging state, including platform-reported Unknown. */
  state: string
  /** Native power source. */
  powerSource: string
  /** Native energy-saver status. */
  energySaverStatus: string
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
    return { chargeLevel, state: text(v.state), powerSource: text(v.powerSource), energySaverStatus: text(v.energySaverStatus) }
  })
}
