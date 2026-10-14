import { connect, object, strings, subscribeShared, text } from '../core.js'

/** Native network conditions; these are not an internet reachability test. */
export interface ConnectivityInfo {
  /** OS-reported access: Unknown, None, Local, ConstrainedInternet or Internet. */
  networkAccess: string
  /** Active native profiles, such as WiFi, Cellular, Ethernet or Bluetooth. */
  connectionProfiles: string[]
}

function decode(value: unknown): ConnectivityInfo {
  const v = object(value)
  return { networkAccess: text(v.networkAccess), connectionProfiles: strings(v.connectionProfiles) }
}

/**
 * Reads fresh network conditions from the native Connectivity interface.
 * @returns OS-reported access and active connection profiles.
 * @throws `NativeError` for missing providers, unsupported native APIs or permission failures.
 * @remarks Requires `connectivity.info`. Even Internet does not guarantee that a
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
