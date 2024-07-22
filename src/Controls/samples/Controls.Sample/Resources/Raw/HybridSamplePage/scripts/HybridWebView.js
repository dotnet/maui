window.HybridWebView = {
    "Init": function () {
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

    "SendRawMessage": function (message) {
        window.HybridWebView.__SendMessageInternal('RawMessage', message);
    },

    "__SendMessageInternal": function (type, message) {

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

    "InvokeMethod": function (taskId, methodName, args) {
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

    "__TriggerAsyncCallback": function (taskId, result) {
        // Make sure the result is a string
        if (result && typeof (result) !== 'string') {
            result = JSON.stringify(result);
        }

        window.HybridWebView.__SendMessageInternal('InvokeMethodCompleted', taskId + '|' + result);
    }
}

window.HybridWebView.Init();
