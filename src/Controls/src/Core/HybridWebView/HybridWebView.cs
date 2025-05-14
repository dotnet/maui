using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="View"/> that presents local HTML content in a web view and allows JavaScript and C# code to
	/// communicate by using messages and by invoking methods.
	/// </summary>
	[ElementHandler<HybridWebViewHandler>]
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

		/// <inheritdoc/>
		object? IHybridWebView.InvokeJavaScriptTarget { get; set; }

		[UnconditionalSuppressMessage("Trimming", "IL2114", Justification = "Base type VisualElement specifies DynamicallyAccessedMemberTypes.NonPublicFields: https://github.com/dotnet/runtime/issues/108978#issuecomment-2420091986")]
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type? _invokeJavaScriptType;

		/// <inheritdoc/>
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type? IHybridWebView.InvokeJavaScriptType
		{
			get => _invokeJavaScriptType;
			set => _invokeJavaScriptType = value;
		}

		/// <inheritdoc/>
		public void SetInvokeJavaScriptTarget<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T target) where T : class
		{
			((IHybridWebView)this).InvokeJavaScriptTarget = target;
			((IHybridWebView)this).InvokeJavaScriptType = typeof(T);
		}

		void IHybridWebView.RawMessageReceived(string rawMessage)
		{
			RawMessageReceived?.Invoke(this, new HybridWebViewRawMessageReceivedEventArgs(rawMessage));
		}

		/// <summary>
		/// Raised when a raw message is received from the web view. Raw messages are strings that have no additional processing.
		/// </summary>
		public event EventHandler<HybridWebViewRawMessageReceivedEventArgs>? RawMessageReceived;

		/// <inheritdoc/>
		void IInitializationAwareWebView.WebViewInitializationStarted(WebViewInitializationStartedEventArgs args)
		{
			var platformArgs = new PlatformWebViewInitializingEventArgs(args);
			var e = new WebViewInitializingEventArgs(platformArgs);
			WebViewInitializing?.Invoke(this, e);
		}

		/// <summary>
		/// Raised when the web view is initializing. This event allows the application to perform additional configuration.
		/// </summary>
		public event EventHandler<WebViewInitializingEventArgs>? WebViewInitializing;

		/// <inheritdoc/>
		void IInitializationAwareWebView.WebViewInitializationCompleted(WebViewInitializationCompletedEventArgs args)
		{
			var platformArgs = new PlatformWebViewInitializedEventArgs(args);
			var e = new WebViewInitializedEventArgs(platformArgs);
			WebViewInitialized?.Invoke(this, e);
		}

		/// <summary>
		/// Raised when the web view has been initialized.
		/// </summary>
		public event EventHandler<WebViewInitializedEventArgs>? WebViewInitialized;

		/// <inheritdoc/>
		bool IWebRequestInterceptingWebView.WebResourceRequested(WebResourceRequestedEventArgs args)
		{
			var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(args);
			var e = new WebViewWebResourceRequestedEventArgs(platformArgs);
			WebResourceRequested?.Invoke(this, e);
			return e.Handled;
		}

		/// <summary>
		/// Raised when a web resource is requested. This event allows the application to intercept the request and provide a
		/// custom response.
		/// The event handler can set the <see cref="WebViewWebResourceRequestedEventArgs.Handled"/> property to true
		/// to indicate that the request has been handled and no further processing is needed. If the event handler does set this
		/// property to true, it must also call the
		/// <see cref="WebViewWebResourceRequestedEventArgs.SetResponse(int, string, System.Collections.Generic.IReadOnlyDictionary{string, string}?, System.IO.Stream?)"/>
		/// or <see cref="WebViewWebResourceRequestedEventArgs.SetResponse(int, string, System.Collections.Generic.IReadOnlyDictionary{string, string}?, System.Threading.Tasks.Task{System.IO.Stream?})"/>
		/// method to provide a response to the request.
		/// </summary>
		public event EventHandler<WebViewWebResourceRequestedEventArgs>? WebResourceRequested;

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

		/// <summary>
		/// Invokes a JavaScript method named <paramref name="methodName"/> and optionally passes in the parameter values specified
		/// by <paramref name="paramValues"/> by JSON-encoding each one.
		/// </summary>
		/// <param name="methodName">The name of the JavaScript method to invoke.</param>
		/// <param name="paramValues">Optional array of objects to be passed to the JavaScript method by JSON-encoding each one.</param>
		/// <param name="paramJsonTypeInfos">Optional array of metadata about serializing the types of the parameters specified by <paramref name="paramValues"/>.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public async Task InvokeJavaScriptAsync(
			string methodName,
			object?[]? paramValues = null,
			JsonTypeInfo?[]? paramJsonTypeInfos = null)
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

			await Handler?.InvokeAsync(
				nameof(IHybridWebView.InvokeJavaScriptAsync),
				new HybridWebViewInvokeJavaScriptRequest(methodName, null, paramValues, paramJsonTypeInfos))!;
		}

		/// <summary>
		/// Invokes a JavaScript method named <paramref name="methodName"/> and optionally passes in the parameter values specified
		/// by <paramref name="paramValues"/> by JSON-encoding each one.
		/// </summary>
		/// <param name="methodName">The name of the JavaScript method to invoke.</param>
		/// <param name="returnTypeJsonTypeInfo">Metadata about deserializing the return value from the JavaScript method call to type <typeparamref name="TReturnType"/>.</param>
		/// <param name="paramValues">Optional array of objects to be passed to the JavaScript method by JSON-encoding each one.</param>
		/// <param name="paramJsonTypeInfos">Optional array of metadata about serializing the types of the parameters specified by <paramref name="paramValues"/>.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public async Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(
			string methodName,
			JsonTypeInfo<TReturnType> returnTypeJsonTypeInfo,
			object?[]? paramValues = null,
			JsonTypeInfo?[]? paramJsonTypeInfos = null)
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

			var invokeResult = await Handler?.InvokeAsync(
				nameof(IHybridWebView.InvokeJavaScriptAsync),
				new HybridWebViewInvokeJavaScriptRequest(methodName, returnTypeJsonTypeInfo, paramValues, paramJsonTypeInfos))!;

			if (invokeResult is null)
			{
				return default;
			}
			return (TReturnType)invokeResult;
		}

		/// <inheritdoc/>
		public async Task<string?> EvaluateJavaScriptAsync(string script)
		{
			if (script == null)
			{
				return null;
			}

			var result = await Handler!.InvokeAsync(nameof(IHybridWebView.EvaluateJavaScriptAsync),
				new EvaluateJavaScriptAsyncRequest(script));

			return result;
		}
	}
}
