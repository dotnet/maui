using System;
using System.Threading.Tasks;
using Android.Window;
using Android.Webkit;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using static global::Android.Views.ViewGroup;
using AWebView = global::Android.Webkit.WebView;
using Path = System.IO.Path;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, AWebView>
	{
		private WebViewClient? _webViewClient;
		private WebChromeClient? _webChromeClient;
		private AndroidWebKitWebViewManager? _webviewManager;
		internal AndroidWebKitWebViewManager? WebviewManager => _webviewManager;
		private AndroidLifecycle.OnBackPressed? _onBackPressedHandler;
		BlazorWebViewPredictiveBackCallback? _predictiveBackCallback;

		private ILogger? _logger;
		internal ILogger Logger => _logger ??= Services!.GetService<ILogger<BlazorWebViewHandler>>() ?? NullLogger<BlazorWebViewHandler>.Instance;

		/// <summary>
		/// Gets the concrete LifecycleEventService to access internal RemoveEvent method.
		/// RemoveEvent is internal because it's not part of the public ILifecycleEventService contract,
		/// but is needed for proper cleanup of lifecycle event handlers.
		/// </summary>
		private LifecycleEventService? TryGetLifecycleEventService()
		{
			var services = MauiContext?.Services;
			if (services != null)
			{
				return services.GetService<ILifecycleEventService>() as LifecycleEventService;
			}
			return null;
		}

		protected override AWebView CreatePlatformView()
		{
			Logger.CreatingAndroidWebkitWebView();

#pragma warning disable CA1416, CA1412, CA1422 // Validate platform compatibility
			var blazorAndroidWebView = new BlazorAndroidWebView(Context!)
			{
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
				LayoutParameters = new AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0)
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

		/// <summary>
		/// Connects the handler to the Android <see cref="AWebView"/> and registers platform-specific
		/// back navigation handling so that the WebView can consume back presses before the page is popped.
		/// </summary>
		/// <param name="platformView">The native Android <see cref="AWebView"/> instance associated with this handler.</param>
		/// <remarks>
		/// This override calls the base implementation and then registers an <see cref="AndroidLifecycle.OnBackPressed"/>
		/// lifecycle event handler. The handler checks <see cref="AWebView.CanGoBack"/> and, when possible, navigates
		/// back within the WebView instead of allowing the back press (or predictive back gesture on Android 13+)
		/// to propagate and pop the containing page.
		/// <para>
		/// When multiple BlazorWebView instances exist, the handler includes focus and visibility checks to ensure
		/// only the currently visible and focused WebView handles the back navigation, preventing conflicts between instances.
		/// </para>
		/// Inheritors that override this method should call the base implementation to preserve this back navigation
		/// behavior unless they intentionally replace it.
		/// </remarks>
		protected override void ConnectHandler(AWebView platformView)
		{
			base.ConnectHandler(platformView);

			// Register OnBackPressed lifecycle event handler to check WebView's back navigation
			// This ensures predictive back gesture (Android 13+) checks WebView.CanGoBack() before popping page
			var lifecycleService = TryGetLifecycleEventService();
			if (lifecycleService != null)
			{
				// Create a weak reference to avoid memory leaks
				var weakPlatformView = new WeakReference<AWebView>(platformView);

				AndroidLifecycle.OnBackPressed handler = (activity) =>
				{
					// Check if WebView is still alive, attached to window, and has focus
					// This prevents non-visible or unfocused BlazorWebView instances from
					// incorrectly intercepting back navigation when multiple instances exist
					if (weakPlatformView.TryGetTarget(out var webView) &&
						webView.IsAttachedToWindow &&
						webView.HasWindowFocus &&
						webView.CanGoBack())
					{
						webView.GoBack();
						return true; // Prevent back propagation - handled by WebView
					}

					return false; // Allow back propagation - let page be popped
				};

				// Register with lifecycle service - will be invoked by HandleBackNavigation in MauiAppCompatActivity
				lifecycleService.AddEvent(nameof(AndroidLifecycle.OnBackPressed), handler);
				_onBackPressedHandler = handler;
			}

			if (OperatingSystem.IsAndroidVersionAtLeast(33) && _predictiveBackCallback is null)
			{
				if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is not null)
				{
					_predictiveBackCallback = new BlazorWebViewPredictiveBackCallback(this);
					Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.OnBackInvokedDispatcher?.RegisterOnBackInvokedCallback(0, _predictiveBackCallback);
				}
			}
		}

		private const string AndroidFireAndForgetAsyncSwitch = "BlazorWebView.AndroidFireAndForgetAsync";

		protected override void DisconnectHandler(AWebView platformView)
		{
			if (OperatingSystem.IsAndroidVersionAtLeast(33) && _predictiveBackCallback is not null)
			{
				Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.OnBackInvokedDispatcher?.UnregisterOnBackInvokedCallback(_predictiveBackCallback);
				_predictiveBackCallback.Dispose();
				_predictiveBackCallback = null;
			}

			// Clean up lifecycle event handler to prevent memory leaks
			if (_onBackPressedHandler != null)
			{
				var lifecycleService = TryGetLifecycleEventService();
				if (lifecycleService != null)
				{
					lifecycleService.RemoveEvent(nameof(AndroidLifecycle.OnBackPressed), _onBackPressedHandler);
					_onBackPressedHandler = null;
				}
			}

			platformView.StopLoading();

			if (_webviewManager != null)
			{
				// Dispose this component's contents so that user-written disposal logic and Blazor disposal logic will complete.

				// Start the disposal...
				var disposalTask = _webviewManager?
					.DisposeAsync()
					.AsTask()!;

				// When determining whether to block on disposal, we respect the more specific AndroidFireAndForgetAsync switch
				// if specified. If not, we fall back to the general UseBlockingDisposal switch, defaulting to false.
				var shouldBlockOnDispose = AppContext.TryGetSwitch(AndroidFireAndForgetAsyncSwitch, out var enableFireAndForget)
					? !enableFireAndForget
					: IsBlockingDisposalEnabled;

				if (shouldBlockOnDispose)
				{
					// If the app is configured to block on dispose via an AppContext switch,
					// we'll synchronously wait for the disposal to complete. This can cause a deadlock.
					disposalTask
						.GetAwaiter()
						.GetResult();
				}
				else
				{
					// Otherwise, by default, we'll fire-and-forget the disposal task.
					disposalTask.FireAndForget(_logger);
				}

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

		sealed class BlazorWebViewPredictiveBackCallback : Java.Lang.Object, IOnBackInvokedCallback
		{
			WeakReference<BlazorWebViewHandler> _weakBlazorWebViewHandler;

			public BlazorWebViewPredictiveBackCallback(BlazorWebViewHandler handler)
			{
				_weakBlazorWebViewHandler = new WeakReference<BlazorWebViewHandler>(handler);
			}

			public void OnBackInvoked()
			{
				// KeyDown for Back button is handled in BlazorAndroidWebView.
				// Here we just need to check if it was handled there.
				// If not, we propagate the back press to the Activity's OnBackPressedDispatcher.
				if (_weakBlazorWebViewHandler is not null && _weakBlazorWebViewHandler.TryGetTarget(out var handler))
				{
					var webView = handler.PlatformView as BlazorAndroidWebView;
					if (webView is not null)
					{
						var wasBackNavigationHandled = webView.BackNavigationHandled;
						// reset immediately for next back event
						webView.BackNavigationHandled = false;

						if (!wasBackNavigationHandled)
						{
							if (webView.CanGoBack()) // If we can go back in WeView, Navigate back
							{
								webView.GoBack();
								return;
							}
							// Otherwise propagate back press to Activity
							(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity as AndroidX.AppCompat.App.AppCompatActivity)?.OnBackPressedDispatcher?.OnBackPressed();
						}
					}
				}
			}
		}
	}
}
