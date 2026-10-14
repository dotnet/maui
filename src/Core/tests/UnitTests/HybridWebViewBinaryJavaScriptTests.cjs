const assert = require('node:assert/strict');
const { test } = require('node:test');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const source = fs.readFileSync(path.join(__dirname, '../../src/Handlers/HybridWebView/HybridWebView.js'), 'utf8');
const session = '0123456789abcdef0123456789abcdef';
const success = bytes => ({
    ok: true,
    arrayBuffer: async () => Uint8Array.from([0, ...bytes]).buffer
});

function load(fetch, { platform = 'android', origin = 'https://0.0.0.1', enabled = true, mainFrame = true } = {}) {
    const events = {};
    const window = {
        location: { origin },
        addEventListener: (event, callback) => { events[event] = callback; },
        dispatchEvent() {}
    };
    window.top = mainFrame ? window : {};
    if (platform === 'windows') window.chrome = { webview: { addEventListener() {}, postMessage() {} } };
    if (platform === 'apple') window.webkit = { messageHandlers: { webwindowinterop: { postMessage() {} } } };
    const context = {
        window, console, fetch, ArrayBuffer, Uint8Array, AbortController, TextDecoder,
        setTimeout, clearTimeout, btoa: value => Buffer.from(value, 'binary').toString('base64'),
        CustomEvent: class {}
    };
    vm.runInNewContext(enabled ? source.replaceAll('__MAUI_BINARY_DOCUMENT_SESSION__', session) : source, context);
    return { api: window.HybridWebView, events };
}

for (const platform of ['android', 'windows', 'apple']) {
    for (const length of [0, 256, 150000]) {
        test(`${platform}: whole-buffer bytes survive exactly (${length})`, async () => {
            const input = Uint8Array.from({ length }, (_, i) => i);
            let lengthFromMetadata;
            const chunks = [];
            let invokes = 0;
            const { api } = load(async (url, request) => {
                assert.equal(url, (platform === 'apple' ? 'app' : 'https') + '://0.0.0.1/__hwvInvokeDotNetBinary');
                assert.equal(request.headers['X-Maui-Binary-Token'], 'HybridWebView/1');
                assert.equal(request.headers['X-Maui-Binary-Session'], session);
                assert.equal(request.cache, 'no-store');
                switch (request.headers['X-Maui-Binary-Operation']) {
                    case 'begin': {
                        const metadata = JSON.parse(decodeURIComponent(request.headers['X-Maui-Binary-Metadata']));
                        assert.equal(metadata.MethodName, 'echo.bytes');
                        assert.deepEqual(metadata.Headers, { 'app-tag': 'value' });
                        lengthFromMetadata = metadata.Length;
                        return success([]);
                    }
                    case 'chunk': {
                        assert.equal(platform, 'android');
                        assert.equal(request.body, undefined);
                        assert.equal(Number(request.headers['X-Maui-Binary-Index']), chunks.length);
                        const chunk = Buffer.from(request.headers['X-Maui-Binary-Fragment'], 'base64');
                        assert.ok(chunk.length <= 12 * 1024);
                        chunks.push(chunk);
                        return success([]);
                    }
                    case 'invoke': {
                        invokes++;
                        const received = platform === 'android' ? Buffer.concat(chunks) : Buffer.from(request.body);
                        assert.equal(received.length, lengthFromMetadata);
                        assert.deepEqual(received, Buffer.from(input));
                        return success(received);
                    }
                    default: throw new Error('Unexpected operation');
                }
            }, { platform, origin: (platform === 'apple' ? 'app' : 'https') + '://0.0.0.1' });
            const output = await api.InvokeDotNetBinary('echo.bytes', input, { headers: { 'app-tag': 'value' } });
            assert.ok(output instanceof ArrayBuffer);
            assert.deepEqual(Buffer.from(output), Buffer.from(input));
            assert.equal(invokes, 1);
        });
    }
}

test('binary API is absent without opt-in, in guests, and in remote documents', () => {
    for (const options of [{ enabled: false }, { mainFrame: false }, { origin: 'https://remote.invalid' }]) {
        const { api } = load(() => { throw new Error('Must not fetch'); }, options);
        assert.equal(api.InvokeDotNetBinary, undefined);
        assert.equal(typeof api.InvokeDotNet, 'function');
    }
});

test('typed-array subranges are snapshotted before awaits', async () => {
    const data = new Uint8Array([10, 0, 128, 255, 20]);
    const { api } = load(async (_, request) => {
        if (request.headers['X-Maui-Binary-Operation'] === 'begin') {
            data.fill(99);
            return success([]);
        }
        assert.deepEqual(Array.from(request.body), [0, 128, 255]);
        return success(request.body);
    }, { platform: 'windows' });
    assert.deepEqual(Array.from(new Uint8Array(await api.InvokeDotNetBinary('echo', data.subarray(1, 4)))), [0, 128, 255]);
});

test('limits fail locally without starting an upload', async () => {
    const { api } = load(() => { throw new Error('Must not fetch'); });
    await assert.rejects(api.InvokeDotNetBinary('echo', new Uint8Array(4194305)), /4 MiB/);
    await assert.rejects(api.InvokeDotNetBinary('bad name', new Uint8Array()), /command/);
    await assert.rejects(api.InvokeDotNetBinary('echo', 'not binary'), /binary bytes/);
});

test('structured native errors reject instead of resolving with JSON bytes', async () => {
    const { api } = load(async (_, request) => {
        if (request.headers['X-Maui-Binary-Operation'] !== 'invoke') return success([]);
        return {
            ok: true,
            arrayBuffer: async () => Uint8Array.from([1, ...Buffer.from(JSON.stringify({
                IsError: true, ErrorType: 'UnauthorizedAccessException', ErrorMessage: 'Command denied'
            }))]).buffer
        };
    }, { platform: 'windows' });
    await assert.rejects(api.InvokeDotNetBinary('denied', new Uint8Array()), error =>
        error.message === 'Command denied' && error.dotNetErrorType === 'UnauthorizedAccessException');
});

test('four in-flight calls bound the begin queue; overflow is rejected', async () => {
    const releases = [];
    const { api } = load(async (_, request) => {
        if (request.headers['X-Maui-Binary-Operation'] !== 'invoke') return success([]);
        return new Promise(resolve => releases.push(() => resolve(success([]))));
    }, { platform: 'windows' });
    const calls = Array.from({ length: 4 }, () => api.InvokeDotNetBinary('echo', new Uint8Array()));
    await assert.rejects(api.InvokeDotNetBinary('echo', new Uint8Array()), /four/);
    while (releases.length < 4) await new Promise(resolve => setImmediate(resolve));
    releases.forEach(release => release());
    await Promise.all(calls);
});

test('abort propagates and cancels upload without retrying native dispatch', async () => {
    const controller = new AbortController();
    const operations = [];
    const { api } = load(async (_, request) => {
        const operation = request.headers['X-Maui-Binary-Operation'];
        operations.push(operation);
        if (operation === 'chunk') {
            controller.abort();
            throw new Error('Aborted');
        }
        return success([]);
    });
    await assert.rejects(api.InvokeDotNetBinary('echo', new Uint8Array(150000), { signal: controller.signal }), /Aborted/);
    assert.deepEqual(operations, ['begin', 'chunk', 'cancel']);
});

test('legacy JSON invocation preserves request and result shapes', async () => {
    const { api } = load(async (url, request) => {
        assert.equal(url, 'https://0.0.0.1/__hwvInvokeDotNet');
        assert.deepEqual(JSON.parse(request.body), { MethodName: 'Echo', ParamValues: ['{"value":123}'] });
        assert.equal(request.headers['X-Maui-Request-Body'], request.body);
        return { json: async () => ({ IsJson: true, IsError: false, Result: '{"value":123}' }) };
    });
    assert.equal((await api.InvokeDotNet('Echo', { value: 123 })).value, 123);
});
