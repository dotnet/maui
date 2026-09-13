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
					var container = Assert.IsType<WrapperView>(handler.ContainerView);
					parent.AddView(container);

					Assert.True(container.ClipChildren);
					Assert.Same(container, handler.PlatformView.Parent);
					Assert.Null(handler.PlatformView.ClipBounds);

					((IElementHandler)handler).DisconnectHandler();

					var destroyTrackingWebView = platformView ?? throw new InvalidOperationException("Expected the WebView factory to create a platform view.");
					Assert.True(destroyTrackingWebView.DestroyCalled);
					Assert.Null(destroyTrackingWebView.ParentWhenDestroyed);
					Assert.Null(container.Parent);
					Assert.Null(handler.ContainerView);
					Assert.False(handler.HasContainer);
					Assert.Equal(0, parent.ChildCount);

					handler.SetVirtualView(webView);
					var newContainer = Assert.IsType<WrapperView>(handler.ContainerView);
					Assert.NotSame(container, newContainer);
					Assert.NotSame(destroyTrackingWebView, handler.PlatformView);
					Assert.Same(newContainer, handler.PlatformView.Parent);
					Assert.True(newContainer.ClipChildren);
					Assert.Null(handler.PlatformView.ClipBounds);
					parent.AddView(newContainer);

					((IElementHandler)handler).DisconnectHandler();
					((IElementHandler)handler).DisconnectHandler();
					Assert.Null(handler.ContainerView);
					Assert.False(handler.HasContainer);
					Assert.Equal(0, parent.ChildCount);
				});
			}
			finally
			{
				WebViewHandler.PlatformViewFactory = originalFactory;
			}
		}

		[Fact]
		public async Task InitialOpacityIsAppliedToClippingContainer()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var webView = new WebViewStub
				{
					Opacity = 0.5,
				};
				var handler = CreateHandler(webView);
				var container = Assert.IsType<WrapperView>(handler.ContainerView);

				Assert.Equal(0.5f, container.Alpha, 3);
				Assert.Equal(1f, handler.PlatformView.Alpha);

				webView.Opacity = 0.25;
				handler.UpdateValue(nameof(IView.Opacity));

				Assert.Equal(0.25f, container.Alpha, 3);
				Assert.Equal(1f, handler.PlatformView.Alpha);
			});
		}

		[Theory]
		[InlineData(0, 100)]
		[InlineData(100, 0)]
		public async Task ClippingContainerSurvivesZeroDimensionThenNormalSize(int initialWidth, int initialHeight)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(new WebViewStub());
				var platformView = handler.PlatformView;
				var container = Assert.IsType<WrapperView>(handler.ContainerView);

				MeasureAndLayout(platformView, initialWidth, initialHeight);
				Assert.Equal(initialWidth, platformView.MeasuredWidth);
				Assert.Equal(initialHeight, platformView.MeasuredHeight);
				Assert.Same(container, platformView.Parent);
				Assert.True(container.ClipChildren);
				Assert.Null(platformView.ClipBounds);

				MeasureAndLayout(platformView, 100, 100);
				Assert.Equal(100, platformView.MeasuredWidth);
				Assert.Equal(100, platformView.MeasuredHeight);
				Assert.Same(container, platformView.Parent);
				Assert.True(container.ClipChildren);
				Assert.Null(platformView.ClipBounds);
			});
		}

		static void MeasureAndLayout(View platformView, int width, int height)
		{
			platformView.Measure(
				View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
				View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
			platformView.Layout(0, 0, width, height);
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