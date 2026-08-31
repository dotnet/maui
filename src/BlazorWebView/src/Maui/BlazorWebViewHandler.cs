using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
#if ANDROID
	[SupportedOSPlatform(BlazorWebView.AndroidSupportedOSPlatformVersion)]
#elif IOS
	[SupportedOSPlatform(BlazorWebView.iOSSupportedOSPlatformVersion)]
#elif MACCATALYST
	[SupportedOSPlatform(BlazorWebView.MacCatalystSupportedOSPlatformVersion)]
#endif
	public partial class BlazorWebViewHandler : IBlazorWebViewHandler
	{
		private const string UseBlockingDisposalSwitch = "BlazorWebView.UseBlockingDisposal";

		private static bool IsBlockingDisposalEnabled =>
			AppContext.TryGetSwitch(UseBlockingDisposalSwitch, out var enabled) && enabled;

		/// <summary>
		/// This field is part of MAUI infrastructure and is not intended for use by application code.
		/// </summary>
		public static PropertyMapper<IBlazorWebView, BlazorWebViewHandler> BlazorWebViewMapper = new(ViewMapper)
		{
			[nameof(IBlazorWebView.HostPage)] = MapHostPage,
			[nameof(IBlazorWebView.RootComponents)] = MapRootComponents,
#if WINDOWS
            [nameof(IView.FlowDirection)] = MapFlowDirection,
#endif

		};

		/// <summary>
		/// Initializes a new instance of <see cref="BlazorWebViewHandler"/> with default mappings.
		/// </summary>
		public BlazorWebViewHandler() : this(BlazorWebViewMapper)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="BlazorWebViewHandler"/> using the specified mappings.
		/// </summary>
		/// <param name="mapper">The property mappings.</param>
		public BlazorWebViewHandler(PropertyMapper? mapper) : base(mapper ?? BlazorWebViewMapper)
		{
		}

		IFileProvider IBlazorWebViewHandler.CreateFileProvider(string contentRootDir) =>
			CreateFileProvider(contentRootDir);

		Task<bool> IBlazorWebViewHandler.TryDispatchAsync(Action<IServiceProvider> workItem) =>
			TryDispatchAsync(workItem);

		internal BlazorWebViewDeveloperTools DeveloperTools => MauiContext!.Services.GetRequiredService<BlazorWebViewDeveloperTools>();

		/// <summary>
		/// Maps the <see cref="IBlazorWebView.HostPage"/> property to the specified handler.
		/// </summary>
		/// <param name="handler">The <see cref="BlazorWebViewHandler"/>.</param>
		/// <param name="webView">The <see cref="IBlazorWebView"/>.</param>
		public static void MapHostPage(BlazorWebViewHandler handler, IBlazorWebView webView)
		{
#if !(NETSTANDARD || !PLATFORM)
			handler.HostPage = webView.HostPage;
			handler.StartWebViewCoreOrRenderAppType();
#endif
		}

		/// <summary>
		/// Maps the <see cref="IBlazorWebView.RootComponents"/> property to the specified handler.
		/// </summary>
		/// <param name="handler">The <see cref="BlazorWebViewHandler"/>.</param>
		/// <param name="webView">The <see cref="IBlazorWebView"/>.</param>
		public static void MapRootComponents(BlazorWebViewHandler handler, IBlazorWebView webView)
		{
#if !(NETSTANDARD || !PLATFORM)
			handler.RootComponents = webView.RootComponents;
			handler.StartWebViewCoreOrRenderAppType();
#endif
		}

#if !(NETSTANDARD || !PLATFORM)
		private bool _appTypeRenderScheduled;

		// When AppType is set, the host document is rendered asynchronously on the MAUI dispatcher (the
		// same dispatcher the live components render on) BEFORE the web view core starts, so we never block
		// the UI thread. Startup resumes once the render completes. The legacy HostPage path is unchanged
		// and starts synchronously.
		private void StartWebViewCoreOrRenderAppType()
		{
			if (VirtualView is BlazorWebView { AppType: not null } appTypeView && !appTypeView.IsAppTypeRendered)
			{
				if (_appTypeRenderScheduled)
				{
					return;
				}

				_appTypeRenderScheduled = true;
				_ = RenderAppTypeThenStartAsync(appTypeView);
				return;
			}

			StartWebViewCoreIfPossible();
		}

		private async Task RenderAppTypeThenStartAsync(BlazorWebView appTypeView)
		{
			var dispatcher = MauiContext?.Services?.GetService<IDispatcher>();
			try
			{
				await appTypeView.EnsureAppTypeRenderedAsync(dispatcher);
			}
			catch (Exception ex)
			{
				MauiContext?.Services?.GetService<ILoggerFactory>()?
					.CreateLogger<BlazorWebViewHandler>()?
					.LogError(ex, "Failed to render the BlazorWebView AppType host document.");
			}
			finally
			{
				// Clear the in-flight guard so a later AppType reassignment (which resets the view's
				// rendered state) is able to schedule a fresh render.
				_appTypeRenderScheduled = false;
			}

			// Start the web view core on the UI dispatcher regardless of render outcome: on success it
			// serves the rendered host document; on failure the normal host-page 404 surfaces through the
			// platform pipeline instead of a silent, permanently blank web view.
			if (dispatcher is not null && dispatcher.IsDispatchRequired)
			{
				await dispatcher.DispatchAsync(StartWebViewCoreIfPossible);
			}
			else
			{
				StartWebViewCoreIfPossible();
			}
		}

		private string? HostPage { get; set; }

		internal void UrlLoading(UrlLoadingEventArgs args) =>
			VirtualView.UrlLoading(args);

		private RootComponentsCollection? _rootComponents;

		private RootComponentsCollection? RootComponents
		{
			get => _rootComponents;
			set
			{
				if (_rootComponents != null)
				{
					// Remove any previously-known root components and unhook events
					_rootComponents.Clear();
					_rootComponents.CollectionChanged -= OnRootComponentsCollectionChanged;
				}

				_rootComponents = value;

				if (_rootComponents != null)
				{
					// Add new root components and hook events
					if (_rootComponents.Count > 0 && _webviewManager != null)
					{
						_webviewManager.Dispatcher.AssertAccess();
						foreach (var component in _rootComponents)
						{
							_ = component.AddToWebViewManagerAsync(_webviewManager);
						}
					}
					_rootComponents.CollectionChanged += OnRootComponentsCollectionChanged;
				}
			}
		}

		private void OnRootComponentsCollectionChanged(object? sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs eventArgs)
		{
			// If we haven't initialized yet, this is a no-op
			if (_webviewManager != null)
			{
				// Dispatch because this is going to be async, and we want to catch any errors
				_ = _webviewManager.Dispatcher.InvokeAsync(async () =>
				{
					var newItems = eventArgs.NewItems!.Cast<RootComponent>();
					var oldItems = eventArgs.OldItems!.Cast<RootComponent>();

					foreach (var item in newItems.Except(oldItems))
					{
						await item.AddToWebViewManagerAsync(_webviewManager);
					}

					foreach (var item in oldItems.Except(newItems))
					{
						await item.RemoveFromWebViewManagerAsync(_webviewManager);
					}
				});
			}
		}
#endif
	}
}