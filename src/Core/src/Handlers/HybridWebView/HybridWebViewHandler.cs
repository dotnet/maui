#if __IOS__ || MACCATALYST
using PlatformView = WebKit.WKWebView;
#elif MONOANDROID
using PlatformView = Android.Webkit.WebView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.WebView2;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
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
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Maui.Devices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : IHybridWebViewHandler, IHybridWebViewTaskManager
	{
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

		public static IPropertyMapper<IHybridWebView, IHybridWebViewHandler> Mapper = new PropertyMapper<IHybridWebView, IHybridWebViewHandler>(ViewHandler.ViewMapper)
		{
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


		/// <summary>
		/// Handler for when the an Async JavaScript task has completed and needs to notify .NET.
		/// </summary>
		private void AsyncTaskCompleted(string taskId, string result)
		{
			// Look for the callback in the list of pending callbacks
			if (!string.IsNullOrEmpty(taskId) && _asyncTaskCallbacks.TryGetValue(taskId, out var callback))
			{
				// Get the callback and remove it from the list
				callback.SetResult(result);

				// Remove the callback
				_asyncTaskCallbacks.TryRemove(taskId, out var _);
			}
		}

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
						AsyncTaskCompleted(taskId, result);
					}
					break;
				case "__RawMessage":
					VirtualView?.RawMessageReceived(messageContent);
					break;
				default:
					throw new ArgumentException($"The message type '{messageType}' is not recognized.", nameof(rawMessage));
			}
		}

		[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
		[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
		internal (byte[]? ContentBytes, string? ContentType) InvokeDotNet(NameValueCollection invokeQueryString)
		{
			try
			{
				var invokeTarget = VirtualView.InvokeJavaScriptTarget ?? throw new NotImplementedException($"The {nameof(IHybridWebView)}.{nameof(IHybridWebView.InvokeJavaScriptTarget)} property must have a value in order to invoke a .NET method from JavaScript.");
				var invokeDataString = invokeQueryString["data"];
				if (string.IsNullOrEmpty(invokeDataString))
				{
					throw new ArgumentException("The 'data' query string parameter is required.", nameof(invokeQueryString));
				}

				byte[]? contentBytes = null;
				string? contentType = null;

				var invokeData = JsonSerializer.Deserialize<JSInvokeMethodData>(invokeDataString, HybridWebViewHandlerJsonContext.Default.JSInvokeMethodData);

				if (invokeData != null && invokeData.MethodName != null)
				{
					var result = InvokeDotNetMethod(invokeTarget, invokeData);

					contentType = "application/json";

					DotNetInvokeResult dotNetInvokeResult;

					if (result is not null)
					{
						var resultType = result.GetType();
						if (resultType.IsArray || resultType.IsClass)
						{
							dotNetInvokeResult = new DotNetInvokeResult()
							{
								Result = JsonSerializer.Serialize(result),
								IsJson = true,
							};
						}
						else
						{
							dotNetInvokeResult = new DotNetInvokeResult()
							{
								Result = result,
							};
						}
					}
					else
					{
						dotNetInvokeResult = new();
					}

					contentBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dotNetInvokeResult));
				}

				return (contentBytes, contentType);
			}
			catch (Exception)
			{
				// TODO: Log this

				return (null, null);
			}
		}

		[UnconditionalSuppressMessage("Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "<Pending>")]
		[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
		[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
		private static object? InvokeDotNetMethod(object jsInvokeTarget, JSInvokeMethodData invokeData)
		{
			var invokeMethod = jsInvokeTarget.GetType().GetMethod(invokeData.MethodName!, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod);
			if (invokeMethod == null)
			{
				throw new InvalidOperationException($"The method {invokeData.MethodName} couldn't be found on the {nameof(jsInvokeTarget)} of type {jsInvokeTarget.GetType().FullName}.");
			}

			if (invokeData.ParamValues != null && invokeMethod.GetParameters().Length != invokeData.ParamValues.Length)
			{
				throw new InvalidOperationException($"The number of parameters on {nameof(jsInvokeTarget)}'s method {invokeData.MethodName} ({invokeMethod.GetParameters().Length}) doesn't match the number of values passed from JavaScript code ({invokeData.ParamValues.Length}).");
			}

			var paramObjectValues =
				invokeData.ParamValues?
					.Zip(invokeMethod.GetParameters(), (s, p) => s == null ? null : JsonSerializer.Deserialize(s, p.ParameterType))
					.ToArray();

			return invokeMethod.Invoke(jsInvokeTarget, paramObjectValues);
		}


		private sealed class JSInvokeMethodData
		{
			public string? MethodName { get; set; }
			public string[]? ParamValues { get; set; }
		}

		private sealed class DotNetInvokeResult
		{
			public object? Result { get; set; }
			public bool IsJson { get; set; }
		}

		[JsonSourceGenerationOptions()]
		[JsonSerializable(typeof(JSInvokeMethodData))]
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
				script = EscapeJsString(script);

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
			if (arg is not HybridWebViewInvokeJavaScriptRequest invokeJavaScriptRequest ||
				handler.PlatformView is not MauiHybridWebView hybridPlatformWebView ||
				handler is not IHybridWebViewTaskManager taskManager)
			{
				return;
			}

			// Create a callback for async JavaScript methods to invoke when they are done
			var callback = new TaskCompletionSource<string>();
			var currentInvokeTaskId = $"{taskManager.GetNextInvokeTaskId()}";
			taskManager.AsyncTaskCallbacks.TryAdd(currentInvokeTaskId, callback);

			var paramsValuesStringArray =
				invokeJavaScriptRequest.ParamValues == null
				? string.Empty
				: string.Join(
					", ",
					invokeJavaScriptRequest.ParamValues.Select((v, i) => (v == null ? "null" : JsonSerializer.Serialize(v, invokeJavaScriptRequest.ParamJsonTypeInfos![i]!))));

			await handler.InvokeAsync(nameof(IHybridWebView.EvaluateJavaScriptAsync),
				new EvaluateJavaScriptAsyncRequest($"window.HybridWebView.__InvokeJavaScript({currentInvokeTaskId}, {invokeJavaScriptRequest.MethodName}, [{paramsValuesStringArray}])"));

			var stringResult = await callback.Task;

			if (stringResult is null)
			{
				invokeJavaScriptRequest.SetResult(null);
			}
			else
			{
				var typedResult = JsonSerializer.Deserialize(stringResult, invokeJavaScriptRequest.ReturnTypeJsonTypeInfo);
				invokeJavaScriptRequest.SetResult(typedResult);
			}
#else
			await Task.CompletedTask;
#endif
		}

#if PLATFORM && !TIZEN
		// Copied from WebView.cs
		internal static string? EscapeJsString(string js)
		{
			if (js == null)
				return null;

			if (!js.Contains('\'', StringComparison.Ordinal))
				return js;

			//get every quote in the string along with all the backslashes preceding it
			var singleQuotes = Regex.Matches(js, @"(\\*?)'");

			var uniqueMatches = new List<string>();

			for (var i = 0; i < singleQuotes.Count; i++)
			{
				var matchedString = singleQuotes[i].Value;
				if (!uniqueMatches.Contains(matchedString))
				{
					uniqueMatches.Add(matchedString);
				}
			}

			uniqueMatches.Sort((x, y) => y.Length.CompareTo(x.Length));

			//escape all quotes from the script as well as add additional escaping to all quotes that were already escaped
			for (var i = 0; i < uniqueMatches.Count; i++)
			{
				var match = uniqueMatches[i];
				var numberOfBackslashes = match.Length - 1;
				var slashesToAdd = (numberOfBackslashes * 2) + 1;
				var replacementStr = "'".PadLeft(slashesToAdd + 1, '\\');
				js = Regex.Replace(js, @"(?<=[^\\])" + Regex.Escape(match), replacementStr);
			}

			return js;
		}
#endif

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

#if !NETSTANDARD
		internal static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
#endif

		// IHybridWebViewTaskManager implementation
		ConcurrentDictionary<string, TaskCompletionSource<string>> _asyncTaskCallbacks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
		int _asyncInvokeTaskId;

		int IHybridWebViewTaskManager.GetNextInvokeTaskId()
		{
			return Interlocked.Increment(ref _asyncInvokeTaskId);
		}
		ConcurrentDictionary<string, TaskCompletionSource<string>> IHybridWebViewTaskManager.AsyncTaskCallbacks => _asyncTaskCallbacks;
	}
}
