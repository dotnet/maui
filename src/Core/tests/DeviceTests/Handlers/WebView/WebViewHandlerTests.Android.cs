using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		AWebView GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler) =>
			GetNativeWebView(webViewHandler).Url;

		[Fact(DisplayName = "MauiWebView has JS bridge registered at construction time")]
		public async Task WebView_HasScrollCaptureBridge_AfterConstruction()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var stub = new WebViewStub();
				var handler = CreateHandler<WebViewHandler>(stub);
				var webView = new MauiWebView(handler, handler.MauiContext!.Context!);
				Assert.True(RefreshViewWebViewScrollCapture.IsAttached(webView),
					"JS bridge must be registered in the constructor, before any page load.");
			});
		}
	}
}