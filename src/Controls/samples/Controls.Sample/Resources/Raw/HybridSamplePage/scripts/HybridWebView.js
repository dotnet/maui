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

    "__InvokeJavaScript": async function __InvokeJavaScript(taskId, methodName, args) {
        try {
            var result = null;
            if (methodName[Symbol.toStringTag] === 'AsyncFunction') {
                result = await methodName(...args);
            } else {
                result = methodName(...args);
            }
            window.HybridWebView.__TriggerAsyncCallback(taskId, result);
        } catch (ex) {
            console.error(ex);
            window.HybridWebView.__TriggerAsyncFailedCallback(taskId, ex);
        }
    },

    "__TriggerAsyncFailedCallback": function __TriggerAsyncCallback(taskId, error) {

        if (!error) {
            json = {
                Message: "Unknown error",
                StackTrace: Error().stack
            };
        } else if (error instanceof Error) {
            json = {
                Name: error.name,
                Message: error.message,
                StackTrace: error.stack
            };
        } else if (typeof (error) === 'string') {
            json = {
                Message: error,
                StackTrace: Error().stack
            };
        } else {
            json = {
                Message: JSON.stringify(error),
                StackTrace: Error().stack
            };
        }

        json = JSON.stringify(json);

        window.HybridWebView.__SendMessageInternal('__InvokeJavaScriptFailed', taskId + '|' + json);
    },

    "__TriggerAsyncCallback": function __TriggerAsyncCallback(taskId, result) {
        const json = JSON.stringify(result);
        window.HybridWebView.__SendMessageInternal('__InvokeJavaScriptCompleted', taskId + '|' + json);
    }
}

window.HybridWebView.Init();
