window.HybridWebView = {
    "Init": function Init() {
        function DispatchHybridWebViewMessage(message) {
            const event = new CustomEvent("HybridWebViewMessageReceived", { detail: { message: message } });
            window.dispatchEvent(event);
        }

        if (window.chrome && window.chrome.webview) {
            // Windows WebView2
            window.chrome.webview.addEventListener('message', arg => {
                DispatchHybridWebViewMessage(arg.data);
            });
        }
        else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            window.external = {
                "receiveMessage": message => {
                    DispatchHybridWebViewMessage(message);
                }
            };
        }
        else {
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

        if (typeof paramValues !== 'undefined') {
            if (!Array.isArray(paramValues)) {
                paramValues = [paramValues];
            }

            for (var i = 0; i < paramValues.length; i++) {
                paramValues[i] = JSON.stringify(paramValues[i]);
            }

            if (paramValues.length > 0) {
                body.ParamValues = paramValues;
            }
        }

        const message = JSON.stringify(body);

        var requestUrl = `${window.location.origin}/__hwvInvokeDotNet?data=${encodeURIComponent(message)}`;

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

    "__InvokeJavaScript": function __InvokeJavaScript(taskId, methodName, args) {
        if (methodName[Symbol.toStringTag] === 'AsyncFunction') {
            // For async methods, we need to call the method and then trigger the callback when it's done
            const asyncPromise = methodName(...args);
            asyncPromise
                .then(asyncResult => {
                    window.HybridWebView.__TriggerAsyncCallback(taskId, asyncResult);
                })
                .catch(error => console.error(error));
        } else {
            // For sync methods, we can call the method and trigger the callback immediately
            const syncResult = methodName(...args);
            window.HybridWebView.__TriggerAsyncCallback(taskId, syncResult);
        }
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
