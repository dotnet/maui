using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Controls
{
	internal class WebViewCoreGalleryPage : CoreGalleryPage<WebView>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement (WebView element)
		{
			element.HeightRequest = 200;

			element.Source = new UrlWebViewSource { Url = "http://xamarin.com/" };
		}

		protected override void Build (StackLayout stackLayout)
		{
			base.Build (stackLayout);

			var urlWebViewSourceContainer = new ViewContainer<WebView> (Test.WebView.UrlWebViewSource, 
				new WebView {
					Source = new UrlWebViewSource { Url = "https://www.google.com/" },
					HeightRequest = 200
				}
			);

			const string html = "<html><div class=\"test\"><h2>I am raw html</h2></div></html>";
			var htmlWebViewSourceContainer = new ViewContainer<WebView> (Test.WebView.HtmlWebViewSource, 
				new WebView {
					Source = new HtmlWebViewSource { Html = html },
					HeightRequest = 200
				}
			);

			var htmlFileWebSourceContainer = new ViewContainer<WebView> (Test.WebView.LoadHtml,
				new WebView {
					Source = new HtmlWebViewSource { 
						Html = @"<html>
<head>
<link rel=""stylesheet"" href=""default.css"">
</head>
<body>
<h1>Xamarin.Forms</h1>
<p>The CSS and image are loaded from local files!</p>
<img src='WebImages/XamarinLogo.png'/>
<p><a href=""local.html"">next page</a></p>
</body>
</html>"
					},
					HeightRequest = 200
				}
			);

			// NOTE: Currently the ability to programmatically enable/disable mixed content only exists on Android
			if (Device.RuntimePlatform == Device.Android)
			{
				var mixedContentTestPage = "https://mixed-content-test.appspot.com/";

				var mixedContentDisallowedWebView = new WebView() { HeightRequest = 1000 };
				mixedContentDisallowedWebView.On<Android>().SetMixedContentMode(MixedContentHandling.NeverAllow);
				mixedContentDisallowedWebView.Source = new UrlWebViewSource
				{
					Url = mixedContentTestPage
				};

				var mixedContentAllowedWebView = new WebView() { HeightRequest = 1000 };
				mixedContentAllowedWebView.On<Android>().SetMixedContentMode(MixedContentHandling.AlwaysAllow);
				mixedContentAllowedWebView.Source = new UrlWebViewSource
				{
					Url = mixedContentTestPage
				};

				var mixedContentDisallowedContainer = new ViewContainer<WebView>(Test.WebView.MixedContentDisallowed,
					mixedContentDisallowedWebView);
				var mixedContentAllowedContainer = new ViewContainer<WebView>(Test.WebView.MixedContentAllowed,
					mixedContentAllowedWebView);

				Add(mixedContentDisallowedContainer);
				Add(mixedContentAllowedContainer);
			}


			var jsAlertWebView = new WebView
			{
				Source = new HtmlWebViewSource
				{
					Html = @"<html>
<head>
<link rel=""stylesheet"" href=""default.css"">
</head>
<body>
<button onclick=""window.alert('foo');"">Click</button>
</body>
</html>"
				},
				HeightRequest = 200
			};

			jsAlertWebView.On<Windows>().SetIsJavaScriptAlertEnabled(true);
			
			var javascriptAlertWebSourceContainer = new ViewContainer<WebView>(Test.WebView.JavaScriptAlert,
				jsAlertWebView
			);

			var evaluateJsWebView = new WebView
			{
				Source = new UrlWebViewSource { Url = "https://www.google.com/" },
				HeightRequest = 50
			};
			var evaluateJsWebViewSourceContainer = new ViewContainer<WebView>(Test.WebView.EvaluateJavaScript,
				evaluateJsWebView
			);

			var resultsLabel = new Label();
			var execButton = new Button();
			execButton.Text = "Evaluate Javascript";
			execButton.Command = new Command(async() => resultsLabel.Text = await evaluateJsWebView.EvaluateJavaScriptAsync(
												"var test = function(){ return 'This string came from Javascript!'; }; test();"));

			evaluateJsWebViewSourceContainer.ContainerLayout.Children.Add(resultsLabel);
			evaluateJsWebViewSourceContainer.ContainerLayout.Children.Add(execButton);


			Add (urlWebViewSourceContainer);
			Add (htmlWebViewSourceContainer);
			Add (htmlFileWebSourceContainer);
			Add (javascriptAlertWebSourceContainer);
			Add (evaluateJsWebViewSourceContainer);
		}
	}
}