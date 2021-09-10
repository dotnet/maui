using System;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.Maui
{
	public class MauiWebView : WidgetLayout
	{
		public TWebView WebView { get; }

		public MauiWebView(EvasObject parent) : base(parent)
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