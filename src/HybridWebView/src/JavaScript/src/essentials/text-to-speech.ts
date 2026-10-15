import { connect, NativeError, object, text } from '../core.js'
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
  /** Native locale; constructor nulls normalize to empty strings. Omit to use the default voice. */
  locale?: { [K in keyof SpeechLocale]: SpeechLocale[K] | null } | null
  /** Native pitch; the destination validates its supported range. */
  pitch?: number | null
  /** Native volume; the destination validates its supported range. */
  volume?: number | null
  /** Native speech rate; the destination validates its supported range. */
  rate?: number | null
}

/**
 * Queries the real native speech engine's available locales.
 * @returns Engine locales, possibly an empty list when no voices are installed.
 * @throws `NativeError` for missing providers, native unsupported/errors or malformed output.
 * @remarks Requires `speech.locales`. It never speaks or substitutes browser voices.
 */
export async function getLocales(): Promise<SpeechLocale[]> {
  return (await connect()).invoke('speech.locales', {}, value => {
    if (!Array.isArray(value)) throw new NativeError('invalid_response', 'Expected native speech locales.')
    return value.map(value => {
      const v = object(value)
      return { id: text(v.id), language: text(v.language), country: text(v.country), name: text(v.name) }
    })
  })
}

/**
 * Starts native speech with an operation-specific cooperative cancellation handle.
 * @param value - Text passed unchanged to native SpeakAsync.
 * @param options - Native locale, pitch, volume and rate; omitted fields retain OS defaults.
 * @returns A core Operation whose result completes when native speech finishes.
 * @throws `NativeError` for invalid inputs, missing providers or native unsupported/errors.
 * @remarks Call only from a user action. Requires `speech.speak`, registered as a
 * bounded interactive operation. Await operation.result for completion or call
 * operation.cancel to pass cancellation to this native SpeakAsync invocation.
 * Cancellation never cancels unrelated feature operations and is not a global
 * stop API or proof of OS audio undo. Disconnect cancels document-owned speech.
 */
export async function speak(value: string, options?: SpeechOptions | null): Promise<Operation<void>> {
  if (typeof value !== 'string') throw new NativeError('invalid_arguments', 'Speech text must be a string.')
  if (options !== undefined && options !== null && (typeof options !== 'object' || Array.isArray(options)))
    throw new NativeError('invalid_arguments', 'Use a native speech options object.')
  if (options && Object.keys(options).some(key => !['locale', 'pitch', 'volume', 'rate'].includes(key)))
    throw new NativeError('invalid_arguments', 'Use native locale, pitch, volume and rate options.')
  const nativeOptions: Record<string, Json> | null = options == null ? null : {}
  for (const key of ['pitch', 'volume', 'rate'] as const) {
    const number = options?.[key]
    if (number !== undefined && number !== null && (typeof number !== 'number' || !Number.isFinite(number) || !Number.isFinite(Math.fround(number))))
      throw new NativeError('invalid_arguments', `Speech ${key} must be a finite native float or null.`)
    if (number !== undefined) nativeOptions![key] = number
  }
  if (options?.locale !== undefined) {
    const locale = options.locale
    if (locale !== null && (typeof locale !== 'object' || Array.isArray(locale) ||
      Object.keys(locale).some(key => !['id', 'language', 'country', 'name'].includes(key)) ||
      [locale.id, locale.language, locale.country, locale.name].some(value => value !== null && typeof value !== 'string')))
      throw new NativeError('invalid_arguments', 'Use native speech locale strings.')
    nativeOptions!.locale = locale === null ? null : { id: locale.id, language: locale.language, country: locale.country, name: locale.name }
  }
  const client = await connect()
  return client.start('speech.speak', { text: value, options: nativeOptions }, value => {
    if (value !== null) throw new NativeError('invalid_response', 'Expected native void completion.')
  })
}
