using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

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

		void IHybridWebView.RawMessageReceived(string rawMessage)
		{
			RawMessageReceived?.Invoke(this, new HybridWebViewRawMessageReceivedEventArgs(rawMessage));
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

		private const string TrimmerJustification = "The caller of this method must ensure necessary types and members are preserved.";

		/// <summary>
		/// Invokes a JavaScript method named <paramref name="methodName"/> and optionally passes in the parameter values
		/// specified by <paramref name="paramValues"/>.
		/// </summary>
		/// <param name="methodName">The name of the JavaScript method to invoke.</param>
		/// <param name="paramValues">Optional array of objects to be passed to the JavaScript method.</param>
		/// <returns>A string containing the return value of the called method.</returns>
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = TrimmerJustification)]
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = TrimmerJustification)]
		public async Task<string?> InvokeJavaScriptAsync(string methodName, params object[] paramValues)
		{
#if !WINDOWS && !ANDROID && !IOS && !MACCATALYST
			await Task.Delay(0);
			throw new NotImplementedException();
#else
			if (string.IsNullOrEmpty(methodName))
			{
				throw new ArgumentException($"The method name cannot be null or empty.", nameof(methodName));
			}

			var js = $"{methodName}({(paramValues == null ? string.Empty : string.Join(", ", paramValues.Select(v => System.Text.Json.JsonSerializer.Serialize(v))))})";
			return await EvaluateJavaScriptAsync(js);
#endif
		}

		/// <summary>
		/// Invokes a JavaScript method named <paramref name="methodName"/> and optionally passes in the parameter values specified
		/// by <paramref name="paramValues"/> by JSON-encoding each one.
		/// </summary>
		/// <typeparam name="TReturnType">The type of the return value to deserialize from JSON.</typeparam>
		/// <param name="methodName">The name of the JavaScript method to invoke.</param>
		/// <param name="paramValues">Optional array of objects to be passed to the JavaScript method by JSON-encoding each one.</param>
		/// <returns>An object of type <typeparamref name="TReturnType"/> containing the return value of the called method.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = TrimmerJustification)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = TrimmerJustification)]
		public async Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(string methodName, params object[] paramValues)
		{
#if !WINDOWS && !ANDROID && !IOS && !MACCATALYST
			await Task.Delay(0);
			throw new NotImplementedException();
#else
			var stringResult = await InvokeJavaScriptAsync(methodName, paramValues);

			if (stringResult is null)
			{
				return default;
			}
			return System.Text.Json.JsonSerializer.Deserialize<TReturnType>(stringResult);
#endif
		}

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
