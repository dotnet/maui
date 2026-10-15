import { boolean, connect, enumValue, NativeError, number, object, subscribeShared } from '../core.js'

/** Native MAUI main-display orientation names. */
export type DisplayOrientation = 'unknown' | 'portrait' | 'landscape'
/** Native MAUI display rotation names, not numeric degrees. */
export type DisplayRotation = 'unknown' | 'rotation0' | 'rotation90' | 'rotation180' | 'rotation270'

/** Read-only main-display metrics from the native OS, not CSS viewport measurements. */
export interface DisplayInfo {
  /** Main-display pixel width. */
  width: number
  /** Main-display pixel height. */
  height: number
  /** Pixels per device-independent unit. */
  density: number
  /** Native display orientation. */
  orientation: DisplayOrientation
  /** Native display rotation. */
  rotation: DisplayRotation
  /** Native refresh rate in Hz; zero can mean unknown. */
  refreshRate: number
}

/**
 * Refreshes the native main-display snapshot.
 * @returns Validated native display dimensions, density and orientation.
 * @throws `NativeError` for unsupported platforms, missing providers or invalid results.
 * @remarks Reads annotated IDeviceDisplay.MainDisplayInfo, without a wrapper DTO.
 */
export async function getDisplayInfo(): Promise<DisplayInfo> {
  return (await connect()).invoke('display.mainDisplayInfo', {}, decode)
}
function decode(value: unknown): DisplayInfo {
  const v = object(value), width = number(v.width), height = number(v.height), density = number(v.density), refreshRate = number(v.refreshRate)
  return { width, height, density, orientation: enumValue(v.orientation, ['unknown', 'portrait', 'landscape']),
    rotation: enumValue(v.rotation, ['unknown', 'rotation0', 'rotation90', 'rotation180', 'rotation270']), refreshRate }
}
/** Reads IDeviceDisplay.KeepScreenOn. @returns Actual native keep-awake preference. */
export async function getKeepScreenOn(): Promise<boolean> { return (await connect()).invoke('display.keepScreenOn', {}, boolean) }
/**
 * Sets IDeviceDisplay.KeepScreenOn directly.
 * @param value - Actual native preference.
 * @returns Native void completion.
 */
export async function setKeepScreenOn(value: boolean): Promise<void> {
  await (await connect()).invoke('display.setKeepScreenOn', { value }, result => {
    if (result !== null) throw new NativeError('invalid_response', 'Expected native void completion.')
  })
}
/**
 * Observes actual native display changes.
 * @param listener - Receives the native event's DisplayInfo.
 * @returns Idempotent unsubscribe; no synthetic initial read.
 */
export async function onMainDisplayInfoChanged(listener: (value: DisplayInfo) => void): Promise<() => Promise<void>> {
  return subscribeShared(await connect(), 'display.mainDisplayInfoChanged', value => decode(object(value).displayInfo), listener)
}
