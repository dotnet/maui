import { boolean, connect } from '../core.js'

/**
 * Queries the native ILauncher URI capability.
 * @param uri - Native URI, including supported custom schemes; the destination validates it.
 * @returns Actual native boolean, not a reachability guarantee.
 */
export async function canOpen(uri: string): Promise<boolean> {
  return (await connect()).invoke('launcher.canOpen', { uri }, boolean)
}
/**
 * Opens a URI through the native system launcher, not a remote guest WebView.
 * @param uri - Native URI, including custom schemes. Native platform policy is authoritative.
 * @returns Actual ILauncher.OpenAsync boolean, including false refusal.
 * @throws `NativeError` for native errors, unsupported APIs or malformed transport.
 * @remarks Call from an explicit user action. OpenFileRequest overloads and arbitrary file handles are not exported.
 */
export async function openUrl(uri: string): Promise<boolean> {
  return (await connect()).invoke('launcher.open', { uri }, boolean)
}
/**
 * Calls native ILauncher.TryOpenAsync, without retrying or substituting OpenAsync.
 * @param uri - Actual native URI.
 * @returns Native success/refusal boolean.
 */
export async function tryOpen(uri: string): Promise<boolean> {
  return (await connect()).invoke('launcher.tryOpen', { uri }, boolean)
}
