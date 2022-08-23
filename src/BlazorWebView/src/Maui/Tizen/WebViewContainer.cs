using System;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A Tizen WebView browser control container.
	/// </summary>
	public class WebViewContainer : WidgetLayout
	{

		/// <summary>
		/// A Tizen WebView.
		/// </summary>
		public TWebView WebView { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="WebViewContainer"/>
		/// </summary>
		/// <param name="parent">The <see cref="EvasObject"/>.</param>
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
