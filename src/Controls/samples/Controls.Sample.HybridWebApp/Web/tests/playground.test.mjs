import { test } from 'vitest'
import assert from 'node:assert/strict'
import { setImmediate } from 'node:timers/promises'
import { flushSync } from 'svelte'
import { NativeClient, configureBridge, resetBridge } from '@maui/api/core'
import { availableFeatures, navigationPages } from '../src/catalog.ts'
import { components } from '../src/registry.ts'
import { PlaygroundSession } from '../src/session.ts'
import { mountShell } from '../src/shell.ts'
import { createConnection } from '../src/connection.ts'
import { readRoute } from '../src/routes.ts'
import html from '../index.html?raw'

const identity = { appName: 'Native', version: '1', build: '2', platform: 'MacCatalyst', osVersion: '27', model: 'Mac',
  architecture: 'arm64', pid: 123, startupId: 'native-startup', startedAt: '2026-10-12T00:00:00Z' }
const actions = {
  identity: ['run', 'reload'], clipboard: ['copy', 'paste'], preferences: ['save', 'read', 'remove'],
  echo: ['echo', 'counter'], errors: ['fail', 'counter'], policy: ['missing', 'disabled', 'invalid'],
  lifetime: ['increment', 'counter', 'reload'], events: ['echo', 'subscribe', 'unsubscribe'],
  work: ['work', 'cancel', 'reload'], development: ['identity', 'reload'],
  welcome: [], showcase: ['title', 'copy', 'prompt', 'device', 'open-window'],
  windows: ['window-info', 'title', 'bounds', 'center', 'open-window'], dialogs: ['alert', 'confirm', 'prompt'],
  filePicker: ['pick-file'], remoteWebView: ['browser-info', 'remote', 'browser-back', 'browser-forward', 'browser-reload', 'browser-focus', 'browser-close'],
  battery: ['battery'], display: ['display'], menus: ['menus-install', 'menus-remove', 'menu-rename', 'menu-enable', 'menu-check'],
  theme: ['theme-system', 'theme-light', 'theme-dark'],
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
      return JSON.stringify({ version: 1, session: request.session, id: request.id,
        status: output?.error ? output.status : 'success', result: output?.error ? null : output, error: output?.error ?? null })
    },
    SendRawMessage(wire) { messages.push(JSON.parse(wire)) },
  }
  return { requests, messages, bridge }
}

function defaultResult(request) {
  switch (request.command) {
    case 'core.connect': return { commands: ['app.info', 'sample.echo'], preferenceNamespace: 'maui.playground.v1.', maximumTextLength: 4096 }
    case 'app.info': return { appName: identity.appName, version: identity.version, build: identity.build }
    case 'device.info': return { platform: identity.platform, osVersion: identity.osVersion, model: identity.model }
    case 'sample.diagnostics': return { architecture: identity.architecture, pid: identity.pid,
      startupId: identity.startupId, startedAt: identity.startedAt }
    case 'sample.echo': return { ...request.args, counter: 7 }
    case 'sample.counter': return { counter: 7 }
    case 'clipboard.read': return { text: 'Native clipboard 👋' }
    case 'preferences.read':
    case 'preferences.write': return { key: request.args.key, value: request.args.value ?? 'Native preference' }
    case 'preferences.remove': return { key: request.args.key, value: null }
    case 'preferences.get': return { kind: 'string', text: 'Native preference' }
    case 'preferences.set':
    case 'preferences.delete': return { accepted: true }
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
    case 'filePicker.pick': return { file: null }
    case 'battery.info': return { chargeLevel: 0.5, state: 'Charging', powerSource: 'AC', energySaverStatus: 'Off' }
    case 'display.info': return { width: 1080, height: 1920, density: 3, orientation: 'Portrait', rotation: 'Rotation0', refreshRate: 60 }
    case 'remote.info': return { supported: true, open: false, id: null, url: null, status: 'closed', canGoBack: false, canGoForward: false }
    case 'remote.open': return { supported: true, open: true, id: 'guest', url: request.args.url, status: 'loading', canGoBack: false, canGoForward: false }
    case 'remote.bounds':
    case 'remote.control': return { supported: true, open: false, id: null, url: null, status: 'closed', canGoBack: false, canGoForward: false }
    case 'menus.info': return { supported: true, supportsChecked: true, sourceId: null }
    case 'menus.install': return { supported: true, supportsChecked: true, sourceId: 'menu-source' }
    case 'menus.update':
    case 'menus.clear': return { supported: true, supportsChecked: true, sourceId: request.command === 'menus.clear' ? null : 'menu-source' }
    case 'application.theme': return { effective: 'light', preference: 'system' }
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
  let shell = mountShell(dom.window.document, session, () => { reloads++ }, () => compact)
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
      shell = mountShell(dom.window.document, session, () => { reloads++ }, () => compact)
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
  assert.equal(app.find('#catalog-count').textContent, '28 experiments · 2 guides')
  for (const feature of navigationPages) {
    app.select(feature.id)
    assert.equal(app.find('#feature-title').textContent, feature.title)
    assert.equal(app.find('#feature-meta').textContent, feature.category)
    assert.equal(app.find('#feature-description').textContent, feature.description)
    assert.equal(app.find('#api-link').href, feature.docs)
    assert.equal(app.find(`[data-feature="${feature.id}"]`).getAttribute('aria-current'), 'page')
    assert.deepEqual([...app.find('#controls').querySelectorAll('[data-action]')].map(button => button.dataset.action), actions[feature.experiment])
    assert.equal(app.find('#progress').hidden, feature.experiment !== 'work')
  }
  assert.equal(app.host.requests.length, 0, 'mounting never makes a native call')
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
  assert.equal(app.find('#catalog-count').textContent, '28 experiments · 2 guides')
  app.find('[data-group="Essentials"]').click()
  app.session.writeLog('A native refresh', 'Success', { accepted: true })
  assert.equal(app.find('[data-group="Essentials"]').getAttribute('aria-expanded'), 'false',
    'unrelated session refresh must not replace an active group collapse preference')
})

test('compact navigation focuses the experiment instead of a replaced catalog button', t => {
  const app = fixture(t, defaultResult, { compact: true })
  app.select('preferences')
  assert.equal(app.dom.window.document.activeElement.id, 'experiment')
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
  app.select('registration')
  oldAction.click()
  assert.equal(app.session.selected.id, 'registration')
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
  assert.equal(app.host.requests.at(-1).command, 'preferences.delete')
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
  ['clipboard', 'paste', '#clipboard-text', 'clipboard.read', { text: 'Late clipboard' }],
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
  const back = new Promise(resolve => window.addEventListener('hashchange', resolve, { once: true }))
  window.history.back()
  await back
  assert.equal(app.session.selected.id, 'showcase')
  const forward = new Promise(resolve => window.addEventListener('hashchange', resolve, { once: true }))
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
  assert.deepEqual(app.host.requests.at(-1).args, { title: 'Native window 👋', windowId: 'native-window', windowLease: 'private-lease' })
  app.click('bounds')
  await app.idle()
  assert.deepEqual(app.host.requests.at(-1).args, { x: 80, y: 80, windowId: 'native-window', windowLease: 'private-lease' })
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
})

test('selected text is rendered as text and excluded from result logs, with ordinary picker dismissal', async t => {
  let cancelled = false
  const app = fixture(t, request => {
    if (request.command === 'filePicker.pick') return { file: cancelled ? null : { id: 'selected', name: 'example.txt', contentType: 'text/plain' } }
    if (request.command === 'file.openRead') return { id: 'reader', canSeek: true, maximumChunkBytes: 65536 }
    if (request.command === 'file.readBytes') return { base64: btoa('<script>private</script>'), eof: true }
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
  assert.equal(app.find('#progress').hidden, true)
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
  assert.equal(app.find('#progress').value, 2, 'hidden progress must not repaint another page')
  app.select('cleanup')
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
  const app = fixture(t, request => request.command === 'clipboard.read' ? { text: markup }
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
  ['clipboard', 'paste', '#clipboard-text', 'clipboard.read', { text: 'Late clipboard' }],
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
