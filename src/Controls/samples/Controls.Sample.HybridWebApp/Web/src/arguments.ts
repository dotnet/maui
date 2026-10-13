import { boolean, nullableText, number, object, strings } from '@maui/api/core'
import type { EchoInput } from './native/sample.ts'

/** Validates authored form input, not a native response. */
export function parseEcho(input: string): EchoInput {
  try {
    const value = object(JSON.parse(input))
    return {
      text: nullableText(value.text), number: number(value.number),
      flag: boolean(value.flag), items: strings(value.items),
    }
  } catch (error) {
    throw new Error(error instanceof Error ? error.message : 'Invalid command arguments.')
  }
}
