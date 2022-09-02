using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace Maui.Controls.Sample.Pages
{
	public partial class WebViewPage
	{
		public WebViewPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			MauiWebView.Navigating += OnMauiWebViewNavigating;
			MauiWebView.Navigated += OnMauiWebViewNavigated;
		}

		protected override void OnDisappearing()
		{
			MauiWebView.Navigating -= OnMauiWebViewNavigating;
			MauiWebView.Navigated -= OnMauiWebViewNavigated;
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

		void OnMauiWebViewNavigating(object sender, Microsoft.Maui.Controls.WebNavigatingEventArgs e)
		{
			Debug.WriteLine($"Navigating - Url: {e.Url}, Event: {e.NavigationEvent}");
		}

		void OnMauiWebViewNavigated(object sender, Microsoft.Maui.Controls.WebNavigatedEventArgs e)
		{
			Debug.WriteLine($"Navigated - Url: {e.Url}, Event: {e.NavigationEvent}, Result: {e.Result}");
		}

		async void OnEvalAsyncClicked(object sender, EventArgs args)
		{
			MauiWebView.Eval("alert('text')");

			var result = await MauiWebView.EvaluateJavaScriptAsync(
				"var test = function(){ return 'This string came from Javascript!'; }; test();");

			EvalResultLabel.Text = result;
		}

		async void OnLoadHtmlFileClicked(object sender, EventArgs e)
		{
			await LoadMauiAsset();
		}

		async Task LoadMauiAsset()
		{
			using var stream = await FileSystem.OpenAppPackageFileAsync(input.Text.Trim());
			using var reader = new StreamReader(stream);

			var html = reader.ReadToEnd();
			FileWebView.Source = new HtmlWebViewSource { Html = html };
		}

		void OnAllowMixedContentClicked(object sender, EventArgs e)
		{
			MauiWebView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetMixedContentMode(MixedContentHandling.AlwaysAllow);
		}

		void OnEnableZoomControlsClicked(object sender, EventArgs e)
		{
			MauiWebView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().EnableZoomControls(true);
		}
	}
}