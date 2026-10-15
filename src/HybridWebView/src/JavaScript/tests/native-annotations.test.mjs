import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { configureBridge, connect, resetBridge } from '@maui/api/core'
import * as appInfo from '@maui/api/essentials/app-info'
import * as deviceInfo from '@maui/api/essentials/device-info'
import * as battery from '@maui/api/essentials/battery'
import * as display from '@maui/api/essentials/display'
import * as application from '@maui/api/application'
import { getConnectivity } from '@maui/api/essentials/connectivity'
import { mock } from './bridge.mjs'

afterEach(() => resetBridge())
const ready = { commands: [], maximumTextLength: 1 }

test('all curated identity getters return exact native strings enums and extensible identifiers', async () => {
  const values = {
    'appInfo.packageName': 'native.app', 'appInfo.name': 'Native name', 'appInfo.versionString': '1.2-native',
    'appInfo.version': '1.2.3.4', 'appInfo.buildString': 'native build', 'appInfo.requestedTheme': 'unspecified',
    'appInfo.packagingModel': 'unpackaged', 'appInfo.requestedLayoutDirection': 'rightToLeft',
    'deviceInfo.model': 'Native model', 'deviceInfo.manufacturer': 'Native manufacturer', 'deviceInfo.name': 'Native device',
    'deviceInfo.versionString': '16-native', 'deviceInfo.version': '16.0.1.2', 'deviceInfo.platform': 'CustomPlatform',
    'deviceInfo.idiom': 'CustomIdiom', 'deviceInfo.deviceType': 'virtual',
  }
  const host = mock(request => request.command === 'core.connect' ? ready : values[request.command])
  configureBridge(host.bridge)
  for (const [api, prefix, names] of [
    [appInfo, 'appInfo', ['PackageName', 'Name', 'VersionString', 'Version', 'BuildString', 'RequestedTheme', 'PackagingModel', 'RequestedLayoutDirection']],
    [deviceInfo, 'deviceInfo', ['Model', 'Manufacturer', 'Name', 'VersionString', 'Version', 'Platform', 'Idiom', 'DeviceType']],
  ]) for (const name of names) {
    const key = name[0].toLowerCase() + name.slice(1)
    assert.equal(await api[`get${name}`](), values[`${prefix}.${key}`])
    assert.equal(host.requests.at(-1).command, `${prefix}.${key}`)
  }
  assert.equal(host.requests.filter(request => request.command === 'core.connect').length, 1)
})

test('native display and theme setters use ordinary value arguments and void completion', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready :
    request.command === 'display.keepScreenOn' ? true :
    request.command === 'application.userAppTheme' ? 'dark' :
    request.command === 'application.requestedTheme' ? 'light' : null)
  configureBridge(host.bridge)
  assert.equal(await display.getKeepScreenOn(), true)
  assert.equal(await application.getUserAppTheme(), 'dark')
  assert.equal(await application.getTheme(), 'light')
  assert.equal(await display.setKeepScreenOn(false), undefined)
  assert.deepEqual(host.requests.at(-1).args, { value: false })
  assert.equal(host.requests.at(-1).command, 'display.setKeepScreenOn')
  assert.equal(await application.setTheme('unspecified'), undefined)
  assert.deepEqual(host.requests.at(-1).args, { value: 'unspecified' })
  assert.equal(host.requests.at(-1).command, 'application.setUserAppTheme')
})

test('native battery/display results and network profile duplicates have no semantic normalization', async () => {
  const metrics = { width: 0, height: 0, density: 0, refreshRate: 0, orientation: 'unknown', rotation: 'unknown' }
  const values = { 'battery.chargeLevel': 5.25, 'battery.state': 'unknown', 'battery.powerSource': 'unknown',
    'battery.energySaverStatus': 'unknown', 'display.mainDisplayInfo': metrics,
    'connectivity.networkAccess': 'local', 'connectivity.connectionProfiles': ['wiFi', 'wiFi', 'ethernet'] }
  const host = mock(request => request.command === 'core.connect' ? ready : values[request.command])
  configureBridge(host.bridge)
  assert.equal((await battery.getBatteryInfo()).chargeLevel, 5.25)
  assert.deepEqual(await display.getDisplayInfo(), metrics)
  assert.deepEqual((await getConnectivity()).connectionProfiles, ['wiFi', 'wiFi', 'ethernet'])
})

test('power and display callbacks use actual native event shapes with first/last listener ownership', async () => {
  const host = mock(request => request.command === 'core.connect' ? ready : { accepted: true })
  configureBridge(host.bridge)
  const client = await connect(), first = [], second = [], saving = [], displays = []
  const emit = (topic, data, session = client.session) => client.receive(JSON.stringify({
    version: 1, session, id: crypto.randomUUID(), topic, data,
  }))
  const release1 = await battery.onBatteryInfoChanged(value => first.push(value))
  const release2 = await battery.onBatteryInfoChanged(value => second.push(value))
  const releaseSaver = await battery.onEnergySaverStatusChanged(value => saving.push(value))
  const releaseDisplay = await display.onMainDisplayInfoChanged(value => displays.push(value))
  assert.deepEqual(first, [])
  assert.equal(host.requests.filter(request => request.command === 'core.subscribe').length, 3)
  const native = { chargeLevel: 0.125, state: 'charging', powerSource: 'usb' }
  emit('battery.infoChanged', native)
  emit('battery.energySaverStatusChanged', { energySaverStatus: 'on' })
  const metrics = { width: 0, height: 0, density: 0, refreshRate: 59.5, orientation: 'landscape', rotation: 'rotation270' }
  emit('display.mainDisplayInfoChanged', { displayInfo: metrics })
  assert.deepEqual(first, [native]); assert.deepEqual(second, [native])
  assert.deepEqual(saving, ['on']); assert.deepEqual(displays, [metrics])
  await release1()
  assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 0)
  emit('battery.infoChanged', native)
  assert.equal(first.length, 1); assert.equal(second.length, 2)
  emit('battery.infoChanged', native, 'retired-document')
  assert.equal(second.length, 2)
  await release2(); await releaseSaver(); await releaseDisplay()
  assert.equal(host.requests.filter(request => request.command === 'core.unsubscribe').length, 3)
  emit('battery.infoChanged', native)
  assert.equal(second.length, 2)
})
