using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.WebView2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A <see cref="ViewHandler"/> for <see cref="BlazorWebView"/>.
	/// </summary>
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WebView2Control>
	{
		private WebView2WebViewManager? _webviewManager;

		/// <inheritdoc />
		protected override WebView2Control CreatePlatformView()
		{
			return new WebView2Control();
		}

		/// <inheritdoc />
		protected override void DisconnectHandler(WebView2Control platformView)
		{
			if (_webviewManager != null)
			{
				// Start the disposal...
				var disposalTask = _webviewManager?
					.DisposeAsync()
					.AsTask()!;

				if (IsBlockingDisposalEnabled)
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
					disposalTask.FireAndForget();
				}

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
			if (PlatformView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
			}

			var logger = Services!.GetService<ILogger<BlazorWebViewHandler>>() ?? NullLogger<BlazorWebViewHandler>.Instance;

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

			logger.CreatingFileProvider(contentRootDir, hostPageRelativePath);
			var fileProvider = VirtualView.CreateFileProvider(contentRootDir);

			_webviewManager = new WinUIWebViewManager(
				PlatformView,
				Services!,
				new MauiDispatcher(Services!.GetRequiredService<IDispatcher>()),
				fileProvider,
				VirtualView.JSComponents,
				contentRootDir,
				hostPageRelativePath,
				this,
				logger);

			StaticContentHotReloadManager.AttachToWebViewManagerIfEnabled(_webviewManager);

			if (RootComponents != null)
			{
				foreach (var rootComponent in RootComponents)
				{
					if (rootComponent is null)
					{
						continue;
					}

					logger.AddingRootComponent(rootComponent.ComponentType?.FullName ?? string.Empty, rootComponent.Selector ?? string.Empty, rootComponent.Parameters?.Count ?? 0);

					// Since the page isn't loaded yet, this will always complete synchronously
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}

			logger.StartingInitialNavigation(VirtualView.StartPath);
			_webviewManager.Navigate(VirtualView.StartPath);
		}

		internal static void MapFlowDirection(BlazorWebViewHandler handler, IView view)
		{
			// Explicitly do nothing here to override the base ViewHandler.MapFlowDirection behavior
			// This prevents the WebView2.FlowDirection from being set, avoiding content mirroring
		}

		internal IFileProvider CreateFileProvider(string contentRootDir)
		{
			// On WinUI we override HandleWebResourceRequest in WinUIWebViewManager so that loading static assets is done entirely there in an async manner.
			// This allows the code to be async because in WinUI all the file storage APIs are async-only, but IFileProvider is sync-only and we need to control
			// the precedence of which files are loaded from where.
			return new NullFileProvider();
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
