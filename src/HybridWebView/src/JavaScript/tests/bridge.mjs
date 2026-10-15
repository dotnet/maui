import assert from 'node:assert/strict'

// Snapshot-shaped test fixtures are projected into the actual native property results.
export function nativeProperty(command, fixture) {
  const fields = {
    'appInfo.name': 'appName', 'appInfo.versionString': 'version', 'appInfo.buildString': 'build',
    'deviceInfo.platform': 'platform', 'deviceInfo.versionString': 'osVersion', 'deviceInfo.model': 'model',
    'battery.chargeLevel': 'chargeLevel', 'battery.state': 'state', 'battery.powerSource': 'powerSource',
    'battery.energySaverStatus': 'energySaverStatus', 'connectivity.networkAccess': 'networkAccess',
    'connectivity.connectionProfiles': 'connectionProfiles', 'application.requestedTheme': 'effective',
    'application.userAppTheme': 'preference',
  }
  return Object.hasOwn(fields, command) ? fixture?.[fields[command]] : fixture
}

export function mock(handler, binaryHandler) {
  const requests = [], messages = [], binaries = []
  return {
    requests, messages, binaries,
    bridge: {
      ...(binaryHandler ? {
        async InvokeDotNetBinary(method, bytes, options) {
          assert.equal(method, 'hybrid.invoke')
          const metadata = Object.entries(options.headers).sort(([a], [b]) => a.localeCompare(b)).map(([, value]) => value).join('')
          const request = JSON.parse(metadata)
          requests.push(request); binaries.push({ request, bytes, options })
          const output = await binaryHandler(request, bytes, options)
          assert.ok(output instanceof Uint8Array)
          return Uint8Array.from(output).buffer
        },
      } : {}),
      async InvokeDotNet(method, values) {
        assert.equal(method, 'Invoke')
        assert.equal(values.length, 1)
        const header = JSON.stringify({ MethodName: method, ParamValues: values.map(value => JSON.stringify(value)) })
        assert.match(header, /^[\x00-\x7f]*$/)
        new Headers({ 'X-Maui-Request-Body': header })
        const request = JSON.parse(values[0])
        requests.push(request)
        const output = await handler(request)
        return JSON.stringify({ version: 1, session: request.session, id: request.id, status: 'success', result: output, error: null })
      },
      SendRawMessage(message) { messages.push(JSON.parse(message)) },
    },
  }
}
