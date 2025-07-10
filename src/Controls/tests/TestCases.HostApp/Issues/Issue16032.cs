#if ANDROID
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using AWebView = Android.Webkit.WebView;
using IWebResourceRequest = Android.Webkit.IWebResourceRequest;
using Microsoft.Maui.Platform;
using WebResourceResponse = Android.Webkit.WebResourceResponse;

namespace Maui.Controls.Sample.Issues
{
	
	[Issue(IssueTracker.Github, 16032, "Improve the customization of WebView on Android", PlatformAffected.Android, isInternetRequired: true)]
	public class Issue16032 : ContentPage
	{
		public Issue16032()
		{
			Content = new VerticalStackLayout()
			{
				new Issue16032WebView
				{
					Background = new SolidPaint(Colors.Red),
					WidthRequest = 300,
					HeightRequest = 300		
				}
			};
		}

		class Issue16032WebView : WebView, IPropertyMapperView
		{
			PropertyMapper<Issue16032WebView, WebViewHandler> TestMapper =
				new PropertyMapper<Issue16032WebView, WebViewHandler>(WebViewHandler.Mapper);

			public Issue16032WebView()
			{				
				TestMapper.ModifyMapping(
					"WebViewClient",
					(handler, view, setter) =>
					{
						WebClient ??= new CustomWebClient((WebViewHandler)handler);
						handler.PlatformView.SetWebViewClient(WebClient);
					});
				
			}

			protected async override void OnHandlerChanged()
			{
				if (Handler is not WebViewHandler webViewHandler)
				{
					return;
				}

				var platformWebView = webViewHandler.PlatformView;
				platformWebView.Settings.AllowFileAccess = true;
				
				Source = new UrlWebViewSource { Url = "extracontent.html" };				

				var tcsLoaded = new TaskCompletionSource<bool>();
				var tcsNavigating = new TaskCompletionSource();
				var tcsRequested = new TaskCompletionSource();

				// if the timeout happens, cancel everything
				var pageLoadTimeout = TimeSpan.FromSeconds(30);
				var ctsTimeout = new CancellationTokenSource(pageLoadTimeout);
				ctsTimeout.Token.Register(() =>
				{
					tcsRequested.TrySetException(new TimeoutException($"Failed to request the image"));
					tcsNavigating.TrySetException(new TimeoutException($"Failed to navigate to the loaded page"));
					tcsLoaded.TrySetException(new TimeoutException($"Failed to load HTML"));
				});

				// attach some event handlers to track things
				var navigatingCount = 0;
				Navigating += ((sender, args) =>
				{
					navigatingCount++;

					if (args.Url == "file:///android_asset/extracontent.html")
						tcsNavigating.TrySetResult();
				});
				var shouldRequestCount = 0;
				ShouldInterceptRequestDelegate = new((view, request) =>
				{
					shouldRequestCount++;

					if (request.Url.ToString().StartsWith("https://raw.githubusercontent.com/dotnet/maui/4c096c1f17e9a23bf3961ba5778d3936039ad881/Assets/icon.png"))
						tcsRequested.TrySetResult();
				});

				// set up a task to wait for the page to load
				Navigated += (sender, args) =>
				{
					// Set success when we have a successful nav result
					if (args.Result == WebNavigationResult.Success && args.Url == "file:///android_asset/extracontent.html")
						tcsLoaded.TrySetResult(args.Result == WebNavigationResult.Success);
				};

				string failureMessage = String.Empty;

				try
				{

					if (WebClient is not CustomWebClient)
					{
						throw new Exception("CustomWebClient was not set");
					}

					// wait for the navigation to complete
					await tcsNavigating.Task;
					if	(navigatingCount < 1)
					{
						throw new Exception("Navigating event did not fire");
					}

					if ((!await tcsLoaded.Task))
					{
						throw new Exception("HTML Source Failed to Load");
					}

					// wait for the image to be requested
					await tcsRequested.Task;

					if	(shouldRequestCount != 1)
					{
						throw new Exception("only 1 request for the image to load");
					}
				}
				catch(Exception ex)
				{
					failureMessage = ex.Message;
				}

				if (String.IsNullOrEmpty(failureMessage))
				{
					((VerticalStackLayout)Parent).Insert(0, new Label { Text = "All Expectations Have Been Met", AutomationId = "Success" });
				}
				else
				{
					((VerticalStackLayout)Parent).Insert(0, new Label { Text = failureMessage });
				}
			}

			PropertyMapper IPropertyMapperView.GetPropertyMapperOverrides() => TestMapper;
			
			CustomWebClient WebClient { get; set; }

			public Action<AWebView, IWebResourceRequest> ShouldInterceptRequestDelegate { get; set; }

			class CustomWebClient : MauiWebViewClient
			{
				WebViewHandler _handler;

				public CustomWebClient(WebViewHandler handler)
					: base(handler)
				{
					_handler = handler;
				}

				public override WebResourceResponse ShouldInterceptRequest(AWebView view, IWebResourceRequest request)
				{
					if (_handler.VirtualView is Issue16032WebView customWebView)
					customWebView.ShouldInterceptRequestDelegate(view, request);

					return base.ShouldInterceptRequest(view, request);
				}
			}
		}
	}
}


#endif
