using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class WebViewPage
	{
		public WebViewPage()
		{
			InitializeComponent();
		}

		void OnGoBackClicked(object sender, EventArgs args)
		{
			// TODO: Implement CanGoBack

			MauiWebView.GoBack();
		}

		void OnGoForwardClicked(object sender, EventArgs args)
		{
			// TODO: Implement CanGoForward

			MauiWebView.GoForward();
		}

		void OnReloadClicked(object sender, EventArgs args)
		{
			MauiWebView.Reload();
		}
	}
}