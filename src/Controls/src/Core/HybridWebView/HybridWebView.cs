using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using System.Collections.Generic;

#if WINDOWS || ANDROID || IOS || MACCATALYST
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
#endif

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="View"/> that presents local HTML content in a web view and allows JavaScript and C# code to
	/// communicate by using messages and by invoking methods.
	/// </summary>
	public class HybridWebView : View, IHybridWebView
	{
		/// <summary>Bindable property for <see cref="DefaultFile"/>.</summary>
		public static readonly BindableProperty DefaultFileProperty =
			BindableProperty.Create(nameof(DefaultFile), typeof(string), typeof(HybridWebView), defaultValue: "index.html");
		/// <summary>Bindable property for <see cref="HybridRoot"/>.</summary>
		public static readonly BindableProperty HybridRootProperty =
			BindableProperty.Create(nameof(HybridRoot), typeof(string), typeof(HybridWebView), defaultValue: "wwwroot");


		/// <inheritdoc/>
		public string? DefaultFile
		{
			get { return (string)GetValue(DefaultFileProperty); }
			set { SetValue(DefaultFileProperty, value); }
		}

		/// <inheritdoc/>
		public string? HybridRoot
		{
			get { return (string)GetValue(HybridRootProperty); }
			set { SetValue(HybridRootProperty, value); }
		}

		void IHybridWebView.MessageReceived(string rawMessage)
		{
			if (string.IsNullOrEmpty(rawMessage))
			{
				throw new ArgumentException($"The raw message cannot be null or empty.", nameof(rawMessage));
			}
			var indexOfPipe = rawMessage.IndexOf("|", StringComparison.Ordinal);
			if (indexOfPipe == -1)
			{
				throw new ArgumentException($"The raw message must contain a pipe character ('|').", nameof(rawMessage));
			}

			var messageType = rawMessage.Substring(0, indexOfPipe);
			var messageContent = rawMessage.Substring(indexOfPipe + 1);

			switch (messageType)
			{
				case "InvokeMethodCompleted":
					{
						var sections = messageContent.Split('|');
						var taskId = sections[0];
						var result = sections[1];
						AsyncTaskCompleted(taskId, result);
					}
					break;
				case "RawMessage":
					RawMessageReceived?.Invoke(this, new HybridWebViewRawMessageReceivedEventArgs(messageContent));
					break;
				default:
					throw new ArgumentException($"The message type '{messageType}' is not recognized.", nameof(rawMessage));
			}
		}

		/// <summary>
		/// Raised when a raw message is received from the web view. Raw messages are strings that have no additional processing.
		/// </summary>
		public event EventHandler<HybridWebViewRawMessageReceivedEventArgs>? RawMessageReceived;

		/// <summary>
		/// Sends a raw message to the code running in the web view. Raw messages have no additional processing.
		/// </summary>
		/// <param name="rawMessage"></param>
		public void SendRawMessage(string rawMessage)
		{
			Handler?.Invoke(
				nameof(IHybridWebView.SendRawMessage),
				new HybridWebViewRawMessage
				{
					Message = rawMessage,
				});
		}

		private int _invokeTaskId;
		private Dictionary<string, TaskCompletionSource<string>> asyncTaskCallbacks = new Dictionary<string, TaskCompletionSource<string>>();

		/// <summary>
		/// Handler for when the an Async JavaScript task has completed and needs to notify .NET.
		/// </summary>
		private void AsyncTaskCompleted(string taskId, string result)
		{
			//Look for the callback in the list of pending callbacks.
			if (!string.IsNullOrEmpty(taskId) && asyncTaskCallbacks.ContainsKey(taskId))
			{
				//Get the callback and remove it from the list.
				var callback = asyncTaskCallbacks[taskId];
				callback.SetResult(result);

				//Remove the callback.
				asyncTaskCallbacks.Remove(taskId);
			}
		}

		/// <summary>
		/// Invokes a JavaScript method named <paramref name="methodName"/> and optionally passes in the parameter values
		/// specified by <paramref name="paramValues"/>.
		/// </summary>
		/// <param name="methodName">The name of the JavaScript method to invoke.</param>
		/// <param name="paramValues">Optional array of objects to be passed to the JavaScript method.</param>
		/// <param name="paramJsonTypeInfos">Optional array of metadata about serializing the types of the parameters specified by <paramref name="paramValues"/>.</param>
		/// <returns>A string containing the return value of the called method.</returns>
#if WINDOWS || ANDROID || IOS || MACCATALYST
		public async Task<string?> InvokeJavaScriptAsync(string methodName, object?[]? paramValues, JsonTypeInfo?[]? paramJsonTypeInfos = null)
		{
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentException($"The method name cannot be null or empty.", nameof(methodName));
			}
			if (paramValues != null && paramJsonTypeInfos == null)
			{
				throw new ArgumentException($"The parameter values were provided, but the parameter JSON type infos were not.", nameof(paramJsonTypeInfos));
			}
			if (paramValues == null && paramJsonTypeInfos != null)
			{
				throw new ArgumentException($"The parameter JSON type infos were provided, but the parameter values were not.", nameof(paramValues));
			}
			if (paramValues != null && paramValues.Length != paramJsonTypeInfos!.Length)
			{
				throw new ArgumentException($"The number of parameter values does not match the number of parameter JSON type infos.", nameof(paramValues));
			}

			// Create a callback for async JavaScript methods to invoke when they are done
			var callback = new TaskCompletionSource<string>();
			var currentInvokeTaskId = $"{_invokeTaskId++}";
			asyncTaskCallbacks.Add(currentInvokeTaskId, callback);

			var paramsValuesStringArray =
				paramValues == null
				? string.Empty
				: string.Join(
					", ",
					paramValues.Select((v, i) => (v == null ? "null" : JsonSerializer.Serialize(v, paramJsonTypeInfos![i]!))));

			await EvaluateJavaScriptAsync($"window.HybridWebView.InvokeMethod({currentInvokeTaskId}, {methodName}, [{paramsValuesStringArray}])");

			return await callback.Task;
		}
#else
		public Task<string?> InvokeJavaScriptAsync(string methodName, object?[]? paramValues = null, object?[]? paramJsonTypeInfos = null)
		{
			_invokeTaskId++; // This is to avoid the compiler warning about the field not being used
			throw new NotImplementedException();
		}
#endif

		/// <summary>
		/// Invokes a JavaScript method named <paramref name="methodName"/> and optionally passes in the parameter values specified
		/// by <paramref name="paramValues"/> by JSON-encoding each one.
		/// </summary>
		/// <typeparam name="TReturnType">The type of the return value to deserialize from JSON.</typeparam>
		/// <param name="methodName">The name of the JavaScript method to invoke.</param>
		/// <param name="returnTypeJsonTypeInfo">Metadata about deserializing the type of the return value specified by <typeparamref name="TReturnType"/>.</param>
		/// <param name="paramValues">Optional array of objects to be passed to the JavaScript method by JSON-encoding each one.</param>
		/// <param name="paramJsonTypeInfos">Optional array of metadata about serializing the types of the parameters specified by <paramref name="paramValues"/>.</param>
		/// <returns>An object of type <typeparamref name="TReturnType"/> containing the return value of the called method.</returns>
#if WINDOWS || ANDROID || IOS || MACCATALYST
		public async Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(string methodName, JsonTypeInfo<TReturnType?> returnTypeJsonTypeInfo, object?[]? paramValues = null, JsonTypeInfo?[]? paramJsonTypeInfos = null)
		{
			var stringResult = await InvokeJavaScriptAsync(methodName, paramValues, paramJsonTypeInfos);

			if (stringResult is null)
			{
				return default;
			}
			return JsonSerializer.Deserialize<TReturnType?>(stringResult, returnTypeJsonTypeInfo);
	}
#else
		public Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(string methodName, object returnTypeJsonTypeInfo, object?[]? paramValues, object?[]? paramJsonTypeInfos)
		{
			throw new NotImplementedException();
		}
#endif

		/// <inheritdoc/>
		public async Task<string?> EvaluateJavaScriptAsync(string script)
		{
			if (script == null)
			{
				return null;
			}

			// Make all the platforms mimic Android's implementation, which is by far the most complete.
			if (DeviceInfo.Platform != DevicePlatform.Android)
			{
				script = WebView.EscapeJsString(script);

				if (DeviceInfo.Platform != DevicePlatform.WinUI)
				{
					// Use JSON.stringify() method to converts a JavaScript value to a JSON string
					script = "try{JSON.stringify(eval('" + script + "'))}catch(e){'null'};";
				}
				else
				{
					script = "try{eval('" + script + "')}catch(e){'null'};";
				}
			}

			string? result;

			// Use the handler command to evaluate the JS
			result = await Handler!.InvokeAsync(nameof(IHybridWebView.EvaluateJavaScriptAsync),
				new EvaluateJavaScriptAsyncRequest(script));

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

			return result;
		}
	}
}
