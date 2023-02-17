using System;
using System.Threading.Tasks;
using Android.Webkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;
using Path = System.IO.Path;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, AWebView>
	{
		private WebViewClient? _webViewClient;
		private WebChromeClient? _webChromeClient;
		private AndroidWebKitWebViewManager? _webviewManager;
		internal AndroidWebKitWebViewManager? WebviewManager => _webviewManager;

		private ILogger? _logger;
		internal ILogger Logger => _logger ??= Services!.GetService<ILogger<BlazorWebViewHandler>>() ?? NullLogger<BlazorWebViewHandler>.Instance;

		protected override AWebView CreatePlatformView()
		{
			Logger.CreatingAndroidWebkitWebView();

#pragma warning disable CA1416, CA1412, CA1422 // Validate platform compatibility
			var blazorAndroidWebView = new BlazorAndroidWebView(Context!)
			{
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
				LayoutParameters = new Android.Widget.AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0)
#pragma warning restore 618
			};
#pragma warning restore CA1416, CA1412, CA1422 // Validate platform compatibility

			BlazorAndroidWebView.SetWebContentsDebuggingEnabled(enabled: DeveloperTools.Enabled);

			if (blazorAndroidWebView.Settings != null)
			{
				// To allow overriding UrlLoadingStrategy.OpenInWebView and open links in browser with a _blank target
				blazorAndroidWebView.Settings.SetSupportMultipleWindows(true);

				blazorAndroidWebView.Settings.JavaScriptEnabled = true;
				blazorAndroidWebView.Settings.DomStorageEnabled = true;
			}

			_webViewClient = new WebKitWebViewClient(this);
			blazorAndroidWebView.SetWebViewClient(_webViewClient);

			_webChromeClient = new BlazorWebChromeClient();
			blazorAndroidWebView.SetWebChromeClient(_webChromeClient);

			Logger.CreatedAndroidWebkitWebView();

			return blazorAndroidWebView;
		}

		protected override void DisconnectHandler(AWebView platformView)
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

			Logger.CreatingFileProvider(contentRootDir, hostPageRelativePath);

			var fileProvider = VirtualView.CreateFileProvider(contentRootDir);

			_webviewManager = new AndroidWebKitWebViewManager(
				PlatformView,
				Services!,
				new MauiDispatcher(Services!.GetRequiredService<IDispatcher>()),
				fileProvider,
				VirtualView.JSComponents,
				contentRootDir,
				hostPageRelativePath,
				Logger);

			StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

			VirtualView.BlazorWebViewInitializing(new BlazorWebViewInitializingEventArgs());
			VirtualView.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs
			{
				WebView = PlatformView,
			});

			if (RootComponents != null)
			{
				foreach (var rootComponent in RootComponents)
				{
					Logger.AddingRootComponent(rootComponent.ComponentType?.FullName ?? string.Empty, rootComponent.Selector ?? string.Empty, rootComponent.Parameters?.Count ?? 0);

					// Since the page isn't loaded yet, this will always complete synchronously
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}

			Logger.StartingInitialNavigation(VirtualView.StartPath);
			_webviewManager.Navigate(VirtualView.StartPath);
		}

		internal IFileProvider CreateFileProvider(string contentRootDir)
		{
			return new AndroidMauiAssetFileProvider(Context.Assets, contentRootDir);
		}

		/// <summary>
		/// Calls the specified <paramref name="workItem"/> asynchronously and passes in the scoped services available to Razor components.
		/// </summary>
		/// <param name="workItem">The action to call.</param>
		/// <returns>Returns a <see cref="Task"/> representing <c>true</c> if the <paramref name="workItem"/> was called, or <c>false</c> if it was not called because Blazor is not currently running.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="workItem"/> is <c>null</c>.</exception>
		public virtual async Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
		{
			ArgumentNullException.ThrowIfNull(workItem);
			if (_webviewManager is null)
			{
				return false;
			}

			return await _webviewManager.TryDispatchAsync(workItem);
		}
	}
}
