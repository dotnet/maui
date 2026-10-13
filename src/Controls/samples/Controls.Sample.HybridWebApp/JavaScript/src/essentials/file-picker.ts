import { ack, boolean, connect, NativeError, nullableText, number, object, text } from '../core.js'
import type { NativeClient, Json } from '../core.js'

/** MAUI PickOptions supplied from TypeScript; folder/save/defaultPath are not invented picker features. */
export interface PickOptions {
  /** Native picker title; omitted to use the OS/MAUI default. */
  pickerTitle?: string
  /** Native platform filters: Android MIME types, Apple UTType identifiers, Windows extensions. */
  fileTypes?: Partial<Record<'android' | 'ios' | 'maccatalyst' | 'windows', string[]>>
}
/** One bounded transport read, not a whole-file capacity limit. */
export interface FileBytes {
  /** Bytes actually read; partial reads are normal. */
  bytes: Uint8Array
  /** Native end-of-stream state. */
  eof: boolean
}
/** Incrementally decoded UTF-8 chunk with decoder state preserved across reads. */
export interface FileText {
  /** Decoded text from this chunk; a split multibyte character may produce empty text. */
  text: string
  /** Native EOF, after flushing the UTF-8 decoder. */
  eof: boolean
}
/** Native OpenReadAsync stream resource; read/seek/close calls are queued in invocation order. */
export interface FileReader {
  /** Native stream identity, scoped to this owning view/document. */
  readonly id: string
  /** Whether the actual native stream supports byte seeking. */
  readonly canSeek: boolean
  /** Configured maximum bytes in one read message, not file size. */
  readonly maximumChunkBytes: number
  /**
   * Reads binary data without text coercion.
   * @param count - Desired byte count, positive and within transport chunk budget.
   * @returns Partial bytes and actual native EOF.
   */
  readBytes(count?: number): Promise<FileBytes>
  /**
   * Reads incremental strict UTF-8 text; do not mix with binary reads on this reader.
   * @param count - Desired raw byte count within transport budget.
   * @returns Decoded chunk and EOF; split UTF-8 sequences retain decoder state.
   * @throws `NativeError` for invalid UTF-8, invalid read mode or native failure.
   */
  readText(count?: number): Promise<FileText>
  /**
   * Seeks a native seekable stream and resets decoder state.
   * @param position - Nonnegative safe-integer byte position.
   * @returns Native seek completion; nonseekable streams reject.
   */
  seek(position: number): Promise<void>
  /** Closes this actual native read stream. @returns Native release completion. */
  close(): Promise<void>
}
/** User-selected FileResult resource; no private FullPath or arbitrary path authority is exposed. */
export interface PickedFile {
  /** Native selection identity. */
  readonly id: string
  /** OS-reported filename, not a persistent private path. */
  readonly name: string
  /** Native content/MIME type, possibly null. */
  readonly contentType: string | null
  /** Opens an actual native FileResult.OpenReadAsync stream. @returns Scoped binary/text reader resource. */
  openRead(): Promise<FileReader>
  /** Releases selected-file authority and its open readers. @returns Release completion. */
  dispose(): Promise<void>
}
/** Actual native single/multiple selection operations; there is no text-only whole-file wrapper. */
function picked(value: unknown, client: NativeClient): PickedFile {
  const v = object(value), id = text(v.id), name = text(v.name), contentType = nullableText(v.contentType)
  let disposed = false
  return {
    id, name, contentType,
    openRead: async () => {
      if (disposed) throw new NativeError('invalid_file', 'This selected file was released.')
      const opened = await client.invoke('file.openRead', { id }, value => {
        const v = object(value), maximumChunkBytes = number(v.maximumChunkBytes)
        if (!Number.isInteger(maximumChunkBytes) || maximumChunkBytes <= 0 || maximumChunkBytes > 65536)
          throw new NativeError('invalid_response', 'Invalid native file chunk budget.')
        return { id: text(v.id), canSeek: boolean(v.canSeek), maximumChunkBytes }
      })
      let closed = false, mode: 'bytes' | 'text' | undefined, decoder = new TextDecoder('utf-8', { fatal: true })
      let tail: Promise<void> = Promise.resolve()
      const serial = <T>(action: () => Promise<T>): Promise<T> => {
        const result = tail.then(action)
        tail = result.then(() => {}, () => {})
        return result
      }
      const read = async (count = opened.maximumChunkBytes): Promise<FileBytes> => {
        if (closed || disposed) throw new NativeError('invalid_file', 'The selected file or reader was closed.')
        if (!Number.isInteger(count) || count <= 0 || count > opened.maximumChunkBytes)
          throw new NativeError('invalid_arguments', 'Read count must fit the native transport chunk budget.')
        return client.invoke('file.readBytes', { id: opened.id, count }, value => {
          const v = object(value), base64 = text(v.base64)
          let decoded: string
          try { decoded = atob(base64) } catch { throw new NativeError('invalid_response', 'Native file read was not valid base64.') }
          if (decoded.length > count) throw new NativeError('invalid_response', 'Native file read exceeded requested count.')
          return { bytes: Uint8Array.from(decoded, character => character.charCodeAt(0)), eof: boolean(v.eof) }
        })
      }
      return {
        ...opened,
        readBytes: count => serial(async () => {
          if (closed || disposed) throw new NativeError('invalid_file', 'The selected file or reader was closed.')
          if (mode === 'text') throw new NativeError('invalid_arguments', 'Use a separate reader for mixed binary/text access.')
          mode = 'bytes'; return read(count)
        }),
        readText: count => serial(async () => {
          if (closed || disposed) throw new NativeError('invalid_file', 'The selected file or reader was closed.')
          if (mode === 'bytes') throw new NativeError('invalid_arguments', 'Use a separate reader for mixed binary/text access.')
          mode = 'text'
          const result = await read(count)
          try { return { text: decoder.decode(result.bytes, { stream: !result.eof }), eof: result.eof } }
          catch { throw new NativeError('unsupported_encoding', 'Selected content is not valid UTF-8; use readBytes for binary/other encodings.') }
        }),
        seek: position => serial(async () => {
          if (closed || disposed) throw new NativeError('invalid_file', 'The reader was closed.')
          if (!opened.canSeek) throw new NativeError('unsupported', 'The native selected stream is not seekable.')
          if (!Number.isSafeInteger(position) || position < 0) throw new NativeError('invalid_arguments', 'Seek requires a nonnegative safe-integer position.')
          await client.invoke('file.seek', { id: opened.id, position }, ack)
          decoder = new TextDecoder('utf-8', { fatal: true }); mode = undefined
        }),
        close: () => serial(async () => { if (closed) return; await client.invoke('file.closeReader', { id: opened.id }, ack); closed = true }),
      }
    },
    dispose: async () => { if (disposed) return; await client.invoke('file.release', { id }, ack); disposed = true },
  }
}
function options(value: PickOptions): Json {
  if (value.pickerTitle !== undefined && typeof value.pickerTitle !== 'string')
    throw new NativeError('invalid_arguments', 'Picker title must be text.')
  const allowed = ['pickerTitle', 'fileTypes']
  if (Object.keys(value).some(key => !allowed.includes(key)))
    throw new NativeError('unsupported', 'MAUI FilePicker supports title/platform filters, not defaultPath, folder or save options.')
  return { ...value }
}
/**
 * Opens actual native MAUI single selection using the document's shared connection.
 * @param value - Native picker title/platform filters.
 * @returns User-selected file resource, or null on normal cancellation.
 * @remarks Selection does not read content or impose a 64KiB whole-file limit.
 * Binary/large files are read in bounded chunks through OpenReadAsync resources.
 * No FullPath/File.Open or saved arbitrary-path authority exists. Release unused
 * selections/readers; owning document disconnect/scope destruction invalidates
 * them. OS picker interaction uses the explicit bounded interactive deadline.
 */
export async function pickFile(value: PickOptions = {}): Promise<PickedFile | null> {
  const c = await connect()
  return c.invoke('filePicker.pick', options(value), result => {
    const file = object(result).file
    return file === null ? null : picked(file, c)
  })
}
/**
 * Opens native multiple selection without whole-file reads or a file-capacity limit.
 * @param value - Native title/platform file filters.
 * @returns Selected owning resources, or an empty array on normal cancellation.
 */
export async function pickMultipleFiles(value: PickOptions = {}): Promise<PickedFile[]> {
  const c = await connect()
  return c.invoke('filePicker.pickMultiple', options(value), result => {
    const files = object(result).files
    if (!Array.isArray(files)) throw new NativeError('invalid_response', 'Native multiple selection must return an array.')
    return files.map(value => picked(value, c))
  })
}
