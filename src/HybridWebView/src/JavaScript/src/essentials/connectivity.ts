import { connect, enumValue, object, strings, subscribeShared } from '../core.js'

/** Native MAUI access levels; internet is not a reachability guarantee. */
export type NetworkAccess = 'unknown' | 'none' | 'local' | 'constrainedInternet' | 'internet'
/** Native MAUI connection profiles; WiFi is encoded as wiFi. */
export type ConnectionProfile = 'unknown' | 'bluetooth' | 'cellular' | 'ethernet' | 'wiFi'

/** Native network conditions; these are not an internet reachability test. */
export interface ConnectivityInfo {
  /** OS-reported access using exact native camelCase names. */
  networkAccess: NetworkAccess
  /** Active native profiles using exact native camelCase names. */
  connectionProfiles: ConnectionProfile[]
}

function decode(value: unknown): ConnectivityInfo {
  const v = object(value)
  return { networkAccess: enumValue(v.networkAccess, ['unknown', 'none', 'local', 'constrainedInternet', 'internet']),
    connectionProfiles: strings(v.connectionProfiles).map(profile => enumValue(profile, ['unknown', 'bluetooth', 'cellular', 'ethernet', 'wiFi'])) }
}

/**
 * Reads fresh network conditions from the native Connectivity interface.
 * @returns OS-reported access and active connection profiles.
 * @throws `NativeError` for missing providers, unsupported native APIs or permission failures.
 * @remarks Requires `connectivity.info`. Even internet does not guarantee that a
 * particular endpoint is reachable. No HTTP probe, navigator.onLine or browser fallback is used.
 */
export async function getConnectivity(): Promise<ConnectivityInfo> {
  return (await connect()).invoke('connectivity.info', {}, decode)
}

/**
 * Observes actual native network changes, without synthesizing an initial event.
 * @param listener - Receives each validated native network-change snapshot.
 * @returns Idempotent asynchronous unsubscribe; the last listener detaches the native source.
 * @throws `NativeError` when the host cannot subscribe to `connectivity.changed`.
 * @remarks Call getConnectivity for a fresh read. Listeners share the document's
 * topic subscription; unsubscribe removes only this callback. Disconnect retires
 * all document listeners. Native access is not proof of internet reachability.
 */
export async function onConnectivityChanged(listener: (value: ConnectivityInfo) => void): Promise<() => Promise<void>> {
  return subscribeShared(await connect(), 'connectivity.changed', decode, listener)
}
