using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui
{
	/// <summary>
	/// For internal use by generated HybridWebView invocation code.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class HybridWebViewInvoker
	{
		static readonly ConditionalWeakTable<IHybridWebView, InvokerHolder> s_invokers = new();

		/// <summary>
		/// Sets the invoker used to dispatch JavaScript calls to .NET methods.
		/// </summary>
		/// <param name="hybridWebView">The hybrid web view that owns the invoker.</param>
		/// <param name="invoker">The invoker used to dispatch JavaScript calls.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetInvoker(IHybridWebView hybridWebView, IHybridWebViewInvoker invoker)
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

		internal static IHybridWebViewInvoker? GetInvoker(IHybridWebView hybridWebView)
		{
			return s_invokers.TryGetValue(hybridWebView, out var holder)
				? holder.Invoker
				: null;
		}

		internal static void SetInvokeJavaScriptType(IHybridWebView hybridWebView, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type? invokeJavaScriptType)
		{
			if (hybridWebView is null)
			{
				throw new ArgumentNullException(nameof(hybridWebView));
			}

			s_invokers.GetOrCreateValue(hybridWebView).InvokeJavaScriptType = invokeJavaScriptType;
		}

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		internal static Type? GetInvokeJavaScriptType(IHybridWebView hybridWebView)
		{
			if (hybridWebView is null)
			{
				throw new ArgumentNullException(nameof(hybridWebView));
			}

			return s_invokers.TryGetValue(hybridWebView, out var holder)
				? holder.InvokeJavaScriptType
				: null;
		}

		sealed class InvokerHolder
		{
			public IHybridWebViewInvoker? Invoker { get; set; }

			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
			public Type? InvokeJavaScriptType { get; set; }
		}
	}
}
