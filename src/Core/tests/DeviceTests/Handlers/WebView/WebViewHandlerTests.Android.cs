using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Storage;
using Xunit;
using AWebView = Android.Webkit.WebView;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		AWebView GetNativeWebView(WebViewHandler webViewHandler) =>
			webViewHandler.PlatformView;

		string GetNativeSource(WebViewHandler webViewHandler) =>
			GetNativeWebView(webViewHandler).Url;

		[Fact]
		public void FileChooserRejectsFileUriInsideAppDataDirectory()
		{
			var filePath = Path.Combine(FileSystem.AppDataDirectory, $"webview_picker_{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "application data");

			try
			{
				using var file = new Java.IO.File(filePath);
				using var uri = AndroidUri.FromFile(file);
				using var intent = new Intent();
				intent.SetData(uri);

				Assert.False(MauiWebChromeClient.IsFileChooserResultValid(intent));
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Fact]
		public void FileChooserRejectsClipDataFileUriInsideAppDataDirectory()
		{
			var filePath = Path.Combine(FileSystem.AppDataDirectory, $"webview_picker_{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "application data");

			try
			{
				using var file = new Java.IO.File(filePath);
				using var uri = AndroidUri.FromFile(file);
				using var clipData = ClipData.NewRawUri("selected file", uri);
				using var intent = new Intent { ClipData = clipData };

				Assert.False(MauiWebChromeClient.IsFileChooserResultValid(intent));
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Fact]
		public void FileChooserAllowsExternalFileUri()
		{
			var externalCacheDirectory = Application.Context.ExternalCacheDir;
			Assert.NotNull(externalCacheDirectory);

			var filePath = Path.Combine(externalCacheDirectory.AbsolutePath, $"webview_picker_{Guid.NewGuid():N}.txt");
			File.WriteAllText(filePath, "external data");

			try
			{
				using var file = new Java.IO.File(filePath);
				using var uri = AndroidUri.FromFile(file);
				using var intent = new Intent();
				intent.SetData(uri);

				Assert.True(MauiWebChromeClient.IsFileChooserResultValid(intent));
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		[Fact]
		public void FileChooserAllowsContentUri()
		{
			using var uri = AndroidUri.Parse("content://maui-test/selected-file");
			using var intent = new Intent();
			intent.SetData(uri);

			Assert.True(MauiWebChromeClient.IsFileChooserResultValid(intent));
		}

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

					Assert.Same(parent, container.Parent);
					Assert.Same(container, handler.PlatformView.Parent);
					Assert.True(container.ClipChildren);
					Assert.Null(handler.PlatformView.ClipBounds);

					((IElementHandler)handler).DisconnectHandler();

					var destroyTrackingWebView = platformView ?? throw new InvalidOperationException("Expected the WebView factory to create a platform view.");
					Assert.True(destroyTrackingWebView.DestroyCalled);
					Assert.Null(destroyTrackingWebView.ParentWhenDestroyed);
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