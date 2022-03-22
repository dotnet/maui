using System;
using Android.Webkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using static Android.Views.ViewGroup;
using Path = System.IO.Path;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, BlazorAndroidWebView>
	{
		private WebViewClient? _webViewClient;
		private WebChromeClient? _webChromeClient;
		private AndroidWebKitWebViewManager? _webviewManager;
		internal AndroidWebKitWebViewManager? WebviewManager => _webviewManager;

		protected override BlazorAndroidWebView CreatePlatformView()
		{
			var blazorAndroidWebView = new BlazorAndroidWebView(Context!)
			{
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
				LayoutParameters = new Android.Widget.AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0)
#pragma warning restore 618
			};

			// To allow overriding ExternalLinkMode.InsecureOpenInWebView and open links in browser with a _blank target
			blazorAndroidWebView.Settings.SetSupportMultipleWindows(true);

			BlazorAndroidWebView.SetWebContentsDebuggingEnabled(enabled: DeveloperTools.Enabled);

			if (blazorAndroidWebView.Settings != null)
			{
				blazorAndroidWebView.Settings.JavaScriptEnabled = true;
				blazorAndroidWebView.Settings.DomStorageEnabled = true;
			}

			_webViewClient = GetWebViewClient();
			blazorAndroidWebView.SetWebViewClient(_webViewClient);

			_webChromeClient = GetWebChromeClient();
			blazorAndroidWebView.SetWebChromeClient(_webChromeClient);

			return blazorAndroidWebView;
		}

		protected override void DisconnectHandler(BlazorAndroidWebView platformView)
		{
			platformView.StopLoading();

			if (_webviewManager != null)
			{
				// Dispose this component's contents and block on completion so that user-written disposal logic and
				// Blazor disposal logic will complete.
				_webviewManager?
					.DisposeAsync()
					.AsTask()
					.GetAwaiter()
					.GetResult();

				_webviewManager = null;
			}

			_webViewClient?.Dispose();
			_webChromeClient?.Dispose();
		}

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet ||
				_webviewManager != null)
			{
				return;
			}
			if (PlatformView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
			}

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

			var fileProvider = VirtualView.CreateFileProvider(contentRootDir);

			_webviewManager = new AndroidWebKitWebViewManager(
				PlatformView,
				Services!,
				new MauiDispatcher(Services!.GetRequiredService<IDispatcher>()),
				fileProvider,
				VirtualView.JSComponents,
				hostPageRelativePath);

			if (RootComponents != null)
			{
				foreach (var rootComponent in RootComponents)
				{
					// Since the page isn't loaded yet, this will always complete synchronously
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}

			_webviewManager.Navigate("/");
		}

		internal IFileProvider CreateFileProvider(string contentRootDir)
		{
			return new AndroidMauiAssetFileProvider(Context.Assets, contentRootDir);
		}

		protected virtual WebViewClient GetWebViewClient() =>
			new WebKitWebViewClient(this);

		protected virtual WebChromeClient GetWebChromeClient() =>
			new BlazorWebChromeClient();
	}
}
