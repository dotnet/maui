using System;
using Tizen.UIExtensions.ElmSharp;
using ElmSharp;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class WebViewContainer : WidgetLayout
	{
		public TWebView WebView { get; }

		public WebViewContainer(EvasObject parent) : base(parent)
		{
			WebView = new TWebView(parent);
			SetContent(WebView);
			AllowFocus(true);
			Focused += OnFocused;
			Unfocused += OnUnfocused;
		}

		void OnFocused(object? sender, EventArgs e)
		{
			WebView.SetFocus(true);
		}

		void OnUnfocused(object? sender, EventArgs e)
		{
			WebView.SetFocus(false);
		}
	}
}
