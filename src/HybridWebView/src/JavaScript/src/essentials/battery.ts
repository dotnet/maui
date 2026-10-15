import { connect, enumValue, number, object, subscribeShared } from '../core.js'

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
 * @remarks Composes independent battery.chargeLevel/state/powerSource/energySaverStatus native property reads.
 */
export async function getBatteryInfo(): Promise<BatteryInfo> {
  const [chargeLevel, state, powerSource, energySaverStatus] = await Promise.all([
    getChargeLevel(), getState(), getPowerSource(), getEnergySaverStatus()])
  return { chargeLevel, state, powerSource, energySaverStatus }
}
const state = (value: unknown): BatteryState => enumValue(value, ['unknown', 'charging', 'discharging', 'full', 'notCharging', 'notPresent'])
const powerSource = (value: unknown): BatteryPowerSource => enumValue(value, ['unknown', 'battery', 'ac', 'usb', 'wireless'])
const energySaverStatus = (value: unknown): EnergySaverStatus => enumValue(value, ['unknown', 'on', 'off'])
/** Reads IBattery.ChargeLevel without scaling. @returns Actual native charge level. */
export async function getChargeLevel(): Promise<number> { return (await connect()).invoke('battery.chargeLevel', {}, number) }
/** Reads IBattery.State. @returns Native battery state. */
export async function getState(): Promise<BatteryState> { return (await connect()).invoke('battery.state', {}, state) }
/** Reads IBattery.PowerSource. @returns Native power source. */
export async function getPowerSource(): Promise<BatteryPowerSource> { return (await connect()).invoke('battery.powerSource', {}, powerSource) }
/** Reads IBattery.EnergySaverStatus. @returns Native energy saver status. */
export async function getEnergySaverStatus(): Promise<EnergySaverStatus> { return (await connect()).invoke('battery.energySaverStatus', {}, energySaverStatus) }
/** Native BatteryInfoChangedEventArgs, not a second native snapshot. */
export type BatteryInfoChanged = Pick<BatteryInfo, 'chargeLevel' | 'state' | 'powerSource'>
/**
 * Observes actual IBattery.BatteryInfoChanged events without an initial read.
 * @param listener - Receives each native event.
 * @returns Idempotent unsubscribe; the last listener detaches the native source.
 */
export async function onBatteryInfoChanged(listener: (value: BatteryInfoChanged) => void): Promise<() => Promise<void>> {
  return subscribeShared(await connect(), 'battery.infoChanged', value => {
    const v = object(value)
    return { chargeLevel: number(v.chargeLevel), state: state(v.state), powerSource: powerSource(v.powerSource) }
  }, listener)
}
/**
 * Observes actual native energy saver changes.
 * @param listener - Receives native energy saver state.
 * @returns Idempotent unsubscribe.
 */
export async function onEnergySaverStatusChanged(listener: (value: EnergySaverStatus) => void): Promise<() => Promise<void>> {
  return subscribeShared(await connect(), 'battery.energySaverStatusChanged', value => energySaverStatus(object(value).energySaverStatus), listener)
}
