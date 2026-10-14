import { test } from 'vitest'
import assert from 'node:assert/strict'
import { setImmediate } from 'node:timers/promises'
import { Blob as NodeBlob } from 'node:buffer'
import { flushSync } from 'svelte'
import { NativeClient, configureBridge, resetBridge } from '@maui/api/core'
import { availableFeatures, navigationPages } from '../src/catalog.ts'
import { components } from '../src/registry.ts'
import { PlaygroundSession } from '../src/session.ts'
import { mountShell } from '../src/shell.ts'
import { createConnection } from '../src/connection.ts'
import { readRoute } from '../src/routes.ts'
import { sessionWidthLimit } from '../src/panes.ts'
import { measureBrowserSlot } from '../src/browser-geometry.ts'
import html from '../index.html?raw'

// jsdom omits the dialog lifecycle; this shim supplies DOM behavior, never native APIs.
if (!HTMLDialogElement.prototype.close) {
  HTMLDialogElement.prototype.close = function () { this.removeAttribute('open') }
  HTMLDialogElement.prototype.showModal = function () { this.setAttribute('open', '') }
}

const identity = { appName: 'Native', version: '1', build: '2', platform: 'MacCatalyst', osVersion: '27', model: 'Mac',
  architecture: 'arm64', pid: 123, startupId: 'native-startup', startedAt: '2026-10-12T00:00:00Z' }
const actions = {
  identity: ['run', 'reload'], clipboard: ['copy', 'paste'], preferences: ['save', 'read', 'remove'],
  echo: ['echo', 'counter', 'binary-roundtrip', 'binary-document', 'binary-chunks', 'cancel'], errors: ['fail', 'counter'], policy: ['missing', 'disabled', 'invalid'],
  lifetime: ['increment', 'counter', 'reload', 'local-target', 'shared-target', 'read-targets', 'targets-window'], events: ['echo', 'subscribe', 'unsubscribe'],
  work: ['work', 'cancel', 'reload'], development: ['identity', 'reload'],
  welcome: [], showcase: ['title', 'copy', 'prompt', 'device', 'open-window'],
  windows: ['window-info', 'title', 'bounds', 'position', 'center'], multiWindow: ['window-list', 'open-window'],
  dialogs: ['alert', 'confirm', 'prompt', 'action-sheet'],
  filePicker: ['pick-file'], remoteWebView: ['browser-info', 'remote', 'browser-back', 'browser-forward', 'browser-reload', 'browser-focus', 'browser-close'],
  battery: ['battery'], display: ['display'], menus: ['menus-install', 'menus-remove', 'menu-rename', 'menu-enable', 'menu-check'],
  theme: ['theme-system', 'theme-light', 'theme-dark'],
  connectivity: ['connectivity-read', 'connectivity-subscribe', 'connectivity-unsubscribe'],
  textToSpeech: ['speech-locales', 'speech-speak', 'cancel'],
  launcher: ['launch-url'],
  share: ['share-text', 'share-stage-pdf', 'share-pdf', 'share-multiple'],
  geolocation: ['geolocation-read', 'cancel'],
}
const deferred = () => {
  let resolve
  const promise = new Promise(done => { resolve = done })
  return { promise, resolve }
}
const failure = (code, status = 'error') => ({ status, error: { code, message: `Expected ${code}` } })

// Only this test fixture supplies a transport; production never emulates native APIs.
function transport(handler) {
  const requests = [], messages = []
  let browserState, menuSource = null
  const bridge = {
    async InvokeDotNetBinary(method, bytes, options) {
      assert.equal(method, 'hybrid.invoke')
      const request = JSON.parse(Object.entries(options.headers).sort(([a], [b]) => a.localeCompare(b)).map(([, value]) => value).join(''))
      requests.push(request)
      const output = await handler(request, bytes)
      assert.ok(output instanceof Uint8Array)
      return Uint8Array.from(output).buffer
    },
    async InvokeDotNet(method, [wire]) {
      assert.equal(method, 'Invoke')
      const request = JSON.parse(wire)
      requests.push(request)
      let output = await handler(request)
      if (request.command === 'remote.open') browserState = output
      if (request.command === 'remote.info' && browserState) output = browserState
      if (request.command === 'remote.control' && request.args.action === 'close') browserState = undefined
      if (request.command === 'menus.install') menuSource = 'menu-source'
      if (request.command === 'menus.clear') menuSource = null
      if (request.command === 'menus.info') output = { ...output, sourceId: menuSource }
      const failed = output?.error && typeof output.error === 'object' && typeof output.error.code === 'string'
      return JSON.stringify({ version: 1, session: request.session, id: request.id,
        status: failed ? output.status : 'success', result: failed ? null : output, error: failed ? output.error : null })
    },
    SendRawMessage(wire) { messages.push(JSON.parse(wire)) },
  }
  return { requests, messages, bridge }
}

function defaultResult(request, bytes) {
  switch (request.command) {
    case 'core.connect': return { commands: ['app.info', 'sample.echo'], preferenceNamespace: 'maui.playground.v1.', maximumTextLength: 4096 }
    case 'app.info': return { appName: identity.appName, version: identity.version, build: identity.build }
    case 'device.info': return { platform: identity.platform, osVersion: identity.osVersion, model: identity.model }
    case 'sample.diagnostics': return { architecture: identity.architecture, pid: identity.pid,
      startupId: identity.startupId, startedAt: identity.startedAt }
    case 'sample.echo': return { ...request.args, counter: 7 }
    case 'sample.counter': return { counter: 7 }
    case 'clipboard.read': return 'Native clipboard 👋'
    case 'clipboard.hasText': return true
    case 'clipboard.write':
    case 'dialogs.alert': return null
    case 'dialogs.confirm': return false
    case 'dialogs.actionSheet': return 'Choice'
    case 'window.flowDirection': return 'matchParent'
    case 'sample.localCounter':
    case 'sample.sharedCounter': return { instanceId: request.command, count: request.args.increment ?? 0 }
    case 'share.stage': return Uint8Array.from(new TextEncoder().encode(JSON.stringify({ id: crypto.randomUUID(),
      fileName: request.args.fileName, contentType: request.args.contentType, size: bytes.byteLength })))
    case 'geolocation.location': return { latitude: 51.5, longitude: -0.12, timestamp: '2026-10-14T00:00:00Z',
      altitude: null, accuracy: 8, verticalAccuracy: null, reducedAccuracy: false,
      speed: null, course: null, isFromMockProvider: false, altitudeReferenceSystem: 'unspecified' }
    case 'preferences.get': return { kind: 'string', text: 'Native preference' }
    case 'preferences.set':
    case 'preferences.remove': return { accepted: true }
    case 'sample.work': return { completed: request.args.steps, total: request.args.steps }
    case 'window.info':
    case 'window.title':
    case 'window.bounds': return { id: 'native-window', title: request.args.title ?? 'Native window',
      x: 80, y: 80, width: 1000, height: 760, canResize: true, canPosition: true, canOpenWindows: true,
      initialLocation: 'index.html#/showcase', windowLease: 'private-lease' }
    case 'window.find': return { window: { id: request.args.id, title: 'Child', x: 80, y: 80, width: 1000, height: 760,
      canResize: true, canPosition: true, canOpenWindows: true, initialLocation: 'index.html#/showcase', windowLease: 'child-lease' } }
    case 'window.list': return { windows: [] }
    case 'window.open': return { id: 'child-window' }
    case 'dialogs.prompt': return { cancelled: true, text: null }
    case 'filePicker.pick': return null
    case 'battery.info': return { chargeLevel: 0.5, state: 'charging', powerSource: 'ac', energySaverStatus: 'off' }
    case 'display.info': return { width: 1080, height: 1920, density: 3, orientation: 'portrait', rotation: 'rotation0', refreshRate: 60 }
    case 'remote.info': return { supported: true, open: false, id: null, url: null, status: 'closed', error: null, navigationResult: null, canGoBack: false, canGoForward: false }
    case 'remote.open': return { supported: true, open: true, id: 'guest', url: request.args.url, status: 'loading', error: null, navigationResult: null, canGoBack: false, canGoForward: false }
    case 'remote.bounds':
    case 'remote.control': return { supported: true, open: false, id: null, url: null, status: 'closed', error: null, navigationResult: null, canGoBack: false, canGoForward: false }
    case 'menus.info': return { supported: true, supportsChecked: true, sourceId: null }
    case 'menus.install': return { supported: true, supportsChecked: true, sourceId: 'menu-source' }
    case 'menus.update':
    case 'menus.clear': return { supported: true, supportsChecked: true, sourceId: request.command === 'menus.clear' ? null : 'menu-source' }
    case 'application.theme': return { effective: 'light', preference: 'unspecified' }
    case 'application.setTheme': return { effective: request.args.theme === 'dark' ? 'dark' : 'light', preference: request.args.theme }
    default: return { accepted: true }
  }
}

function fixture(t, handler = defaultResult, { native = true, compact = false } = {}) {
  document.body.innerHTML = new DOMParser().parseFromString(html, 'text/html').body.innerHTML
  window.history.replaceState(null, '', '/')
  const dom = { window }
  const host = transport(handler)
  const session = new PlaygroundSession()
  if (native) session.setClient(configureBridge(host.bridge))
  let reloads = 0
  const compactMode = () => typeof compact === 'function' ? compact() : compact
  let shell = mountShell(dom.window.document, session, () => { reloads++ }, compactMode)
  flushSync()
  t.onTestFinished(() => {
    shell.dispose()
    session.dispose()
    resetBridge()
    flushSync()
    document.body.replaceChildren()
  })
  const find = selector => {
    flushSync()
    const value = dom.window.document.querySelector(selector)
    assert.ok(value, `Missing ${selector}`)
    return value
  }
  return {
    dom, host, session, find,
    get shell() { return shell },
    get reloads() { return reloads },
    select: id => { find(`[data-feature="${id}"]`).click(); flushSync() },
    click: action => { find(`#controls [data-action="${action}"]`).click(); flushSync() },
    input: (selector, value) => {
      const input = find(selector)
      input.value = value
      input.dispatchEvent(new dom.window.Event('input', { bubbles: true }))
      if (input instanceof HTMLSelectElement) input.dispatchEvent(new dom.window.Event('change', { bubbles: true }))
      flushSync()
    },
    remount: () => {
      shell.dispose()
      shell = mountShell(dom.window.document, session, () => { reloads++ }, compactMode)
      flushSync()
    },
    event: (topic, data, id, eventSession = session.client.session) => {
      session.client.receive(JSON.stringify({ version: 1, session: eventSession, id, topic, data }))
      flushSync()
    },
    idle: async () => {
      await setImmediate()
      flushSync()
      assert.equal(session.busy, false, 'form action should have finished')
    },
  }
}

test('typed registry mounts implemented experiments and both guides with actions and metadata', t => {
  const app = fixture(t)
  assert.deepEqual(Object.keys(components).sort(), Object.keys(actions).sort())
  assert.equal(app.find('#catalog-count').textContent, '24 experiments · 2 guides')
  for (const feature of navigationPages) {
    app.select(feature.id)
    assert.equal(app.find('#feature-title').textContent, feature.title)
    assert.equal(app.find('#feature-meta').textContent, feature.category)
    assert.equal(app.find('#feature-description').textContent, feature.description)
    assert.equal(app.find('#api-link').href, feature.docs)
    assert.equal(app.find(`[data-feature="${feature.id}"]`).getAttribute('aria-current'), 'page')
    assert.deepEqual([...app.find('#controls').querySelectorAll('[data-action]')].map(button => button.dataset.action), actions[feature.experiment])
    assert.equal(app.find('#progress').hidden, true, 'session progress waits for an active operation')
  }
  assert.ok(app.host.requests.every(request => request.command === 'window.info'),
    'only the live current-window page starts a native observation on mount')
})

test('share page stages browser PDF bytes then invokes native single/multi/text shapes without claiming consumption', async t => {
  const previousCreate = URL.createObjectURL, previousRevoke = URL.revokeObjectURL
  URL.createObjectURL = () => 'blob:browser-pdf-test'
  URL.revokeObjectURL = () => {}
  t.onTestFinished(() => { URL.createObjectURL = previousCreate; URL.revokeObjectURL = previousRevoke })
  const app = fixture(t, (request, bytes) => {
    if (request.command === 'share.stage' && request.args.contentType === 'application/pdf')
      assert.equal(new TextDecoder().decode(bytes).slice(0, 8), '%PDF-1.4')
    return defaultResult(request, bytes)
  })
  app.select('share')
  assert.equal(app.find('[data-action="share-pdf"]').disabled, true)
  app.input('#pdf-message', 'Edited browser-generated document')
  app.click('share-stage-pdf')
  await app.idle()
  assert.equal(app.find('[data-action="share-pdf"]').disabled, false, app.find('#result-json').textContent)
  app.click('share-pdf')
  await app.idle()
  const shared = app.host.requests.findLast(request => request.command === 'share.request')
  assert.ok(shared, app.find('#result-json').textContent)
  const single = shared.args.request
  assert.deepEqual(Object.keys(single.file), ['id'])
  assert.equal(single.title, 'Share browser-produced PDF')
  assert.match(app.find('#result-json').textContent, /not known/)
  app.click('share-multiple')
  await app.idle()
  assert.equal(app.host.requests.findLast(request => request.command === 'share.request').args.request.files.length, 2)
  app.click('share-text')
  await app.idle()
  assert.equal(typeof app.host.requests.findLast(request => request.command === 'share.request').args.request.text, 'string')
  app.select('clipboard')
  await app.idle()
  assert.equal(app.host.requests.filter(request => request.command === 'share.release').length, 2)
})

test('geolocation panel displays explicitly requested native coordinates without copying them into operation logs', async t => {
  const app = fixture(t)
  app.select('geolocation')
  assert.equal(app.host.requests.some(request => request.command === 'geolocation.location'), false)
  app.click('geolocation-read')
  await app.idle()
  assert.match(app.find('[data-result="native-location"]').textContent, /51.5/)
  assert.doesNotMatch(app.find('#result-json').textContent, /51\.5|latitude|longitude/)
  assert.deepEqual(app.host.requests.find(request => request.command === 'geolocation.location').args,
    { request: { desiredAccuracy: 'medium', timeout: '00:00:20', requestFullAccuracy: false } })
})

test('lifetime page reads local and explicitly shared target identities through normal annotated actions', async t => {
  const app = fixture(t)
  app.select('lifetime')
  app.click('read-targets')
  await app.idle()
  const result = JSON.parse(app.find('#result-json').textContent)
  assert.equal(result.local.instanceId, 'sample.localCounter')
  assert.equal(result.shared.instanceId, 'sample.sharedCounter')
})

test('binary experiment roundtrips a real browser Blob over the separate byte primitive and reports exact proof', async t => {
  // jsdom's Blob omits arrayBuffer; supply the standard binary API, not a native transport fallback.
  const previousBlob = globalThis.Blob
  globalThis.Blob = NodeBlob
  t.onTestFinished(() => { globalThis.Blob = previousBlob })
  const app = fixture(t, (request, input) => {
    if (request.command !== 'sample.binaryEcho') return defaultResult(request)
    assert.deepEqual(request.args, {})
    assert.equal(input.byteLength, 262144)
    const bytes = new Uint8Array(input)
    assert.ok(bytes.every((value, index) => value === index % 256))
    return bytes
  })
  app.select('arguments')
  app.click('binary-roundtrip')
  await app.idle()
  assert.deepEqual(JSON.parse(app.find('#result-json').textContent), { exact: true, bytes: 262144, all256Values: true, transport: 'Core binary' })
  assert.equal(app.host.requests.filter(request => request.command === 'sample.binaryEcho').length, 1)
})

test('binary experiment consumes ordered chunks and progress after async work using the shared operation cancellation pattern', async t => {
  const buffers = new Map(), pending = new Map(), consumed = []
  let app
  app = fixture(t, async request => {
    if (request.command === 'core.channel.fetch') return buffers.get(request.args.dataId)
    if (request.command === 'core.channel.ack') {
      assert.equal(request.args.accepted, true)
      consumed.push(request.args.channelId)
      pending.get(request.args.channelId).resolve()
      return { accepted: true }
    }
    if (request.command !== 'sample.binaryChunks') return defaultResult(request)
    for (let i = 1; i <= 4; i++) {
      const dataId = crypto.randomUUID(), raw = deferred()
      buffers.set(dataId, Uint8Array.from({ length: 65536 }, (_, n) => n % 256))
      pending.set(request.args.chunks, raw)
      app.event('core.channel', { channelId: request.args.chunks, sequence: i, kind: 'bytes', dataId }, request.id)
      await raw.promise
      const update = deferred()
      pending.set(request.args.progress, update)
      app.event('core.channel', { channelId: request.args.progress, sequence: i, kind: 'json', value: { completed: i, total: 4 } }, request.id)
      await update.promise
    }
    for (const id of [request.args.chunks, request.args.progress])
      app.event('core.channel', { channelId: id, sequence: 5, kind: 'complete' }, request.id)
    return { accepted: true }
  })
  app.select('arguments')
  app.click('binary-chunks')
  assert.equal(app.find('[data-action="cancel"]').disabled, false)
  for (let i = 0; i < 100 && app.session.busy; i++) await new Promise(resolve => setTimeout(resolve, 10))
  await app.idle()
  assert.equal(consumed.length, 8)
  assert.ok(app.find('[data-testid="binary-stream-proof"]').textContent.includes('4 ordered chunks'))
  assert.ok(app.find('#result-json').textContent.includes('262144'))
  assert.equal(app.find('[data-action="cancel"]').disabled, true)
})

test('navigation groups are heading disclosures with a decorative arrow, not collapse-label buttons', t => {
  const app = fixture(t)
  const disclosure = app.find('[data-group="Welcome"]')
  assert.equal(disclosure.closest('h3')?.className, 'group-heading')
  assert.equal(disclosure.textContent.trim(), 'Welcome')
  assert.equal(disclosure.querySelector('svg')?.getAttribute('aria-hidden'), 'true')
  disclosure.click()
  flushSync()
  assert.equal(disclosure.getAttribute('aria-expanded'), 'false')
  assert.equal(app.find(`#${disclosure.getAttribute('aria-controls')}`).hidden, true)
})

test('action sheet passes editable native labels/options and preserves selection, cancel and dismissal', async t => {
  let selection = 'Share'
  const app = fixture(t, request => request.command === 'dialogs.actionSheet'
    ? selection : defaultResult(request))
  app.select('dialogs')
  assert.deepEqual([...app.find('#controls').querySelectorAll('.dialog-section h3')].map(heading => heading.textContent),
    ['Alert', 'Confirmation', 'Text prompt', 'Action sheet'])
  app.input('#action-sheet-title', 'Choose an action')
  app.input('#action-sheet-buttons', 'Duplicate\nShare\nExport')
  app.input('#action-sheet-cancel', 'Not now')
  app.input('#action-sheet-destruction', 'Delete')
  app.input('#action-sheet-direction', 'rightToLeft')
  app.click('action-sheet')
  await app.idle()
  assert.deepEqual(app.host.requests.at(-1).args, {
    title: 'Choose an action', buttons: ['Duplicate', 'Share', 'Export'],
    cancel: 'Not now', destruction: 'Delete', flowDirection: 'rightToLeft',
  })
  assert.equal(app.find('#result-json').textContent, 'Share')
  selection = 'Not now'
  app.click('action-sheet')
  await app.idle()
  assert.equal(app.find('#result-json').textContent, 'Not now')
  selection = null
  app.input('#action-sheet-destruction', '')
  app.click('action-sheet')
  await app.idle()
  assert.equal(app.host.requests.at(-1).args.destruction, null)
  assert.equal(JSON.parse(app.find('#result-json').textContent), null)
})

test('battery presentation formats a native float as a percentage without changing its raw result', async t => {
  const level = 0.009999999776482582
  const app = fixture(t, request => request.command === 'battery.info'
    ? { chargeLevel: level, state: 'charging', powerSource: 'ac', energySaverStatus: 'off' }
    : defaultResult(request))
  app.select('battery')
  app.click('battery')
  await app.idle()
  assert.match(app.find('#battery-charge').textContent, /1%/)
  assert.equal(JSON.parse(app.find('#result-json').textContent).chargeLevel, level)
})

test('search temporarily reveals grouped matches and preserves collapse preferences and keyboard focus', t => {
  const app = fixture(t)
  app.select('clipboard')
  assert.equal(app.dom.window.document.activeElement.dataset.feature, 'clipboard')
  app.find('[data-group="Development"]').click()
  assert.equal(app.find('[data-group="Development"]').getAttribute('aria-expanded'), 'false')
  app.input('#search', 'TEXT CLIPBOARD')
  assert.equal(app.find('#catalog-count').textContent, '1 matching pages')
  assert.equal(app.find('#feature-list').children.length, 1)
  app.input('#search', 'no matching phrase')
  assert.match(app.find('#feature-list').textContent, /No matching features/)
  app.input('#search', 'Vite')
  assert.equal(app.find('[data-group="Development"]').getAttribute('aria-expanded'), 'true')
  app.input('#search', '')
  assert.equal(app.find('[data-group="Development"]').getAttribute('aria-expanded'), 'false')
  assert.equal(app.find('#catalog-count').textContent, '24 experiments · 2 guides')
  app.find('[data-group="Essentials"]').click()
  app.session.writeLog('A native refresh', 'Success', { accepted: true })
  assert.equal(app.find('[data-group="Essentials"]').getAttribute('aria-expanded'), 'false',
    'unrelated session refresh must not replace an active group collapse preference')
})

test('compact navigation focuses the experiment instead of a replaced catalog button', t => {
  const app = fixture(t, defaultResult, { compact: true })
  app.find('#navigation-toggle').click()
  flushSync()
  app.select('preferences')
  assert.equal(app.dom.window.document.activeElement.id, 'experiment')
})

test('desktop panes collapse without unmounting or clearing shared session state; refresh belongs to Session', t => {
  const app = fixture(t, defaultResult, { native: false })
  app.session.writeLog('Persistent entry', 'Success', { value: 7 })
  const browser = { id: 'retained-native-guest' }
  const menu = { id: 'retained-native-menu' }
  const release = async () => {}
  app.session.browser = browser
  app.session.menu = menu
  app.session.unsubscribeCounter = release
  const navigation = app.find('#feature-list')
  const log = app.find('#operation-log')
  for (const toggle of ['navigation', 'session']) {
    const button = app.find(`#${toggle}-toggle`)
    button.click(); flushSync()
    assert.equal(button.getAttribute('aria-expanded'), 'false')
    assert.equal(app.find(`#${toggle}-pane`).open, false)
    button.click(); flushSync()
    assert.equal(button.getAttribute('aria-expanded'), 'true')
    assert.equal(app.find(`#${toggle}-pane`).open, true)
  }
  assert.equal(app.find('#feature-list'), navigation)
  assert.equal(app.find('#operation-log'), log)
  assert.match(log.textContent, /Persistent entry/)
  assert.equal(app.session.browser, browser)
  assert.equal(app.session.menu, menu)
  assert.equal(app.session.unsubscribeCounter, release)
  assert.equal(app.find('.app-header').querySelector('#frontend-refresh'), null)
  assert.equal(app.find('#frontend-refresh').closest('#session-pane'), app.find('#session-pane'))
  app.find('#frontend-refresh').click()
  assert.equal(app.reloads, 1)
  assert.equal(app.host.requests.length, 0, 'refresh does not issue a native restart/navigation command')
})

test('mobile flyouts are mutually exclusive, Escape returns focus, selection closes navigation', async t => {
  const app = fixture(t, defaultResult, { compact: true })
  const navigation = app.find('#navigation-pane'), diagnostics = app.find('#session-pane')
  assert.equal(navigation.open, false)
  assert.equal(diagnostics.open, false)
  app.find('#navigation-toggle').click()
  flushSync()
  await setImmediate(); flushSync()
  assert.equal(navigation.open, true)
  assert.equal(navigation.getAttribute('aria-modal'), 'true')
  assert.equal(navigation.dataset.browserObscured, 'true')
  assert.equal(document.activeElement.id, 'search')
  app.find('#session-toggle').click()
  flushSync()
  await setImmediate(); flushSync()
  assert.equal(navigation.open, false)
  assert.equal(diagnostics.open, true)
  assert.equal(app.find('#navigation-toggle').getAttribute('aria-expanded'), 'false')
  diagnostics.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }))
  flushSync()
  assert.equal(diagnostics.open, false)
  assert.equal(document.activeElement.id, 'session-toggle')
  assert.equal(diagnostics.dataset.browserObscured, 'false')
  app.find('#navigation-toggle').click(); flushSync()
  app.select('clipboard')
  assert.equal(navigation.open, false)
  assert.equal(document.activeElement.id, 'experiment')
  assert.equal(app.find('#navigation-toggle').getAttribute('aria-expanded'), 'false')
})

test('flyouts contain keyboard focus and close only for actual outside clicks; mobile refresh uses the same document', async t => {
  const app = fixture(t, defaultResult, { compact: true, native: false })
  app.find('#session-toggle').click(); flushSync()
  await setImmediate(); flushSync()
  const dialog = app.find('#session-pane'), first = dialog.querySelector('.pane-close')
  const clear = app.find('#clear-log')
  // Summary follows the log and remains a focusable control when details are closed.
  const summary = dialog.querySelector('summary')
  summary.focus()
  summary.dispatchEvent(new KeyboardEvent('keydown', { key: 'Tab', bubbles: true, cancelable: true }))
  assert.equal(document.activeElement, first)
  first.dispatchEvent(new KeyboardEvent('keydown', { key: 'Tab', shiftKey: true, bubbles: true, cancelable: true }))
  assert.equal(document.activeElement, summary)
  app.find('#frontend-refresh').click(); flushSync()
  assert.equal(app.reloads, 1)
  assert.equal(dialog.open, true)
  dialog.getBoundingClientRect = () => new DOMRect(30, 0, 360, 844)
  clear.click(); flushSync()
  assert.equal(dialog.open, true, 'interacting with panel content is not an outside click')
  dialog.dispatchEvent(new MouseEvent('click', { bubbles: true, clientX: 10, clientY: 400 }))
  flushSync()
  assert.equal(dialog.open, false)
  assert.equal(document.activeElement.id, 'session-toggle')
  assert.equal(app.host.requests.length, 0)
})

test('session separator supports keyboard and pointer resizing, clamps both ends and becomes inactive on mobile', t => {
  let compact = false
  const app = fixture(t, defaultResult, { compact: () => compact })
  const separator = app.find('.pane-separator')
  const key = name => {
    separator.dispatchEvent(new KeyboardEvent('keydown', { key: name, bubbles: true, cancelable: true }))
    flushSync()
  }
  const width = () => Number(separator.getAttribute('aria-valuenow'))
  assert.equal(width(), 300)
  key('ArrowLeft'); assert.equal(width(), 324)
  key('ArrowRight'); assert.equal(width(), 300)
  key('Home'); assert.equal(width(), 240)
  key('ArrowRight'); assert.equal(width(), 240)
  key('End'); assert.equal(width(), Number(separator.getAttribute('aria-valuemax')))
  const pointer = (target, name, x, id = 7) => {
    const event = new Event(name, { bubbles: true, cancelable: true })
    Object.assign(event, { clientX: x, pointerId: id, button: 0 })
    target.dispatchEvent(event); flushSync()
  }
  pointer(separator, 'pointerdown', 700)
  pointer(window, 'pointermove', 3000)
  assert.equal(width(), 240)
  pointer(window, 'pointermove', -3000)
  assert.equal(width(), Number(separator.getAttribute('aria-valuemax')))
  pointer(window, 'pointerup', 700)
  const previous = width()
  pointer(window, 'pointermove', 500)
  assert.equal(width(), previous)
  assert.equal(sessionWidthLimit(1100, true), 440)
  assert.equal(sessionWidthLimit(1001, true), 381)
  compact = true
  window.dispatchEvent(new Event('resize')); flushSync()
  assert.equal(separator.hidden, true)
  assert.equal(separator.tabIndex, -1)
  assert.equal(app.find('#navigation-pane').open, false)
  assert.equal(app.find('#session-pane').open, false)
  key('Home')
  assert.equal(width(), previous)
  pointer(separator, 'pointerdown', 700)
  pointer(window, 'pointermove', 3000)
  assert.equal(width(), previous)
})

test('session progress and cancellation remain accessible after navigating and collapsing diagnostics', async t => {
  const work = deferred()
  const app = fixture(t, request => request.command === 'sample.work' ? work.promise : defaultResult(request))
  app.select('work'); app.click('work')
  await setImmediate(); flushSync()
  const request = app.session.active.id
  app.select('preferences')
  app.find('#session-toggle').click(); flushSync()
  app.event('operation.progress', { completed: 5, total: 20 }, request)
  assert.equal(app.find('#progress').value, 5)
  assert.equal(app.find('#progress').closest('#session-pane'), app.find('#session-pane'))
  assert.match(app.find('#current-operation').textContent, /Run native work/)
  app.find('#header-cancel').click()
  await setImmediate()
  assert.equal(app.host.requests.at(-1).command, 'core.cancel')
  assert.equal(app.host.requests.at(-1).args.requestId, request)
  app.find('#session-toggle').click(); flushSync()
  assert.equal(app.find('#progress').value, 5)
  assert.equal(app.host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  work.resolve(failure('cancelled', 'cancelled'))
  await app.idle()
})

test('desktop panes do not obscure native guests, mobile flyouts use the existing geometry contract', t => {
  let compact = false
  const app = fixture(t, defaultResult, { compact: () => compact })
  const slot = document.createElement('div')
  slot.getBoundingClientRect = () => new DOMRect(300, 100, 400, 300)
  app.find('#experiment').append(slot)
  assert.equal(measureBrowserSlot(slot).visible, true)
  compact = true
  window.dispatchEvent(new Event('resize')); flushSync()
  assert.equal(measureBrowserSlot(slot).visible, true)
  app.find('#navigation-toggle').click(); flushSync()
  assert.equal(measureBrowserSlot(slot).visible, false)
  app.find('#navigation-pane').dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }))
  flushSync()
  assert.equal(measureBrowserSlot(slot).visible, true)
})

test('current window readouts follow real scoped callbacks and unregister on teardown; Mac never subscribes to move events', async t => {
  const app = fixture(t)
  app.select('windows')
  await setImmediate(); flushSync()
  assert.equal(app.find('#window-live-size').textContent, '1000 × 760')
  assert.equal(app.find('[data-action="center"]').disabled, true)
  const sources = app.host.requests.filter(request => request.command === 'core.subscribe' && request.args.topic === 'window.geometryChanged')
  assert.equal(sources.length, 1)
  app.event('window.geometryChanged', { windowId: 'native-window', size: { width: 930, height: 680 },
    position: { x: 20, y: 30 } }, 'geometry-1')
  assert.equal(app.find('#window-live-size').textContent, '930 × 680')
  assert.equal(app.find('#window-live-position').textContent, '80, 80', 'unverified Mac move callback is ignored')
  assert.equal(app.find('#window-live-events').textContent, '1 size · 0 position')
  app.event('window.geometryChanged', { windowId: 'another-window', size: { width: 500, height: 400 }, position: null }, 'geometry-2')
  assert.equal(app.find('#window-live-size').textContent, '930 × 680')
  app.select('preferences')
  await setImmediate(); flushSync()
  assert.equal(app.host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
  const entries = app.session.entries.length
  app.event('window.geometryChanged', { windowId: 'native-window', size: { width: 900, height: 600 }, position: null }, 'geometry-late')
  assert.equal(app.session.entries.length, entries)
  app.select('windows')
  await setImmediate(); flushSync()
  assert.equal(app.host.requests.filter(request => request.command === 'core.subscribe').length, 2)
})

test('Windows supports real position callbacks and shares one native geometry subscription', async t => {
  const app = fixture(t, request => {
    const value = defaultResult(request)
    return request.command === 'window.info' ? { ...value, canCenter: true, canObservePosition: true } : value
  })
  app.select('windows')
  await setImmediate(); flushSync()
  assert.equal(app.find('[data-action="center"]').disabled, false)
  app.event('window.geometryChanged', { windowId: 'native-window', size: { width: 900, height: 600 },
    position: { x: 37, y: 91 } }, 'windows-frame')
  assert.equal(app.find('#window-live-size').textContent, '900 × 600')
  assert.equal(app.find('#window-live-position').textContent, '37, 91')
  assert.equal(app.find('#window-live-events').textContent, '1 size · 1 position')
  assert.equal(app.host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  app.select('clipboard')
  await setImmediate()
  assert.equal(app.host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
})

test('an older async window read cannot overwrite a newer native geometry callback', async t => {
  const snapshot = deferred()
  let delay = false
  const app = fixture(t, request => {
    if (request.command === 'window.info' && delay) { delay = false; return snapshot.promise }
    return defaultResult(request)
  })
  app.select('windows')
  await setImmediate(); flushSync()
  delay = true
  app.click('window-info')
  app.event('window.geometryChanged', { windowId: 'native-window',
    size: { width: 975, height: 690 }, position: null }, 'newer-frame')
  snapshot.resolve(defaultResult({ command: 'window.info', args: {} }))
  await app.idle()
  assert.equal(app.find('#window-live-size').textContent, '975 × 690')
})

test('unsupported mobile placement is disabled while actual native size observation remains useful', async t => {
  const app = fixture(t, request => request.command === 'window.info'
    ? { ...defaultResult(request), canResize: false, canPosition: false, canOpenWindows: false }
    : defaultResult(request))
  app.select('windows')
  await setImmediate(); flushSync()
  for (const action of ['bounds', 'position', 'center']) {
    assert.equal(app.find(`[data-action="${action}"]`).disabled, true)
    app.click(action)
  }
  assert.equal(app.find('#window-live-position').textContent, 'Unsupported on this host')
  app.event('window.geometryChanged', { windowId: 'native-window',
    size: { width: 390, height: 844 }, position: null }, 'mobile-frame')
  assert.equal(app.find('#window-live-size').textContent, '390 × 844')
  assert.equal(app.host.requests.some(request => ['window.size', 'window.position', 'window.center'].includes(request.command)), false)
})

test('opening a flyout queues immediate native guest hiding behind an in-flight bounds update', async t => {
  const firstBounds = deferred()
  let count = 0
  const app = fixture(t, request => request.command === 'remote.bounds' && ++count === 1
    ? firstBounds.promise : defaultResult(request), { compact: true })
  app.select('remote-webview')
  app.find('#browser-slot').getBoundingClientRect = () => new DOMRect(10, 100, 300, 300)
  app.click('browser-info'); await app.idle()
  app.click('remote')
  await setImmediate(); flushSync()
  assert.equal(app.host.requests.find(request => request.command === 'remote.bounds').args.visible, true)
  app.find('#session-toggle').click(); flushSync()
  await setImmediate()
  firstBounds.resolve(defaultResult({ command: 'remote.bounds', args: {} }))
  await app.idle()
  assert.equal(app.host.requests.filter(request => request.command === 'remote.bounds').at(-1).args.visible, false)
  assert.equal(app.host.requests.filter(request => request.command === 'remote.bounds').length, 2,
    'overlay hiding is sent as soon as the in-flight geometry request finishes')
})

test('late window registration is released and late setters do not read or mutate a replacement page', async t => {
  const registration = deferred(), title = deferred()
  const app = fixture(t, request => request.command === 'core.subscribe' ? registration.promise
    : request.command === 'window.title' ? title.promise : defaultResult(request))
  app.select('windows')
  await setImmediate()
  app.select('clipboard')
  const requests = app.host.requests.length
  registration.resolve({ accepted: true })
  await setImmediate()
  assert.equal(app.host.requests.length, requests + 1)
  assert.equal(app.host.requests.at(-1).command, 'core.unsubscribe')
  app.select('windows')
  await setImmediate(); flushSync()
  app.click('title')
  app.select('preferences')
  const before = app.host.requests.filter(request => request.command === 'window.info').length
  title.resolve(defaultResult({ command: 'window.title', args: { title: 'My MAUI window' } }))
  await app.idle()
  assert.equal(app.host.requests.filter(request => request.command === 'window.info').length, before,
    'late setter completion does not start fresh native reads')
  assert.equal(app.session.selected.id, 'preferences')
})

test('multiple windows owns creation options and closes only retained caller-owned child generations', async t => {
  let opened = false
  const child = { id: 'child-window', windowLease: 'private-child-generation' }
  const app = fixture(t, request => {
    if (request.command === 'window.open') { opened = true; return child }
    if (request.command === 'window.list') return { windows: [
      { id: 'native-window', windowLease: 'private-lease' },
      { id: 'not-our-child', windowLease: 'other-generation' },
      ...(opened ? [child] : []),
    ] }
    if (request.command === 'window.info' && request.args.windowId && request.args.windowId !== 'native-window')
      return { ...defaultResult(request), id: request.args.windowId,
        title: 'Child title', initialLocation: 'index.html#/clipboard',
        windowLease: request.args.windowId === child.id ? child.windowLease : 'other-generation' }
    return defaultResult(request)
  })
  app.select('multi-window')
  assert.equal(app.find('#controls').querySelector('#window-title'), null)
  assert.equal(app.find('#controls').querySelector('[data-action="bounds"]'), null)
  app.click('window-list'); await app.idle()
  assert.equal(app.find('#owned-windows').querySelectorAll('[data-action^="close-"]').length, 0)
  app.input('#child-url', 'index.html?example=1#/clipboard')
  app.input('#child-id', 'my-child')
  app.input('#child-title', 'Child title')
  app.find('#child-custom-size').click(); flushSync()
  app.input('#child-width', '900'); app.input('#child-height', '650')
  app.click('open-window'); await app.idle()
  assert.deepEqual(app.host.requests.find(request => request.command === 'window.open').args, {
    url: 'index.html?example=1#/clipboard', options: { id: 'my-child', title: 'Child title', width: 900, height: 650 },
  })
  assert.equal(app.find('#owned-windows').querySelectorAll('[data-action^="close-"]').length, 1)
  const handle = app.session.childWindows.get(child.id)
  app.select('clipboard'); app.select('multi-window')
  app.click('window-list'); await app.idle()
  assert.equal(app.session.childWindows.get(child.id), handle)
  app.click('close-child-window'); await app.idle()
  assert.deepEqual(app.host.requests.find(request => request.command === 'window.close').args, {
    id: child.id, windowLease: child.windowLease,
  })
  assert.equal(app.host.requests.some(request => request.command === 'window.closeCurrent'), false)
  assert.equal(app.session.childWindows.has(child.id), false)
})

test('browser-only actions remain disabled on every page, with only Reload enabled and no emulation', async t => {
  const app = fixture(t, defaultResult, { native: false })
  const connection = createConnection(app.session, async () => undefined)
  await connection.start()
  assert.equal(app.find('#host-status').dataset.state, 'unavailable')
  assert.equal(app.find('#native-unavailable').hidden, false)
  for (const feature of availableFeatures) {
    app.select(feature.id)
    for (const button of app.find('#controls').querySelectorAll('[data-action]')) {
      assert.equal(button.disabled, button.dataset.action !== 'reload', `${feature.id}: ${button.dataset.action}`)
      if (button.dataset.action !== 'reload') button.click()
    }
  }
  assert.equal(app.host.requests.length, 0)
  assert.equal(app.find('#native-identity').textContent, 'No native identity received.')
  app.select('identity')
  app.click('reload')
  assert.equal(app.reloads, 1)
})

test('replacement removes old form/navigation handlers and shell remount does not multiply handlers or groups', async t => {
  const app = fixture(t)
  app.select('arguments')
  const oldAction = app.find('[data-action="counter"]')
  app.select('arguments')
  oldAction.click()
  assert.equal(app.session.selected.id, 'arguments')
  assert.equal(app.host.requests.length, 0)
  const categoryCount = app.find('#feature-list').querySelectorAll('[data-group]').length
  app.remount()
  app.remount()
  assert.equal(app.find('#feature-list').querySelectorAll('[data-group]').length, categoryCount)
  app.click('counter')
  await app.idle()
  assert.equal(app.host.requests.length, 1)
  assert.equal(app.find('#operation-log').children.length, 1)
})

test('clipboard and preference pages own actual SDK calls, values, defaults and limits', async t => {
  const app = fixture(t)
  app.select('clipboard')
  const clipboard = app.find('#clipboard-text')
  assert.equal(clipboard.maxLength, 4096)
  assert.match(clipboard.value, /你好 👋/)
  assert.match(clipboard.value, /backslash \\/)
  app.input('#clipboard-text', 'Copy 👋\n"quote" \\ slash')
  app.click('copy')
  await app.idle()
  assert.deepEqual(app.host.requests.at(-1).args, { text: clipboard.value })
  app.click('paste')
  await app.idle()
  assert.equal(clipboard.value, 'Native clipboard 👋')
  app.select('preferences')
  assert.equal(app.find('#preference-key').value, 'example')
  assert.equal(app.find('#preference-key').maxLength, 48)
  assert.equal(app.find('#preference-key').pattern, '[a-zA-Z0-9_-]{1,48}')
  assert.equal(app.find('#preference-value').maxLength, 4096)
  app.input('#preference-key', 'custom_key')
  app.input('#preference-value', 'Saved text')
  app.click('save')
  await app.idle()
  assert.deepEqual(app.host.requests.at(-1).args, { key: 'custom_key', value: { kind: 'string', text: 'Saved text' }, sharedName: null })
  app.click('read')
  await app.idle()
  assert.equal(app.find('#preference-value').value, 'Native preference')
  app.click('remove')
  await app.idle()
  assert.equal(app.host.requests.at(-1).command, 'preferences.remove')
  assert.deepEqual(JSON.parse(app.find('#result-json').textContent), { key: 'custom_key', value: null })
})

test('commands validate input and identity/lifetime/development pages call the app helpers', async t => {
  const app = fixture(t)
  app.select('identity')
  app.click('run')
  await app.idle()
  assert.deepEqual(JSON.parse(app.find('#native-identity').textContent), identity)
  assert.deepEqual(app.host.requests.map(request => request.command), ['sample.diagnostics', 'core.connect', 'app.info', 'device.info'])
  app.select('arguments')
  const dto = { text: null, number: 2.5, flag: false, items: ['Unicode 👋'] }
  app.input('#echo-input', JSON.stringify(dto))
  app.click('echo')
  await app.idle()
  assert.deepEqual(app.host.requests.at(-1).args, dto)
  app.input('#echo-input', '{bad JSON')
  app.click('echo')
  await app.idle()
  assert.equal(app.find('#result-state').dataset.state, 'error')
  assert.equal(JSON.parse(app.find('#result-json').textContent).code, 'input_error')
  assert.equal(app.host.requests.length, 5)
  app.select('lifetime')
  app.click('increment')
  await app.idle()
  assert.deepEqual(app.host.requests.at(-1).args, { text: null, number: 0, flag: false, items: [] })
  app.click('counter')
  await app.idle()
  assert.equal(app.host.requests.at(-1).command, 'sample.counter')
  app.click('reload')
  assert.equal(app.reloads, 1)
  app.select('vite')
  app.click('identity')
  await app.idle()
  assert.deepEqual(JSON.parse(app.find('#result-json').textContent), identity)
})

test('native error and policy codes are displayed/logged accurately and the bridge recovers', async t => {
  const codes = { 'sample.fail': 'intentional_error', 'sample.notRegistered': 'unknown_command',
    'sample.disabled': 'disabled_command', 'sample.echo': 'invalid_arguments' }
  const app = fixture(t, request => codes[request.command] ? failure(codes[request.command]) : defaultResult(request))
  for (const [feature, action, code] of [['errors', 'fail', 'intentional_error'], ['policy', 'missing', 'unknown_command'],
    ['policy', 'disabled', 'disabled_command'], ['policy', 'invalid', 'invalid_arguments']]) {
    app.select(feature)
    app.click(action)
    await app.idle()
    assert.equal(app.find('#result-state').textContent, 'Error')
    assert.equal(JSON.parse(app.find('#result-json').textContent).code, code)
    assert.equal(app.find('#operation-log').firstElementChild.dataset.state, 'error')
  }
  app.select('errors')
  app.click('counter')
  await app.idle()
  assert.equal(app.find('#result-state').textContent, 'Success')
  assert.equal(app.find('#result-json').textContent, '7')
})

test('transport failures are distinct from native errors in the shared result', async t => {
  const app = fixture(t, () => { throw new TypeError('Fetch failed') })
  app.select('errors')
  app.click('fail')
  await app.idle()
  assert.equal(JSON.parse(app.find('#result-json').textContent).code, 'transport_error')
  assert.equal(app.find('#operation-log').firstElementChild.dataset.state, 'error')
})

for (const [feature, action, selector, command, output] of [
  ['clipboard', 'paste', '#clipboard-text', 'clipboard.read', 'Late clipboard'],
  ['preferences', 'read', '#preference-value', 'preferences.get', { kind: 'string', text: 'Late setting' }],
]) {
  test(`delayed ${command} captures its original input and cannot paint into a replacement page`, async t => {
    const pending = deferred()
    const app = fixture(t, request => request.command === command ? pending.promise : defaultResult(request))
    app.select(feature)
    const original = app.find(selector)
    const originalValue = original.value
    app.click(action)
    assert.equal(app.session.busy, true)
    app.select('arguments')
    app.select(feature)
    const replacement = app.find(selector)
    app.input(selector, 'Keep new input')
    pending.resolve(output)
    await app.idle()
    assert.equal(original.isConnected, false)
    assert.equal(original.value, originalValue)
    assert.equal(replacement.value, 'Keep new input')
    assert.equal(app.find('#result-state').textContent, 'Ready to run')
    assert.ok(!app.find('#result-json').textContent.includes('Late'))
    assert.match(app.find('#operation-log').textContent, /Late/)
  })
}

test('Welcome is the default, hands-on is distinct, and route parsing accepts only implemented pages', t => {
  const app = fixture(t)
  assert.equal(app.session.selected.id, 'welcome')
  assert.match(app.find('#controls').textContent, /Web-first/)
  app.select('showcase')
  assert.equal(window.location.hash, '#/showcase')
  assert.match(app.find('#controls').textContent, /Make this window yours/)
  for (const fragment of ['#/planned-90', '#/not-implemented', '#/%2fshowcase', '#/showcase?url=evil', '#https://example.com'])
    assert.equal(readRoute(fragment).page.id, 'welcome')
  assert.ok(readRoute('#/planned-90').error)
  assert.equal(readRoute('#/clipboard').page.id, 'clipboard')
})

test('native initial routes apply query and fragment without rewriting the loaded document path', t => {
  const app = fixture(t)
  const path = window.location.pathname
  app.session.initialLocation('index.html?mode=preview#/showcase')
  assert.equal(window.location.pathname, path, 'the native host already loaded the bundled document')
  assert.equal(window.location.search, '?mode=preview')
  assert.equal(window.location.hash, '#/showcase')
  assert.equal(app.session.selected.id, 'showcase')
})

test('hash navigation, remount, unknown-route recovery and native initial routes preserve trusted routing', async t => {
  const app = fixture(t)
  app.session.initialLocation('index.html#/showcase')
  assert.equal(window.location.hash, '#/showcase')
  assert.equal(app.session.selected.id, 'showcase')
  window.history.replaceState(null, '', '#/clipboard')
  window.dispatchEvent(new HashChangeEvent('hashchange'))
  app.find('#clipboard-text')
  app.remount()
  assert.equal(app.session.selected.id, 'clipboard')
  app.session.initialLocation('index.html#/showcase')
  assert.equal(app.session.selected.id, 'clipboard', 'frontend route wins over the initial host route on reload')
  window.history.replaceState(null, '', '#/unknown')
  window.dispatchEvent(new HashChangeEvent('hashchange'))
  assert.equal(app.session.selected.id, 'welcome')
  assert.equal(app.find('#result-state').textContent, 'Error')
  app.select('showcase')
  assert.equal(app.find('#result-state').textContent, 'Ready to run')
  assert.equal(app.host.requests.length, 0, 'fragment routes never become native command names')
})

test('browser history back and forward restore the selected internal feature without native calls', async t => {
  const app = fixture(t)
  app.select('showcase')
  await setImmediate()
  app.select('clipboard')
  await setImmediate()
  const changedTo = hash => new Promise(resolve => {
    const changed = () => {
      if (window.location.hash !== hash) return
      window.removeEventListener('hashchange', changed)
      resolve()
    }
    window.addEventListener('hashchange', changed)
  })
  const back = changedTo('#/showcase')
  window.history.back()
  await back
  assert.equal(app.session.selected.id, 'showcase')
  const forward = changedTo('#/clipboard')
  window.history.forward()
  await forward
  assert.equal(app.session.selected.id, 'clipboard')
  assert.equal(app.host.requests.length, 0)
})

test('native Windows, dialogs, device and embedded remote pages use the same SDK modules as hands-on', async t => {
  const app = fixture(t)
  app.select('windows')
  assert.equal(app.find('[data-action="bounds"]').disabled, true)
  app.click('window-info')
  await app.idle()
  assert.equal(app.find('[data-action="bounds"]').disabled, false)
  app.input('#window-title', 'Native window 👋')
  app.click('title')
  await app.idle()
  assert.deepEqual(app.host.requests.findLast(request => request.command === 'window.title').args,
    { title: 'Native window 👋', windowId: 'native-window', windowLease: 'private-lease' })
  app.click('bounds')
  await app.idle()
  assert.deepEqual(app.host.requests.findLast(request => request.command === 'window.size').args,
    { width: 1000, height: 760, windowId: 'native-window', windowLease: 'private-lease' })
  app.click('position')
  await app.idle()
  assert.deepEqual(app.host.requests.findLast(request => request.command === 'window.position').args,
    { x: 80, y: 80, windowId: 'native-window', windowLease: 'private-lease' })
  app.select('multi-window')
  app.click('open-window')
  await app.idle()
  assert.ok(app.host.requests.some(request => request.command === 'window.open' && request.args.url === 'index.html#/showcase'))
  app.select('dialogs')
  app.click('prompt')
  await app.idle()
  assert.deepEqual(JSON.parse(app.find('#result-json').textContent), { cancelled: true, text: null })
  for (const [page, action, command] of [['battery', 'battery', 'battery.info'], ['display', 'display', 'display.info'],
    ['showcase', 'prompt', 'dialogs.prompt']]) {
    app.select(page)
    app.click(action)
    await app.idle()
    assert.equal(app.host.requests.at(-1).command, command)
  }
  app.select('remote-webview')
  app.click('browser-info')
  await app.idle()
  app.click('remote')
  await app.idle()
  assert.match(app.find('#result-json').textContent, /guest/)
  const guest = defaultResult({ command: 'remote.open', args: { url: 'https://dot.net/maui' } })
  app.event('remote.changed', { ...guest, status: 'failed', navigationResult: 'timeout' }, crypto.randomUUID())
  assert.match(app.find('#browser-status').textContent, /failed.*timeout/)
  app.event('remote.changed', { ...guest, status: 'blocked', error: 'Only public HTTPS navigation is allowed.' }, crypto.randomUUID())
  assert.match(app.find('#browser-status').textContent, /blocked.*Only public HTTPS/)
})

test('follow-system theme button sends actual native unspecified without a string remapping', async t => {
  const app = fixture(t)
  app.select('theme')
  app.click('theme-system')
  await app.idle()
  assert.deepEqual(app.host.requests.findLast(request => request.command === 'application.setTheme').args, { theme: 'unspecified' })
})

test('selected text is rendered as text and excluded from result logs, with ordinary picker dismissal', async t => {
  let cancelled = false
  const app = fixture(t, request => {
    if (request.command === 'filePicker.pick') return cancelled ? null : { id: 'selected', name: 'example.txt', contentType: 'text/plain' }
    if (request.command === 'file.openRead') return { id: 'reader', canSeek: true, maximumChunkBytes: 65536 }
    if (request.command === 'file.readBytes') return Uint8Array.of(1, ...new TextEncoder().encode('<script>private</script>'))
    return defaultResult(request)
  })
  app.select('file-picker')
  app.click('pick-file')
  await app.idle()
  assert.equal(app.find('#file-preview').textContent, '<script>private</script>')
  assert.equal(app.find('#file-preview').children.length, 0)
  assert.doesNotMatch(app.find('#operation-log').textContent, /private/)
  assert.doesNotMatch(app.find('#result-json').textContent, /private/)
  cancelled = true
  app.click('pick-file')
  await app.idle()
  assert.equal(app.find('#file-cancelled').textContent, 'No file selected.')
  assert.equal(document.querySelector('#file-preview'), null)
})

test('delayed file selection cannot populate a new page or fresh picker form after navigation', async t => {
  const selected = deferred()
  const app = fixture(t, request => request.command === 'filePicker.pick' ? selected.promise : defaultResult(request))
  app.select('file-picker')
  app.click('pick-file')
  app.select('clipboard')
  app.select('file-picker')
  selected.resolve({ file: { id: 'old-selection', name: 'old.txt', contentType: 'text/plain' } })
  await app.idle()
  assert.equal(document.querySelector('#file-preview'), null)
  assert.equal(app.find('#result-state').textContent, 'Ready to run')
})

test('TypeScript creates the actual native menu tree and handles native item IDs through public modules', async t => {
  const app = fixture(t)
  app.select('menus')
  app.click('menus-install')
  await app.idle()
  const installed = app.host.requests.find(request => request.command === 'menus.install')
  assert.equal(installed.args.windowId, 'native-window')
  assert.deepEqual(installed.args.items.map(item => item.text), ['File', 'Navigate', 'View', 'Help'])
  assert.equal(installed.args.items[0].items[0].action, undefined)
  app.event('menus.clicked', { sourceId: 'menu-source', id: 'clipboard', checked: false }, 'native-menu-click')
  await app.idle()
  assert.equal(app.session.selected.id, 'clipboard')
  app.select('menus')
  app.input('#menu-text', 'Updated from TS')
  app.click('menu-rename')
  await app.idle()
  assert.deepEqual(app.host.requests.at(-1).args, {
    windowId: 'native-window', sourceId: 'menu-source', id: 'showcase', text: 'Updated from TS',
  })
})

test('busy guard blocks concurrent form actions but Reload remains available on replacement pages', async t => {
  const pending = deferred()
  const app = fixture(t, request => request.command === 'sample.echo' ? pending.promise : defaultResult(request))
  app.select('arguments')
  app.click('echo')
  app.click('echo')
  app.select('lifetime')
  app.click('counter')
  app.click('reload')
  assert.equal(app.host.requests.length, 1)
  assert.equal(app.reloads, 1)
  assert.equal(app.find('[data-action="increment"]').disabled, true)
  pending.resolve({ ...app.host.requests[0].args, counter: 9 })
  await app.idle()
  assert.equal(app.find('#result-state').textContent, 'Ready to run')
  assert.equal(app.find('[data-action="increment"]').disabled, false)
})

test('delayed native errors stay in the shared log without replacing the selected page result', async t => {
  const pending = deferred()
  const app = fixture(t, () => pending.promise)
  app.select('errors')
  app.click('fail')
  app.select('preferences')
  pending.resolve(failure('intentional_error'))
  await app.idle()
  assert.equal(app.find('#result-state').textContent, 'Ready to run')
  assert.ok(!app.find('#result-json').textContent.includes('intentional_error'))
  assert.match(app.find('#operation-log').textContent, /intentional_error/)
})

test('work captures both inputs before awaited subscription and navigation, then remains cancellable on return', async t => {
  const subscribe = deferred(), work = deferred()
  const app = fixture(t, request => request.command === 'core.subscribe' ? subscribe.promise
    : request.command === 'sample.work' ? work.promise : defaultResult(request))
  app.select('work')
  assert.equal(app.find('#steps').max, '50')
  assert.equal(app.find('#delay').min, '100')
  assert.equal(app.find('#delay').max, '500')
  app.input('#steps', '3')
  app.input('#delay', '100')
  app.click('work')
  assert.equal(app.host.requests[0].command, 'core.subscribe')
  app.input('#steps', '40')
  app.input('#delay', '500')
  app.select('preferences')
  subscribe.resolve({ accepted: true })
  await setImmediate()
  const request = app.host.requests.find(request => request.command === 'sample.work')
  assert.deepEqual(request.args, { steps: 3, delayMs: 100 })
  assert.equal(app.session.active.id, request.id)
  assert.equal(app.find('#progress').hidden, false, 'session progress remains available after changing pages')
  const otherResult = app.find('#result-json').textContent
  app.event('operation.progress', { completed: 1, total: 3 }, request.id)
  assert.equal(app.find('#result-json').textContent, otherResult)
  assert.match(app.find('#operation-log').textContent, /operation.progress/)
  app.select('work')
  assert.equal(app.find('[data-action="cancel"]').disabled, false)
  assert.equal(app.find('[data-action="work"]').disabled, true)
  app.click('cancel')
  await setImmediate()
  assert.equal(app.host.requests.at(-1).command, 'core.cancel')
  assert.equal(app.host.requests.at(-1).args.requestId, request.id)
  work.resolve(failure('cancelled', 'cancelled'))
  await app.idle()
  assert.equal(app.find('#result-state').textContent, 'Ready to run', 'navigation version isolates even a same-experiment remount')
  assert.match(app.find('#operation-log').textContent, /Cancelled/)
  assert.equal(app.find('[data-action="cancel"]').disabled, true)
})

test('progress requires the active correlation/session, and shared work subscription survives navigation and reuse', async t => {
  let work = deferred()
  const app = fixture(t, request => request.command === 'sample.work' ? work.promise : defaultResult(request))
  app.select('work')
  app.click('work')
  await setImmediate()
  const id = app.session.active.id
  const initial = app.find('#result-json').textContent
  app.event('operation.progress', { completed: 1, total: 20 }, 'unrelated')
  app.event('operation.progress', { completed: 1, total: 20 }, id, 'stale-session')
  assert.equal(app.find('#result-json').textContent, initial)
  app.event('operation.progress', { completed: 2, total: 20 }, id)
  assert.equal(app.find('#progress').value, 2)
  assert.deepEqual(JSON.parse(app.find('#result-json').textContent), { completed: 2, total: 20, requestId: id })
  app.select('arguments')
  const replacement = app.find('#result-json').textContent
  app.event('operation.progress', { completed: 3, total: 20 }, id)
  assert.equal(app.find('#result-json').textContent, replacement)
  assert.equal(app.find('#progress').value, 3, 'session progress updates without repainting the feature result')
  app.select('work')
  app.event('operation.progress', { completed: 4, total: 20 }, id)
  assert.equal(app.find('#progress').value, 4, 'catalog families share the work renderer')
  work.resolve({ completed: 20, total: 20 })
  await app.idle()
  const before = app.find('#operation-log').children.length
  app.event('operation.progress', { completed: 20, total: 20 }, id)
  assert.equal(app.find('#operation-log').children.length, before, 'terminal work ignores late progress')
  work = deferred()
  app.click('work')
  await setImmediate()
  assert.equal(app.host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  work.resolve({ completed: 20, total: 20 })
  await app.idle()
  assert.equal(app.find('#result-state').textContent, 'Success')
})

test('cancelling on the original work page reports a terminal Cancelled state, never Success', async t => {
  const work = deferred()
  const app = fixture(t, request => request.command === 'sample.work' ? work.promise : defaultResult(request))
  app.select('work')
  app.click('work')
  await setImmediate()
  app.click('cancel')
  await setImmediate()
  assert.match(app.find('#operation-log').textContent, /Cancellation requested/)
  work.resolve(failure('cancelled', 'cancelled'))
  await app.idle()
  assert.equal(app.find('#result-state').textContent, 'Cancelled')
  assert.equal(JSON.parse(app.find('#result-json').textContent).code, 'cancelled')
  assert.equal(app.find('#operation-log').firstElementChild.dataset.state, 'cancelled')
})

test('cancel rejection is logged without finishing or disabling the still-active operation', async t => {
  const work = deferred()
  const app = fixture(t, request => request.command === 'sample.work' ? work.promise
    : request.command === 'core.cancel' ? failure('transport_error') : defaultResult(request))
  app.select('work')
  app.click('work')
  await setImmediate()
  const active = app.session.active
  app.click('cancel')
  await setImmediate()
  assert.equal(app.session.active, active)
  assert.equal(app.session.busy, true)
  assert.equal(app.find('[data-action="cancel"]').disabled, false)
  assert.equal(app.find('#operation-log').firstElementChild.dataset.state, 'error')
  work.resolve({ completed: 20, total: 20 })
  await app.idle()
})

test('counter subscription is session-scoped and idempotent across mounts, with explicit unsubscribe and disposal', async t => {
  const app = fixture(t)
  app.select('events')
  assert.equal(app.find('[data-action="unsubscribe"]').disabled, true)
  app.click('subscribe')
  app.click('subscribe')
  await app.idle()
  app.select('clipboard')
  app.event('service.changed', { counter: 8 }, 'event-1')
  assert.equal(app.find('#result-state').textContent, 'Ready to run')
  assert.equal(app.find('#operation-log').dataset.lastEventId, 'event-1')
  app.select('events')
  assert.equal(app.find('[data-action="subscribe"]').disabled, true)
  assert.equal(app.find('[data-action="unsubscribe"]').disabled, false)
  app.click('subscribe')
  assert.equal(app.host.requests.filter(request => request.command === 'core.subscribe').length, 1)
  app.click('echo')
  await app.idle()
  assert.equal(app.host.requests.at(-1).command, 'sample.echo')
  app.click('unsubscribe')
  await app.idle()
  assert.equal(app.host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
  const count = app.find('#operation-log').children.length
  app.event('service.changed', { counter: 9 }, 'after-unsubscribe')
  assert.equal(app.find('#operation-log').children.length, count)
  app.click('subscribe')
  await app.idle()
  app.session.dispose()
  const disposedCount = app.find('#operation-log').children.length
  app.event('service.changed', { counter: 10 }, 'after-dispose')
  assert.equal(app.find('#operation-log').children.length, disposedCount)
  assert.equal(app.session.unsubscribeCounter, undefined)
  assert.equal(app.host.messages.filter(message => message.kind === 'disconnect').length, 1)
})

test('disposing active work removes both subscriptions and prevents late cancellation or completion paint', async t => {
  const work = deferred(), cancel = deferred()
  const app = fixture(t, request => request.command === 'sample.work' ? work.promise
    : request.command === 'core.cancel' ? cancel.promise : defaultResult(request))
  app.select('events')
  app.click('subscribe')
  await app.idle()
  app.select('work')
  app.click('work')
  await setImmediate()
  const id = app.session.active.id
  app.click('cancel')
  const log = app.session.entries
  const result = app.session.resultText
  app.shell.dispose()
  app.session.dispose()
  await app.idle()
  assert.equal(app.session.active, undefined)
  assert.equal(app.session.unsubscribeCounter, undefined)
  assert.equal(app.session.unsubscribeProgress, undefined)
  app.event('service.changed', { counter: 99 }, 'disposed-counter')
  app.event('operation.progress', { completed: 20, total: 20 }, id)
  work.resolve({ completed: 20, total: 20 })
  cancel.resolve({ accepted: true })
  await setImmediate()
  assert.equal(app.session.entries, log)
  assert.equal(app.session.resultText, result)
  assert.equal(app.host.messages.filter(message => message.kind === 'disconnect').length, 1)
})

test('operation log remains app-shared, bounded at 40 entries and clearable', t => {
  const app = fixture(t)
  for (let index = 0; index < 45; index++) app.session.writeLog(`Request ${index}`, 'Success', { index })
  assert.equal(app.find('#operation-log').children.length, 40)
  app.select('preferences')
  assert.match(app.find('#operation-log').firstElementChild.textContent, /Request 44/)
  app.find('#clear-log').click()
  assert.equal(app.find('#operation-log').children.length, 1)
  assert.ok(app.find('#operation-log .empty-log'))
})

test('connection starts once, attaches native listeners once, and pagehide/disposal removes them', async t => {
  const app = fixture(t, defaultResult, { native: false })
  let loads = 0
  const connection = createConnection(app.session, async () => {
    loads++
    return app.host.bridge
  })
  const first = connection.start()
  assert.equal(connection.start(), first)
  await first
  assert.equal(loads, 1)
  assert.equal(app.host.requests.filter(request => request.command === 'core.connect').length, 1)
  assert.equal(app.host.requests.filter(request => request.command === 'app.info').length, 1)
  assert.equal(app.host.requests.filter(request => request.command === 'device.info').length, 1)
  assert.equal(app.host.requests.filter(request => request.command === 'sample.diagnostics').length, 1)
  assert.equal(app.find('#host-status').dataset.state, 'ready')
  assert.equal(app.find('#host-status').dataset.startupId, identity.startupId)
  app.select('events')
  app.click('subscribe')
  await app.idle()
  const emit = id => app.dom.window.dispatchEvent(new app.dom.window.CustomEvent('HybridWebViewMessageReceived', {
    detail: { message: JSON.stringify({ version: 1, session: app.session.client.session, id,
      topic: 'service.changed', data: { counter: 11 } }) },
  }))
  const before = app.find('#operation-log').children.length
  emit('one-listener')
  assert.equal(app.find('#operation-log').children.length, before + 1)
  app.dom.window.dispatchEvent(new app.dom.window.PageTransitionEvent('pagehide'))
  assert.equal(app.session.client.available, false)
  emit('removed-listener')
  assert.equal(app.find('#operation-log').children.length, before + 1)
  app.session.dispose()
  assert.equal(app.host.messages.filter(message => message.kind === 'disconnect').length, 1)
})

test('connection failure disables native actions without inventing host metadata', async t => {
  const app = fixture(t, () => failure('connection_failed'), { native: false })
  app.select('identity')
  const connection = createConnection(app.session, async () => app.host.bridge)
  await connection.start()
  assert.equal(app.find('#host-status').dataset.state, 'error')
  assert.equal(app.find('#native-unavailable').hidden, false)
  assert.equal(app.find('#result-state').textContent, 'Error')
  assert.equal(app.find('[data-action="run"]').disabled, true)
  assert.equal(app.find('[data-action="reload"]').disabled, false)
  assert.equal(app.find('#native-identity').textContent, 'No native identity received.')
})

test('disposing during bridge load prevents a late attach or connection after HMR teardown', async t => {
  const bridge = deferred()
  const app = fixture(t, defaultResult, { native: false })
  const connection = createConnection(app.session, () => bridge.promise)
  const started = connection.start()
  app.shell.dispose()
  app.session.dispose()
  bridge.resolve(app.host.bridge)
  await started
  assert.equal(app.host.requests.length, 0)
  assert.equal(app.session.hostState, 'loading')
})

test('raw native values, error messages and shared log text are rendered only as plain text', async t => {
  const markup = '<img src=x onerror="alert(1)">\n<script>throw new Error("injected")</script>'
  const app = fixture(t, request => request.command === 'clipboard.read' ? markup
    : request.command === 'sample.fail' ? { status: 'error', error: { code: 'intentional_error', message: markup } }
    : defaultResult(request))
  app.select('clipboard')
  app.click('paste')
  await app.idle()
  assert.equal(app.find('#clipboard-text').value, markup)
  assert.equal(JSON.parse(app.find('#result-json').textContent).text, markup)
  app.select('errors')
  app.click('fail')
  await app.idle()
  assert.equal(JSON.parse(app.find('#result-json').textContent).message, markup)
  app.session.writeLog(markup, 'Success', markup)
  assert.equal(app.find('#operation-log strong').textContent, markup)
  assert.equal(app.find('#operation-log pre').textContent, markup)
  assert.equal(app.find('#experiment').querySelectorAll('img, script').length, 0)
})

test('typed form validation reports input errors without calling native or mislabelling response errors', async t => {
  const app = fixture(t)
  for (const feature of ['arguments', 'events']) {
    app.select(feature)
    const defaults = JSON.parse(app.find('#echo-input').value)
    assert.deepEqual(defaults, {
      text: 'Unicode: café 你好 👋\n"quotes" \\ slash', number: 42.5, flag: true, items: ['first', '第二'],
    })
    for (const invalid of [{ ...defaults, number: '42' }, { ...defaults, flag: 'true' },
      { ...defaults, text: 42 }, { ...defaults, items: [1] }, null]) {
      app.input('#echo-input', JSON.stringify(invalid))
      app.click('echo')
      await app.idle()
      assert.equal(JSON.parse(app.find('#result-json').textContent).code, 'input_error')
    }
  }
  assert.equal(app.host.requests.length, 0)
})

test('detached navigation and action nodes cannot act after a catalog filter or shell unmount', async t => {
  const app = fixture(t)
  app.select('arguments')
  const navigation = app.find('[data-feature="clipboard"]')
  const counter = app.find('[data-action="counter"]')
  app.input('#search', 'never-matches-anything')
  assert.equal(navigation.isConnected, false)
  navigation.click()
  assert.equal(app.session.selected.id, 'arguments')
  app.shell.dispose()
  counter.click()
  await app.idle()
  assert.equal(app.host.requests.length, 0)
  assert.equal(app.session.disposed, false, 'unmount must not disconnect the document')
})

test('shell remount keeps the same document counter/progress subscriptions and active work', async t => {
  const work = deferred()
  const app = fixture(t, request => request.command === 'sample.work' ? work.promise : defaultResult(request))
  app.select('events')
  app.click('subscribe')
  await app.idle()
  app.select('work')
  assert.equal(app.find('#steps').value, '20')
  assert.equal(app.find('#delay').value, '300')
  app.click('work')
  await setImmediate()
  const active = app.session.active
  app.remount()
  app.remount()
  assert.equal(app.session.active, active)
  assert.equal(app.find('[data-action="cancel"]').disabled, false)
  app.event('service.changed', { counter: 15 }, 'after-remount')
  assert.equal(app.session.entries.filter(entry => entry.label === 'Native service.changed').length, 1)
  assert.equal(app.host.requests.filter(request => request.command === 'core.subscribe').length, 2)
  assert.equal(app.host.messages.filter(message => message.kind === 'disconnect').length, 0)
  app.event('operation.progress', { completed: 3, total: 20 }, active.id)
  assert.equal(app.find('#progress').value, 3)
  work.resolve({ completed: 20, total: 20 })
  await app.idle()
  app.select('events')
  app.click('unsubscribe')
  await app.idle()
  assert.equal(app.host.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
})

for (const feature of ['events', 'work']) {
  test(`disposing during pending ${feature} subscription cannot restore handles or start work`, async t => {
    const subscribe = deferred()
    const app = fixture(t, request => request.command === 'core.subscribe' ? subscribe.promise : defaultResult(request))
    app.select(feature)
    app.click(feature === 'work' ? 'work' : 'subscribe')
    app.session.dispose()
    subscribe.resolve({ accepted: true })
    await app.idle()
    assert.equal(app.session.unsubscribeCounter, undefined)
    assert.equal(app.session.unsubscribeProgress, undefined)
    assert.equal(app.session.active, undefined)
    assert.equal(app.host.requests.some(request => request.command === 'sample.work'), false)
    assert.equal(app.host.messages.filter(message => message.kind === 'disconnect').length, 1)
    assert.equal(app.session.entries.length, 0)
  })
}

test('late identity bootstrap cannot paint after document disposal', async t => {
  const metadata = deferred()
  const app = fixture(t, request => request.command === 'app.info' ? metadata.promise : defaultResult(request),
    { native: false })
  const connection = createConnection(app.session, async () => app.host.bridge)
  const started = connection.start()
  await setImmediate()
  assert.deepEqual(app.host.requests.map(request => request.command),
    ['core.connect', 'sample.diagnostics', 'app.info', 'device.info'])
  app.session.dispose()
  metadata.resolve({ appName: 'Late', version: '9', build: '9' })
  await started
  assert.equal(app.find('#host-status').dataset.state, 'loading')
  assert.equal(app.find('#native-identity').textContent, 'No native identity received.')
  assert.equal(app.session.entries.length, 0)
  assert.equal(app.host.messages.filter(message => message.kind === 'disconnect').length, 1)
})

test('pending preference reads capture the original key before await and preserve replacement state', async t => {
  const read = deferred()
  const app = fixture(t, request => request.command === 'preferences.get' ? read.promise : defaultResult(request))
  app.select('preferences')
  app.input('#preference-key', 'original_key')
  app.click('read')
  app.input('#preference-key', 'later_key')
  app.select('clipboard')
  app.select('preferences')
  app.input('#preference-value', 'New component state')
  read.resolve({ kind: 'string', text: 'Original setting' })
  await app.idle()
  assert.deepEqual(app.host.requests.find(request => request.command === 'preferences.get').args,
    { key: 'original_key', defaultValue: { kind: 'string', text: '' }, sharedName: null })
  assert.equal(app.find('#preference-key').value, 'example')
  assert.equal(app.find('#preference-value').value, 'New component state')
  assert.equal(app.find('#result-state').textContent, 'Ready to run')
})

test('bootstrap disables native actions until metadata is ready without replacing the selected form', async t => {
  const metadata = deferred()
  const app = fixture(t, request => request.command === 'app.info' ? metadata.promise : defaultResult(request),
    { native: false })
  const connection = createConnection(app.session, async () => app.host.bridge)
  const started = connection.start()
  await setImmediate()
  app.select('clipboard')
  const form = app.find('#clipboard-text')
  app.input('#clipboard-text', 'Keep edits made while connecting')
  assert.equal(app.find('[data-action="copy"]').disabled, true)
  app.click('copy')
  assert.equal(app.host.requests.some(request => request.command === 'clipboard.write'), false)
  metadata.resolve({ appName: identity.appName, version: identity.version, build: identity.build })
  await started
  assert.equal(app.find('#host-status').dataset.state, 'ready')
  assert.equal(app.find('#clipboard-text'), form)
  assert.equal(form.value, 'Keep edits made while connecting')
  assert.equal(app.find('[data-action="copy"]').disabled, false)
  app.click('copy')
  await app.idle()
  assert.deepEqual(app.host.requests.at(-1).args, { text: form.value })
})

for (const [feature, action, selector, command, output] of [
  ['clipboard', 'paste', '#clipboard-text', 'clipboard.read', 'Late clipboard'],
  ['preferences', 'read', '#preference-value', 'preferences.get', { kind: 'string', text: 'Late setting' }],
]) {
  test(`shell remount isolates pending ${command} completion even without navigation`, async t => {
    const pending = deferred()
    const app = fixture(t, request => request.command === command ? pending.promise : defaultResult(request))
    app.select(feature)
    app.click(action)
    const original = app.find(selector)
    const originalValue = original.value
    app.remount()
    assert.equal(original.isConnected, false)
    app.input(selector, 'Remounted input')
    pending.resolve(output)
    await app.idle()
    assert.equal(original.value, originalValue)
    assert.equal(app.find(selector).value, 'Remounted input')
    assert.equal(app.find('#result-state').textContent, 'Ready to run')
    assert.match(app.find('#operation-log').textContent, /Late/)
  })
}
