using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Platform;
using WebKit;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	// Raises MovedToWindow like MauiWKWebView does. Without it, UIView.OnLoaded only notices a window through a change
	// of the view's bounds, which does not happen when the same web view is shown again at the same size.
	internal sealed class BlazorWKWebView : WKWebView, IUIViewLifeCycleEvents
	{
		public BlazorWKWebView(CGRect frame, WKWebViewConfiguration configuration)
			: base(frame, configuration)
		{
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}
	}
}
