import { ack, connect, NativeError } from '../core.js'

/**
 * Opens a public HTTPS website using the actual native system URL launcher.
 * @param url - Absolute ASCII HTTPS URL, at most 2048 characters, without credentials or custom ports.
 * @returns Completion after the native system accepts the launch, not proof that the site loaded.
 * @throws `NativeError` for invalid destinations, native unsupported/errors or launch refusal.
 * @remarks Requires `launcher.openUrl`. Call only from an explicit user action.
 * Native validation rejects local/internal names and IP literals. This is an
 * address policy, not a DNS or redirect firewall. No files, custom schemes,
 * shell commands, browser window.open, in-app browser or native guest are used.
 */
export async function openUrl(url: string): Promise<void> {
  let parsed: URL
  try { parsed = new URL(url) } catch { throw new NativeError('invalid_url', 'Use an absolute public HTTPS website.') }
  const host = parsed.hostname.toLowerCase()
  if (typeof url !== 'string' || url.length < 1 || url.length > 2048 || /[\s\\]|[^\x21-\x7f]/.test(url) ||
    parsed.protocol !== 'https:' || parsed.username || parsed.password || parsed.port ||
    !host.includes('.') || host.endsWith('.') || /^\[|^[\d.]+$/.test(host) ||
    host.split('.').some(label => ['localhost', 'local', 'internal', 'test', 'invalid'].includes(label)))
    throw new NativeError('invalid_url', 'Use public HTTPS without credentials, local/IP addresses or custom ports.')
  await (await connect()).invoke('launcher.openUrl', { url }, value => {
    if (!ack(value)) throw new NativeError('launch_failed', 'The native system did not accept the URL.')
  })
}
