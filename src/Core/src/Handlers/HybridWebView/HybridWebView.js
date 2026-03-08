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
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g = Object.create((typeof Iterator === "function" ? Iterator : Object).prototype);
    return g.next = verb(0), g["throw"] = verb(1), g["return"] = verb(2), typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
(function () {
    // Cached function to send messages to the host application.
    var sendMessageFunction = null;
    /*
     * Initialize the HybridWebView messaging system.
     * This method is called once when the page is loaded.
     */
    function initHybridWebView() {
        function dispatchHybridWebViewMessage(message) {
            var event = new CustomEvent('HybridWebViewMessageReceived', { detail: { message: message } });
            window.dispatchEvent(event);
        }
        // Determine the mechanism to receive messages from the host application.
        if (window.chrome && window.chrome.webview && window.chrome.webview.addEventListener) {
            // Windows WebView2
            window.chrome.webview.addEventListener('message', function (arg) {
                dispatchHybridWebViewMessage(arg.data);
            });
        }
        else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            // @ts-ignore - We are extending the global object here
            window.external = {
                receiveMessage: function (message) {
                    dispatchHybridWebViewMessage(message);
                },
            };
        }
        else {
            // Android WebView
            window.addEventListener('message', function (arg) {
                dispatchHybridWebViewMessage(arg.data);
            });
        }
        // Determine the function to use to send messages to the host application.
        if (window.chrome && window.chrome.webview) {
            // Windows WebView2
            sendMessageFunction = function (msg) { return window.chrome.webview.postMessage(msg); };
        }
        else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            sendMessageFunction = function (msg) { return window.webkit.messageHandlers.webwindowinterop.postMessage(msg); };
        }
        else if (window.hybridWebViewHost) {
            // Android WebView
            sendMessageFunction = function (msg) { return window.hybridWebViewHost.sendMessage(msg); };
        }
    }
    /*
     * Send a message to the .NET host application.
     * The message is sent as a string with the following format: `<type>|<message>`.
     */
    function sendMessageToDotNet(type, message) {
        var messageToSend = type + '|' + message;
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
        var json = JSON.stringify(result);
        sendMessageToDotNet('__InvokeJavaScriptCompleted', taskId + '|' + json);
    }
    /*
     * Send a message to the .NET host application indicating that a JavaScript method invocation failed.
     * The error message is sent as a string with the following format: `<taskId>|<JSInvokeError>`.
     */
    function invokeJavaScriptFailedInDotNet(taskId, error) {
        var errorObj;
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
        var json = JSON.stringify(errorObj);
        sendMessageToDotNet('__InvokeJavaScriptFailed', taskId + '|' + json);
    }
    /*
     * Send a raw message to the .NET host application.
     * The message is sent directly and not processed or serialized.
     *
     * @param message The message to send to the .NET host application.
     */
    function sendRawMessage(message) {
        sendMessageToDotNet('__RawMessage', message);
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
    function invokeDotNet(methodName, paramValues) {
        return __awaiter(this, void 0, void 0, function () {
            var body, i, message, requestUrl, rawResponse, response, error;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        body = {
                            MethodName: methodName
                        };
                        // if parameters were provided, serialize them first
                        if (paramValues !== undefined) {
                            if (!Array.isArray(paramValues)) {
                                paramValues = [paramValues];
                            }
                            for (i = 0; i < paramValues.length; i++) {
                                paramValues[i] = JSON.stringify(paramValues[i]);
                            }
                            if (paramValues.length > 0) {
                                body.ParamValues = paramValues;
                            }
                        }
                        message = JSON.stringify(body);
                        requestUrl = "".concat(window.location.origin, "/__hwvInvokeDotNet");
                        return [4 /*yield*/, fetch(requestUrl, {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json',
                                    'Accept': 'application/json',
                                    'X-Maui-Invoke-Token': 'HybridWebView',
                                    'X-Maui-Request-Body': message // Some platforms (Android) do not expose the POST body
                                },
                                body: message
                            })];
                    case 1:
                        rawResponse = _a.sent();
                        return [4 /*yield*/, rawResponse.json()];
                    case 2:
                        response = _a.sent();
                        // a null response is a null response
                        if (!response) {
                            return [2 /*return*/, null];
                        }
                        // Check if the response indicates an error
                        if (response.IsError) {
                            error = new Error(response.ErrorMessage || 'Unknown error occurred in .NET method');
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
                            return [2 /*return*/, JSON.parse(response.Result)];
                        }
                        // otherwise return the primitive
                        return [2 /*return*/, response.Result];
                }
            });
        });
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
    function invokeJavaScript(taskId, methodName, args) {
        return __awaiter(this, void 0, void 0, function () {
            var result, ex_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, methodName.apply(void 0, args)];
                    case 1:
                        result = _a.sent();
                        invokeJavaScriptCallbackInDotNet(taskId, result);
                        return [3 /*break*/, 3];
                    case 2:
                        ex_1 = _a.sent();
                        console.error(ex_1);
                        invokeJavaScriptFailedInDotNet(taskId, ex_1);
                        return [3 /*break*/, 3];
                    case 3: return [2 /*return*/];
                }
            });
        });
    }
    // Define the public API of the HybridWebView control.
    var HybridWebView = {
        SendRawMessage: sendRawMessage,
        InvokeDotNet: invokeDotNet,
        __InvokeJavaScript: invokeJavaScript
    };
    // Make the following APIs available in global scope for invocation from JS
    // @ts-ignore - We are extending the global object here
    window['HybridWebView'] = HybridWebView;
    // Initialize the HybridWebView
    initHybridWebView();
})();
//# sourceMappingURL=HybridWebView.js.map