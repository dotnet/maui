using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="View"/> that presents local HTML content in a web view and allows JavaScript and C# code to
	/// communicate by using messages and by invoking methods.
	/// </summary>
	public class HybridWebView : View, IHybridWebView, IAllowedDomainsWebView
	{
		/// <summary>Bindable property for <see cref="DefaultFile"/>.</summary>
		public static readonly BindableProperty DefaultFileProperty;
		/// <summary>Bindable property for <see cref="HybridRoot"/>.</summary>
		public static readonly BindableProperty HybridRootProperty;
		/// <summary>Bindable property for <see cref="AllowedDomains"/>.</summary>
		public static readonly BindableProperty AllowedDomainsProperty;

		[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "BindableProperty.Create preserves public methods on the declaring type; it does not call the annotated legacy SetInvokeJavaScriptTarget overload.")]
		[UnconditionalSuppressMessage("Trimming", "IL3050:RequiresDynamicCode", Justification = "BindableProperty.Create preserves public methods on the declaring type; it does not call the annotated legacy SetInvokeJavaScriptTarget overload.")]
		static HybridWebView()
		{
			DefaultFileProperty = BindableProperty.Create(nameof(DefaultFile), typeof(string), typeof(HybridWebView), defaultValue: "index.html");
			HybridRootProperty = BindableProperty.Create(nameof(HybridRoot), typeof(string), typeof(HybridWebView), defaultValue: "wwwroot");
			AllowedDomainsProperty = BindableProperty.Create(nameof(AllowedDomains), typeof(IList<string>), typeof(HybridWebView), null);
		}

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

		/// <summary>
		/// Gets or sets the list of domains that this web view is allowed to navigate to.
		/// When <see langword="null"/> or empty, all domains are allowed.
		/// </summary>
		/// <seealso cref="IAllowedDomainsWebView"/>
		public IList<string>? AllowedDomains
		{
			get { return (IList<string>?)GetValue(AllowedDomainsProperty); }
			set { SetValue(AllowedDomainsProperty, value); }
		}

		HybridWebViewInvoker? _invoker;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public HybridWebViewInvoker Invoker
		{
			get => _invoker ?? throw new InvalidOperationException($"No invoker is configured. Call {nameof(SetInvokeJavaScriptTarget)} to set up JS-to-.NET method invocation.");
			set => _invoker = value;
		}

		/// <inheritdoc/>
		object? IHybridWebView.InvokeJavaScriptTarget
		{
			get => _invoker?.InvokeJavaScriptTarget;
			set => Invoker.InvokeJavaScriptTarget = value;
		}

		/// <inheritdoc/>
		Type? IHybridWebView.InvokeJavaScriptType
		{
			get => _invoker?.InvokeJavaScriptType;
			set => Invoker.InvokeJavaScriptType = value;
		}

		[RequiresUnreferencedCode("Use SetInvokeJavaScriptTarget<T>(T target, JsonSerializerContext jsonSerializerContext) for trimming and NativeAOT compatibility.")]
#if !NETSTANDARD
		[RequiresDynamicCode("Use SetInvokeJavaScriptTarget<T>(T target, JsonSerializerContext jsonSerializerContext) for trimming and NativeAOT compatibility.")]
#endif
		public void SetInvokeJavaScriptTarget<T>(T target) where T : class
		{
			if (target is null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			Invoker = new ReflectionHybridWebViewInvoker(target, typeof(T));
		}

		/// <inheritdoc/>
		public void SetInvokeJavaScriptTarget<T>(T target, JsonSerializerContext jsonSerializerContext) where T : class
		{
			if (target is null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			if (jsonSerializerContext is null)
			{
				throw new ArgumentNullException(nameof(jsonSerializerContext));
			}

			// The source generator interceptor must replace this method call at compile time
			// and register a generated implementation with fully typed delegates. Reaching
			// this body means interception failed (for example, the analyzer was not
			// referenced), which is a non-recoverable build/configuration bug.
			throw new InvalidOperationException(
				"SetInvokeJavaScriptTarget<T>(T, JsonSerializerContext) must be intercepted by the source generator. " +
				"Ensure the Microsoft.Maui.Core.HybridWebViewSourceGen analyzer is referenced.");
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
