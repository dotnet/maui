using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.App;
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

		[Fact(DisplayName = "Creating a WebView keeps the night mode AppCompat applied in place")]
		public async Task CreatingWebViewKeepsInPlaceNightMode()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = Assert.IsAssignableFrom<AppCompatActivity>(MauiContext.Context.GetActivity());
				var resources = activity.Resources;
				var systemNight = Application.Context.Resources.Configuration.UiMode & UiMode.NightMask;
				var initialNightMode = activity.Delegate.LocalNightMode;

				try
				{
					// Use the opposite of the system theme, applied in place like UserAppTheme at runtime
					activity.Delegate.SetLocalNightMode(systemNight == UiMode.NightYes ? AppCompatDelegate.ModeNightNo : AppCompatDelegate.ModeNightYes);
					var appNight = resources.Configuration.UiMode & UiMode.NightMask;
					Assert.NotEqual(systemNight, appNight);

					// Loading the WebView provider resets the activity's resources to the system uiMode.
					// Reset them here too, in case an earlier test already loaded the provider.
					var config = new Configuration(resources.Configuration);
					config.UiMode = (config.UiMode & ~UiMode.NightMask) | systemNight;
#pragma warning disable CS0618, CA1422 // Type or member is obsolete
					resources.UpdateConfiguration(config, resources.DisplayMetrics);
#pragma warning restore CS0618, CA1422

					CreateHandler(new WebViewStub());

					Assert.Equal(appNight, resources.Configuration.UiMode & UiMode.NightMask);
				}
				finally
				{
					activity.Delegate.SetLocalNightMode(initialNightMode);
				}
			});
		}

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