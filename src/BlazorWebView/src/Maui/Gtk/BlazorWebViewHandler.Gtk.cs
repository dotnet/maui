using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView.Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using WebViewWidget = WebKit.WebView;

#pragma warning disable RS0016

namespace Microsoft.AspNetCore.Components.WebView.Maui
{

	/// <summary>
	/// A <see cref="ViewHandler"/> for <see cref="BlazorWebView"/>.
	/// </summary>
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, WebViewWidget>
	{

		private WebViewManager? _webviewManager;

		/// <inheritdoc />
		protected override WebViewWidget CreatePlatformView()
		{
			var native = new WebViewWidget();

			return native;
		}

		/// <inheritdoc />
		public virtual IFileProvider CreateFileProvider(string contentRootDir)
		{
			if (Directory.Exists(contentRootDir))
			{
				// Typical case after publishing, or if you're copying content to the bin dir in development for some nonstandard reason
				return new PhysicalFileProvider(contentRootDir);
			}

			// Typical case in development, as the files come from Microsoft.AspNetCore.Components.WebView.StaticContentProvider
			// instead and aren't copied to the bin dir
			return new NullFileProvider();
		}

		/// <inheritdoc />
		protected override void DisconnectHandler(WebViewWidget platformView)
		{
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
		}

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet || _webviewManager != null)
			{
				return;
			}

			if (PlatformView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without platform web view instance.");
			}

			var logger = Services!.GetService<ILogger<BlazorWebViewHandler>>() ?? NullLogger<BlazorWebViewHandler>.Instance;

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var appRootDir = Environment.CurrentDirectory;
			var hostPageFullPath = Path.GetFullPath(Path.Combine(appRootDir, HostPage!)); // HostPage is nonnull because RequiredStartupPropertiesSet is checked above
			var contentRootDirFullPath = Path.GetDirectoryName(hostPageFullPath)!;
			var contentRootRelativePath = Path.GetRelativePath(appRootDir, contentRootDirFullPath);
			var hostPageRelativePath = Path.GetRelativePath(contentRootDirFullPath, hostPageFullPath);
			
			logger.CreatingFileProvider(contentRootDirFullPath, hostPageRelativePath);

			var fileProvider = VirtualView.CreateFileProvider(contentRootDirFullPath);

			_webviewManager = new GtkWebViewManager(
				PlatformView,
				Services!,
				new MauiDispatcher(Services!.GetRequiredService<IDispatcher>()),
				fileProvider,
				VirtualView.JSComponents,
				contentRootRelativePath,
				hostPageRelativePath,
				UrlLoading,
				(args) => VirtualView.BlazorWebViewInitializing(args),
				(args) => VirtualView.BlazorWebViewInitialized(args)
			);

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

		bool RequiredStartupPropertiesSet =>
			HostPage != null &&
			Services != null;

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