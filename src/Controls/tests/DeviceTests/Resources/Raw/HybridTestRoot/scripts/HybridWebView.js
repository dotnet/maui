function HybridWebViewInit() {

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
}

window.HybridWebView = {
    "SendRawMessage": function (message) {

        if (window.chrome && window.chrome.webview) {
            // Windows WebView2
            window.chrome.webview.postMessage(message);
        }
        else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            window.webkit.messageHandlers.webwindowinterop.postMessage(message);
        }
        else {
            // Android WebView
            hybridWebViewHost.sendRawMessage(message);
        }
    }
}

HybridWebViewInit();
