using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

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
		private string? _renderedHostPageHtml;

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
		/// </summary>
		public Type? AppType
		{
			get => _appType;
			set
			{
				_appType = value;

				// Provide a synthetic host page so the existing startup and relative-path logic flows
				// unchanged; the rendered document is overlaid onto the file provider at this path.
				if (value is not null && string.IsNullOrEmpty(HostPage))
				{
					HostPage = AppTypeHostPage;
				}
			}
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
			var platformFileProvider = ((BlazorWebViewHandler)(Handler!)).CreateFileProvider(contentRootDir);

			// Load the bundled static web assets manifest (if present) so that @Assets fingerprinting
			// and fingerprinted-route serving work. Absent (or on platforms without a readable file
			// provider), fingerprinting simply stays off and behaviour is unchanged.
			var manifest = StaticWebAssetsManifest.TryLoad(platformFileProvider);

			string? hostPageRelativePath = null;
			if (AppType is not null)
			{
				// When AppType is set, render the host document once. This also collects any interactive
				// components declared with a render mode and registers them so they attach to the live
				// document, and resolves @Assets using the manifest.
				EnsureAppTypeRendered(manifest?.Assets);
				hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);
			}

			// If there is nothing to add (no AppType document and no manifest), return the platform
			// provider unchanged to preserve existing behaviour exactly.
			if (hostPageRelativePath is null && manifest is null)
			{
				return platformFileProvider;
			}

			return new BlazorWebViewFileProvider(platformFileProvider, hostPageRelativePath, _renderedHostPageHtml, manifest);
		}

		[System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2072",
			Justification = "Blazor components referenced by AppType are preserved by the Razor SDK trimming roots, consistent with RootComponent.ComponentType.")]
		private void EnsureAppTypeRendered(ResourceAssetCollection? assets)
		{
			if (_appTypeRendered || AppType is null)
			{
				return;
			}

			_appTypeRendered = true;

			var services = Handler?.MauiContext?.Services
				?? throw new InvalidOperationException($"Cannot render {nameof(AppType)} because no service provider is available.");

			var result = HybridHostPageRenderer.Render(services, AppType, assets);
			_renderedHostPageHtml = result.Html;

			foreach (var registration in result.Registrations)
			{
				RootComponents.Add(new RootComponent
				{
					Selector = registration.Selector,
					ComponentType = registration.ComponentType,
				});
			}
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
			if (Handler is null)
			{
				return false;
			}

			return await ((BlazorWebViewHandler)(Handler!)).TryDispatchAsync(workItem);
		}

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
