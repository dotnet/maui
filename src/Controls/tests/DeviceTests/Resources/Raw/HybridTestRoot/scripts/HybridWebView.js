// Nit: Since "HybridWebView" is not a class,
// typical JS conventions would suggest it be named "hybridWebView".
window.HybridWebView = {
    
    // This function declaration syntax can be shortened:
    // "Init": function Init() {
    // to:
    Init() {
        // Nit: Generally, JS functions use camelCase.

        function DispatchHybridWebViewMessage(message) {
            const event = new CustomEvent("HybridWebViewMessageReceived", { detail: { message: message } });
            window.dispatchEvent(event);
        }

        // Modern JS supports optional chaining:
        // if (window.chrome && window.chrome.webview) {
        if (window.chrome?.webview) {
            // Windows WebView2
            window.chrome.webview.addEventListener('message', arg => {
                DispatchHybridWebViewMessage(arg.data);
            });
        // } else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
        } else if (window.webkit?.messageHandlers?.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            window.external = {
                "receiveMessage": message => {
                    DispatchHybridWebViewMessage(message);
                }
            };
        } else {
            // Android WebView
            window.addEventListener('message', arg => {
                DispatchHybridWebViewMessage(arg.data);
            });
        }
    },

    "SendRawMessage": function SendRawMessage(message) {
        window.HybridWebView.__SendMessageInternal('__RawMessage', message);
    },

    "InvokeDotNet": async function InvokeDotNetAsync(methodName, paramValues) {
        const body = {
            MethodName: methodName
        };

        // Since there's no question that the 'paramValues' variable was declared,
        // it's possible to sorten this:
        // if (typeof paramValues !== 'undefined') {
        // to:
        if (paramValues !== undefined) {
            if (!Array.isArray(paramValues)) {
                paramValues = [paramValues];
            }

            // Generally, people avoid 'var' nowadays, primarily because its declaration
            // is scoped to the containing function, not the containing block (so, in the below case,
            // you could accidentally reference 'i' outside the scope of the 'for' loop).
            // for (var i = 0; i < paramValues.length; i++) {
            for (let i = 0; i < paramValues.length; i++) {
                paramValues[i] = JSON.stringify(paramValues[i]);
            }

            if (paramValues.length > 0) {
                body.ParamValues = paramValues;
            }
        }

        const message = JSON.stringify(body);

        const requestUrl = `${window.location.origin}/__hwvInvokeDotNet?data=${encodeURIComponent(message)}`;

        const rawResponse = await fetch(requestUrl, {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            }
        });
        const response = await rawResponse.json();

        if (response) {
            if (response.IsJson) {
                return JSON.parse(response.Result);
            }

            return response.Result;
        }

        return null;
    },

    "__SendMessageInternal": function __SendMessageInternal(type, message) {

        const messageToSend = type + '|' + message;

        // Can use optional chaining here too.

        if (window.chrome && window.chrome.webview) {
            // Windows WebView2
            window.chrome.webview.postMessage(messageToSend);
        }
        else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            window.webkit.messageHandlers.webwindowinterop.postMessage(messageToSend);
        }
        else {
            // Android WebView
            hybridWebViewHost.sendMessage(messageToSend);
        }
    },

    "__InvokeJavaScript": async function __InvokeJavaScript(taskId, methodName, args) {
        // As discussed, this can probably be shortened to:
        try {
            await methodName(...args);
            window.HybridWebView.__TriggerAsyncCallback(taskId, asyncResult);
        } catch (e) {
            // Handle somehow
        }

        // if (methodName[Symbol.toStringTag] === 'AsyncFunction') {
        //     // For async methods, we need to call the method and then trigger the callback when it's done
        //     const asyncPromise = methodName(...args);
        //     asyncPromise
        //         .then(asyncResult => {
        //             window.HybridWebView.__TriggerAsyncCallback(taskId, asyncResult);
        //         })
        //         .catch(error => console.error(error));
        // } else {
        //     // For sync methods, we can call the method and trigger the callback immediately
        //     const syncResult = methodName(...args);
        //     window.HybridWebView.__TriggerAsyncCallback(taskId, syncResult);
        // }
    },

    "__TriggerAsyncCallback": function __TriggerAsyncCallback(taskId, result) {
        // Make sure the result is a string
        if (result && typeof (result) !== 'string') {
            result = JSON.stringify(result);
        }

        window.HybridWebView.__SendMessageInternal('__InvokeJavaScriptCompleted', taskId + '|' + result);
    }
}

window.HybridWebView.Init();

// High-level: It looks like some of these function declarations are meant to be "private", i.e.,
// not called by anyone outside this script (Init, and possibly the '__'-prefixed functions).
// A way to make these "actually" private is to do something like this:

(() => {
    // This creates a sort of "scope" for everything declared in this file.

    function init() {
        // ...
    }

    function sendRawMessage() {
        // ...
    }
    
    function sendMessageInternal() {
        // ...
    }

    // ...

    // Then all the stuff that needs to be "public" can be specified here:
    window.hybridWebView = {
        sendRawMessage,
        // ...
    };
})();
