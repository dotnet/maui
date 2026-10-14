import { boolean, connect, enumValue, NativeError, nullableText, object } from '../core.js'

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
/** Named native MAUI keyboards supported by DisplayPromptAsync. */
export type KeyboardKind = 'default' | 'plain' | 'chat' | 'text' | 'email' | 'numeric' | 'telephone' | 'url' | 'date' | 'time' | 'password' | 'custom'
/** MAUI custom keyboard behavior flags. */
export type KeyboardFlag = 'none' | 'capitalizeSentence' | 'spellcheck' | 'suggestions' | 'capitalizeWord' | 'capitalizeCharacter' | 'capitalizeNone' | 'all'
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
  /** Native named keyboard; defaults to default. */
  keyboard?: KeyboardKind
  /** Native flags used only with custom; [] means none, and none/all must stand alone. */
  keyboardFlags?: KeyboardFlag[]
  /** Native initial input; defaults to empty. */
  initialValue?: string
}
/** Normal native prompt acceptance or user dismissal. */
export interface PromptResult {
  /** True on native user dismissal. */
  cancelled: boolean
  /** Actual accepted text (possibly empty), or null on dismissal. */
  text: string | null
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
function labels(...values: string[]): void {
  if (values.some(value => typeof value !== 'string' || !value.trim()))
    throw new NativeError('invalid_arguments', 'Native button labels must not be blank.')
}
function direction(value: FlowDirection = 'matchParent'): void {
  enumValue(value, ['matchParent', 'leftToRight', 'rightToLeft'], 'invalid_arguments')
}
/**
 * Displays an asynchronous native page-owned alert, not browser window.alert.
 * @param message - Native message; defaults to empty, never a preset sample question.
 * @param options - Native title/button/direction overrides.
 * @returns Native completion after dismissal.
 * @throws `NativeError` on invalid options, native absence or OS failure.
 */
export async function alert(message = '', options: AlertOptions = {}): Promise<void> {
  labels(options.cancel ?? 'OK')
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
  labels(options.accept ?? 'OK', options.cancel ?? 'Cancel')
  direction(options.flowDirection)
  return (await connect()).invoke('dialogs.confirm', { title: options.title ?? '', message, accept: options.accept ?? 'OK', cancel: options.cancel ?? 'Cancel', flowDirection: options.flowDirection ?? 'matchParent' }, boolean)
}
/**
 * Displays asynchronous native text input using normal MAUI prompt options.
 * @param message - Native message; defaults to empty.
 * @param options - Title/buttons/placeholder/initialValue/maxLength/keyboard options.
 * @returns Accepted native text or ordinary cancellation.
 * @throws `NativeError` on invalid options, native absence or OS failure.
 * @remarks Native OS UI uses the explicit five-minute interactive budget. MAUI
 * cannot force-dismiss it; document disposal cancels the wait, not OS side effects.
 */
export async function prompt(message = '', options: PromptOptions = {}): Promise<PromptResult> {
  labels(options.accept ?? 'OK', options.cancel ?? 'Cancel')
  const keyboard = enumValue(options.keyboard ?? 'default', ['default', 'plain', 'chat', 'text', 'email', 'numeric', 'telephone', 'url', 'date', 'time', 'password', 'custom'], 'invalid_arguments')
  if (options.keyboardFlags !== undefined) {
    const flags = options.keyboardFlags
    if (keyboard !== 'custom' || !Array.isArray(flags) || new Set(flags).size !== flags.length ||
      (flags.length !== 1 && flags.some(flag => flag === 'none' || flag === 'all')))
      throw new NativeError('invalid_arguments', 'Use custom keyboard with distinct native flags; none/all must stand alone.')
    for (const flag of flags)
      enumValue(flag, ['none', 'capitalizeSentence', 'spellcheck', 'suggestions', 'capitalizeWord', 'capitalizeCharacter', 'capitalizeNone', 'all'], 'invalid_arguments')
  }
  const max = options.maxLength ?? -1
  if (!Number.isInteger(max) || max < -1 || max > 2147483647 || (max >= 0 && (options.initialValue?.length ?? 0) > max))
    throw new NativeError('invalid_arguments', 'Use maxLength -1 or a nonnegative native integer that fits initial text.')
  return (await connect()).invoke('dialogs.prompt', { title: '', message, ...options }, value => {
    const v = object(value), result = { cancelled: boolean(v.cancelled), text: nullableText(v.text) }
    if (result.cancelled !== (result.text === null) || (result.text !== null && max >= 0 && result.text.length > max))
      throw new NativeError('invalid_response', 'Invalid native prompt result.')
    return result
  })
}
/**
 * Displays native MAUI action-sheet choices; no custom-layout/Tauri-kind emulation.
 * @param options - Native title/cancel/destruction/buttons/direction.
 * @returns Selected native label, including the cancel label, or null when native returns no selection.
 * @throws `NativeError` on invalid options, native absence or OS failure.
 */
export async function actionSheet(options: ActionSheetOptions = {}): Promise<string | null> {
  direction(options.flowDirection)
  if (options.cancel) labels(options.cancel)
  if (options.destruction) labels(options.destruction)
  labels(...(options.buttons ?? []))
  return (await connect()).invoke('dialogs.actionSheet', { title: options.title ?? '', cancel: options.cancel ?? null, destruction: options.destruction ?? null, buttons: options.buttons ?? [], flowDirection: options.flowDirection ?? 'matchParent' }, nullableText)
}
