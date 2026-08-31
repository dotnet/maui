using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A <see cref="View"/> that can render Blazor content.
	/// </summary>
#if ANDROID
	[SupportedOSPlatform(AndroidSupportedOSPlatformVersion)]
#elif IOS
	[SupportedOSPlatform(iOSSupportedOSPlatformVersion)]
#elif MACCATALYST
	[SupportedOSPlatform(MacCatalystSupportedOSPlatformVersion)]
#endif
	public partial class BlazorWebView : View, IBlazorWebView
	{
		// NOTE: keep these in *reasonably* in sync with:
		// * src\BlazorWebView\src\Maui\Microsoft.AspNetCore.Components.WebView.Maui.csproj
		// * src\Templates\src\templates\maui-blazor\MauiApp.1.csproj
		// * src\Templates\src\templates\maui-blazor-solution\MauiApp.1\MauiApp.1.csproj
		// * https://learn.microsoft.com/dotnet/maui/supported-platforms
		internal const string AndroidSupportedOSPlatformVersion = "android24.0";
		internal const string iOSSupportedOSPlatformVersion = "ios15.0";
		internal const string MacCatalystSupportedOSPlatformVersion = "maccatalyst15.0";

		internal static string AppHostAddress { get; } = HostAddressHelper.GetAppHostAddress();

		private readonly JSComponentConfigurationStore _jSComponents = new();

		/// <summary>
		/// Initializes a new instance of <see cref="BlazorWebView"/>.
		/// </summary>
		public BlazorWebView()
		{
			RootComponents = new RootComponentsCollection(_jSComponents);
		}

		/// <inheritdoc />
		JSComponentConfigurationStore IBlazorWebView.JSComponents => _jSComponents;

		/// <summary>
		/// Gets or sets the path to the HTML file to render.
		/// <para>This is an app relative path to the file such as <c>wwwroot\index.html</c></para>
		/// </summary>
		public string? HostPage { get; set; }

		/// <summary>
		/// The synthetic host page path used when <see cref="AppType"/> renders the host document.
		/// </summary>
		internal const string AppTypeHostPage = "wwwroot/index.html";

		private Type? _appType;
		private bool _appTypeRendered;
		private bool _syntheticHostPageApplied;
		private string? _renderedHostPageHtml;
		private StaticWebAssetsManifest? _manifest;
		private readonly List<RootComponent> _appTypeRootComponents = new();

		/// <summary>
		/// Gets or sets the type of a root component that renders the entire host HTML document (the
		/// hybrid equivalent of a Blazor Web App's <c>App.razor</c>).
		/// <para>
		/// When set, the component is statically rendered to produce the host page, so a physical
		/// <see cref="HostPage"/> file (such as <c>wwwroot/index.html</c>) is not required. Interactive
		/// components declared inside it with a render mode (for example
		/// <c>&lt;Routes @rendermode="InteractiveAuto" /&gt;</c> or
		/// <c>&lt;HeadOutlet @rendermode="InteractiveAuto" /&gt;</c>) are automatically attached to the
		/// live document, so an explicit <see cref="RootComponents"/> entry is not required either.
		/// </para>
		/// <para>
		/// Trimming/NativeAOT: when set from a XAML <c>{x:Type}</c> reference the type is preserved by the
		/// XAML compiler, and the interactive components it renders are preserved by the Razor SDK trimming
		/// roots. When assigned a runtime-computed type from code (for example
		/// <c>AppType = Type.GetType(name)</c>), the caller is responsible for ensuring that type and its
		/// members are preserved (for example with <c>[DynamicDependency]</c> or a trimming descriptor),
		/// otherwise its members may be trimmed and the host render can fail at runtime.
		/// </para>
		/// </summary>
		public Type? AppType
		{
			get => _appType;
			set
			{
				if (_appType == value)
				{
					return;
				}

				_appType = value;

				// Any change to AppType invalidates a previous render: drop the rendered host page and
				// remove the root components this class appended for it, so a subsequent start renders the
				// new value cleanly instead of ignoring it (non-null -> non-null) or registering a
				// duplicate selector (non-null -> null -> non-null).
				ResetAppTypeRenderState();

				if (value is not null)
				{
					// Provide a synthetic host page so the existing startup and relative-path logic
					// flows unchanged; the rendered document is overlaid onto the file provider at this
					// path. Only applied when the caller has not set an explicit HostPage.
					if (string.IsNullOrEmpty(HostPage))
					{
						HostPage = AppTypeHostPage;
						_syntheticHostPageApplied = true;
					}
				}
				else if (_syntheticHostPageApplied && HostPage == AppTypeHostPage)
				{
					// Clearing AppType: undo the synthetic host page, but only if it is still the one we
					// applied. A caller that set their own HostPage afterwards keeps it.
					HostPage = null;
					_syntheticHostPageApplied = false;
				}
			}
		}

		// Drops the rendered host page and removes any root components this class appended for the
		// current AppType, so the next render (if any) starts from a clean slate.
		private void ResetAppTypeRenderState()
		{
			_appTypeRendered = false;
			_renderedHostPageHtml = null;
			_manifest = null;

			if (_appTypeRootComponents.Count > 0)
			{
				foreach (var rootComponent in _appTypeRootComponents)
				{
					RootComponents.Remove(rootComponent);
				}

				_appTypeRootComponents.Clear();
			}
		}

		/// <summary>Gets whether the <see cref="AppType"/> host document has been rendered.</summary>
		internal bool IsAppTypeRendered => _appTypeRendered;

		/// <summary>
		/// Resolves a fingerprinted <c>@Assets</c> request route (for example <c>app.abc123.css</c>) to its
		/// physical asset file under the web root, using the manifest loaded for the current
		/// <see cref="AppType"/> render. Returns <c>false</c> (and echoes the input) when there is no manifest
		/// or the route needs no remapping.
		/// </summary>
		/// <remarks>
		/// On most platforms the wrapped <see cref="BlazorWebViewFileProvider"/> performs this remap, but the
		/// WinUI static-content pipeline serves files from the package folder directly (its
		/// <c>IFileProvider</c> is a <c>NullFileProvider</c>), so it calls this to apply the same remap and
		/// avoid 404s for fingerprinted assets.
		/// </remarks>
		internal bool TryResolveFingerprintedPath(string requestedPath, out string physicalPath)
		{
			if (_manifest is not null)
			{
				return _manifest.TryResolvePhysicalPath(requestedPath, out physicalPath);
			}

			physicalPath = requestedPath;
			return false;
		}

		/// <summary>
		/// Bindable property for <see cref="StartPath"/>.
		/// </summary>
		public static readonly BindableProperty StartPathProperty = BindableProperty.Create(nameof(StartPath), typeof(string), typeof(BlazorWebView), "/");

		/// <summary>
		/// Gets or sets the path for initial navigation within the Blazor navigation context when the Blazor component is finished loading.
		/// </summary>
		public string StartPath
		{
			get { return (string)GetValue(StartPathProperty); }
			set { SetValue(StartPathProperty, value); }
		}

		/// <inheritdoc cref="IBlazorWebView.RootComponents" />
		public RootComponentsCollection RootComponents { get; }

		/// <summary>
		/// Gets or sets a callback that determines the <c>Cache-Control</c> header value used for static content
		/// (such as images, fonts, or stylesheets) served from the app's content root.
		/// <para>
		/// By default no callback is set and all served content uses <c>no-cache, max-age=0, must-revalidate,
		/// no-store</c>, which disables WebView caching. Provide a callback to opt specific resources into caching,
		/// which can avoid repeated file reads and reduce image reload flicker when navigating between pages.
		/// Return <see langword="null"/> or an empty string from the callback to keep the default behavior for a
		/// given request. Cache entries remain subject to platform limits, expiration, and eviction.
		/// </para>
		/// <para>
		/// The callback is invoked from the platform's request handling, which may run on a background thread, so it
		/// must not access UI state directly. If the callback throws, the exception is logged and the request falls
		/// back to the default header.
		/// </para>
		/// </summary>
		public Func<BlazorWebViewStaticContentRequest, string?>? StaticContentCacheControlProvider { get; set; }

		/// <summary>
		/// Allows customizing how links are opened.
		/// By default, opens internal links in the webview and external links in an external app.
		/// </summary>
		public event EventHandler<UrlLoadingEventArgs>? UrlLoading;

		/// <summary>
		/// Raised before the web view is initialized. On some platforms this enables customizing the web view configuration.
		/// </summary>
		public event EventHandler<BlazorWebViewInitializingEventArgs>? BlazorWebViewInitializing;

		/// <summary>
		/// Raised after the web view is initialized but before any component has been rendered. The event arguments provide the instance of the platform-specific web view control.
		/// </summary>
		public event EventHandler<BlazorWebViewInitializedEventArgs>? BlazorWebViewInitialized;

		/// <summary>
		/// Raised when a web resource is requested. This event allows the application to intercept the request and provide a
		/// custom response.
		/// The event handler can set the <see cref="WebViewWebResourceRequestedEventArgs.Handled"/> property to true
		/// to indicate that the request has been handled and no further processing is needed. If the event handler does set this
		/// property to true, it must also call the
		/// <see cref="WebViewWebResourceRequestedEventArgs.SetResponse(int, string, System.Collections.Generic.IReadOnlyDictionary{string, string}?, System.IO.Stream?)"/>
		/// or <see cref="WebViewWebResourceRequestedEventArgs.SetResponse(int, string, System.Collections.Generic.IReadOnlyDictionary{string, string}?, System.Threading.Tasks.Task{System.IO.Stream?})"/>
		/// method to provide a response to the request.
		/// </summary>
		public event EventHandler<WebViewWebResourceRequestedEventArgs>? WebResourceRequested;

		/// <inheritdoc />
#if ANDROID
		[System.Runtime.Versioning.SupportedOSPlatform(AndroidSupportedOSPlatformVersion)]
#elif IOS
		[System.Runtime.Versioning.SupportedOSPlatform(iOSSupportedOSPlatformVersion)]
#elif MACCATALYST
		[System.Runtime.Versioning.SupportedOSPlatform(MacCatalystSupportedOSPlatformVersion)]
#endif
		public virtual IFileProvider CreateFileProvider(string contentRootDir)
		{
			// Call into the platform-specific code to get that platform's asset file provider
			var platformFileProvider = GetBlazorWebViewHandler().CreateFileProvider(contentRootDir);

			// Everything below is opt-in via AppType. For the legacy HostPage (index.html) path, return
			// the platform provider unchanged so existing behaviour - including the handler's own file
			// provider instance - is preserved exactly.
			if (AppType is null)
			{
				return platformFileProvider;
			}

			// The host document is normally rendered ahead of startup by the handler (asynchronously, on
			// the MAUI dispatcher). If it hasn't been - for example a direct CreateFileProvider call - fall
			// back to a synchronous render so this path always has content to serve.
			if (!_appTypeRendered)
			{
				EnsureAppTypeRendered();
			}

			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);
			return new BlazorWebViewFileProvider(platformFileProvider, hostPageRelativePath, _renderedHostPageHtml, _manifest);
		}

		/// <summary>
		/// Renders the <see cref="AppType"/> host document asynchronously (on the renderer's own dispatcher)
		/// and awaits it rather than blocking, then applies the result on the supplied MAUI dispatcher so the
		/// <see cref="RootComponents"/> mutation happens on the UI thread the <c>WebViewManager</c> reads
		/// from. Idempotent.
		/// </summary>
		internal async Task EnsureAppTypeRenderedAsync(IDispatcher? dispatcher)
		{
			if (_appTypeRendered || AppType is null)
			{
				return;
			}

			var services = GetAppTypeServices();

			// Load the manifest asynchronously so the (possibly genuinely async on Windows) app-package
			// read never blocks the UI thread.
			var logger = services.GetService<ILoggerFactory>()?.CreateLogger<BlazorWebView>();
			var manifest = await StaticWebAssetsManifest.TryLoadAsync(logger).ConfigureAwait(false);

			var result = await RenderAppTypeAsync(services, manifest).ConfigureAwait(false);

			// Apply the render (mutating RootComponents and the latch state) back on the MAUI dispatcher,
			// since RootComponents is read by the WebViewManager on the UI thread and the continuation
			// after the awaits above may be on a thread-pool thread.
			if (dispatcher is not null && dispatcher.IsDispatchRequired)
			{
				await dispatcher.DispatchAsync(() => ApplyAppTypeRender(result, manifest)).ConfigureAwait(false);
			}
			else
			{
				ApplyAppTypeRender(result, manifest);
			}
		}

		// See RenderAppTypeAsync for why the IL2072 suppression is required (AppType is XAML-reflection-set).
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2072",
			Justification = "AppType is set via XAML reflection so it cannot carry a DynamicallyAccessedMembers annotation; the component type is preserved by the XAML compiler ({x:Type}) and the Razor SDK trimming roots.")]
		private void EnsureAppTypeRendered()
		{
			if (_appTypeRendered || AppType is null)
			{
				return;
			}

			var services = GetAppTypeServices();
			var logger = services.GetService<ILoggerFactory>()?.CreateLogger<BlazorWebView>();
			var manifest = StaticWebAssetsManifest.TryLoad(logger);
			var result = HybridHostPageRenderer.Render(services, AppType, manifest?.Assets);
			ApplyAppTypeRender(result, manifest);
		}

		private IServiceProvider GetAppTypeServices() =>
			Handler?.MauiContext?.Services
				?? throw new InvalidOperationException($"Cannot render {nameof(AppType)} because no service provider is available.");

		// IL2072: AppType flows into HybridHostPageRenderer.RenderAsync's [DynamicallyAccessedMembers(All)]
		// parameter. AppType cannot itself be annotated: it is a public property set by XAML via
		// reflection (AppType="{x:Type components:App}"), and a DynamicallyAccessedMembers requirement on
		// a reflection-set property/parameter is not satisfiable by the trimmer (it produces IL2111/IL2114
		// instead). The assigned component type is preserved regardless: a {x:Type} reference is rooted by
		// the XAML compiler, and the interactive components it renders are rooted by the Razor SDK's
		// trimming roots (@rendermode / routable assembly), so their members survive trimming.
		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2072",
			Justification = "AppType is set via XAML reflection so it cannot carry a DynamicallyAccessedMembers annotation; the component type is preserved by the XAML compiler ({x:Type}) and the Razor SDK trimming roots.")]
		private Task<HybridHostPageResult> RenderAppTypeAsync(IServiceProvider services, StaticWebAssetsManifest? manifest)
			=> HybridHostPageRenderer.RenderAsync(services, AppType!, manifest?.Assets);

		private void ApplyAppTypeRender(HybridHostPageResult result, StaticWebAssetsManifest? manifest)
		{
			_renderedHostPageHtml = result.Html;
			_manifest = manifest;

			foreach (var registration in result.Registrations)
			{
				var rootComponent = new RootComponent
				{
					Selector = registration.Selector,
					ComponentType = registration.ComponentType,
				};

				RootComponents.Add(rootComponent);

				// Track what we added so a later AppType change can remove exactly these entries.
				_appTypeRootComponents.Add(rootComponent);
			}

			// Only latch success after the render and registration complete. If rendering throws (an
			// invalid AppType, a failing OnInitializedAsync, a missing service), the flag stays false so
			// a later handler reconnect retries instead of permanently serving a blank host page.
			_appTypeRendered = true;
		}

		/// <summary>
		/// Calls the specified <paramref name="workItem"/> asynchronously and passes in the scoped services available to Razor components.
		/// </summary>
		/// <param name="workItem">The action to call.</param>
		/// <returns>Returns a <see cref="Task"/> representing <c>true</c> if the <paramref name="workItem"/> was called, or <c>false</c> if it was not called because Blazor is not currently running.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="workItem"/> is <c>null</c>.</exception>
#if ANDROID
		[System.Runtime.Versioning.SupportedOSPlatform(AndroidSupportedOSPlatformVersion)]
#elif IOS
		[System.Runtime.Versioning.SupportedOSPlatform(iOSSupportedOSPlatformVersion)]
#elif MACCATALYST
		[System.Runtime.Versioning.SupportedOSPlatform(MacCatalystSupportedOSPlatformVersion)]
#endif
		public virtual async Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
		{
			ArgumentNullException.ThrowIfNull(workItem);
			var handler = Handler;
			if (handler is null)
			{
				return false;
			}

			return await GetBlazorWebViewHandler(handler).TryDispatchAsync(workItem);
		}

		private IBlazorWebViewHandler GetBlazorWebViewHandler()
		{
			var handler = Handler;
			if (handler is null)
			{
				throw new InvalidOperationException(
					$"{nameof(BlazorWebView)} must be connected to a handler before this operation can be performed.");
			}

			return GetBlazorWebViewHandler(handler);
		}

		private static IBlazorWebViewHandler GetBlazorWebViewHandler(IViewHandler handler) =>
			handler as IBlazorWebViewHandler ??
				throw new InvalidOperationException(
					$"The handler type '{handler.GetType().FullName}' must implement {nameof(IBlazorWebViewHandler)}.");

		/// <inheritdoc />
		void IBlazorWebView.UrlLoading(UrlLoadingEventArgs args) =>
			UrlLoading?.Invoke(this, args);

		/// <inheritdoc />
		void IBlazorWebView.BlazorWebViewInitializing(BlazorWebViewInitializingEventArgs args) =>
			BlazorWebViewInitializing?.Invoke(this, args);

		/// <inheritdoc />
		void IBlazorWebView.BlazorWebViewInitialized(BlazorWebViewInitializedEventArgs args) =>
			BlazorWebViewInitialized?.Invoke(this, args);

		/// <inheritdoc />
		bool IWebRequestInterceptingWebView.WebResourceRequested(WebResourceRequestedEventArgs args)
		{
			var platformArgs = new PlatformWebViewWebResourceRequestedEventArgs(args);
			var e = new WebViewWebResourceRequestedEventArgs(platformArgs);
			WebResourceRequested?.Invoke(this, e);
			return e.Handled;
		}
	}
}
