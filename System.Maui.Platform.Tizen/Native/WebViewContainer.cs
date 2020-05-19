using System;
using ElmSharp;
using TWebView = Tizen.WebView.WebView;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class WebViewContainer : ElmSharp.Layout
	{
		public TWebView WebView { get; }

		public WebViewContainer(EvasObject parent) : base(parent)
		{
			SetTheme("layout", "elm_widget", "default");
			WebView = new TWebView(parent);
			SetContent(WebView);
			AllowFocus(true);
			Focused += OnFocused;
			Unfocused += OnUnfocused;
		}

		void OnFocused(object sender, EventArgs e)
		{
			WebView.SetFocus(true);
		}

		void OnUnfocused(object sender, EventArgs e)
		{
			WebView.SetFocus(false);
		}
	}
}
