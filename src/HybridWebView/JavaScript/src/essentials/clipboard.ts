import { ack, connect, nullableText, object } from '../core.js'
import type { NativeClient } from '../core.js'

/** Native text clipboard operations; no browser clipboard fallback is provided. */
export interface ClipboardApi {
  /**
   * Replaces the native clipboard's text.
   * @param text - Text to copy, including Unicode; the reference host allows at most 4096 UTF-16 code units.
   * @returns The native acknowledgement; not a guarantee that another app can read the clipboard.
   * @throws `NativeError` for unavailable native, invalid arguments, OS/provider rejection or invalid output.
   * @remarks Requires the host's `clipboard.write` provider; native input validation and OS policy are authoritative.
   */
  writeText(text: string): Promise<boolean>
  /**
   * Reads text from the native clipboard.
   * @returns Native text, or null when no text is available.
   * @throws `NativeError` for unavailable native, OS/provider rejection or invalid output.
   * @remarks Requires the host's `clipboard.read` provider. Reading may be restricted by OS policy.
   */
  readText(): Promise<string | null>
}

/**
 * Creates native clipboard helpers without making a native call.
 * @param client - Exact connected client to use; omitted to lazily connect the shared document client on each operation.
 * @returns Text clipboard operations with runtime-validated results.
 * @remarks
 * Requires a compatible protocol-1 host with explicitly registered clipboard providers.
 * An explicit client is neither connected, attached nor disposed by the helper.
 * Shared-client operations use the current document session and are not replayed after disconnect.
 * @example
 * ```ts
 * import { clipboard } from '@maui/api/essentials/clipboard'
 * await clipboard().writeText('Hello — 你好 👋')
 * const text = await clipboard().readText()
 * ```
 */
export function clipboard(client?: NativeClient): ClipboardApi {
  return {
    writeText: async (text: string) => (client ?? await connect()).invoke('clipboard.write', { text }, ack),
    readText: async () => (client ?? await connect()).invoke('clipboard.read', {}, value => nullableText(object(value).text)),
  }
}
