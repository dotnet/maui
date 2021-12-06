using System;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : WidgetLayout, IWebViewDelegate
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

		void IWebViewDelegate.LoadHtml(string? html, string? baseUrl)
		{
			WebView.LoadHtml(baseUrl ?? string.Empty, html ?? string.Empty);
		}

		void IWebViewDelegate.LoadUrl(string? url)
		{
			WebView.LoadUrl(url ?? string.Empty);
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