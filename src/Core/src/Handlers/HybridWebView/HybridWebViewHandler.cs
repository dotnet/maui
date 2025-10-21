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
	[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
	[RequiresDynamicCode(DynamicFeatures)]
#endif
	public partial class HybridWebViewHandler : IHybridWebViewHandler
	{
		internal const string DynamicFeatures = "HybridWebView uses dynamic System.Text.Json serialization features.";
		internal const string NotSupportedMessage = DynamicFeatures + " Enable the $(MauiHybridWebViewSupported) property in your .csproj file to use in a trimming unsafe manner.";

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
#if WINDOWS
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

		internal async Task<byte[]?> InvokeDotNetAsync(Stream? streamBody = null, string? stringBody = null)
		{
			try
			{
				var invokeTarget = VirtualView.InvokeJavaScriptTarget ?? throw new InvalidOperationException($"The {nameof(IHybridWebView)}.{nameof(IHybridWebView.InvokeJavaScriptTarget)} property must have a value in order to invoke a .NET method from JavaScript.");
				var invokeTargetType = VirtualView.InvokeJavaScriptType ?? throw new InvalidOperationException($"The {nameof(IHybridWebView)}.{nameof(IHybridWebView.InvokeJavaScriptType)} property must have a value in order to invoke a .NET method from JavaScript.");

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

				var invokeResultRaw = await InvokeDotNetMethodAsync(invokeTargetType, invokeTarget, invokeData);
				var invokeResult = CreateInvokeResult(invokeResultRaw);
				var json = JsonSerializer.Serialize(invokeResult);
				var contentBytes = Encoding.UTF8.GetBytes(json);

				return contentBytes;
			}
			catch (Exception ex)
			{
				MauiContext?.CreateLogger<HybridWebViewHandler>()?.LogError(ex, "An error occurred while invoking a .NET method from JavaScript: {ErrorMessage}", ex.Message);

				// Return error response instead of null so JavaScript can handle the error
				var errorResult = CreateErrorResult(ex);
				var errorJson = JsonSerializer.Serialize(errorResult, HybridWebViewHandlerJsonContext.Default.DotNetInvokeResult);
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
