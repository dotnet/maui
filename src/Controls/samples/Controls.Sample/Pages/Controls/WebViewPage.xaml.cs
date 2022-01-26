using System;
using System.Diagnostics;

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
			Debug.WriteLine($"CanGoBack {MauiWebView.CanGoBack}");

			if (MauiWebView.CanGoBack)
			{
				MauiWebView.GoBack();
			}
		}

		void OnGoForwardClicked(object sender, EventArgs args)
		{
			Debug.WriteLine($"CanGoForward {MauiWebView.CanGoForward}");

			if (MauiWebView.CanGoForward)
			{
				MauiWebView.GoForward();
			}
		}

		void OnReloadClicked(object sender, EventArgs args)
		{
			MauiWebView.Reload();
		}

		void OnEvalClicked(object sender, EventArgs args)
		{
			MauiWebView.Eval("alert('text')");
		}
	}
}