import { test } from 'vitest'
import assert from 'node:assert/strict'
import { setImmediate } from 'node:timers/promises'
import { flushSync, mount, unmount } from 'svelte'
import { configureBridge, resetBridge } from '@maui/api/core'
import { onConnectivityChanged } from '@maui/api/essentials/connectivity'
import { availableFeatures, features } from '../src/catalog.ts'
import { components } from '../src/registry.ts'
import { PlaygroundSession } from '../src/session.ts'

const locale = { id: 'voice', language: 'en-US', country: '', name: 'Native voice' }
const ready = { commands: ['speech.speak', 'launcher.open', 'launcher.canOpen', 'launcher.tryOpen'], interactiveCommands: ['speech.speak', 'launcher.open', 'launcher.tryOpen'],
  interactiveTimeoutMilliseconds: 300000, maximumTextLength: 4096 }
const network = { networkAccess: 'internet', connectionProfiles: ['wiFi'] }
const defaults = request => request.command === 'core.connect' ? ready :
  request.command === 'connectivity.networkAccess' ? network.networkAccess :
    request.command === 'connectivity.connectionProfiles' ? network.connectionProfiles :
    request.command.startsWith('launcher.') ? true :
    request.command === 'display.keepScreenOn' ? false :
    request.command === 'display.setKeepScreenOn' ? null :
    request.command === 'speech.locales' ? [locale] : request.command === 'speech.speak' ? null : { accepted: true }

function fixture(t, experiment, handler = defaults) {
  document.body.replaceChildren()
  const target = document.createElement('div')
  document.body.append(target)
  const requests = []
  const client = configureBridge({
    async InvokeDotNet(_, [wire]) {
      const request = JSON.parse(wire)
      requests.push(request)
      const value = await handler(request)
      return JSON.stringify({ version: 1, id: request.id, session: request.session,
        status: value?.error ? (value.error.code === 'cancelled' ? 'cancelled' : 'error') : 'success',
        result: value?.error ? null : value, error: value?.error ?? null })
    },
    SendRawMessage() {},
  })
  const session = new PlaygroundSession()
  session.setClient(client)
  let page = mount(components[experiment], { target, props: { session } })
  flushSync()
  async function destroy() {
    if (page) { const old = page; page = undefined; await unmount(old); flushSync() }
  }
  t.onTestFinished(async () => {
    await destroy()
    session.dispose(); resetBridge(); document.body.replaceChildren()
  })
  const find = selector => { flushSync(); const value = target.querySelector(selector); assert.ok(value, selector); return value }
  return {
    requests, session, client, find, destroy,
    click(action) { find(`[data-action="${action}"]`).click(); flushSync() },
    input(selector, value) {
      const field = find(selector); field.value = value
      field.dispatchEvent(new Event(field instanceof HTMLSelectElement ? 'change' : 'input', { bubbles: true }))
      flushSync()
    },
    async idle() { await setImmediate(); flushSync(); assert.equal(session.busy, false) },
    event(value, topic = 'connectivity.changed') { client.receive(JSON.stringify({ version: 1, session: client.session, id: crypto.randomUUID(),
      topic, data: value })); flushSync() },
  }
}

test('approved native families are promoted once, use Essentials navigation and distinct real components', () => {
  for (const [title, experiment] of [['Connectivity', 'connectivity'], ['Text-to-speech', 'textToSpeech'], ['Open external links', 'launcher']]) {
    const family = features.filter(feature => feature.title === title)
    assert.equal(family.length, 1)
    assert.equal(family[0].experiment, experiment)
    assert.equal(family[0].category, 'Essentials')
    assert.ok(availableFeatures.includes(family[0]))
  }
  assert.equal(new Set(['connectivity', 'textToSpeech', 'launcher'].map(name => components[name])).size, 3)
})

for (const experiment of ['connectivity', 'textToSpeech', 'launcher']) {
  test(`${experiment} mount does not read, subscribe, speak or launch without user action`, t => {
    const app = fixture(t, experiment)
    assert.deepEqual(app.requests, [])
    assert.equal(app.session.busy, false)
  })
}

test('connectivity refresh and actual events update the page; leaving removes only its own listener', async t => {
  const app = fixture(t, 'connectivity')
  const other = []
  const releaseOther = await onConnectivityChanged(value => other.push(value))
  app.click('connectivity-read'); await app.idle()
  assert.ok(app.find('#network-access').textContent.includes('internet'))
  app.click('connectivity-subscribe'); await app.idle()
  assert.equal(app.requests.filter(request => request.command === 'core.subscribe').length, 1)
  assert.ok(app.find('#network-listening').textContent.includes('0 changes'))
  app.event({ networkAccess: 'none', connectionProfiles: [] })
  assert.ok(app.find('#network-access').textContent.includes('none'))
  assert.ok(app.find('#network-listening').textContent.includes('1 changes'))
  await app.destroy(); await setImmediate()
  assert.equal(app.requests.filter(request => request.command === 'core.unsubscribe').length, 0)
  app.event(network); assert.equal(other.length, 2)
  await releaseOther()
  assert.equal(app.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
})

test('explicit unsubscribe prevents late network callbacks', async t => {
  const app = fixture(t, 'connectivity')
  app.click('connectivity-subscribe'); await app.idle()
  app.event(network)
  app.click('connectivity-unsubscribe'); await app.idle()
  app.event({ networkAccess: 'none', connectionProfiles: [] })
  assert.ok(app.find('#network-access').textContent.includes('internet'))
  assert.ok(app.find('#network-listening').textContent.includes('Not listening'))
  assert.ok(app.find('#network-listening').textContent.includes('1 changes'))
})

test('a pending native subscription is released when its page disappears', async t => {
  let finish
  const app = fixture(t, 'connectivity', request => request.command === 'core.subscribe'
    ? new Promise(resolve => { finish = resolve }) : defaults(request))
  app.click('connectivity-subscribe')
  await setImmediate()
  await app.destroy()
  finish({ accepted: true }); await app.idle()
  assert.equal(app.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
})

test('speech locale/options and scoped cancellation use real SDK operations and leave other listeners alive', async t => {
  let finish
  const app = fixture(t, 'textToSpeech', request => {
    if (request.command === 'speech.speak') return new Promise(resolve => { finish = resolve })
    if (request.command === 'core.cancel') { finish({ error: { code: 'cancelled', message: 'Native speech cancelled' } }); return { accepted: true } }
    return defaults(request)
  })
  const events = []
  const release = await onConnectivityChanged(value => events.push(value))
  app.click('speech-locales'); await app.idle()
  assert.ok(app.find('#speech-locale-count').textContent.includes('1 native locales'))
  app.input('#speech-locale', '0'); app.input('#speech-text', 'Hello native voice')
  app.input('#speech-pitch', '1.5'); app.input('#speech-volume', '0.25'); app.input('#speech-rate', '0.8')
  app.click('speech-speak'); await setImmediate(); flushSync()
  const speech = app.requests.find(request => request.command === 'speech.speak')
  assert.deepEqual(speech.args, { text: 'Hello native voice', options: { pitch: 1.5, volume: 0.25, rate: 0.8, locale } })
  assert.ok(app.find('#speech-status').textContent.includes('in progress'))
  app.click('cancel'); await app.idle()
  assert.equal(app.requests.find(request => request.command === 'core.cancel').args.requestId, speech.id)
  assert.equal(app.session.resultStatus, 'Cancelled')
  app.event(network); assert.equal(events.length, 1)
  await release()
})

test('speech page passes empty long and out-of-range values through and displays native rejection', async t => {
  const app = fixture(t, 'textToSpeech', request => request.command === 'speech.speak'
    ? { error: { code: 'native_error', message: 'Destination rejected options' } } : defaults(request))
  for (const value of ['', ' '.repeat(8), '文'.repeat(8192)]) {
    app.input('#speech-text', value)
    app.input('#speech-pitch', '-3')
    app.input('#speech-volume', '5')
    app.input('#speech-rate', '0')
    app.click('speech-speak'); await app.idle()
    assert.deepEqual(app.requests.at(-1).args, { text: value, options: { pitch: -3, volume: 5, rate: 0 } })
    assert.ok(app.session.entries.some(entry => entry.detail.includes('Destination rejected options')))
  }
})

test('leaving the speech page cancels its own operation instead of a different session operation', async t => {
  let finish
  const app = fixture(t, 'textToSpeech', request => {
    if (request.command === 'speech.speak') return new Promise(resolve => { finish = resolve })
    if (request.command === 'core.cancel') { finish({ error: { code: 'cancelled', message: 'Cancelled' } }); return { accepted: true } }
    return defaults(request)
  })
  app.click('speech-speak'); await setImmediate()
  app.session.active = { id: 'unrelated-operation', result: Promise.resolve(undefined), cancel: () => { throw new Error('Do not cancel other features') } }
  await app.destroy(); await app.idle()
  const speech = app.requests.find(request => request.command === 'speech.speak')
  assert.equal(app.requests.find(request => request.command === 'core.cancel').args.requestId, speech.id)
})

test('URI launch uses explicit native commands and displays actual native boolean results', async t => {
  const app = fixture(t, 'launcher')
  app.input('#launch-url', 'https://dot.net/maui')
  app.click('launch-url'); await app.idle()
  assert.deepEqual(app.requests.map(request => request.command), ['core.connect', 'launcher.open'])
  assert.deepEqual(app.requests[1].args, { uri: 'https://dot.net/maui' })
  assert.equal(app.session.resultStatus, 'Success')
  app.input('#launch-url', 'https://127.0.0.1')
  app.click('launch-url'); await app.idle()
  assert.equal(app.session.resultStatus, 'Success')
  assert.equal(app.requests.filter(request => request.command === 'launcher.open').length, 2)
  app.input('#launch-url', 'custom:action')
  app.click('can-launch-url'); await app.idle()
  assert.deepEqual(app.requests.at(-1).args, { uri: 'custom:action' })
  assert.equal(app.requests.at(-1).command, 'launcher.canOpen')
  app.click('try-launch-url'); await app.idle()
  assert.equal(app.requests.at(-1).command, 'launcher.tryOpen')
})

for (const [experiment, action, command, code] of [['launcher', 'launch-url', 'launcher.open', 'native_error'],
  ['textToSpeech', 'speech-locales', 'speech.locales', 'unsupported']]) {
  test(`${code} is an explicit ${experiment} feature error`, async t => {
    const app = fixture(t, experiment, request => request.command === command
      ? { error: { code, message: 'Native feature failed' } } : defaults(request))
    app.click(action); await app.idle()
    assert.equal(app.session.resultStatus, 'Error')
    assert.ok(app.session.resultText.includes(code))
  })
}

test('native launch refusal remains an observable false result, not an invented error', async t => {
  const app = fixture(t, 'launcher', request => request.command === 'launcher.open' ? false : defaults(request))
  app.click('launch-url'); await app.idle()
  assert.equal(app.session.resultStatus, 'Success')
  assert.deepEqual(JSON.parse(app.session.resultText), { accepted: false })
})

test('battery page explicitly subscribes to native battery and energy saver events and releases them on leave', async t => {
  const app = fixture(t, 'battery')
  assert.deepEqual(app.requests, [])
  app.click('battery-listen'); await app.idle()
  app.click('energy-saver-listen'); await app.idle()
  assert.equal(app.requests.filter(request => request.command === 'core.subscribe').length, 2)
  app.event({ chargeLevel: 0.125, state: 'discharging', powerSource: 'battery' }, 'battery.infoChanged')
  app.event({ energySaverStatus: 'on' }, 'battery.energySaverStatusChanged')
  assert.match(app.find('#battery-charge').textContent, /12[.,]5%/)
  assert.match(app.find('#battery-events').textContent, /2 actual native changes/)
  await app.destroy(); await setImmediate()
  assert.equal(app.requests.filter(request => request.command === 'core.unsubscribe').length, 2)
})

test('display page applies the edited native preference only on click and owns its display listener', async t => {
  const app = fixture(t, 'display')
  assert.deepEqual(app.requests, [])
  const checkbox = app.find('#keep-screen-on')
  checkbox.checked = true
  checkbox.dispatchEvent(new Event('change', { bubbles: true }))
  flushSync()
  assert.deepEqual(app.requests, [])
  app.click('display-awake-set'); await app.idle()
  assert.deepEqual(app.requests.at(-1).args, { value: true })
  assert.equal(app.requests.at(-1).command, 'display.setKeepScreenOn')
  app.click('display-awake-read'); await app.idle()
  assert.equal(checkbox.checked, false)
  app.click('display-listen'); await app.idle()
  app.event({ displayInfo: { width: 0, height: 0, density: 0, refreshRate: 59.5, orientation: 'landscape', rotation: 'rotation270' } },
    'display.mainDisplayInfoChanged')
  assert.match(app.find('#display-events').textContent, /1 actual native display changes/)
  await app.destroy(); await setImmediate()
  assert.equal(app.requests.filter(request => request.command === 'core.unsubscribe').length, 1)
  assert.equal(app.requests.filter(request => request.command === 'display.setKeepScreenOn').length, 1,
    'listener cleanup must not reset a native preference')
})
