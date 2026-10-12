import './style.css'
import { message } from './message.ts'
import { availableFeatures } from './catalog.ts'
import type { Feature } from './catalog.ts'
import { NativeClient, NativeError, loadNativeBridge, object, nullableText, number, boolean, strings } from '@maui/api/core'
import type { Operation, Progress } from '@maui/api/core'
import { clipboard } from '@maui/api/essentials/clipboard'
import { preferences } from '@maui/api/essentials/preferences'
import { getIdentity, sample } from './native/sample.ts'

function element<T extends HTMLElement>(selector: string, type: { new(): T }): T {
  const found = document.querySelector(selector)
  if (!(found instanceof type)) throw new Error(`Missing playground element: ${selector}`)
  return found
}
const controls = element('#controls', HTMLElement)
const result = element('#result-json', HTMLPreElement)
const state = element('#result-state', HTMLSpanElement)
const log = element('#operation-log', HTMLOListElement)
const host = element('#host-status', HTMLDivElement)
const search = element('#search', HTMLInputElement)
const category = element('#category', HTMLSelectElement)
const featureList = element('#feature-list', HTMLElement)
const progressElement = element('#progress', HTMLProgressElement)
let client = new NativeClient(undefined)
let busy = false
let active: Operation<Progress> | undefined
let unsubscribeCounter: (() => Promise<void>) | undefined
let unsubscribeProgress: (() => Promise<void>) | undefined
let selected: Feature = availableFeatures[0]!
let selectionVersion = 0

element('#typescript-message', HTMLParagraphElement).textContent = `TypeScript edit target: ${message}`
void import('./details.ts').then(({ details }) => { element('#module-message', HTMLParagraphElement).textContent = details })

function writeLog(label: string, status: string, value: unknown): void {
  log.querySelector('.empty-log')?.remove()
  const row = document.createElement('li')
  row.dataset.state = status.toLowerCase()
  const time = document.createElement('time')
  time.textContent = new Date().toLocaleTimeString()
  const title = document.createElement('strong')
  title.textContent = label
  const badge = document.createElement('span')
  badge.textContent = status
  const detail = document.createElement('pre')
  detail.textContent = typeof value === 'string' ? value : JSON.stringify(value, null, 2)
  row.append(time, title, badge, detail)
  log.prepend(row)
  while (log.children.length > 40) log.lastElementChild?.remove()
}
function showResult(status: string, value: unknown): void {
  state.textContent = status
  state.dataset.state = status.toLowerCase()
  result.textContent = typeof value === 'string' ? value : JSON.stringify(value, null, 2)
}
function updateButtons(): void {
  for (const button of controls.querySelectorAll<HTMLButtonElement>('button[data-action]')) {
    button.disabled = !client.available || busy
    if (button.dataset.action === 'cancel') button.disabled = !active
    if (button.dataset.action === 'reload') button.disabled = false
    if (button.dataset.action === 'unsubscribe') button.disabled = busy || !unsubscribeCounter
    if (button.dataset.action === 'subscribe') button.disabled = busy || !client.available || !!unsubscribeCounter
  }
}
async function run(label: string, action: () => Promise<unknown>): Promise<void> {
  if (busy) return
  busy = true
  const version = selectionVersion
  showResult('Loading', `${label}…`)
  updateButtons()
  try {
    const value = await action()
    writeLog(label, 'Success', value)
    if (version === selectionVersion) showResult('Success', value)
  } catch (error) {
    const code = error instanceof NativeError ? error.code : 'input_error'
    const value = { code, message: error instanceof Error ? error.message : 'The operation failed.' }
    const status = code === 'cancelled' ? 'Cancelled' : 'Error'
    writeLog(label, status, value)
    if (version === selectionVersion) showResult(status, value)
  } finally {
    busy = false
    active = undefined
    if (version !== selectionVersion && state.dataset.state === 'loading') showResult('Ready to run', 'The previous operation finished. Run this experiment to inspect its result.')
    updateButtons()
  }
}
const field = (id: string) => element(`#${id}`, HTMLInputElement).value
const area = (id: string) => element(`#${id}`, HTMLTextAreaElement).value
function bind(action: string, label: string, task: () => Promise<unknown>): void {
  controls.querySelector<HTMLButtonElement>(`[data-action="${action}"]`)?.addEventListener('click', () => { void run(label, task) })
}
const button = (action: string, title: string, primary = false) => `<button type="button" data-action="${action}"${primary ? ' class="primary"' : ''}>${title}</button>`
const reload = `${button('reload', 'Reload frontend')}`
function render(feature: Feature): void {
  selected = feature
  selectionVersion++
  element('#feature-title', HTMLHeadingElement).textContent = feature.title
  element('#feature-meta', HTMLParagraphElement).textContent = feature.category
  element('#feature-description', HTMLParagraphElement).textContent = feature.description
  const link = element('#api-link', HTMLAnchorElement)
  link.href = feature.docs
  progressElement.hidden = true
  showResult(busy ? 'Loading' : 'Ready to run', busy ? 'A native operation is running. Return to Progress and cancellation to cancel it.' : 'Run an experiment to inspect its actual native result.')
  if (!feature.experiment) {
    controls.innerHTML = '<div class="notice">Not implemented. No native command or mock result is provided for this family.</div>'
    return
  }
  const api = sample(client)
  switch (feature.experiment) {
    case 'identity':
      controls.innerHTML = `<p class="input-help">Compare <code>pid</code>, <code>startupId</code> and <code>startedAt</code> before and after frontend reload.</p><div class="actions">${button('run', 'Read native identity', true)}${reload}</div>`
      bind('run', 'Read native identity', async () => {
        const identity = await getIdentity(client)
        element('#native-identity', HTMLPreElement).textContent = JSON.stringify(identity, null, 2)
        return identity
      })
      break
    case 'clipboard':
      controls.innerHTML = `<label for="clipboard-text">Text to copy · maximum 4096 characters</label><textarea id="clipboard-text" rows="4" maxlength="4096">Hello from MAUI — 你好 👋
"Quotes", backslash \\ and a new line.</textarea><div class="actions">${button('copy', 'Copy to native clipboard', true)}${button('paste', 'Paste from native clipboard')}</div>`
      bind('copy', 'Copy to native clipboard', () => clipboard(client).writeText(area('clipboard-text')))
      bind('paste', 'Paste from native clipboard', async () => {
        const target = element('#clipboard-text', HTMLTextAreaElement)
        const text = await clipboard(client).readText()
        if (text !== null && target.isConnected) target.value = text
        return { text }
      })
      break
    case 'preferences':
      controls.innerHTML = `<label for="preference-key">Key · 1–48 letters, digits, underscores or hyphens</label><input id="preference-key" value="example" maxlength="48" pattern="[a-zA-Z0-9_-]{1,48}" /><label for="preference-value">Value · maximum 4096 characters</label><textarea id="preference-value" rows="3" maxlength="4096">A setting saved by the playground.</textarea><p class="input-help">Native keys use <code>maui.playground.v1.</code>. No other app settings are accessible.</p><div class="actions">${button('save', 'Save setting', true)}${button('read', 'Read setting')}${button('remove', 'Remove setting')}</div>`
      bind('save', 'Save preference', () => preferences(client).set(field('preference-key'), area('preference-value')))
      bind('read', 'Read preference', async () => {
        const target = element('#preference-value', HTMLTextAreaElement)
        const value = await preferences(client).get(field('preference-key'))
        if (value.value !== null && target.isConnected) target.value = value.value
        return value
      })
      bind('remove', 'Remove preference', () => preferences(client).remove(field('preference-key')))
      break
    case 'echo':
    case 'events':
      controls.innerHTML = `<label for="echo-input">Command arguments · JSON DTO</label><textarea id="echo-input" rows="8" spellcheck="false" class="code-input"></textarea><p class="input-help">Use text or null, a finite number, a boolean and up to 16 strings. The response is asynchronous. Successful calls increment the native counter.</p><div class="actions">${button('echo', 'Run native echo', true)}${feature.experiment === 'events' ? button('subscribe', 'Subscribe to changes') + button('unsubscribe', 'Unsubscribe') : button('counter', 'Read native counter')}</div>`
      element('#echo-input', HTMLTextAreaElement).value = JSON.stringify({ text: 'Unicode: café 你好 👋\n"quotes" \\ slash', number: 42.5, flag: true, items: ['first', '第二'] }, null, 2)
      bind('echo', 'Async native DTO echo', async () => {
        const value = object(JSON.parse(area('echo-input')))
        return api.echo({ text: nullableText(value.text), number: number(value.number), flag: boolean(value.flag), items: strings(value.items) })
      })
      bind('counter', 'Read native service counter', () => api.getCounter())
      bind('subscribe', 'Subscribe to native changes', async () => {
        unsubscribeCounter = await api.onCounter((counter, id) => {
          log.dataset.lastEventId = id
          writeLog('Native service.changed', 'Event', { counter, requestId: id })
        })
        return { subscribed: true, topic: 'service.changed' }
      })
      bind('unsubscribe', 'Unsubscribe from native changes', async () => {
        await unsubscribeCounter?.()
        unsubscribeCounter = undefined
        return { subscribed: false, topic: 'service.changed' }
      })
      break
    case 'errors':
      controls.innerHTML = `<p class="input-help">Expected result: <code>intentional_error</code>. Read the native counter afterward to verify recovery.</p><div class="actions">${button('fail', 'Run deliberate failure', true)}${button('counter', 'Verify bridge still works')}</div>`
      bind('fail', 'Intentional native failure', () => api.fail())
      bind('counter', 'Read counter after error', () => api.getCounter())
      break
    case 'policy':
      controls.innerHTML = `<p class="input-help">These calls should reject with distinct codes: <code>unknown_command</code>, <code>disabled_command</code> and <code>invalid_arguments</code>. None requests an OS permission.</p><div class="actions">${button('missing', 'Try missing command', true)}${button('disabled', 'Try disabled command')}${button('invalid', 'Send invalid DTO')}</div>`
      bind('missing', 'Unknown native command', () => api.missing())
      bind('disabled', 'Disabled native command', () => api.disabled())
      bind('invalid', 'Invalid native arguments', () => api.invalid())
      break
    case 'lifetime':
      controls.innerHTML = `<p class="input-help">Increment the native counter, read it, reload, then read it again. The service instance survives a frontend-only reload.</p><div class="actions">${button('increment', 'Increment native counter', true)}${button('counter', 'Read native counter')}${reload}</div>`
      bind('increment', 'Increment service counter', () => api.echo({ text: null, number: 0, flag: false, items: [] }))
      bind('counter', 'Read service counter', () => api.getCounter())
      break
    case 'work':
      controls.innerHTML = `<div class="work-inputs"><div><label for="steps">Steps · 1–50</label><input id="steps" type="number" min="1" max="50" value="20" /></div><div><label for="delay">Delay per step · 100–500 ms</label><input id="delay" type="number" min="100" max="500" step="100" value="300" /></div></div><p class="input-help">At most 15 seconds per operation. Progress comes from native code, not a JavaScript timer. Reload while running to test cleanup.</p><div class="actions">${button('work', 'Run native work', true)}${button('cancel', 'Cancel operation')}${reload}</div>`
      progressElement.hidden = false
      bind('work', 'Native work', async () => {
        const steps = Number(field('steps'))
        const delay = Number(field('delay'))
        unsubscribeProgress ??= await api.onProgress((value, id) => {
          if (active?.id !== id) return
          log.dataset.lastEventId = id
          progressElement.max = value.total
          progressElement.value = value.completed
          writeLog('Native operation.progress', 'Event', { ...value, requestId: id })
          if (selected.experiment === 'work') showResult('Loading', { ...value, requestId: id })
        })
        progressElement.value = 0
        active = api.work(steps, delay)
        updateButtons()
        return active.result
      })
      controls.querySelector<HTMLButtonElement>('[data-action="cancel"]')?.addEventListener('click', async () => {
        if (!active) return
        try {
          const accepted = await active.cancel()
          writeLog('Cancellation requested', 'Loading', { accepted })
        } catch (error) { writeLog('Cancel request', 'Error', error instanceof Error ? error.message : 'Cancel request failed.') }
      })
      break
    case 'development':
      controls.innerHTML = `<p class="input-help">Live-edit markers remain in the HTML title and host-details disclosure. Edit <code>Web/src/message.ts</code>, <code>details.ts</code>, <code>style.css</code> or <code>index.html</code>. For offline: <code>npm --prefix Web run stage</code>, then build native.</p><div class="actions">${button('identity', 'Read native startup identity', true)}${reload}</div>`
      bind('identity', 'Read native startup identity', () => getIdentity(client))
      break
  }
  controls.querySelector<HTMLButtonElement>('[data-action="reload"]')?.addEventListener('click', () => location.reload())
  updateButtons()
}
function renderCatalog(): void {
  const matches = availableFeatures.filter(feature => (!category.value || feature.category === category.value) &&
    `${feature.title} ${feature.category} ${feature.description}`.toLowerCase().includes(search.value.toLowerCase()))
  element('#catalog-count', HTMLParagraphElement).textContent = matches.length === availableFeatures.length
    ? `${availableFeatures.length} experiments`
    : `${matches.length} of ${availableFeatures.length} experiments`
  featureList.replaceChildren()
  if (!matches.length) {
    const empty = document.createElement('p')
    empty.textContent = 'No matching features. Try a broader search or clear the filters.'
    featureList.append(empty)
  }
  for (const feature of matches) {
    const item = document.createElement('button')
    item.type = 'button'
    item.dataset.feature = feature.id
    item.setAttribute('aria-current', String(selected.id === feature.id))
    const title = document.createElement('span')
    title.textContent = feature.title
    const note = document.createElement('small')
    note.textContent = feature.description.split(/\.\s/)[0]
    item.append(title, note)
    item.addEventListener('click', () => {
      render(feature)
      renderCatalog()
      // Keep desktop keyboard focus in navigation; compact navigation moves to the experiment.
      if (matchMedia('(max-width: 760px)').matches) element('#experiment', HTMLElement).focus()
      else featureList.querySelector<HTMLButtonElement>(`[data-feature="${feature.id}"]`)?.focus()
    })
    featureList.append(item)
  }
}
for (const name of [...new Set(availableFeatures.map(feature => feature.category))]) {
  const option = document.createElement('option')
  option.textContent = name
  category.append(option)
}
for (const input of [search, category]) input.addEventListener('input', renderCatalog)
element('#clear-log', HTMLButtonElement).addEventListener('click', () => {
  log.innerHTML = '<li class="empty-log">Log cleared. New native requests and events will appear here.</li>'
})
render(selected)
renderCatalog()
async function bootstrap(): Promise<void> {
  try {
    const bridge = await loadNativeBridge()
    client = new NativeClient(bridge)
    if (!bridge) {
      host.textContent = 'Native host unavailable · browser-only'
      host.dataset.state = 'unavailable'
      element('#native-unavailable', HTMLDivElement).hidden = false
      render(selected)
      return
    }
    client.attachToDocument()
    const ready = await client.connect()
    const identity = await getIdentity(client)
    element('#native-identity', HTMLPreElement).textContent = JSON.stringify(identity, null, 2)
    host.textContent = `Native connected · ${identity.platform} · PID ${identity.pid}`
    host.dataset.state = 'ready'
    host.dataset.session = client.session
    host.dataset.startupId = identity.startupId
    writeLog('Native session connected', 'Success', { ...ready, identity })
    render(selected)
  } catch (error) {
    client.dispose()
    host.textContent = 'Native connection failed · reload to retry'
    host.dataset.state = 'error'
    element('#native-unavailable', HTMLDivElement).hidden = false
    showResult('Error', error instanceof Error ? error.message : 'Bridge failed.')
    updateButtons()
  }
}
void bootstrap()
window.addEventListener('pageshow', event => { if (event.persisted) location.reload() })
if (import.meta.hot) import.meta.hot.dispose(() => client.dispose())
