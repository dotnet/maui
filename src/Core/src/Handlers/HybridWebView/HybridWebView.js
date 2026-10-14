/*
 * This file contains the JavaScript code that the HybridWebView control uses to
 * communicate between the web view and the .NET host application.
 *
 * The JavaScript file is generated from TypeScript and should not be modified
 * directly. To make changes, modify the TypeScript file and then recompile it.
 */
(() => {
    // Must stay in sync with HybridWebViewHandler's reserved endpoint paths.
    const InvokeDotNetEndpoint = '__hwvInvokeDotNet';
    const SendMessageEndpoint = '__hwvSendMessage';
    const BinaryEndpoint = '__hwvInvokeDotNetBinary';
    // Cached function to send messages to the host application.
    let sendMessageFunction = null;
    // Replaced by the native handler when serving the framework script. Never cache this script.
    const binarySession = '__MAUI_BINARY_DOCUMENT_SESSION__';
    let binaryId = 0;
    let binaryActive = 0;
    let binaryBeginQueue = Promise.resolve();
    const binaryAborts = new Set();
    window.addEventListener('pagehide', () => {
        for (const controller of binaryAborts)
            controller.abort();
    });
    /**
     * Invokes an explicitly approved native command with complete binary bytes.
     * Limits: 4 MiB in either direction, four active calls, 30 seconds per transfer.
     * Android uploads sequential 12 KiB base64 fragments; results are always raw binary.
     */
    async function invokeDotNetBinary(methodName, payload, options = {}) {
        if (!/^[0-9a-f]{32}$/.test(binarySession) || window.top !== window ||
            !/^(app|https):\/\/0\.0\.0\.1$/.test(window.location.origin)) {
            throw new Error('Binary invocation is available only in an opted-in local top-level document.');
        }
        if (!/^[A-Za-z0-9_.:-]{1,128}$/.test(methodName))
            throw new Error('Invalid binary command name.');
        if (!(payload instanceof ArrayBuffer) && !ArrayBuffer.isView(payload))
            throw new TypeError('Expected binary bytes.');
        const source = payload instanceof ArrayBuffer ? new Uint8Array(payload) :
            new Uint8Array(payload.buffer, payload.byteOffset, payload.byteLength);
        if (source.byteLength > 4 * 1024 * 1024)
            throw new Error('Binary payloads are limited to 4 MiB.');
        if (binaryActive >= 4)
            throw new Error('At most four binary invocations may be active.');
        const metadata = encodeURIComponent(JSON.stringify({ MethodName: methodName, Length: source.byteLength, Headers: options.headers }));
        if (metadata.length > 16384)
            throw new Error('Binary metadata is too large.');
        if (binaryId >= 2147483647)
            throw new Error('Binary request ids exhausted; reload the document.');
        const id = ++binaryId;
        const bytes = source.slice(); // Snapshot before awaits; caller mutation cannot alter the upload.
        const controller = new AbortController();
        const abort = () => controller.abort();
        options.signal?.addEventListener('abort', abort, { once: true });
        if (options.signal?.aborted)
            controller.abort();
        binaryActive++;
        binaryAborts.add(controller);
        let begun = false;
        const timer = setTimeout(abort, 30000);
        const fragmented = !(window.chrome?.webview || window.webkit?.messageHandlers?.webwindowinterop);
        async function request(operation, extra = {}, body, signal = controller.signal) {
            const response = await fetch(`${window.location.origin}/${BinaryEndpoint}`, {
                method: 'POST',
                cache: 'no-store',
                signal,
                headers: {
                    'Content-Type': 'application/octet-stream',
                    'X-Maui-Binary-Token': 'HybridWebView/1',
                    'X-Maui-Binary-Session': binarySession,
                    'X-Maui-Binary-Id': String(id),
                    'X-Maui-Binary-Operation': operation,
                    ...extra
                },
                body
            });
            const result = await response.arrayBuffer();
            const marker = new Uint8Array(result)[0];
            if (marker === 1) {
                const failure = JSON.parse(new TextDecoder().decode(result.slice(1)));
                const error = new Error(failure.ErrorMessage || 'Binary invocation failed.');
                error.dotNetErrorType = failure.ErrorType;
                throw error;
            }
            if (!response.ok || marker !== 0 || result.byteLength > 4 * 1024 * 1024 + 1) {
                throw new Error('Invalid binary invocation response.');
            }
            return result.slice(1);
        }
        try {
            // Only four calls can enter this queue; serialize starts to make ids replay-proof.
            const begin = binaryBeginQueue.then(async () => {
                begun = true;
                await request('begin', { 'X-Maui-Binary-Metadata': metadata });
            });
            binaryBeginQueue = begin.catch(() => { });
            await begin;
            if (fragmented) {
                for (let offset = 0, index = 0; offset < bytes.length; offset += 12 * 1024, index++) {
                    let characters = '';
                    for (const byte of bytes.subarray(offset, offset + 12 * 1024))
                        characters += String.fromCharCode(byte);
                    await request('chunk', {
                        'X-Maui-Binary-Index': String(index),
                        'X-Maui-Binary-Fragment': btoa(characters)
                    });
                }
            }
            return await request('invoke', {}, fragmented ? undefined : bytes);
        }
        catch (error) {
            if (begun) {
                // Best effort, never retry the host invocation. Native timeout is the final cleanup.
                const cleanup = new AbortController();
                const cleanupTimer = setTimeout(() => cleanup.abort(), 1000);
                try {
                    await request('cancel', {}, undefined, cleanup.signal);
                }
                catch {
                    // The original failure is authoritative, including an already-completed request.
                }
                finally {
                    clearTimeout(cleanupTimer);
                }
            }
            throw error;
        }
        finally {
            clearTimeout(timer);
            options.signal?.removeEventListener('abort', abort);
            binaryAborts.delete(controller);
            binaryActive--;
        }
    }
    /*
     * Initialize the HybridWebView messaging system.
     * This method is called once when the page is loaded.
     */
    function initHybridWebView() {
        function dispatchHybridWebViewMessage(message) {
            const event = new CustomEvent('HybridWebViewMessageReceived', { detail: { message: message } });
            window.dispatchEvent(event);
        }
        // Determine the mechanism to receive messages from the host application.
        if (window.chrome && window.chrome.webview && window.chrome.webview.addEventListener) {
            // Windows WebView2
            // The .NET side URL-encodes messages (see MauiHybridWebView.SendRawMessage) so embedded
            // NUL characters survive WebView2's null-terminated string marshalling. Decode here.
            window.chrome.webview.addEventListener('message', (arg) => {
                dispatchHybridWebViewMessage(decodeURIComponent(arg.data));
            });
        }
        else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            // @ts-ignore - We are extending the global object here
            window.external = {
                receiveMessage: (message) => {
                    dispatchHybridWebViewMessage(message);
                },
            };
        }
        else {
            // Android WebView
            // Native -> JS messages are delivered via WebView.postWebMessage. The resulting
            // MessageEvent has source === null (no sending window) -- that's the invariant we
            // can rely on across WebView versions. Drop any 'message' event whose source is
            // a Window (e.g. a nested iframe calling window.parent.postMessage), which would
            // otherwise be mistaken for a native message. NOTE: we intentionally do not check
            // arg.origin here because Android WebView delivers postWebMessage events with an
            // empty-string origin, not window.location.origin.
            window.addEventListener('message', (arg) => {
                if (arg.source !== null) {
                    console.warn(`HybridWebView: ignored 'message' event from unexpected sender (origin: '${arg.origin}').`);
                    return;
                }
                dispatchHybridWebViewMessage(arg.data);
            });
        }
        // Determine the function to use to send messages to the host application.
        if (window.chrome && window.chrome.webview) {
            // Windows WebView2
            // URL-encode so embedded NUL characters survive WebView2's null-terminated string
            // marshalling (TryGetWebMessageAsString returns an LPWSTR); the .NET side decodes it
            // in HybridWebViewHandler.OnWebMessageReceived.
            sendMessageFunction = msg => window.chrome.webview.postMessage(encodeURIComponent(msg));
        }
        else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            sendMessageFunction = msg => window.webkit.messageHandlers.webwindowinterop.postMessage(msg);
        }
        else {
            // Android WebView. Sends are chained through a single promise to preserve
            // FIFO ordering that callers had with the previous synchronous bridge.
            let sendQueue;
            sendMessageFunction = msg => {
                const url = `${window.location.origin}/${SendMessageEndpoint}`;
                const doSend = () => fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'text/plain',
                        'X-Maui-Invoke-Token': 'HybridWebView',
                        'X-Maui-Request-Body': msg
                    },
                    body: msg
                }).catch(err => {
                    console.error('HybridWebView: failed to send message to .NET host.', err);
                });
                sendQueue = sendQueue ? sendQueue.then(doSend) : doSend();
            };
        }
    }
    /*
     * Send a message to the .NET host application.
     * The message is sent as a string with the following format: `<type>|<message>`.
     */
    function sendMessageToDotNet(type, message) {
        const messageToSend = type + '|' + message;
        if (sendMessageFunction) {
            sendMessageFunction(messageToSend);
        }
        else {
            console.error('Unable to send messages to .NET because the host environment for the HybridWebView was unknown.');
        }
    }
    /*
     * Send a message to the .NET host application indicating that a JavaScript method invocation completed.
     * The result is sent as a string with the following format: `<taskId>|<result-json>`.
     */
    function invokeJavaScriptCallbackInDotNet(taskId, result) {
        const json = JSON.stringify(result);
        sendMessageToDotNet('__InvokeJavaScriptCompleted', taskId + '|' + json);
    }
    /*
     * Send a message to the .NET host application indicating that a JavaScript method invocation failed.
     * The error message is sent as a string with the following format: `<taskId>|<JSInvokeError>`.
     */
    function invokeJavaScriptFailedInDotNet(taskId, error) {
        let errorObj;
        if (!error) {
            errorObj = {
                Message: 'Unknown error',
                StackTrace: Error().stack
            };
        }
        else if (error instanceof Error) {
            errorObj = {
                Name: error.name,
                Message: error.message,
                StackTrace: error.stack
            };
        }
        else if (typeof error === 'string') {
            errorObj = {
                Message: error,
                StackTrace: Error().stack
            };
        }
        else {
            errorObj = {
                Message: JSON.stringify(error),
                StackTrace: Error().stack
            };
        }
        const json = JSON.stringify(errorObj);
        sendMessageToDotNet('__InvokeJavaScriptFailed', taskId + '|' + json);
    }
    /*
     * Send a raw message to the .NET host application.
     * The message is sent directly and not processed or serialized.
     *
     * @param message The message to send to the .NET host application.
     */
    function sendRawMessage(message) {
        // URL-encode the payload so it survives transports that restrict the byte set
        // (the Android fetch X-Maui-Request-Body header rejects CR/LF/NUL). Decoded
        // on the .NET side in HybridWebViewHandler.MessageReceived.
        sendMessageToDotNet('__RawMessage', encodeURIComponent(message));
    }
    /*
     * Invoke a .NET method on the InvokeJavaScriptTarget instance.
     * The method name and parameters are serialized and sent to the .NET host application.
     *
     * @param methodName The name of the .NET method to invoke.
     * @param paramValues The parameters to pass to the .NET method. If the method takes no parameters, this can be omitted.
     *
     * @returns A promise that resolves with the result of the .NET method invocation.
     */
    async function invokeDotNet(methodName, paramValues) {
        const body = {
            MethodName: methodName
        };
        // if parameters were provided, serialize them first
        if (paramValues !== undefined) {
            if (!Array.isArray(paramValues)) {
                paramValues = [paramValues];
            }
            for (let i = 0; i < paramValues.length; i++) {
                paramValues[i] = JSON.stringify(paramValues[i]);
            }
            if (paramValues.length > 0) {
                body.ParamValues = paramValues;
            }
        }
        const message = JSON.stringify(body);
        // send the request to .NET
        const requestUrl = `${window.location.origin}/${InvokeDotNetEndpoint}`;
        const rawResponse = await fetch(requestUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'X-Maui-Invoke-Token': 'HybridWebView',
                'X-Maui-Request-Body': message // Some platforms (Android) do not expose the POST body
            },
            body: message
        });
        const response = await rawResponse.json();
        // a null response is a null response
        if (!response) {
            return null;
        }
        // Check if the response indicates an error
        if (response.IsError) {
            const error = new Error(response.ErrorMessage || 'Unknown error occurred in .NET method');
            if (response.ErrorType) {
                error.dotNetErrorType = response.ErrorType;
            }
            if (response.ErrorStackTrace) {
                error.dotNetStackTrace = response.ErrorStackTrace;
            }
            throw error;
        }
        // deserialize if there is JSON data
        if (response.IsJson) {
            return JSON.parse(response.Result);
        }
        // otherwise return the primitive
        return response.Result;
    }
    /*
     * Invoke a JavaScript method from the .NET host application.
     * This method is called from the HybridWebViewHandler and is not intended to be used by user applications.
     *
     * @param taskId The task ID that was provided by the .NET host application.
     * @param methodName The JavaScript method to invoke in the global scope.
     * @param args The arguments to pass to the JavaScript method.
     *
     * @returns A promise.
     */
    async function invokeJavaScript(taskId, methodName, args) {
        try {
            const result = await methodName(...args);
            invokeJavaScriptCallbackInDotNet(taskId, result);
        }
        catch (ex) {
            console.error(ex);
            invokeJavaScriptFailedInDotNet(taskId, ex);
        }
    }
    // Define the public API of the HybridWebView control.
    const HybridWebView = {
        SendRawMessage: sendRawMessage,
        InvokeDotNet: invokeDotNet,
        ...(/^[0-9a-f]{32}$/.test(binarySession) && window.top === window && /^(app|https):\/\/0\.0\.0\.1$/.test(window.location.origin)
            ? { InvokeDotNetBinary: invokeDotNetBinary } : {}),
        __InvokeJavaScript: invokeJavaScript
    };
    // Make the following APIs available in global scope for invocation from JS
    // @ts-ignore - We are extending the global object here
    window['HybridWebView'] = HybridWebView;
    // Initialize the HybridWebView
    initHybridWebView();
})();
