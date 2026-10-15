import { boolean, connect, enumValue, number, object, text } from '../core.js'
import type { Json, Operation } from '../core.js'

/** Native accuracy policy, not a guarantee of the resulting coordinates. */
export type GeolocationAccuracy = 'default' | 'lowest' | 'low' | 'medium' | 'high' | 'best'
/** Exact native altitude-reference member names in camelCase, never numeric ordinals. */
export type AltitudeReferenceSystem = 'unspecified' | 'terrain' | 'ellipsoid' | 'geoid' | 'surface'
/** Actual MAUI GeolocationRequest fields. */
export interface GeolocationRequest {
  /** Native accuracy requested from the OS. */
  desiredAccuracy?: GeolocationAccuracy
  /** Native TimeSpan in constant format; omitted uses the native request default. */
  timeout?: string
  /** Explicit temporary precise-location permission request; defaults to false. */
  requestFullAccuracy?: boolean
}
/** Native Location fields; never fabricated by a web fallback. */
export interface Location {
  /** Native latitude in degrees. */
  latitude: number
  /** Native longitude in degrees. */
  longitude: number
  /** Native timestamp. */
  timestamp: string
  /** Native altitude, if available. */
  altitude: number | null
  /** Native horizontal accuracy in meters. */
  accuracy: number | null
  /** Native vertical accuracy in meters. */
  verticalAccuracy: number | null
  /** Whether the OS reduced accuracy. */
  reducedAccuracy: boolean
  /** Native speed, if available. */
  speed: number | null
  /** Native course, if available. */
  course: number | null
  /** Whether the OS reports a mock provider. */
  isFromMockProvider: boolean
  /** Native altitude reference system, using its exact camelCase member name. */
  altitudeReferenceSystem: AltitudeReferenceSystem
}
function location(value: unknown): Location | null {
  if (value === null) return null
  const v = object(value)
  const optional = (value: unknown) => value === null ? null : number(value)
  const result: Location = {
    latitude: number(v.latitude), longitude: number(v.longitude), timestamp: text(v.timestamp),
    altitude: optional(v.altitude), accuracy: optional(v.accuracy), verticalAccuracy: optional(v.verticalAccuracy),
    reducedAccuracy: boolean(v.reducedAccuracy), speed: optional(v.speed), course: optional(v.course),
    isFromMockProvider: boolean(v.isFromMockProvider),
    altitudeReferenceSystem: enumValue(v.altitudeReferenceSystem, ['unspecified', 'terrain', 'ellipsoid', 'geoid', 'surface']),
  }
  return result
}
/**
 * Starts a cancellable native location request; OS settings and permissions remain authoritative.
 * @param request - Actual MAUI request fields; omitted uses a default native request.
 * @returns A native operation; null means the OS could not determine a location.
 * @throws `NativeError` for permission denial, missing configuration or unsupported native APIs.
 */
export async function getLocation(request: GeolocationRequest = {}): Promise<Operation<Location | null>> {
  return (await connect()).start('geolocation.location', { request: request as unknown as Json }, location)
}
