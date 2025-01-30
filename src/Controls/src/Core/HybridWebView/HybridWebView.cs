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

		/// <inheritdoc/>
		/// TODO: make this public for .NET 10 (or a .NET 9 SR)
		internal async Task InvokeJavaScriptAsync(
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

		/// <inheritdoc/>
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
