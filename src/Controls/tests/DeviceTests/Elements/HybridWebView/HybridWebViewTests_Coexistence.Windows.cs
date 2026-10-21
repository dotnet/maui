#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Web.WebView2.Core;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
[Collection(WebViewsCollection)]
public class HybridWebViewTests_Coexistence : HybridWebViewTestsBase
{
	[Theory]
	[InlineData(true, false, false)]
	[InlineData(false, false, false)]
	[InlineData(true, true, false)]
	[InlineData(true, false, true)]
	public Task WebViewLoadsWhenCoexistingWithHybridWebView(bool hybridFirst, bool customProfile, bool customEnvironmentOptions)
	{
		EnsureHandlerCreated(builder =>
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
				handlers.AddHandler<WebView, WebViewHandler>();
			});
			builder.Services.AddHybridWebViewDeveloperTools();
			builder.Services.AddScoped<IHybridWebViewTaskManager, HybridWebViewTaskManager>();
		});

		return InvokeOnMainThreadAsync(async () =>
		{
			var hybridWebView = new HybridWebView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				HybridRoot = "HybridTestRoot",
				DefaultFile = "index.html",
			};
			hybridWebView.WebViewInitializing += (_, args) =>
			{
				if (customProfile)
				{
					args.PlatformArgs!.ProfileName = "Coexistence";
				}
				if (customEnvironmentOptions)
				{
					args.PlatformArgs!.EnvironmentOptions = new CoreWebView2EnvironmentOptions();
				}
			};

			var webView = new WebView { WidthRequest = 100, HeightRequest = 100 };
			var navigated = new TaskCompletionSource<WebNavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
			webView.Navigated += (_, args) => navigated.TrySetResult(args.Result);

			var layout = new VerticalStackLayout();
			await AttachAndRun(layout, async _ =>
			{
				try
				{
					if (hybridFirst)
					{
						layout.Add(hybridWebView);
						await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);
					}

					layout.Add(webView);
					webView.Source = new HtmlWebViewSource { Html = "<html><body id='coexistence'>WebView content</body></html>" };
					Assert.Equal(WebNavigationResult.Success, await navigated.Task.WaitAsync(TimeSpan.FromSeconds(25)));
					Assert.Equal("true", await webView.EvaluateJavaScriptAsync("document.getElementById('coexistence') !== null"));

					if (!hybridFirst)
					{
						layout.Add(hybridWebView);
					}

					await WebViewHelpers.WaitForHybridWebViewLoaded(hybridWebView);
					Assert.Equal("true", await webView.EvaluateJavaScriptAsync("document.getElementById('coexistence') !== null"));
					if (customProfile)
					{
						var handler = (HybridWebViewHandler)hybridWebView.Handler!;
						Assert.Equal("Coexistence", handler.PlatformView.CoreWebView2.Profile.ProfileName);
					}
				}
				finally
				{
					webView.Handler?.DisconnectHandler();
					hybridWebView.Handler?.DisconnectHandler();
				}
			});
		});
	}
}
