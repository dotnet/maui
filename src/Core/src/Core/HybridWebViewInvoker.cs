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
	public abstract class HybridWebViewInvoker
	{
		/// <summary>
		/// Creates a new invoker for the specified JavaScript invocation target.
		/// </summary>
		/// <param name="invokeJavaScriptTarget">The object containing methods callable from JavaScript.</param>
		/// <param name="invokeJavaScriptType">The type containing methods callable from JavaScript.</param>
		protected HybridWebViewInvoker(object? invokeJavaScriptTarget, Type? invokeJavaScriptType)
		{
			InvokeJavaScriptTarget = invokeJavaScriptTarget;
			InvokeJavaScriptType = invokeJavaScriptType;
		}

		internal object? InvokeJavaScriptTarget { get; set; }

		internal Type? InvokeJavaScriptType { get; set; }

		/// <summary>
		/// Invokes the named .NET method with JSON-serialized parameters and returns a JSON-serialized result.
		/// </summary>
		/// <param name="methodName">The name of the method to invoke.</param>
		/// <param name="paramJsonValues">JSON-serialized parameter values, or null for parameterless methods.</param>
		/// <returns>JSON-serialized result, or null for void methods or null returns.</returns>
		public abstract Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues);
	}
}
