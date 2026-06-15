using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
		static readonly ConditionalWeakTable<IHybridWebView, InvokerHolder> s_invokers = new();

		/// <summary>
		/// Creates a new invoker for the specified JavaScript invocation target.
		/// </summary>
		/// <param name="invokeJavaScriptTarget">The object containing methods callable from JavaScript.</param>
		/// <param name="invokeJavaScriptType">The type containing methods callable from JavaScript.</param>
		protected HybridWebViewInvoker(object? invokeJavaScriptTarget, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type? invokeJavaScriptType)
		{
			InvokeJavaScriptTarget = invokeJavaScriptTarget;
			InvokeJavaScriptType = invokeJavaScriptType;
		}

		internal object? InvokeJavaScriptTarget { get; set; }

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		internal Type? InvokeJavaScriptType { get; set; }

		internal static void SetInvoker(IHybridWebView hybridWebView, HybridWebViewInvoker invoker)
		{
			if (hybridWebView is null)
			{
				throw new ArgumentNullException(nameof(hybridWebView));
			}

			if (invoker is null)
			{
				throw new ArgumentNullException(nameof(invoker));
			}

			s_invokers.GetOrCreateValue(hybridWebView).Invoker = invoker;
		}

		internal static HybridWebViewInvoker? GetInvoker(IHybridWebView hybridWebView)
		{
			if (hybridWebView is null)
			{
				throw new ArgumentNullException(nameof(hybridWebView));
			}

			return s_invokers.TryGetValue(hybridWebView, out var holder)
				? holder.Invoker
				: null;
		}

		/// <summary>
		/// Invokes the named .NET method with JSON-serialized parameters and returns a JSON-serialized result.
		/// </summary>
		/// <param name="methodName">The name of the method to invoke.</param>
		/// <param name="paramJsonValues">JSON-serialized parameter values, or null for parameterless methods.</param>
		/// <returns>JSON-serialized result, or null for void methods or null returns.</returns>
		public abstract Task<string?> InvokeMethodAsync(string methodName, string[]? paramJsonValues);

		sealed class InvokerHolder
		{
			public HybridWebViewInvoker? Invoker { get; set; }
		}
	}
}
