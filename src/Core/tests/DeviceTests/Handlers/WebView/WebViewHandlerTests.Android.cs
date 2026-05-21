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

		[Fact(DisplayName = "DisconnectHandler Destroys Native WebView")]
		public async Task DisconnectHandlerDestroysNativeWebView()
		{
			var originalFactory = WebViewHandler.PlatformViewFactory;

			try
			{
				await InvokeOnMainThreadAsync(() =>
				{
					DestroyTrackingMauiWebView platformView = null;

					WebViewHandler.PlatformViewFactory = handler =>
					{
						platformView = new DestroyTrackingMauiWebView((WebViewHandler)handler, handler.MauiContext!.Context!);
						return platformView;
					};

					var webView = new WebViewStub();
					var handler = CreateHandler(webView);
					var parent = new FrameLayout(handler.MauiContext!.Context!);
					parent.AddView(handler.PlatformView);

					Assert.Same(parent, handler.PlatformView.Parent);

					((IElementHandler)handler).DisconnectHandler();

					var destroyTrackingWebView = platformView ?? throw new InvalidOperationException("Expected the WebView factory to create a platform view.");
					Assert.True(destroyTrackingWebView.DestroyCalled);
					Assert.Null(destroyTrackingWebView.ParentWhenDestroyed);
					Assert.Equal(0, parent.ChildCount);
				});
			}
			finally
			{
				WebViewHandler.PlatformViewFactory = originalFactory;
			}
		}

		class DestroyTrackingMauiWebView : MauiWebView
		{
			public DestroyTrackingMauiWebView(WebViewHandler handler, Context context)
				: base(handler, context)
			{
			}

			public bool DestroyCalled { get; private set; }

			public IViewParent ParentWhenDestroyed { get; private set; }

			public override void Destroy()
			{
				DestroyCalled = true;
				ParentWhenDestroyed = Parent;
				base.Destroy();
			}
		}
	}
}