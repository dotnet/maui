import { number, object, text } from '../core.ts'
import type { NativeClient } from '../core.ts'

export interface Identity {
  appName: string; version: string; build: string; platform: string; osVersion: string
  model: string; architecture: string; pid: number; startupId: string; startedAt: string
}
export function getInfo(client: NativeClient): Promise<Identity> {
  return client.invoke('app.info', {}, value => {
    const v = object(value)
    return { appName: text(v.appName), version: text(v.version), build: text(v.build), platform: text(v.platform),
      osVersion: text(v.osVersion), model: text(v.model), architecture: text(v.architecture),
      pid: number(v.pid), startupId: text(v.startupId), startedAt: text(v.startedAt) }
  })
}
