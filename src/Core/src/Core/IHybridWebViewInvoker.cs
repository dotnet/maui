using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Abstraction for JS-to-.NET method invocation in HybridWebView.
	/// Implemented by the reflection-based fallback and by the source generator.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class IHybridWebViewInvoker
	{
		/// <summary>
		/// Creates a new invoker for the specified JavaScript invocation target.
		/// </summary>
		/// <param name="invokeJavaScriptTarget">The object containing methods callable from JavaScript.</param>
		/// <param name="invokeJavaScriptType">The type containing methods callable from JavaScript.</param>
		protected IHybridWebViewInvoker(object? invokeJavaScriptTarget, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type? invokeJavaScriptType)
		{
			InvokeJavaScriptTarget = invokeJavaScriptTarget;
			InvokeJavaScriptType = invokeJavaScriptType;
		}

		internal object? InvokeJavaScriptTarget { get; set; }

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		internal Type? InvokeJavaScriptType { get; set; }

		/// <summary>
		/// Attaches the invoker to a HybridWebView instance.
		/// </summary>
		/// <param name="hybridWebView">The hybrid web view that owns the invoker.</param>
		public void AttachTo(IHybridWebView hybridWebView)
		{
			if (hybridWebView is null)
			{
				throw new ArgumentNullException(nameof(hybridWebView));
			}

			if (hybridWebView is not IHybridWebViewInvokerHost host)
			{
				throw new InvalidOperationException("HybridWebView invokers can only be attached to Microsoft.Maui.Controls.HybridWebView instances.");
			}

			host.SetInvoker(this);
		}

		/// <summary>
		/// Invokes the named .NET method with JSON-serialized parameters and returns a JSON-serialized result.
		/// </summary>
		/// <param name="methodName">The name of the method to invoke.</param>
		/// <param name="paramJsonValues">JSON-serialized parameter values, or null for parameterless methods.</param>
		/// <returns>JSON-serialized result, or null for void methods or null returns.</returns>
		public abstract Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues);
	}

	internal interface IHybridWebViewInvokerHost
	{
		IHybridWebViewInvoker? Invoker { get; }

		void SetInvoker(IHybridWebViewInvoker invoker);
	}
}
