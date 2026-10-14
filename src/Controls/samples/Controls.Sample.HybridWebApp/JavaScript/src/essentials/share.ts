import { ack, connect, NativeError, number, object, text, documentConnectionGeneration } from '../core.js'
import type { BinaryInput, Json } from '../core.js'

/** An opaque document-owned staged file, never a native filesystem path. */
export interface StagedFile {
  /** Native capability ID, scoped to the current view/document. */
  readonly id: string
  /** Validated plain file name. */
  readonly fileName: string
  /** Explicit prototype content type. */
  readonly contentType: 'application/pdf' | 'text/plain'
  /** Exact staged byte count. */
  readonly size: number
}
/** Native ShareTextRequest fields enabled by this prototype. */
export interface ShareTextRequest {
  /** Text to share on an explicit user action. */
  text?: string
  /** Absolute HTTPS link to share. */
  uri?: string
  /** Optional chooser title. */
  title?: string
  /** Optional native subject. */
  subject?: string
}
/** Native single-file request. */
export interface ShareFileRequest {
  /** Authorized staged resource. */
  file: StagedFile
  /** Optional chooser title. */
  title?: string
}
/** Native multi-file request. */
export interface ShareMultipleFilesRequest {
  /** One to eight distinct authorized staged resources. */
  files: StagedFile[]
  /** Optional chooser title. */
  title?: string
}
/** Explicit closed structural share request, not an open polymorphic base type. */
export type ShareRequest = ShareTextRequest | ShareFileRequest | ShareMultipleFilesRequest
const handles = new WeakMap<StagedFile, number>()
function handle(file: StagedFile): { id: string } {
  if (handles.get(file) !== documentConnectionGeneration())
    throw new NativeError('invalid_resource', 'This staged resource was not acquired by the current document.')
  return { id: file.id }
}
/**
 * Stages bounded browser-produced bytes in the app-owned native share cache.
 * @param fileName - Plain ASCII file name (no paths), up to 100 characters.
 * @param contentType - PDF or plain-text prototype policy.
 * @param bytes - Raw bytes, never base64 JSON.
 * @returns An opaque resource for a later interactive share request.
 * @throws `NativeError` if the transport, view budget or retained cache budget rejects the file.
 * @remarks At most 4 MiB per file, eight resources / 8 MiB per document. Native OS
 * files are retained for 24 hours after sharing; chooser launch is not consumption.
 */
export async function stageFile(fileName: string, contentType: 'application/pdf' | 'text/plain', bytes: BinaryInput): Promise<StagedFile> {
  if (!/^[A-Za-z0-9][A-Za-z0-9_.-]{0,99}$/.test(fileName) || !['application/pdf', 'text/plain'].includes(contentType))
    throw new NativeError('invalid_arguments', 'Use a plain file name and an enabled content type.')
  const expectedSize = bytes instanceof Blob ? bytes.size : bytes.byteLength
  if (expectedSize === 0) throw new NativeError('invalid_arguments', 'Stage a nonempty document.')
  const generation = documentConnectionGeneration()
  const result = await (await connect()).invokeBinary('share.stage', { fileName, contentType }, bytes)
  let value: Record<string, unknown>
  try { value = object(JSON.parse(new TextDecoder('utf-8', { fatal: true }).decode(result))) }
  catch { throw new NativeError('invalid_response', 'Staging must return bounded UTF-8 resource metadata.') }
  const size = number(value.size)
  const file: StagedFile = Object.freeze({ id: text(value.id), fileName: text(value.fileName), contentType, size })
  if (file.fileName !== fileName || value.contentType !== contentType || size !== expectedSize || !Number.isInteger(size) || size <= 0 || size > 4194304 ||
    !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(file.id))
    throw new NativeError('invalid_response', 'Invalid staged native file metadata.')
  if (generation !== documentConnectionGeneration()) throw new NativeError('disconnected', 'The staging document retired before authority was acquired.')
  handles.set(file, generation)
  return file
}
/**
 * Releases this document's staged file authority and capacity.
 * @param file - A current document-owned staged resource.
 * @returns Completion; files already shared with the OS keep their finite retention.
 */
export async function releaseFile(file: StagedFile): Promise<void> {
  await (await connect()).invoke('share.release', handle(file), ack)
  handles.delete(file)
}
/**
 * Calls the OS share UI using the actual native text/file/multi-file structural union.
 * @param request - Exactly one request shape; files must be current-document staged resources.
 * @returns Native request completion, not proof another application consumed the bytes.
 * @throws `NativeError` for ambiguous/empty shapes, stale authority, permissions or unsupported APIs.
 * @remarks This is a separate JSON interactive operation, never a 30-second binary UI wait.
 */
export async function share(request: ShareRequest): Promise<void> {
  if (request === null || typeof request !== 'object' || Array.isArray(request))
    throw new NativeError('invalid_arguments', 'Choose one declared native share request object.')
  const record = object(request)
  const shapes = Number('file' in request) + Number('files' in request) + Number('text' in request || 'uri' in request)
  if (shapes !== 1) throw new NativeError('invalid_arguments', 'Choose exactly one nonempty native share shape.')
  let native: Record<string, Json>
  if ('file' in request) {
    keys(['file', 'title'])
    native = { file: handle(request.file) }
  }
  else if ('files' in request) {
    keys(['files', 'title'])
    if (!Array.isArray(request.files) || request.files.length < 1 || request.files.length > 8)
      throw new NativeError('invalid_arguments', 'Use one to eight distinct staged files.')
    const files = request.files.map(handle)
    if (new Set(files.map(file => file.id)).size !== files.length)
      throw new NativeError('invalid_arguments', 'Use distinct staged files.')
    native = { files }
  } else {
    keys(['text', 'uri', 'subject', 'title'])
    if (!request.text?.trim() && !request.uri?.trim()) throw new NativeError('invalid_arguments', 'Provide text or a native HTTPS link.')
    native = {}
    if (request.text !== undefined) native.text = request.text
    if (request.uri !== undefined) native.uri = request.uri
    if (request.subject !== undefined) native.subject = request.subject
  }
  if (request.title !== undefined) native.title = request.title
  await (await connect()).invoke('share.request', { request: native }, ack)
  function keys(allowed: string[]): void {
    if (Object.keys(request).some(key => !allowed.includes(key)))
      throw new NativeError('invalid_arguments', 'Unknown native share request property.')
    for (const key of ['text', 'uri', 'subject', 'title'])
      if (record[key] !== undefined && typeof record[key] !== 'string')
        throw new NativeError('invalid_arguments', 'Native share text fields must be strings.')
  }
}
