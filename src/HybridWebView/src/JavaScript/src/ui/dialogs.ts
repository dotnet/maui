import { boolean, connect, enumValue, NativeError, nullableText } from '../core.js'
import type { Json } from '../core.js'

/** Native MAUI popup text direction; matchParent is the native default. */
export type FlowDirection = 'matchParent' | 'leftToRight' | 'rightToLeft'

/** Native informational popup options; callbacks and UX choices stay in TypeScript. */
export interface AlertOptions {
  /** Native title; defaults to empty. */
  title?: string
  /** Dismiss/acknowledgement label; defaults to OK. */
  cancel?: string
  /** MAUI popup text direction. */
  flowDirection?: FlowDirection
}
/** Native confirmation options and both button labels. */
export interface ConfirmOptions extends AlertOptions {
  /** Positive button label; defaults to OK. */
  accept?: string
}
/** Wire names for actual MAUI Keyboard singleton instances, not a native enum. */
export type KeyboardName = 'default' | 'plain' | 'chat' | 'text' | 'email' | 'numeric' | 'telephone' | 'url' | 'date' | 'time' | 'password'
/** MAUI custom keyboard behavior flags. */
export type KeyboardFlag = 'none' | 'capitalizeSentence' | 'spellcheck' | 'suggestions' | 'capitalizeWord' | 'capitalizeCharacter' | 'capitalizeNone' | 'all'
/** Exactly one singleton name or the actual flags passed to MAUI Keyboard.Create. */
export type Keyboard = { name: KeyboardName; flags?: never } | { flags: KeyboardFlag[]; name?: never }
/** Options actually supported by the native MAUI text prompt. */
export interface PromptOptions {
  /** Native title; defaults to empty. */
  title?: string
  /** Acceptance label; defaults to OK. */
  accept?: string
  /** Dismissal label; defaults to Cancel. */
  cancel?: string
  /** Native placeholder. */
  placeholder?: string | null
  /** Native maximum text length; -1 (default) is unspecified. */
  maxLength?: number
  /** Serialized native Keyboard; null/omission leaves the native default. Flags are distinct names; none/all stand alone. */
  keyboard?: Keyboard | null
  /** Native initial input; defaults to empty. */
  initialValue?: string
}
/** Native action-sheet choices and destructive/cancel labels. */
export interface ActionSheetOptions {
  /** Native title; defaults to empty. */
  title?: string
  /** Native cancel label, or null to omit. */
  cancel?: string | null
  /** Native destructive-choice label, or null to omit. */
  destruction?: string | null
  /** Actual native action buttons; defaults to empty. */
  buttons?: string[]
  /** MAUI action-sheet text direction. */
  flowDirection?: FlowDirection
}
function direction(value: FlowDirection = 'matchParent'): void {
  enumValue(value, ['matchParent', 'leftToRight', 'rightToLeft'], 'invalid_arguments')
}
function keyboard(value: Keyboard | null | undefined): Json | undefined {
  if (value == null) return value
  if (typeof value !== 'object' || Array.isArray(value) || Object.keys(value).length !== 1)
    throw new NativeError('invalid_arguments', 'Expected exactly one keyboard name or flags property.')
  if (Object.hasOwn(value, 'name'))
    return { name: enumValue(value.name, ['default', 'plain', 'chat', 'text', 'email', 'numeric', 'telephone', 'url', 'date', 'time', 'password'], 'invalid_arguments') }
  else if (Object.hasOwn(value, 'flags')) {
    const flags = value.flags
    if (!Array.isArray(flags) || new Set(flags).size !== flags.length ||
      (flags.length !== 1 && flags.some(flag => flag === 'none' || flag === 'all')))
      throw new NativeError('invalid_arguments', 'Expected distinct native flag names; none/all must stand alone.')
    for (const flag of flags)
      enumValue(flag, ['none', 'capitalizeSentence', 'spellcheck', 'suggestions', 'capitalizeWord', 'capitalizeCharacter', 'capitalizeNone', 'all'], 'invalid_arguments')
    return { flags }
  } else throw new NativeError('invalid_arguments', 'Unknown keyboard representation.')
}
/**
 * Displays an asynchronous native page-owned alert, not browser window.alert.
 * @param message - Native message; defaults to empty, never a preset sample question.
 * @param options - Native title/button/direction overrides.
 * @returns Native completion after dismissal.
 * @throws `NativeError` on invalid options, native absence or OS failure.
 */
export async function alert(message = '', options: AlertOptions = {}): Promise<void> {
  direction(options.flowDirection)
  return (await connect()).invoke('dialogs.alert', { title: options.title ?? '', message, cancel: options.cancel ?? 'OK', flowDirection: options.flowDirection ?? 'matchParent' }, value => {
    if (value !== null) throw new NativeError('invalid_response', 'Expected native void completion.')
  })
}
/**
 * Displays asynchronous native confirmation with caller-chosen labels.
 * @param message - Native message; defaults to empty.
 * @param options - Native title/accept/cancel/direction overrides.
 * @returns False is ordinary user cancellation.
 * @throws `NativeError` on invalid options, native absence or OS failure.
 */
export async function confirm(message = '', options: ConfirmOptions = {}): Promise<boolean> {
  direction(options.flowDirection)
  return (await connect()).invoke('dialogs.confirm', { title: options.title ?? '', message, accept: options.accept ?? 'OK', cancel: options.cancel ?? 'Cancel', flowDirection: options.flowDirection ?? 'matchParent' }, boolean)
}
/**
 * Displays asynchronous native text input using normal MAUI prompt options.
 * @param message - Native message; defaults to empty.
 * @param options - Title/buttons/placeholder/initialValue/maxLength/keyboard options.
 * @returns Actual native text (including empty text), or null on ordinary dismissal.
 * @throws `NativeError` on invalid options, native absence or OS failure.
 * @remarks Native OS UI uses the explicit five-minute interactive budget. MAUI
 * cannot force-dismiss it; document disposal cancels the wait, not OS side effects.
 */
export async function prompt(message = '', options: PromptOptions = {}): Promise<string | null> {
  const { keyboard: value, ...parameters } = options
  const encoded = keyboard(value)
  return (await connect()).invoke('dialogs.prompt', { title: '', message, ...parameters, ...(encoded === undefined ? {} : { keyboard: encoded }) }, nullableText)
}
/**
 * Displays native MAUI action-sheet choices; no custom-layout/Tauri-kind emulation.
 * @param options - Native title/cancel/destruction/buttons/direction.
 * @returns Selected native label, including the cancel label, or null when native returns no selection.
 * @throws `NativeError` on invalid options, native absence or OS failure.
 */
export async function actionSheet(options: ActionSheetOptions = {}): Promise<string | null> {
  direction(options.flowDirection)
  return (await connect()).invoke('dialogs.actionSheet', { title: options.title ?? '', cancel: options.cancel ?? null, destruction: options.destruction ?? null, buttons: options.buttons ?? [], flowDirection: options.flowDirection ?? 'matchParent' }, nullableText)
}
