import { ack, connect, NativeError, object, text } from '../core.js'
import type { Json, Operation } from '../core.js'

/** An actual native engine locale, with platform-specific values preserved verbatim. */
export interface SpeechLocale {
  /** Engine-specific voice identifier; empty when the engine does not supply one. */
  id: string
  /** Native language code or name, not necessarily the same format on every OS. */
  language: string
  /** Native country code or name; may be empty. */
  country: string
  /** Native display name; may be empty. */
  name: string
}

/** Existing MAUI SpeechOptions; omitted fields retain native engine defaults. */
export interface SpeechOptions {
  /** Locale returned by getLocales; omitted to use the native default voice. */
  locale?: SpeechLocale
  /** Native pitch from zero through two. */
  pitch?: number
  /** Native volume from zero through one. */
  volume?: number
  /** Native speech rate from 0.1 through two; one is normal speed. */
  rate?: number
}

/**
 * Queries the real native speech engine's available locales.
 * @returns Engine locales, possibly an empty list when no voices are installed.
 * @throws `NativeError` for missing providers, native unsupported/errors or malformed output.
 * @remarks Requires `speech.locales`. It never speaks or substitutes browser voices.
 */
export async function getLocales(): Promise<SpeechLocale[]> {
  return (await connect()).invoke('speech.locales', {}, value => {
    const locales = object(value).locales
    if (!Array.isArray(locales)) throw new NativeError('invalid_response', 'Expected native speech locales.')
    return locales.map(value => {
      const v = object(value)
      return { id: text(v.id), language: text(v.language), country: text(v.country), name: text(v.name) }
    })
  })
}

/**
 * Starts native speech with an operation-specific cooperative cancellation handle.
 * @param value - Nonblank text within the host-advertised UTF-16 text budget.
 * @param options - Native locale, pitch, volume and rate; omitted fields retain OS defaults.
 * @returns A core Operation whose result completes when native speech finishes.
 * @throws `NativeError` for invalid inputs, missing providers or native unsupported/errors.
 * @remarks Call only from a user action. Requires `speech.speak`, registered as a
 * bounded interactive operation. Await operation.result for completion or call
 * operation.cancel to pass cancellation to this native SpeakAsync invocation.
 * Cancellation never cancels unrelated feature operations and is not a global
 * stop API or proof of OS audio undo. Disconnect cancels document-owned speech.
 */
export async function speak(value: string, options: SpeechOptions = {}): Promise<Operation<void>> {
  if (typeof value !== 'string' || !value.trim()) throw new NativeError('invalid_arguments', 'Speech requires nonblank text.')
  if (!options || typeof options !== 'object' || Array.isArray(options))
    throw new NativeError('invalid_arguments', 'Use a native speech options object.')
  if (Object.keys(options).some(key => !['locale', 'pitch', 'volume', 'rate'].includes(key)))
    throw new NativeError('invalid_arguments', 'Use native locale, pitch, volume and rate options.')
  const nativeOptions: Record<string, Json> = {}
  for (const [key, min, max] of [['pitch', 0, 2], ['volume', 0, 1], ['rate', 0.1, 2]] as const) {
    const number = options[key]
    if (number !== undefined && (!Number.isFinite(number) || number < min || number > max))
      throw new NativeError('invalid_arguments', `Speech ${key} must be between ${min} and ${max}.`)
    if (number !== undefined) nativeOptions[key] = number
  }
  if (options.locale !== undefined) {
    const locale = options.locale
    if (!locale || [locale.id, locale.language, locale.country, locale.name].some(value => typeof value !== 'string' || value.length > 512))
      throw new NativeError('invalid_arguments', 'Use a bounded native speech locale.')
    nativeOptions.locale = { id: locale.id, language: locale.language, country: locale.country, name: locale.name }
  }
  const client = await connect()
  if (value.length > client.readiness!.maximumTextLength)
    throw new NativeError('invalid_arguments', 'Speech text exceeds the host text budget.')
  return client.start('speech.speak', { text: value, options: nativeOptions }, value => {
    if (!ack(value)) throw new NativeError('invalid_response', 'Native speech did not complete.')
  })
}
