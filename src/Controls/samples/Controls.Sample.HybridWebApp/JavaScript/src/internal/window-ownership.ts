import { boolean, number, object, text } from '../core.js'
import type { NativeClient } from '../core.js'

export interface WindowSnapshot {
  id: string
  windowLease: string
  initialLocation: string
  title: string
  x: number | null
  y: number | null
  width: number | null
  height: number | null
  canResize: boolean
  canPosition: boolean
  canOpenWindows: boolean
  canCenter: boolean
  canObservePosition: boolean
}

// Acquisition retains the returned generation, even if the ID is closed/reused before the first method call.
export const acquiredWindows = new WeakMap<object, { client: NativeClient; owner: Pick<WindowSnapshot, 'id' | 'windowLease'> }>()

export function windowIdentity(value: unknown): Pick<WindowSnapshot, 'id' | 'windowLease'> {
  const v = object(value)
  return { id: text(v.id), windowLease: text(v.windowLease) }
}

export function windowSnapshot(value: unknown): WindowSnapshot {
  const v = object(value), nullable = (value: unknown) => value === null ? null : number(value)
  return {
    id: text(v.id), windowLease: text(v.windowLease), initialLocation: text(v.initialLocation), title: text(v.title),
    x: nullable(v.x), y: nullable(v.y), width: nullable(v.width), height: nullable(v.height),
    canResize: boolean(v.canResize), canPosition: boolean(v.canPosition), canOpenWindows: boolean(v.canOpenWindows),
    canCenter: v.canCenter === undefined ? false : boolean(v.canCenter),
    canObservePosition: v.canObservePosition === undefined ? false : boolean(v.canObservePosition),
  }
}
