import assert from 'node:assert/strict'

export function mock(handler) {
  const requests = [], messages = []
  return {
    requests, messages,
    bridge: {
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
