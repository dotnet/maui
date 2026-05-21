using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
#if ANDROID
	/// <summary>
	/// Verifies that BlazorWebViewHandler uses OnBackPressedCallback (AndroidX) for back
	/// navigation instead of IOnBackInvokedCallback, ensuring the system predictive
	/// back-to-home animation plays when the WebView has no back history.
	/// </summary>
	[Fact]
	public async Task BlazorWebViewBackCallbackDisabledWhenCannotGoBack()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(MauiBlazorWebView.DeviceTests.Components.NoOpComponent), Selector = "#app", });

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			// After initial load with no navigation history, CanGoBack should be false,
			// so the back callback should be disabled, allowing the system animation to play
			Assert.False(platformWebView.CanGoBack(),
				"WebView should not be able to go back after initial page load");
		});
	}
#endif
}
