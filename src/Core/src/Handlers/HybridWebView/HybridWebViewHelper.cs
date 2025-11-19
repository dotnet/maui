#if PLATFORM && !TIZEN
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui;

/// <summary>
/// Helper class containing all business logic for HybridWebView operations.
/// Keeps both Controls and Handler layers thin by centralizing processing logic.
/// </summary>
[RequiresUnreferencedCode("HybridWebView uses dynamic System.Text.Json serialization features.")]
#if !NETSTANDARD
[RequiresDynamicCode("HybridWebView uses dynamic System.Text.Json serialization features.")]
#endif
internal static partial class HybridWebViewHelper
{
	/// <summary>
	/// Processes an EvaluateJavaScriptAsync request by wrapping the script with error handling
	/// and processing the result.
	/// </summary>
	public static async Task<string?> ProcessEvaluateJavaScriptAsync(IHybridWebViewHandler handler, IHybridWebView hybridWebView, EvaluateJavaScriptAsyncRequest request)
	{
		var script = request.Script;

		if (script == null)
		{
			return null;
		}

		// Escape and wrap script with try-catch and error handling
		var escapedScript = WebViewHelper.EscapeJsString(script);
		var wrappedScript =
			$$"""
			(function() {
				try {
					let result = eval('{{escapedScript}}');
					let resultObj = {
						IsError: false,
						Result: JSON.stringify(result)
					};
					return JSON.stringify(resultObj);
				} catch (error) {
					console.error(error);
					let errorObj;
					if (!error) {
						errorObj = {
							IsError: true,
							Message: 'Unknown error',
							StackTrace: Error().stack
						};
					} else if (error instanceof Error) {
						errorObj = {
							IsError: true,
							Name: error.name,
							Message: error.message,
							StackTrace: error.stack
						};
					} else if (typeof error === 'string') {
						errorObj = {
							IsError: true,
							Message: error,
							StackTrace: Error().stack
						};
					} else {
						errorObj = {
							IsError: true,
							Message: JSON.stringify(error),
							StackTrace: Error().stack
						};
					}
					return JSON.stringify(errorObj);
				}
			})()
			""";

		// Use the handler command to evaluate the JS
		var innerRequest = new EvaluateJavaScriptAsyncRequest(wrappedScript);

		// Execute via platform evaluator
		handler.PlatformView.EvaluateJavaScript(innerRequest);

		var result = await innerRequest.Task;

		if (result == null)
			return null;

		var jsResult = JsonSerializer.Deserialize<JSInvokeResult>(result);
		if (jsResult?.IsError == true)
		{
			var jsException = new HybridWebViewInvokeJavaScriptException(jsResult?.Message, jsResult?.Name, jsResult?.StackTrace);
			throw new HybridWebViewInvokeJavaScriptException($"EvaluateJavaScriptAsync threw an exception: {jsException.Message}", jsException);
		}

		var returnValue = jsResult?.Result;

		//if the js function errored or returned null/undefined treat it as null
		if (returnValue == "null" || returnValue == "undefined")
		{
			returnValue = null;
		}
		//JSON.stringify wraps the result - we need to unwrap it properly
		//note that if the js function returns the string "null" we will get here and not above
		else if (returnValue != null)
		{
			// Check if the result is a JSON string (starts and ends with quotes)
			if (returnValue.Length >= 2 && returnValue[0] == '"' && returnValue[^1] == '"')
			{
				// Properly deserialize the JSON string to handle escaped characters
				returnValue = JsonSerializer.Deserialize<string>(returnValue);
			}
			// Otherwise it's a primitive value (number, boolean, etc.) that's already in string form
			// No need to deserialize - just return as-is
		}

		return returnValue;
	}

	/// <summary>
	/// Processes an InvokeJavaScriptAsync request by building the JS call string,
	/// executing it, and processing the result.
	/// </summary>
	public static async Task<object?> ProcessInvokeJavaScriptAsync(IHybridWebViewHandler handler, IHybridWebView hybridWebView, HybridWebViewInvokeJavaScriptRequest request)
	{
		var taskManager = handler.GetRequiredService<IHybridWebViewTaskManager>();

		// Create a callback for async JavaScript methods to invoke when they are done
		var task = taskManager.CreateTask();

		var paramsValuesStringArray = request.ParamValues == null
			? string.Empty
			: string.Join(
				", ",
				request.ParamValues.Select((v, i) => v is null ? "null" : JsonSerializer.Serialize(v, request.ParamJsonTypeInfos![i]!)));

		var js = $"window.HybridWebView.__InvokeJavaScript({task.TaskId}, {request.MethodName}, [{paramsValuesStringArray}])";

		var innerRequest = new EvaluateJavaScriptAsyncRequest(js);

		handler.PlatformView.EvaluateJavaScript(innerRequest);

		// Don't await innerRequest.Task because __InvokeJavaScript is async and returns a Promise,
		// which iOS can't convert to a string. Instead, we wait for the callback message from JavaScript.
		// The JavaScript function will call invokeJavaScriptCallbackInDotNet() when done.

		var stringResult = await task.TaskCompletionSource.Task;

		// if there is no result or if the result was null/undefined, then treat it as null
		if (stringResult is null || stringResult == "null" || stringResult == "undefined")
		{
			return null;
		}
		// if we are not looking for a return object, then return null
		else if (request.ReturnTypeJsonTypeInfo is null)
		{
			return null;
		}
		// if we are expecting a result, then deserialize what we have
		else
		{
			var typedResult = JsonSerializer.Deserialize(stringResult, request.ReturnTypeJsonTypeInfo);
			return typedResult;
		}
	}

	/// <summary>
	/// Invokes a .NET method from JavaScript.
	/// </summary>
	public static async Task<byte[]?> ProcessInvokeDotNetAsync(
		object? invokeTarget,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type? invokeTargetType,
		ILogger? logger,
		Stream? streamBody = null,
		string? stringBody = null)
	{
		try
		{
			if (invokeTarget is null)
			{
				throw new InvalidOperationException($"The InvokeJavaScriptTarget property must have a value in order to invoke a .NET method from JavaScript.");
			}

			if (invokeTargetType is null)
			{
				throw new InvalidOperationException($"The InvokeJavaScriptType property must have a value in order to invoke a .NET method from JavaScript.");
			}

			JSInvokeMethodData? invokeData = null;
			if (streamBody is not null)
			{
				invokeData = await JsonSerializer.DeserializeAsync<JSInvokeMethodData>(streamBody, HybridWebViewHelperJsonContext.Default.JSInvokeMethodData);
			}
			else if (stringBody is not null && !string.IsNullOrWhiteSpace(stringBody))
			{
				invokeData = JsonSerializer.Deserialize<JSInvokeMethodData>(stringBody, HybridWebViewHelperJsonContext.Default.JSInvokeMethodData);
			}

			if (invokeData?.MethodName is null)
			{
				throw new InvalidOperationException("The invoke data did not provide a method name.");
			}

			var invokeResultRaw = await InvokeDotNetMethodAsync(invokeTargetType, invokeTarget, invokeData);
			var invokeResult = CreateInvokeResult(invokeResultRaw);
			var json = JsonSerializer.Serialize(invokeResult, HybridWebViewHelperJsonContext.Default.DotNetInvokeResult);
			var contentBytes = Encoding.UTF8.GetBytes(json);

			return contentBytes;
		}
		catch (Exception ex)
		{
			logger?.LogError(ex, "An error occurred while invoking a .NET method from JavaScript: {ErrorMessage}", ex.Message);

			// Return error response instead of null so JavaScript can handle the error
			var errorResult = CreateErrorResult(ex);
			var errorJson = JsonSerializer.Serialize(errorResult, HybridWebViewHelperJsonContext.Default.DotNetInvokeResult);
			var errorBytes = Encoding.UTF8.GetBytes(errorJson);
			return errorBytes;
		}
	}

	private static DotNetInvokeResult CreateInvokeResult(object? result)
	{
		// null invoke result means an empty result
		if (result is null)
		{
			return new();
		}

		// a reference type or an array should be serialized to JSON
		var resultType = result.GetType();
		if (resultType.IsArray || resultType.IsClass)
		{
			return new DotNetInvokeResult()
			{
				Result = JsonSerializer.Serialize(result),
				IsJson = true,
			};
		}

		// a value type should be returned as is
		return new DotNetInvokeResult()
		{
			Result = result,
		};
	}

	private static DotNetInvokeResult CreateErrorResult(Exception ex)
	{
		return new DotNetInvokeResult()
		{
			IsError = true,
			ErrorMessage = ex.Message,
			ErrorType = ex.GetType().Name,
			ErrorStackTrace = ex.StackTrace
		};
	}

	private static async Task<object?> InvokeDotNetMethodAsync(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType,
		object jsInvokeTarget,
		JSInvokeMethodData invokeData)
	{
		var requestMethodName = invokeData.MethodName!;
		var requestParams = invokeData.ParamValues;

		// get the method and its parameters from the .NET object instance
		var dotnetMethod = targetType.GetMethod(requestMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
		if (dotnetMethod is null)
		{
			throw new InvalidOperationException($"The method {requestMethodName} couldn't be found on the {nameof(jsInvokeTarget)} of type {jsInvokeTarget.GetType().FullName}.");
		}
		var dotnetParams = dotnetMethod.GetParameters();
		if (requestParams is not null && dotnetParams.Length != requestParams.Length)
		{
			throw new InvalidOperationException($"The number of parameters on {nameof(jsInvokeTarget)}'s method {requestMethodName} ({dotnetParams.Length}) doesn't match the number of values passed from JavaScript code ({requestParams.Length}).");
		}

		// deserialize the parameters from JSON to .NET types
		object?[]? invokeParamValues = null;
		if (requestParams is not null)
		{
			invokeParamValues = new object?[requestParams.Length];
			for (var i = 0; i < requestParams.Length; i++)
			{
				var reqValue = requestParams[i];
				var paramType = dotnetParams[i].ParameterType;
				var deserialized = JsonSerializer.Deserialize(reqValue, paramType);
				invokeParamValues[i] = deserialized;
			}
		}

		// invoke the .NET method
		var dotnetReturnValue = GetDotNetMethodReturnValue(jsInvokeTarget, dotnetMethod, invokeParamValues);

		if (dotnetReturnValue is null) // null result
		{
			return null;
		}

		if (dotnetReturnValue is Task task) // Task or Task<T> result
		{
			await task;

			// Task<T>
			if (dotnetMethod.ReturnType.IsGenericType)
			{
				var resultProperty = dotnetMethod.ReturnType.GetProperty(nameof(Task<object>.Result));
				return resultProperty?.GetValue(task);
			}

			// Task
			return null;
		}

		return dotnetReturnValue; // regular result
	}

	private static object? GetDotNetMethodReturnValue(object jsInvokeTarget, MethodInfo dotnetMethod, object?[]? invokeParamValues)
	{
		try
		{
			// invoke the .NET method
			return dotnetMethod.Invoke(jsInvokeTarget, invokeParamValues);
		}
		catch (TargetInvocationException tie) // unwrap while preserving original stack trace
		{
			if (tie.InnerException is not null)
			{
				// Rethrow the underlying exception without losing its original stack trace
				ExceptionDispatchInfo.Capture(tie.InnerException).Throw();

				// unreachable, but required for compiler flow analysis
				throw;
			}

			// no inner exception; rethrow the TargetInvocationException itself preserving its stack
			throw;
		}
	}

	/// <summary>
	/// Processes raw messages from the web view, handling special message types like JavaScript invoke results.
	/// </summary>
	public static void ProcessRawMessage(IHybridWebViewHandler handler, IHybridWebView virtualView, string rawMessage)
	{
		if (string.IsNullOrEmpty(rawMessage))
		{
			throw new ArgumentException($"The raw message cannot be null or empty.", nameof(rawMessage));
		}
#if !NETSTANDARD2_0
		var indexOfPipe = rawMessage.IndexOf('|', StringComparison.Ordinal);
#else
		var indexOfPipe = rawMessage.IndexOf("|", StringComparison.Ordinal);
#endif
		if (indexOfPipe == -1)
		{
			throw new ArgumentException($"The raw message must contain a pipe character ('|').", nameof(rawMessage));
		}

		var messageType = rawMessage.Substring(0, indexOfPipe);
		var messageContent = rawMessage.Substring(indexOfPipe + 1);

		switch (messageType)
		{
			case "__InvokeJavaScriptFailed":
			case "__InvokeJavaScriptCompleted":
				{
#if !NETSTANDARD2_0
					var indexOfPipeInContent = messageContent.IndexOf('|', StringComparison.Ordinal);
#else
					var indexOfPipeInContent = messageContent.IndexOf("|", StringComparison.Ordinal);
#endif
					if (indexOfPipeInContent == -1)
					{
						throw new ArgumentException($"The '{messageType}' message content must contain a pipe character ('|').", nameof(rawMessage));
					}

					var taskId = messageContent.Substring(0, indexOfPipeInContent);
					var result = messageContent.Substring(indexOfPipeInContent + 1);

					var taskManager = handler.GetRequiredService<IHybridWebViewTaskManager>();
					if (messageType == "__InvokeJavaScriptFailed")
					{
						if (IsInvokeJavaScriptThrowsExceptionsEnabled)
						{
							if (string.IsNullOrWhiteSpace(result))
							{
								taskManager.SetTaskFailed(taskId, new HybridWebViewInvokeJavaScriptException());
							}
							else
							{
								var jsError = JsonSerializer.Deserialize(result, HybridWebViewHelperJsonContext.Default.JSInvokeError);
								var jsException = new HybridWebViewInvokeJavaScriptException(jsError?.Message, jsError?.Name, jsError?.StackTrace);
								var ex = new HybridWebViewInvokeJavaScriptException($"InvokeJavaScriptAsync threw an exception: {jsException.Message}", jsException);
								taskManager.SetTaskFailed(taskId, ex);
							}
						}
					}
					else
					{
						taskManager.SetTaskCompleted(taskId, result);
					}
				}
				break;
			case "__RawMessage":
				virtualView?.RawMessageReceived(messageContent);
				break;
			default:
				throw new ArgumentException($"The message type '{messageType}' is not recognized.", nameof(rawMessage));
		}
	}

	private const string InvokeJavaScriptThrowsExceptionsSwitch = "HybridWebView.InvokeJavaScriptThrowsExceptions";

	private static bool IsInvokeJavaScriptThrowsExceptionsEnabled =>
		!AppContext.TryGetSwitch(InvokeJavaScriptThrowsExceptionsSwitch, out var enabled) || enabled;

	// DTOs for JSON serialization
	internal sealed class JSInvokeResult
	{
		public string? Result { get; set; }
		public bool IsError { get; set; }
		public string? Name { get; set; }
		public string? Message { get; set; }
		public string? StackTrace { get; set; }
	}

	internal sealed class JSInvokeMethodData
	{
		public string? MethodName { get; set; }
		public string[]? ParamValues { get; set; }
	}

	internal sealed class JSInvokeError
	{
		public string? Name { get; set; }
		public string? Message { get; set; }
		public string? StackTrace { get; set; }
	}

	internal sealed class DotNetInvokeResult
	{
		public object? Result { get; set; }
		public bool IsJson { get; set; }
		public bool IsError { get; set; }
		public string? ErrorMessage { get; set; }
		public string? ErrorType { get; set; }
		public string? ErrorStackTrace { get; set; }
	}

	[JsonSourceGenerationOptions()]
	[JsonSerializable(typeof(JSInvokeMethodData))]
	[JsonSerializable(typeof(JSInvokeError))]
	[JsonSerializable(typeof(DotNetInvokeResult))]
	internal partial class HybridWebViewHelperJsonContext : JsonSerializerContext
	{
	}
}
#endif
