/*
 * This file contains the JavaScript code that the HybridWebView control uses to 
 * communicate between the web view and the .NET host application.
 * 
 * The JavaScript file is generated from TypeScript and should not be modified
 * directly. To make changes, modify the TypeScript file and then recompile it.
 */

/*
 * Extend the global objects to include the interfaces that are used by the
 * HybridWebView and some operating systems and/or browser hosts.
 */
interface External {
    receiveMessage: (message: any) => void;
}
interface Window {
    chrome?: {
        webview?: {
            addEventListener: (event: string, handler: (arg: any) => void) => void;
            postMessage: (message: any) => void;
        };
    };
    webkit?: {
        messageHandlers?: {
            webwindowinterop?: {
                postMessage: (message: any) => void;
            };
        };
    };

    // Declare the global object that we have added on Android.
    hybridWebViewHost?: {
        sendMessage: (message: string) => void;
    };
}

/*
 * The following interfaces define the shape of the messages that are sent between
 * the web view and the .NET host application.
 */
interface JSInvokeMethodData {
    MethodName: string;
    ParamValues?: string[];
}
interface JSInvokeError {
    Name?: string;
    Message: string;
    StackTrace?: string;
}
interface DotNetInvokeResult {
    IsJson?: boolean;
    Result?: any;
    IsError?: boolean;
    ErrorMessage?: string;
    ErrorType?: string;
    ErrorStackTrace?: string;
}

(() => {

    // Cached function to send messages to the host application.
    let sendMessageFunction: ((message: any) => void) | null = null;

    /*
     * Initialize the HybridWebView messaging system.
     * This method is called once when the page is loaded.
     */
    function initHybridWebView() {
        function dispatchHybridWebViewMessage(message: any) {
            const event = new CustomEvent('HybridWebViewMessageReceived', { detail: { message: message } });
            window.dispatchEvent(event);
        }

        // Determine the mechanism to receive messages from the host application.
        if (window.chrome && window.chrome.webview && window.chrome.webview.addEventListener) {
            // Windows WebView2
            window.chrome.webview.addEventListener('message', (arg: any) => {
                dispatchHybridWebViewMessage(arg.data);
            });
        } else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            // @ts-ignore - We are extending the global object here
            window.external = {
                receiveMessage: (message: any) => {
                    dispatchHybridWebViewMessage(message);
                },
            };
        } else {
            // Android WebView
            window.addEventListener('message', (arg: any) => {
                dispatchHybridWebViewMessage(arg.data);
            });
        }

        // Determine the function to use to send messages to the host application.
        if (window.chrome && window.chrome.webview) {
            // Windows WebView2
            sendMessageFunction = msg => window.chrome.webview.postMessage(msg);
        } else if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.webwindowinterop) {
            // iOS and MacCatalyst WKWebView
            sendMessageFunction = msg => window.webkit.messageHandlers.webwindowinterop.postMessage(msg);
        } else if (window.hybridWebViewHost) {
            // Android WebView
            sendMessageFunction = msg => window.hybridWebViewHost.sendMessage(msg);
        }
    }

    /*
     * Send a message to the .NET host application.
     * The message is sent as a string with the following format: `<type>|<message>`.
     */
    function sendMessageToDotNet(type: string, message: string) {
        const messageToSend = type + '|' + message;

        if (sendMessageFunction) {
        sendMessageFunction(messageToSend);
        } else {
            console.error('Unable to send messages to .NET because the host environment for the HybridWebView was unknown.');
        }
    }

    /*
     * Send a message to the .NET host application indicating that a JavaScript method invocation completed.
     * The result is sent as a string with the following format: `<taskId>|<result-json>`.
     */
    function invokeJavaScriptCallbackInDotNet(taskId: string, result?: any) {
        const json = JSON.stringify(result);

        sendMessageToDotNet('__InvokeJavaScriptCompleted', taskId + '|' + json);
    }

    /*
     * Send a message to the .NET host application indicating that a JavaScript method invocation failed.
     * The error message is sent as a string with the following format: `<taskId>|<JSInvokeError>`.
     */
    function invokeJavaScriptFailedInDotNet(taskId: string, error: any) {
        let errorObj: JSInvokeError;

        if (!error) {
            errorObj = {
                Message: 'Unknown error',
                StackTrace: Error().stack
            };
        } else if (error instanceof Error) {
            errorObj = {
                Name: error.name,
                Message: error.message,
                StackTrace: error.stack
            };
        } else if (typeof error === 'string') {
            errorObj = {
                Message: error,
                StackTrace: Error().stack
            };
        } else {
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
    function sendRawMessage(message: string) {
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
    async function invokeDotNet(methodName: string, paramValues?: any) {
        const body: JSInvokeMethodData = {
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
        const requestUrl = `${window.location.origin}/__hwvInvokeDotNet`;
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

        const response: DotNetInvokeResult = await rawResponse.json();

        // a null response is a null response
        if (!response) {
            return null;
        }

        // Check if the response indicates an error
        if (response.IsError) {
            const error = new Error(response.ErrorMessage || 'Unknown error occurred in .NET method');
            if (response.ErrorType) {
                (error as any).dotNetErrorType = response.ErrorType;
            }
            if (response.ErrorStackTrace) {
                (error as any).dotNetStackTrace = response.ErrorStackTrace;
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
    async function invokeJavaScript(taskId: string, methodName: Function, args: any[]) {
        try {
            const result = await methodName(...args);
            invokeJavaScriptCallbackInDotNet(taskId, result);
        } catch (ex) {
            console.error(ex);
            invokeJavaScriptFailedInDotNet(taskId, ex);
        }
    }

    // Define the public API of the HybridWebView control.
    const HybridWebView = {
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
