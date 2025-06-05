/*
 * This file contains the JavaScript code that the HybridWebView control uses to
 * communicate between the web view and the .NET host application.
 *
 * The JavaScript file is generated from TypeScript and should not be modified
 * directly. To make changes, modify the TypeScript file and then recompile it.
 */
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
(() => {
    // Cached function to send messages to the host application.
    let sendMessageFunction = null;
    /*
     * Initialize the HybridWebView messaging system.
     * This method is called once when the page is loaded.
     */
    function initHybridWebView() {
        var _a, _b, _c, _d, _e, _f, _g;
        function dispatchHybridWebViewMessage(message) {
            const event = new CustomEvent('HybridWebViewMessageReceived', { detail: { message: message } });
            window.dispatchEvent(event);
        }
        // Determine the mechanism to receive messages from the host application.
        if ((_b = (_a = window.chrome) === null || _a === void 0 ? void 0 : _a.webview) === null || _b === void 0 ? void 0 : _b.addEventListener) {
            // Windows WebView2
            window.chrome.webview.addEventListener('message', (arg) => {
                dispatchHybridWebViewMessage(arg.data);
            });
        }
        else if ((_d = (_c = window.webkit) === null || _c === void 0 ? void 0 : _c.messageHandlers) === null || _d === void 0 ? void 0 : _d.webwindowinterop) {
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
            window.addEventListener('message', (arg) => {
                dispatchHybridWebViewMessage(arg.data);
            });
        }
        // Determine the function to use to send messages to the host application.
        if ((_e = window.chrome) === null || _e === void 0 ? void 0 : _e.webview) {
            // Windows WebView2
            sendMessageFunction = msg => window.chrome.webview.postMessage(msg);
        }
        else if ((_g = (_f = window.webkit) === null || _f === void 0 ? void 0 : _f.messageHandlers) === null || _g === void 0 ? void 0 : _g.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            sendMessageFunction = msg => window.webkit.messageHandlers.webwindowinterop.postMessage(msg);
        }
        else if (window.hybridWebViewHost) {
            // Android WebView
            sendMessageFunction = msg => window.hybridWebViewHost.sendMessage(msg);
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
    const HybridWebView = {
        /*
         * Send a raw message to the .NET host application.
         * The message is sent directly and not processed or serialized.
         *
         * @param message The message to send to the .NET host application.
         */
        SendRawMessage: function (message) {
            sendMessageToDotNet('__RawMessage', message);
        },
        /*
         * Invoke a .NET method on the InvokeJavaScriptTarget instance.
         * The method name and parameters are serialized and sent to the .NET host application.
         *
         * @param methodName The name of the .NET method to invoke.
         * @param paramValues The parameters to pass to the .NET method. If the method takes no parameters, this can be omitted.
         *
         * @returns A promise that resolves with the result of the .NET method invocation.
         */
        InvokeDotNet: function (methodName, paramValues) {
            return __awaiter(this, void 0, void 0, function* () {
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
                const requestUrl = `${window.location.origin}/__hwvInvokeDotNet?data=${encodeURIComponent(message)}`;
                const rawResponse = yield fetch(requestUrl, {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json'
                    }
                });
                const response = yield rawResponse.json();
                if (!response) {
                    return null;
                }
                if (response.IsJson) {
                    return JSON.parse(response.Result);
                }
                return response.Result;
            });
        },
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
        __InvokeJavaScript: function (taskId, methodName, args) {
            return __awaiter(this, void 0, void 0, function* () {
                try {
                    const result = yield methodName(...args);
                    invokeJavaScriptCallbackInDotNet(taskId, result);
                }
                catch (ex) {
                    console.error(ex);
                    invokeJavaScriptFailedInDotNet(taskId, ex);
                }
            });
        }
    };
    // Make the following APIs available in global scope for invocation from JS
    // @ts-ignore - We are extending the global object here
    window['HybridWebView'] = HybridWebView;
    // Initialize the HybridWebView
    initHybridWebView();
})();
