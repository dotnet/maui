#if __IOS__ || MACCATALYST
using PlatformView = WebKit.WKWebView;
using HeaderPairType = Foundation.NSObject;
#elif MONOANDROID
using PlatformView = Android.Webkit.WebView;
using HeaderPairType = System.String;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.WebView2;
using HeaderPairType = System.String;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
using HeaderPairType = System.String;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
using HeaderPairType = System.String;
#endif

#if __ANDROID__
using Android.Webkit;
#elif __IOS__
using WebKit;
#endif
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Maui.Devices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Runtime.ExceptionServices;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : IHybridWebViewHandler
	{
		internal const string DynamicFeatures = "HybridWebView's legacy SetInvokeJavaScriptTarget API uses reflection and dynamic System.Text.Json serialization features.";

		/// <summary>
		/// The delegate type stored in the method cache. Each entry handles the full pipeline:
		/// deserialize params → invoke method → await if Task → serialize result.
		/// </summary>
		internal delegate Task<string?> DotNetMethodDelegate(object target, string[]? paramJsonValues);

		// Using an IP address means that the web view doesn't wait for any DNS resolution,
		// making it substantially faster. Note that this isn't real HTTP traffic, since
		// we intercept all the requests within this origin.
		private static readonly string AppHostAddress = "0.0.0.1";

		private static readonly string AppHostScheme =
#if IOS || MACCATALYST
			"app";
#else
			"https";
#endif

		/// <summary>
		/// Gets the application's base URI. Defaults to <c>https://0.0.0.1/</c> on Windows and Android,
		/// and <c>app://0.0.0.1/</c> on iOS and MacCatalyst (because <c>https</c> is reserved).
		/// </summary>
		internal static readonly string AppOrigin = $"{AppHostScheme}://{AppHostAddress}/";

		internal static readonly Uri AppOriginUri = new(AppOrigin);

		internal const string InvokeDotNetPath = "__hwvInvokeDotNet";
		internal const string HybridWebViewDotJsPath = "_framework/hybridwebview.js";

		internal const string InvokeDotNetTokenHeaderName = "X-Maui-Invoke-Token";
		internal const string InvokeDotNetTokenHeaderValue = "HybridWebView";
		internal const string InvokeDotNetBodyHeaderName = "X-Maui-Request-Body";


		public static IPropertyMapper<IHybridWebView, IHybridWebViewHandler> Mapper = new PropertyMapper<IHybridWebView, IHybridWebViewHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS || IOS || MACCATALYST
			[nameof(IView.FlowDirection)] = MapFlowDirection,
#endif
		};

		public static CommandMapper<IHybridWebView, IHybridWebViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IHybridWebView.EvaluateJavaScriptAsync)] = MapEvaluateJavaScriptAsync,
			[nameof(IHybridWebView.InvokeJavaScriptAsync)] = MapInvokeJavaScriptAsync,
			[nameof(IHybridWebView.SendRawMessage)] = MapSendRawMessage,
		};

		public HybridWebViewHandler() : base(Mapper, CommandMapper)
		{
		}

		public HybridWebViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IHybridWebView IHybridWebViewHandler.VirtualView => VirtualView;

		PlatformView IHybridWebViewHandler.PlatformView => PlatformView;

		internal HybridWebViewDeveloperTools DeveloperTools => MauiContext?.Services.GetService<HybridWebViewDeveloperTools>() ?? new HybridWebViewDeveloperTools();

		private const string InvokeJavaScriptThrowsExceptionsSwitch = "HybridWebView.InvokeJavaScriptThrowsExceptions";

		private static bool IsInvokeJavaScriptThrowsExceptionsEnabled =>
			!AppContext.TryGetSwitch(InvokeJavaScriptThrowsExceptionsSwitch, out var enabled) || enabled;

		void MessageReceived(string rawMessage)
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

						var taskManager = this.GetRequiredService<IHybridWebViewTaskManager>();
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
									var jsError = JsonSerializer.Deserialize(result, HybridWebViewHandlerJsonContext.Default.JSInvokeError);
									var jsException = new HybridWebViewInvokeJavaScriptException(jsError?.Message, jsError?.Name, jsError?.StackTrace);
									var ex = new HybridWebViewInvokeJavaScriptException($"InvokeJavaScript threw an exception: {jsException.Message}", jsException);
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
					VirtualView?.RawMessageReceived(messageContent);
					break;
				default:
					throw new ArgumentException($"The message type '{messageType}' is not recognized.", nameof(rawMessage));
			}
		}

		[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
		[RequiresDynamicCode(DynamicFeatures)]
#endif
		internal async Task<byte[]?> InvokeDotNetAsync(Stream? streamBody = null, string? stringBody = null)
		{
			try
			{
				JSInvokeMethodData? invokeData = null;
				if (streamBody is not null)
				{
					invokeData = await JsonSerializer.DeserializeAsync<JSInvokeMethodData>(streamBody, HybridWebViewHandlerJsonContext.Default.JSInvokeMethodData);
				}
				else if (stringBody is not null && !string.IsNullOrWhiteSpace(stringBody))
				{
					invokeData = JsonSerializer.Deserialize<JSInvokeMethodData>(stringBody, HybridWebViewHandlerJsonContext.Default.JSInvokeMethodData);
				}

				if (invokeData?.MethodName is null)
				{
					throw new InvalidOperationException("The invoke data did not provide a method name.");
				}

				DotNetInvokeResult invokeResult;
				if (VirtualView.InvokeJavaScriptMethodCache is { } methodCache)
				{
					// AOT-safe path: use pre-built delegates from method cache
					invokeResult = await InvokeCachedMethodAsync(
						VirtualView.InvokeJavaScriptTarget!,
						methodCache,
						invokeData);
				}
				else
				{
					// Legacy path: full reflection
					invokeResult = await InvokeLegacyAsync(invokeData);
				}

				var json = JsonSerializer.Serialize(invokeResult, HybridWebViewHandlerJsonContext.Default.DotNetInvokeResult);
				return Encoding.UTF8.GetBytes(json);
			}
			catch (Exception ex)
			{
				MauiContext?.CreateLogger<HybridWebViewHandler>()?.LogError(ex, "An error occurred while invoking a .NET method from JavaScript: {ErrorMessage}", ex.Message);

				var errorResult = new DotNetInvokeResult
				{
					IsError = true,
					ErrorMessage = ex.Message,
					ErrorType = ex.GetType().Name,
					ErrorStackTrace = ex.StackTrace
				};
				var errorJson = JsonSerializer.Serialize(errorResult, HybridWebViewHandlerJsonContext.Default.DotNetInvokeResult);
				return Encoding.UTF8.GetBytes(errorJson);
			}
		}

		/// <summary>
		/// AOT-safe invocation path: uses pre-built typed delegates from the method cache.
		/// No MakeGenericMethod, no GetProperty, no reflection-based JSON.
		/// </summary>
		private static async Task<DotNetInvokeResult> InvokeCachedMethodAsync(
			object target,
			IReadOnlyDictionary<string, object> methodCache,
			JSInvokeMethodData invokeData)
		{
			var methodName = invokeData.MethodName!;
			if (!methodCache.TryGetValue(methodName, out var entry) || entry is not DotNetMethodDelegate handler)
			{
				throw new InvalidOperationException($"The method '{methodName}' was not found on the invoke target. Available methods: {string.Join(", ", methodCache.Keys)}.");
			}

			var jsonResult = await handler(target, invokeData.ParamValues);
			if (jsonResult is null)
			{
				return new DotNetInvokeResult();
			}

			return new DotNetInvokeResult
			{
				Result = jsonResult,
				IsJson = true,
			};
		}

		/// <summary>
		/// Legacy reflection-based invocation path for backward compatibility.
		/// </summary>
		[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
		[RequiresDynamicCode(DynamicFeatures)]
#endif
		private async Task<DotNetInvokeResult> InvokeLegacyAsync(JSInvokeMethodData invokeData)
		{
			var invokeTarget = VirtualView.InvokeJavaScriptTarget ?? throw new InvalidOperationException($"The {nameof(IHybridWebView)}.{nameof(IHybridWebView.InvokeJavaScriptTarget)} property must have a value in order to invoke a .NET method from JavaScript.");
			var invokeTargetType = VirtualView.InvokeJavaScriptType ?? throw new InvalidOperationException($"The {nameof(IHybridWebView)}.{nameof(IHybridWebView.InvokeJavaScriptType)} property must have a value in order to invoke a .NET method from JavaScript.");

			var result = await InvokeDotNetMethodLegacyAsync(invokeTargetType, invokeTarget, invokeData);
			return CreateInvokeResultLegacy(result);
		}

		[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
		[RequiresDynamicCode(DynamicFeatures)]
#endif
		private static DotNetInvokeResult CreateInvokeResultLegacy(object? result)
		{
			if (result is null)
			{
				return new();
			}

			var resultType = result.GetType();
			if (resultType.IsArray || resultType.IsClass)
			{
				return new DotNetInvokeResult()
				{
					Result = JsonSerializer.Serialize(result),
					IsJson = true,
				};
			}

			return new DotNetInvokeResult()
			{
				Result = result,
			};
		}

		[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
		[RequiresDynamicCode(DynamicFeatures)]
#endif
		private static async Task<object?> InvokeDotNetMethodLegacyAsync(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType,
			object jsInvokeTarget,
			JSInvokeMethodData invokeData)
		{
			var requestMethodName = invokeData.MethodName!;
			var requestParams = invokeData.ParamValues;

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

			object?[]? invokeParamValues = null;
			if (requestParams is not null)
			{
				invokeParamValues = new object?[requestParams.Length];
				for (var i = 0; i < requestParams.Length; i++)
				{
					invokeParamValues[i] = JsonSerializer.Deserialize(requestParams[i], dotnetParams[i].ParameterType);
				}
			}

			var dotnetReturnValue = InvokeMethod(jsInvokeTarget, dotnetMethod, invokeParamValues);

			if (dotnetReturnValue is null)
			{
				return null;
			}

			if (dotnetReturnValue is Task task)
			{
				await task;

				if (dotnetMethod.ReturnType.IsGenericType)
				{
					var resultProperty = dotnetMethod.ReturnType.GetProperty(nameof(Task<object>.Result));
					return resultProperty?.GetValue(task);
				}

				return null;
			}

			return dotnetReturnValue;
		}

		private static object? InvokeMethod(object jsInvokeTarget, MethodInfo dotnetMethod, object?[]? invokeParamValues)
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
		/// Builds a method cache at registration time using reflection.
		/// This is the runtime fallback — the source generator will replace this with
		/// fully typed delegates that don't need any suppressions.
		/// </summary>
		internal static IReadOnlyDictionary<string, object> BuildMethodCache(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType,
			JsonSerializerContext jsonSerializerContext)
		{
			var cache = new Dictionary<string, object>(StringComparer.Ordinal);

			var methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			foreach (var method in methods)
			{
				if (method.IsSpecialName || method.IsGenericMethodDefinition)
				{
					continue;
				}

				var methodName = method.Name;
				if (cache.ContainsKey(methodName))
				{
					throw new InvalidOperationException($"The type '{targetType.FullName}' has multiple methods named '{methodName}'. Method overloads are not supported for JavaScript invocation.");
				}

				var parameters = method.GetParameters();

				// Pre-resolve parameter JsonTypeInfos
				var paramJsonTypeInfos = new JsonTypeInfo[parameters.Length];
				for (var i = 0; i < parameters.Length; i++)
				{
					var paramType = parameters[i].ParameterType;
					if (paramType.IsByRef || paramType.IsPointer)
					{
						throw new InvalidOperationException($"The method '{methodName}' on type '{targetType.FullName}' has a ref/pointer parameter '{parameters[i].Name}' which is not supported for JavaScript invocation.");
					}

					paramJsonTypeInfos[i] = jsonSerializerContext.GetTypeInfo(paramType)
						?? throw new InvalidOperationException($"The JSON serializer context does not contain metadata for parameter type '{paramType.FullName}' (parameter '{parameters[i].Name}' of method '{methodName}'). Add [JsonSerializable(typeof({paramType.Name}))] to your JsonSerializerContext.");
				}

				// Pre-resolve return type handling
				var returnType = method.ReturnType;
				JsonTypeInfo? returnJsonTypeInfo = null;
				PropertyInfo? taskResultProperty = null;
				bool isTask = false;
				bool isTaskOfT = false;

				if (returnType == typeof(void))
				{
					// no-op
				}
				else if (returnType == typeof(Task))
				{
					isTask = true;
				}
				else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
				{
					isTask = true;
					isTaskOfT = true;
					var resultType = returnType.GetGenericArguments()[0];
					returnJsonTypeInfo = jsonSerializerContext.GetTypeInfo(resultType)
						?? throw new InvalidOperationException($"The JSON serializer context does not contain metadata for return type '{resultType.FullName}' (Task<{resultType.Name}> of method '{methodName}'). Add [JsonSerializable(typeof({resultType.Name}))] to your JsonSerializerContext.");
					taskResultProperty = GetTaskResultProperty(returnType);
				}
				else
				{
					returnJsonTypeInfo = jsonSerializerContext.GetTypeInfo(returnType)
						?? throw new InvalidOperationException($"The JSON serializer context does not contain metadata for return type '{returnType.FullName}' of method '{methodName}'. Add [JsonSerializable(typeof({returnType.Name}))] to your JsonSerializerContext.");
				}

				// Capture everything in a closure — this delegate handles the full pipeline
				var capturedMethod = method;
				var capturedParamInfos = paramJsonTypeInfos;
				var capturedReturnInfo = returnJsonTypeInfo;
				var capturedTaskResultProp = taskResultProperty;
				var capturedIsTask = isTask;
				var capturedIsTaskOfT = isTaskOfT;

				cache[methodName] = new DotNetMethodDelegate(async (object target, string[]? paramJsonValues) =>
				{
					// Deserialize parameters
					object?[]? args = null;
					if (capturedParamInfos.Length > 0)
					{
						if (paramJsonValues is null || paramJsonValues.Length != capturedParamInfos.Length)
						{
							throw new InvalidOperationException($"The method '{methodName}' expects {capturedParamInfos.Length} parameter(s) but {paramJsonValues?.Length ?? 0} were provided from JavaScript.");
						}

						args = new object?[capturedParamInfos.Length];
						for (var i = 0; i < capturedParamInfos.Length; i++)
						{
							args[i] = JsonSerializer.Deserialize(paramJsonValues[i], capturedParamInfos[i]);
						}
					}

					// Invoke
					var returnValue = InvokeMethod(target, capturedMethod, args);

					// Handle result
					if (capturedIsTask && returnValue is Task task)
					{
						await task;

						if (capturedIsTaskOfT)
						{
							var result = capturedTaskResultProp!.GetValue(task);
							return result is null ? null : JsonSerializer.Serialize(result, capturedReturnInfo!);
						}

						return null; // Task (void)
					}

					if (returnValue is null)
					{
						return null;
					}

					return JsonSerializer.Serialize(returnValue, capturedReturnInfo!);
				});
			}

			return cache;
		}

		[UnconditionalSuppressMessage("Trimming", "IL2070",
			Justification = "Task<T> is a BCL framework type whose public properties (including Result) are always preserved and never trimmed. This will be replaced by the source generator.")]
		private static PropertyInfo GetTaskResultProperty(Type taskOfTType) =>
			taskOfTType.GetProperty(nameof(Task<object>.Result))!;

		private sealed class JSInvokeMethodData
		{
			public string? MethodName { get; set; }
			public string[]? ParamValues { get; set; }
		}

		private sealed class JSInvokeError
		{
			public string? Name { get; set; }
			public string? Message { get; set; }
			public string? StackTrace { get; set; }
		}

		private sealed class DotNetInvokeResult
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
		private partial class HybridWebViewHandlerJsonContext : JsonSerializerContext
		{
		}



#if PLATFORM && !TIZEN
		public static async void MapEvaluateJavaScriptAsync(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg)
		{
			if (arg is not EvaluateJavaScriptAsyncRequest request ||
				handler.PlatformView is not MauiHybridWebView hybridPlatformWebView)
			{
				return;
			}

			if (handler.PlatformView is null)
			{
				request.SetCanceled();
				return;
			}

			var script = request.Script;
			// Make all the platforms mimic Android's implementation, which is by far the most complete.
			if (!OperatingSystem.IsAndroid())
			{
				script = WebViewHelper.EscapeJsString(script);

				if (!OperatingSystem.IsWindows())
				{
					// Use JSON.stringify() method to converts a JavaScript value to a JSON string
					script = "try{JSON.stringify(eval('" + script + "'))}catch(e){'null'};";
				}
				else
				{
					script = "try{eval('" + script + "')}catch(e){'null'};";
				}
			}

			// Use the handler command to evaluate the JS
			var innerRequest = new EvaluateJavaScriptAsyncRequest(script);
			EvaluateJavaScript(handler, hybridWebView, innerRequest);

			var result = await innerRequest.Task;

			//if the js function errored or returned null/undefined treat it as null
			if (result == "null")
			{
				result = null;
			}
			//JSON.stringify wraps the result in literal quotes, we just want the actual returned result
			//note that if the js function returns the string "null" we will get here and not above
			else if (result != null)
			{
				result = result.Trim('"');
			}

			request.SetResult(result!);

		}
#endif

		public static async void MapInvokeJavaScriptAsync(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg)
		{
#if PLATFORM && !TIZEN
			if (arg is not HybridWebViewInvokeJavaScriptRequest invokeJavaScriptRequest)
			{
				return;
			}

			try
			{
				var result = await MapInvokeJavaScriptAsyncImpl(handler, hybridWebView, invokeJavaScriptRequest);

				invokeJavaScriptRequest.SetResult(result);
			}
			catch (Exception ex)
			{
				invokeJavaScriptRequest.SetException(ex);
			}
#else
			await Task.CompletedTask;
#endif
		}

		static async Task<object?> MapInvokeJavaScriptAsyncImpl(IHybridWebViewHandler handler, IHybridWebView hybridWebView, HybridWebViewInvokeJavaScriptRequest invokeJavaScriptRequest)
		{
			// Create a callback for async JavaScript methods to invoke when they are done
			var taskManager = handler.GetRequiredService<IHybridWebViewTaskManager>();
			var (currentInvokeTaskId, callback) = taskManager.CreateTask();

			var paramsValuesStringArray =
				invokeJavaScriptRequest.ParamValues == null
				? string.Empty
				: string.Join(
					", ",
					invokeJavaScriptRequest.ParamValues.Select((v, i) => (v == null ? "null" : JsonSerializer.Serialize(v, invokeJavaScriptRequest.ParamJsonTypeInfos![i]!))));

			await handler.InvokeAsync(nameof(IHybridWebView.EvaluateJavaScriptAsync),
				new EvaluateJavaScriptAsyncRequest($"window.HybridWebView.__InvokeJavaScript({currentInvokeTaskId}, {invokeJavaScriptRequest.MethodName}, [{paramsValuesStringArray}])"));

			var stringResult = await callback.Task;

			// if there is no result or if the result was null/undefined, then treat it as null
			if (stringResult is null || stringResult == "null" || stringResult == "undefined")
			{
				return null;
			}
			// if we are not looking for a return object, then return null
			else if (invokeJavaScriptRequest.ReturnTypeJsonTypeInfo is null)
			{
				return null;
			}
			// if we are expecting a result, then deserialize what we have
			else
			{
				var typedResult = JsonSerializer.Deserialize(stringResult, invokeJavaScriptRequest.ReturnTypeJsonTypeInfo);
				return typedResult;
			}
		}

		internal static async Task<string?> GetAssetContentAsync(string assetPath)
		{
			using var stream = await GetAssetStreamAsync(assetPath);
			if (stream == null)
			{
				return null;
			}
			using var reader = new StreamReader(stream);

			var contents = reader.ReadToEnd();

			return contents;
		}

		internal static async Task<Stream?> GetAssetStreamAsync(string assetPath)
		{
			if (!await FileSystem.AppPackageFileExistsAsync(assetPath))
			{
				return null;
			}
			return await FileSystem.OpenAppPackageFileAsync(assetPath);
		}

		internal static Stream? GetEmbeddedStream(string embeddedPath)
		{
			var assembly = typeof(HybridWebViewHandler).Assembly;

			var resourceName = assembly
				.GetManifestResourceNames()
				.FirstOrDefault(name => name.Equals(embeddedPath.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));

			if (resourceName is null)
			{
				return null;
			}

			return assembly.GetManifestResourceStream(resourceName);
		}

		private static bool IsExpectedHeader((string? Name, string? Value) header, (string Name, string Value) expected) =>
			string.Equals(header.Name, expected.Name, StringComparison.OrdinalIgnoreCase) &&
			string.Equals(header.Value, expected.Value, StringComparison.OrdinalIgnoreCase);

		internal static bool HasExpectedHeaders(IEnumerable<KeyValuePair<HeaderPairType, HeaderPairType>>? headers)
		{
			if (headers is null)
				return false;

			var expectedOrigin = AppOrigin.TrimEnd('/');

			var hasExpectedToken = false;
			var hasExpectedOrigin = false;
			var hasExpectedReferer = false;

			foreach (var header in headers)
			{
#if IOS || MACCATALYST
				var name = header.Key?.ToString();
				var value = header.Value?.ToString();
#else
				var name = header.Key;
				var value = header.Value;
#endif

				// Is this from the script
				hasExpectedToken = hasExpectedToken || IsExpectedHeader((name, value), (InvokeDotNetTokenHeaderName, InvokeDotNetTokenHeaderValue));

				// Is this from a local script
				var urlValue = value?.TrimEnd('/');
				hasExpectedOrigin = hasExpectedOrigin || IsExpectedHeader((name, urlValue), ("Origin", expectedOrigin));
				hasExpectedReferer = hasExpectedReferer || IsExpectedHeader((name, urlValue), ("Referer", expectedOrigin));

				if (hasExpectedToken && (hasExpectedOrigin || hasExpectedReferer))
					return true;
			}

			return false;
		}

#if !NETSTANDARD
		internal static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
#endif
	}
}
