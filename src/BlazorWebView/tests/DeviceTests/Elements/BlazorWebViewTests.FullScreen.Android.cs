using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content.PM;
using Android.Webkit;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.MauiBlazorWebView.DeviceTests.Components;
using Xunit;
using AView = Android.Views.View;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

public partial class BlazorWebViewTests
{
	[Fact]
	public async Task WebContentReportsFullScreenAsEnabled()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = CreateFullScreenTestBlazorWebView();

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			var isFullScreenEnabled = await WebViewHelpers.ExecuteScriptAsync(platformWebView, "document.fullscreenEnabled");

			Assert.Equal("true", isFullScreenEnabled);
		});
	}

	[Fact]
	public async Task ShowCustomViewCoversActivityUntilHidden()
	{
		EnsureHandlerCreated(additionalCreationActions: appBuilder =>
		{
			appBuilder.Services.AddMauiBlazorWebView();
		});

		var bwv = CreateFullScreenTestBlazorWebView();

		await InvokeOnMainThreadAsync(async () =>
		{
			var bwvHandler = CreateHandler<BlazorWebViewHandler>(bwv);
			var platformWebView = bwvHandler.PlatformView;
			await WebViewHelpers.WaitForWebViewReady(platformWebView);

			var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
			Assert.NotNull(activity);
			var originalOrientation = activity.RequestedOrientation;
			var chromeClient = platformWebView.WebChromeClient;
			Assert.NotNull(chromeClient);
			var customView = new AView(platformWebView.Context);
			var customViewCallback = new RecordingCustomViewCallback();

			chromeClient.OnShowCustomView(customView, customViewCallback);

			Assert.Equal<object>(activity.Window!.DecorView, customView.Parent);
			Assert.Equal(ScreenOrientation.Landscape, activity.RequestedOrientation);

			chromeClient.OnHideCustomView();

			Assert.Null(customView.Parent);
			Assert.Equal(originalOrientation, activity.RequestedOrientation);
			Assert.Equal(1, customViewCallback.HiddenCount);
		});
	}

	static BlazorWebViewWithCustomFiles CreateFullScreenTestBlazorWebView()
	{
		var bwv = new BlazorWebViewWithCustomFiles
		{
			HostPage = "wwwroot/index.html",
			CustomFiles = new Dictionary<string, string>
			{
				{ "index.html", TestStaticFilesContents.DefaultMauiIndexHtmlContent },
			},
		};
		bwv.RootComponents.Add(new RootComponent { ComponentType = typeof(NoOpComponent), Selector = "#app", });
		return bwv;
	}

	sealed class RecordingCustomViewCallback : Java.Lang.Object, WebChromeClient.ICustomViewCallback
	{
		public int HiddenCount { get; private set; }

		public void OnCustomViewHidden()
		{
			HiddenCount++;
		}
	}
}
