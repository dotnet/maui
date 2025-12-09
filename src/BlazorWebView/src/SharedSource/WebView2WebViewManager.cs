// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !WEBVIEW2_WINFORMS && !WEBVIEW2_WPF && !WEBVIEW2_MAUI
#error Must specify which WebView2 is targeted
#endif

#if WINDOWS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
#if WEBVIEW2_WINFORMS
using System.Diagnostics;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2;
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;
using System.Reflection;
#elif WEBVIEW2_WPF
using System.Diagnostics;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2CompositionControl;
using System.Reflection;
#elif WEBVIEW2_MAUI
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
using Launcher = Windows.System.Launcher;
#endif

namespace Microsoft.AspNetCore.Components.WebView.WebView2
{
	/// <summary>
	/// An implementation of <see cref="WebViewManager"/> that uses the Edge WebView2 browser control
	/// to render web content.
	/// </summary>
	internal class WebView2WebViewManager : WebViewManager
	{
		// Using an IP address means that WebView2 doesn't wait for any DNS resolution,
		// making it substantially faster. Note that this isn't real HTTP traffic, since
		// we intercept all the requests within this origin.
		internal static readonly string AppHostAddress = HostAddressHelper.GetAppHostAddress();

		/// <summary>
		/// Gets the application's base URI. Defaults to <c>https://0.0.0.1/</c>.
		/// </summary>
		protected static readonly string AppOrigin = $"https://{AppHostAddress}/";

		internal static readonly Uri AppOriginUri = new(AppOrigin);
		private readonly ILogger _logger;
		private readonly WebView2Control _webview;
		private readonly Task<bool> _webviewReadyTask;
		private readonly string _contentRootRelativeToAppRoot;

#if WEBVIEW2_WINFORMS || WEBVIEW2_WPF
		private protected CoreWebView2Environment? _coreWebView2Environment;
		private readonly Action<UrlLoadingEventArgs> _urlLoading;
		private readonly Action<BlazorWebViewInitializingEventArgs> _blazorWebViewInitializing;
		private readonly Action<BlazorWebViewInitializedEventArgs> _blazorWebViewInitialized;
		private readonly BlazorWebViewDeveloperTools _developerTools;

		/// <summary>
		/// Constructs an instance of <see cref="WebView2WebViewManager"/>.
		/// </summary>
		/// <param name="webview">A <see cref="WebView2Control"/> to access platform-specific WebView2 APIs.</param>
		/// <param name="services">A service provider containing services to be used by this class and also by application code.</param>
		/// <param name="dispatcher">A <see cref="Dispatcher"/> instance that can marshal calls to the required thread or sync context.</param>
		/// <param name="fileProvider">Provides static content to the webview.</param>
		/// <param name="jsComponents">Describes configuration for adding, removing, and updating root components from JavaScript code.</param>
		/// <param name="contentRootRelativeToAppRoot">Path to the app's content root relative to the application root directory.</param>
		/// <param name="hostPagePathWithinFileProvider">Path to the host page within the <paramref name="fileProvider"/>.</param>
		/// <param name="urlLoading">Callback invoked when a url is about to load.</param>
		/// <param name="blazorWebViewInitializing">Callback invoked before the webview is initialized.</param>
		/// <param name="blazorWebViewInitialized">Callback invoked after the webview is initialized.</param>
		/// <param name="logger">Logger to send log messages to.</param>
		internal WebView2WebViewManager(
			WebView2Control webview,
			IServiceProvider services,
			Dispatcher dispatcher,
			IFileProvider fileProvider,
			JSComponentConfigurationStore jsComponents,
			string contentRootRelativeToAppRoot,
			string hostPagePathWithinFileProvider,
			Action<UrlLoadingEventArgs> urlLoading,
			Action<BlazorWebViewInitializingEventArgs> blazorWebViewInitializing,
			Action<BlazorWebViewInitializedEventArgs> blazorWebViewInitialized,
			ILogger logger)
			: base(services, dispatcher, AppOriginUri, fileProvider, jsComponents, hostPagePathWithinFileProvider)

		{
			ArgumentNullException.ThrowIfNull(webview);

#if WEBVIEW2_WINFORMS
			if (services.GetService<WindowsFormsBlazorMarkerService>() is null)
			{
				throw new InvalidOperationException(
					"Unable to find the required services. " +
					$"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(BlazorWebViewServiceCollectionExtensions.AddWindowsFormsBlazorWebView)}' in the application startup code.");
			}
#elif WEBVIEW2_WPF
			if (services.GetService<WpfBlazorMarkerService>() is null)
			{
				throw new InvalidOperationException(
					"Unable to find the required services. " +
					$"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(BlazorWebViewServiceCollectionExtensions.AddWpfBlazorWebView)}' in the application startup code.");
			}
#endif

			_logger = logger;
			_webview = webview;
			_urlLoading = urlLoading;
			_blazorWebViewInitializing = blazorWebViewInitializing;
			_blazorWebViewInitialized = blazorWebViewInitialized;
			_developerTools = services.GetRequiredService<BlazorWebViewDeveloperTools>();
			_contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;

			// Unfortunately the CoreWebView2 can only be instantiated asynchronously.
			// We want the external API to behave as if initalization is synchronous,
			// so keep track of a task we can await during LoadUri.
			_webviewReadyTask = TryInitializeWebView2();
		}
#elif WEBVIEW2_MAUI
		private protected CoreWebView2Environment? _coreWebView2Environment;
		private readonly BlazorWebViewHandler _blazorWebViewHandler;

		/// <summary>
		/// Constructs an instance of <see cref="WebView2WebViewManager"/>.
		/// </summary>
		/// <param name="webview">A <see cref="WebView2Control"/> to access platform-specific WebView2 APIs.</param>
		/// <param name="services">A service provider containing services to be used by this class and also by application code.</param>
		/// <param name="dispatcher">A <see cref="Dispatcher"/> instance that can marshal calls to the required thread or sync context.</param>
		/// <param name="fileProvider">Provides static content to the webview.</param>
		/// <param name="jsComponents">Describes configuration for adding, removing, and updating root components from JavaScript code.</param>
		/// <param name="contentRootRelativeToAppRoot">Path to the app's content root relative to the application root directory.</param>
		/// <param name="hostPagePathWithinFileProvider">Path to the host page within the <paramref name="fileProvider"/>.</param>
		/// <param name="blazorWebViewHandler">The <see cref="BlazorWebViewHandler" />.</param>
		/// <param name="logger">Logger to send log messages to.</param>
		internal WebView2WebViewManager(
			WebView2Control webview,
			IServiceProvider services,
			Dispatcher dispatcher,
			IFileProvider fileProvider,
			JSComponentConfigurationStore jsComponents,
			string contentRootRelativeToAppRoot,
			string hostPagePathWithinFileProvider,
			BlazorWebViewHandler blazorWebViewHandler,
			ILogger logger
		)
			: base(services, dispatcher, AppOriginUri, fileProvider, jsComponents, hostPagePathWithinFileProvider)
		{
			ArgumentNullException.ThrowIfNull(webview);

			if (services.GetService<MauiBlazorMarkerService>() is null)
			{
				throw new InvalidOperationException(
					"Unable to find the required services. " +
					$"Please add all the required services by calling '{nameof(IServiceCollection)}.{nameof(BlazorWebViewServiceCollectionExtensions.AddMauiBlazorWebView)}' in the application startup code.");
			}

			_logger = logger;
			_webview = webview;
			_blazorWebViewHandler = blazorWebViewHandler;
			_contentRootRelativeToAppRoot = contentRootRelativeToAppRoot;

			// Unfortunately the CoreWebView2 can only be instantiated asynchronously.
			// We want the external API to behave as if initalization is synchronous,
			// so keep track of a task we can await during LoadUri.
			_webviewReadyTask = TryInitializeWebView2();
		}
#endif

		/// <inheritdoc />
		protected override void NavigateCore(Uri absoluteUri)
		{
			_ = Dispatcher.InvokeAsync(async () =>
			{
				var isWebviewInitialized = await _webviewReadyTask;

				if (isWebviewInitialized)
				{
					_logger.NavigatingToUri(absoluteUri);
					_webview.Source = absoluteUri;
				}
			});
		}

		/// <inheritdoc />
		protected override void SendMessage(string message)
			=> _webview.CoreWebView2.PostWebMessageAsString(message);

		private async Task<bool> TryInitializeWebView2()
		{
			var args = new BlazorWebViewInitializingEventArgs();
#if WEBVIEW2_MAUI
			_blazorWebViewHandler.VirtualView.BlazorWebViewInitializing(args);

			try
			{
				_coreWebView2Environment = await CoreWebView2Environment.CreateWithOptionsAsync(
					browserExecutableFolder: args.BrowserExecutableFolder,
					userDataFolder: args.UserDataFolder,
					options: args.EnvironmentOptions)
					.AsTask()
					.ConfigureAwait(true);
			}
			catch (FileNotFoundException)
			{
				_logger.FailedToCreateWebView2Environment();

				// This method needs to be invoked even if the WebView2 Runtime is not installed,
				// since it is reponsible for creating the warning label and WebView2 Runtime
				// download link.
				await _webview.EnsureCoreWebView2Async();
				return false;
			}

			_logger.StartingWebView2();
			await _webview.EnsureCoreWebView2Async();
			_logger.StartedWebView2();

			var developerTools = _blazorWebViewHandler.DeveloperTools;
#elif WEBVIEW2_WINFORMS || WEBVIEW2_WPF
			_blazorWebViewInitializing?.Invoke(args);
			var userDataFolder = args.UserDataFolder ?? GetWebView2UserDataFolder();
			_coreWebView2Environment = await CoreWebView2Environment.CreateAsync(
				browserExecutableFolder: args.BrowserExecutableFolder,
				userDataFolder: userDataFolder,
				options: args.EnvironmentOptions)
			.ConfigureAwait(true);

			_logger.StartingWebView2();
			await _webview.EnsureCoreWebView2Async(_coreWebView2Environment);
			_logger.StartedWebView2();

			var developerTools = _developerTools;
#endif

			ApplyDefaultWebViewSettings(developerTools);

#if WEBVIEW2_MAUI
			_blazorWebViewHandler.VirtualView.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs
			{
				WebView = _webview,
			});
#elif WEBVIEW2_WINFORMS || WEBVIEW2_WPF
			_blazorWebViewInitialized?.Invoke(new BlazorWebViewInitializedEventArgs
			{
				WebView = _webview,
			});
#endif

			_webview.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

			_webview.CoreWebView2.WebResourceRequested += async (s, eventArgs) =>
			{
				await HandleWebResourceRequest(eventArgs);
			};

			_webview.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
			_webview.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

			// The code inside blazor.webview.js is meant to be agnostic to specific webview technologies,
			// so the following is an adaptor from blazor.webview.js conventions to WebView2 APIs
			await _webview.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
				window.external = {
					sendMessage: message => {
						window.chrome.webview.postMessage(message);
					},
					receiveMessage: callback => {
						window.chrome.webview.addEventListener('message', e => callback(e.data));
					}
				};
			")
#if WEBVIEW2_MAUI
				.AsTask()
#endif
				.ConfigureAwait(true);

			QueueBlazorStart();

			_webview.CoreWebView2.WebMessageReceived += (s, e) => MessageReceived(new Uri(e.Source), e.TryGetWebMessageAsString());

			return true;
		}

		/// <summary>
		/// Handles outbound URL requests.
		/// </summary>
		/// <param name="eventArgs">The <see cref="CoreWebView2WebResourceRequestedEventArgs"/>.</param>
		protected virtual Task HandleWebResourceRequest(CoreWebView2WebResourceRequestedEventArgs eventArgs)
		{
#if WEBVIEW2_WINFORMS || WEBVIEW2_WPF
			// Unlike server-side code, we get told exactly why the browser is making the request,
			// so we can be smarter about fallback. We can ensure that 'fetch' requests never result
			// in fallback, for example.
			var allowFallbackOnHostPage =
				eventArgs.ResourceContext == CoreWebView2WebResourceContext.Document ||
				eventArgs.ResourceContext == CoreWebView2WebResourceContext.Other; // e.g., dev tools requesting page source

			var requestUri = QueryStringHelper.RemovePossibleQueryString(eventArgs.Request.Uri);

			_logger.HandlingWebRequest(requestUri);

			if (TryGetResponseContent(requestUri, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
			{
				StaticContentHotReloadManager.TryReplaceResponseContent(_contentRootRelativeToAppRoot, requestUri, ref statusCode, ref content, headers);

				var headerString = GetHeaderString(headers);

				var autoCloseStream = new AutoCloseOnReadCompleteStream(content);

				_logger.ResponseContentBeingSent(requestUri, statusCode);

				eventArgs.Response = _coreWebView2Environment!.CreateWebResourceResponse(autoCloseStream, statusCode, statusMessage, headerString);
			}
			else
			{
				_logger.ResponseContentNotFound(requestUri);
			}
#elif WEBVIEW2_MAUI
			// No-op here because all the work is done in the derived WinUIWebViewManager
#endif
			return Task.CompletedTask;
		}

		/// <summary>
		/// Override this method to queue a call to Blazor.start(). Not all platforms require this.
		/// </summary>
		protected virtual void QueueBlazorStart()
		{
		}

		private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs args)
		{
			if (Uri.TryCreate(args.Uri, UriKind.RelativeOrAbsolute, out var uri))
			{
				var callbackArgs = UrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(uri, AppOriginUri);

#if WEBVIEW2_WINFORMS || WEBVIEW2_WPF
				_urlLoading?.Invoke(callbackArgs);
#elif WEBVIEW2_MAUI
				_blazorWebViewHandler.UrlLoading(callbackArgs);
#endif
				_logger.NavigationEvent(uri, callbackArgs.UrlLoadingStrategy);

				if (callbackArgs.UrlLoadingStrategy == UrlLoadingStrategy.OpenExternally)
				{
					LaunchUriInExternalBrowser(uri);
				}

				args.Cancel = callbackArgs.UrlLoadingStrategy != UrlLoadingStrategy.OpenInWebView;
			}
		}

		private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs args)
		{
			// Intercept _blank target <a> tags to always open in device browser.
			// The ExternalLinkCallback is not invoked.
			if (Uri.TryCreate(args.Uri, UriKind.RelativeOrAbsolute, out var uri))
			{
				LaunchUriInExternalBrowser(uri);
				args.Handled = true;
			}
		}

		private void LaunchUriInExternalBrowser(Uri uri)
		{
			_logger.LaunchExternalBrowser(uri);

#if WEBVIEW2_WINFORMS || WEBVIEW2_WPF
			using (var launchBrowser = new Process())
			{
				launchBrowser.StartInfo.UseShellExecute = true;
				launchBrowser.StartInfo.FileName = uri.ToString();
				launchBrowser.Start();
			}
#elif WEBVIEW2_MAUI
			_ = Launcher.LaunchUriAsync(uri);
#endif
		}

		private protected static string GetHeaderString(IDictionary<string, string> headers) =>
			string.Join(Environment.NewLine, headers.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

		private void ApplyDefaultWebViewSettings(BlazorWebViewDeveloperTools devTools)
		{
			_webview.CoreWebView2.Settings.AreDevToolsEnabled = devTools.Enabled;

			// Desktop applications typically don't want the default web browser context menu
			_webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

			// Desktop applications almost never want to show a URL preview when hovering over a link
			_webview.CoreWebView2.Settings.IsStatusBarEnabled = false;
		}

#if WEBVIEW2_WINFORMS || WEBVIEW2_WPF
		private static string? GetWebView2UserDataFolder()
		{
			if (Assembly.GetEntryAssembly() is { } mainAssembly)
			{
				// In case the application is running from a non-writable location (e.g., program files if you're not running
				// elevated), use our own convention of %LocalAppData%\YourApplicationName.WebView2.
				// We may be able to remove this if https://github.com/MicrosoftEdge/WebView2Feedback/issues/297 is fixed.
				var applicationName = mainAssembly.GetName().Name;
				var result = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					$"{applicationName}.WebView2");

				return result;
			}

			return null;
		}
#endif
	}
}

#endif
