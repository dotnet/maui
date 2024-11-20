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
			MauiWebView.ProcessTerminated += OnMauiWebViewProcessTerminated;
		}

		protected override void OnDisappearing()
		{
			MauiWebView.Navigating -= OnMauiWebViewNavigating;
			MauiWebView.Navigated -= OnMauiWebViewNavigated;
			MauiWebView.ProcessTerminated -= OnMauiWebViewProcessTerminated;
		}

		void OnUpdateHtmlSourceClicked(object sender, EventArgs args)
		{
			Random rnd = new();
			HtmlWebViewSource htmlWebViewSource = new();
			HtmlSourceWebView.Source = htmlWebViewSource;
			htmlWebViewSource.Html += $"<h1>Updated Content {rnd.Next()}!</h1><br>";
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

		void OnMauiWebViewNavigating(object? sender, Microsoft.Maui.Controls.WebNavigatingEventArgs e)
		{
			Debug.WriteLine($"Navigating - Url: {e.Url}, Event: {e.NavigationEvent}");
		}

		void OnMauiWebViewNavigated(object? sender, Microsoft.Maui.Controls.WebNavigatedEventArgs e)
		{
			Debug.WriteLine($"Navigated - Url: {e.Url}, Event: {e.NavigationEvent}, Result: {e.Result}");
		}

		void OnMauiWebViewProcessTerminated(object? sender, WebViewProcessTerminatedEventArgs e)
		{
#if ANDROID
			var renderProcessGoneDetail = e.PlatformArgs.RenderProcessGoneDetail;
			Debug.WriteLine($"WebView process failed. DidCrash: {renderProcessGoneDetail?.DidCrash()}");
#elif WINDOWS
			var coreWebView2ProcessFailedEventArgs = e.PlatformArgs.CoreWebView2ProcessFailedEventArgs;
			Debug.WriteLine($"WebView process failed. ExitCode: {coreWebView2ProcessFailedEventArgs.ExitCode}");
#else
			Debug.WriteLine("WebView process failed.");
#endif
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

		async void OnSetUserAgentClicked(object sender, EventArgs e)
		{
			input.Text = "useragent.html";
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

		void OnLoadHtml5VideoClicked(object sender, EventArgs e)
		{
			MauiWebView.Source = new UrlWebViewSource { Url = "video.html" };
		}

		void OnLoadHttpBinClicked(object sender, EventArgs e)
		{
			// on httpbin.org find the section where you can load the cookies for the website.
			// The cookie that is set below should show up for this to succeed.
			MauiWebView.Cookies = new System.Net.CookieContainer();
			MauiWebView.Cookies.Add(new System.Net.Cookie("MauiCookie", "Hmmm Cookies!", "/", "httpbin.org"));

			MauiWebView.Source = new UrlWebViewSource { Url = "https://httpbin.org/#/Cookies/get_cookies" };
		}
	}
}