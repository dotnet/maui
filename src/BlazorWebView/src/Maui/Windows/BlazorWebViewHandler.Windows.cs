using System;
using System.IO;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Handlers;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WebView2Control>
	{
		private WebView2WebViewManager? _webviewManager;

		protected override WebView2Control CreateNativeView()
		{
			return new WebView2Control();
		}

		protected override void DisconnectHandler(WebView2Control nativeView)
		{
			if (_webviewManager != null)
			{
				// Dispose this component's contents and block on completion so that user-written disposal logic and
				// Blazor disposal logic will complete.
				_webviewManager?
					.DisposeAsync()
					.AsTask()
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

				_webviewManager = null;
			}
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
			if (NativeView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
			}

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

			// On Windows we don't use IFileProvider because it is sync-only, whereas in WinUI all the
			// file storage APIs are async-only. So instead we override HandleWebResourceRequest in
			// WinUIWebViewManager so that loading static assets is done entirely there.
			var mauiAssetFileProvider = new NullFileProvider();

			var jsComponents = new JSComponentConfigurationStore();
			_webviewManager = new WinUIWebViewManager(NativeView, new WinUIWebView2Wrapper(NativeView), Services!, MauiDispatcher.Instance, mauiAssetFileProvider, jsComponents, hostPageRelativePath, contentRootDir);
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
	}
}
