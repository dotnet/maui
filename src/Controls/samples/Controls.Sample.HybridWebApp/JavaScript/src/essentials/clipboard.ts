import { ack, boolean, connect, nullableText, NativeError, subscribeShared } from '../core.js'
/**
 * Replaces native clipboard text using the shared document connection.
 * @param text - Native Unicode text; host text policy/OS permission remains authoritative.
 * @returns Completion of the native call, not a browser clipboard emulation.
 * @throws `NativeError` on invalid input, unavailable native or OS failure.
 */
export async function setText(text: string): Promise<void> {
  return (await connect()).invoke('clipboard.write', { text }, value => {
    if (value !== null) throw new NativeError('invalid_response', 'Expected native void completion.')
  })
}
/** Reads actual native clipboard text. @returns Native text or null, never web fallback. */
export async function getText(): Promise<string | null> {
  return (await connect()).invoke('clipboard.read', {}, nullableText)
}
/** Reads actual native text availability. @returns Native HasText; OS policy remains authoritative. */
export async function hasText(): Promise<boolean> {
  return (await connect()).invoke('clipboard.hasText', {}, boolean)
}
/**
 * Observes actual native clipboard content changes without automatically reading private text.
 * @param listener - Runs on native change; call getText explicitly when wanted.
 * @returns Unsubscribe; multiple listeners share one native handler.
 */
export async function onContentChanged(listener: () => void): Promise<() => Promise<void>> {
  return subscribeShared(await connect(), 'clipboard.changed', ack, () => listener())
}
