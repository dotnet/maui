import { connect, enumValue, NativeError, number, object } from '../core.js'

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
 * @remarks Requires `display.info`. It neither subscribes to sensors nor changes keep-screen-awake preferences.
 */
export async function getDisplayInfo(): Promise<DisplayInfo> {
  return (await connect()).invoke('display.info', {}, value => {
    const v = object(value), width = number(v.width), height = number(v.height), density = number(v.density), refreshRate = number(v.refreshRate)
    if (width <= 0 || height <= 0 || density <= 0 || refreshRate < 0) throw new NativeError('invalid_response', 'Invalid native display metrics.')
    return { width, height, density, orientation: enumValue(v.orientation, ['unknown', 'portrait', 'landscape']),
      rotation: enumValue(v.rotation, ['unknown', 'rotation0', 'rotation90', 'rotation180', 'rotation270']), refreshRate }
  })
}
