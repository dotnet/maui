using System;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using WebKit;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable RS0016

namespace Microsoft.AspNetCore.Components.WebView.Gtk;

public class GtkWebViewManager : GtkSharp.BlazorWebKit.GtkWebViewManager
{

	#region CopiedFromWebView2WebViewManager

	protected readonly Action<UrlLoadingEventArgs>? _urlLoading;
	protected readonly Action<BlazorWebViewInitializingEventArgs>? _blazorWebViewInitializing;
	protected readonly Action<BlazorWebViewInitializedEventArgs>? _blazorWebViewInitialized;

	internal readonly BlazorWebViewDeveloperTools? _developerTools;

	internal void ApplyDefaultWebViewSettings(BlazorWebViewDeveloperTools? devTools)
	{
		if (devTools is not { })
			return;

		if (WebView is not { })
			return;

		WebView.Settings.EnableDeveloperExtras = devTools.Enabled;
		WebView.Settings.EnablePageCache = false;
		WebView.Settings.EnableOfflineWebApplicationCache = false;
	}

	private void NavigationStarting(object? sender, LoadChangedArgs args)
	{
		if (args.LoadEvent != LoadEvent.Started)
		{
			return;
		}

		if (WebView is not { })
			return;

		var argsUri = WebView.Uri;

		if (Uri.TryCreate(argsUri, UriKind.RelativeOrAbsolute, out var uri))
		{
			var callbackArgs = UrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, AppOriginUri);

			_urlLoading?.Invoke(callbackArgs);

			if (callbackArgs.UrlLoadingStrategy == UrlLoadingStrategy.OpenExternally)
			{
				LaunchUriInExternalBrowser(uri);
			}

			if (callbackArgs.UrlLoadingStrategy != UrlLoadingStrategy.OpenInWebView)
				WebView.StopLoading();
		}
	}

	#endregion

	/// <summary>
	/// Constructs an instance of <see cref="GtkWebViewManager"/>.
	/// </summary>
	/// <param name="webview">A <see cref="WebKit.WebView"/> to access platform-specific WebView2 APIs.</param>
	/// <param name="services">A service provider containing services to be used by this class and also by application code.</param>
	/// <param name="dispatcher">A <see cref="Dispatcher"/> instance that can marshal calls to the required thread or sync context.</param>
	/// <param name="fileProvider">Provides static content to the webview.</param>
	/// <param name="jsComponents">Describes configuration for adding, removing, and updating root components from JavaScript code.</param>
	/// <param name="contentRootRelativeToAppRoot">Path to the app's content root relative to the application root directory.</param>
	/// <param name="hostPagePathWithinFileProvider">Path to the host page within the <paramref name="fileProvider"/>.</param>
	/// <param name="urlLoading">Callback invoked when a url is about to load.</param>
	/// <param name="blazorWebViewInitializing">Callback invoked before the webview is initialized.</param>
	/// <param name="blazorWebViewInitialized">Callback invoked after the webview is initialized.</param>
	internal GtkWebViewManager(
		WebKit.WebView webview,
		IServiceProvider services,
		Dispatcher dispatcher,
		IFileProvider fileProvider,
		JSComponentConfigurationStore jsComponents,
		string contentRootRelativeToAppRoot,
		string hostPagePathWithinFileProvider,
		Action<UrlLoadingEventArgs> urlLoading,
		Action<BlazorWebViewInitializingEventArgs> blazorWebViewInitializing,
		Action<BlazorWebViewInitializedEventArgs> blazorWebViewInitialized)
		: base(services, dispatcher, AppOriginUri, fileProvider, jsComponents, hostPagePathWithinFileProvider)

	{
		ArgumentNullException.ThrowIfNull(webview);

		WebView = webview;
		_urlLoading = urlLoading;
		_blazorWebViewInitializing = blazorWebViewInitializing;
		_blazorWebViewInitialized = blazorWebViewInitialized;
		_developerTools = services.GetRequiredService<BlazorWebViewDeveloperTools>();

		Attach();
	}

	protected override void Attach()
	{
		_blazorWebViewInitializing?.Invoke(new BlazorWebViewInitializingEventArgs { });
		base.Attach();
		_blazorWebViewInitialized?.Invoke(new BlazorWebViewInitializedEventArgs { WebView = WebView });
		this.ApplyDefaultWebViewSettings(_developerTools);
	}

}