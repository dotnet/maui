using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Tizen.NUI;
using NWebView = Tizen.NUI.BaseComponents.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// The Tizen <see cref="ViewHandler"/> for <see cref="BlazorWebView"/>.
	/// </summary>
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, NWebView>
	{
		private const string BlazorWebViewIdentifier = "BlazorWebView:";
		private const string UserAgentHeaderKey = "User-Agent";
		private const string AppOrigin = "http://0.0.0.0/";
		private const string BlazorInitScript = @"
			window.__receiveMessageCallbacks = [];
			window.__dispatchMessageCallback = function(message) {
				window.__receiveMessageCallbacks.forEach(function(callback) { callback(message); });
			};
			window.external = {
				sendMessage: function(message) {
					window.BlazorHandler.postMessage(message);
				},
				receiveMessage: function(callback) {
					window.__receiveMessageCallbacks.push(callback);
				}
			};

			Blazor.start();

			(function () {
				window.onpageshow = function(event) {
					if (event.persisted) {
						window.location.reload();
					}
				};
			})();
		";

		static private Dictionary<string, WeakReference<BlazorWebViewHandler>> s_webviewHandlerTable = new(StringComparer.Ordinal);

		private TizenWebViewManager? _webviewManager;

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		/// <inheritdoc />
		protected override NWebView CreatePlatformView()
		{
			return new NWebView()
			{
				MouseEventsEnabled = true,
				KeyEventsEnabled = true,
			};
		}

		/// <inheritdoc />
		protected override void ConnectHandler(NWebView platformView)
		{
			platformView.PageLoadFinished += OnLoadFinished;
			platformView.Context.RegisterHttpRequestInterceptedCallback(OnRequestInterceptStaticCallback);
			platformView.AddJavaScriptMessageHandler("BlazorHandler", PostMessageFromJS);
			platformView.UserAgent += $" {BlazorWebViewIdentifier}{GetHashCode()}";
			s_webviewHandlerTable[GetHashCode().ToString()] = new WeakReference<BlazorWebViewHandler>(this);
		}

		/// <inheritdoc />
		protected override void DisconnectHandler(NWebView platformView)
		{
			platformView.PageLoadFinished -= OnLoadFinished;
			base.DisconnectHandler(platformView);
			s_webviewHandlerTable.Remove(GetHashCode().ToString());
		}


		private void PostMessageFromJS(string message)
		{
			_webviewManager!.MessageReceivedInternal(new Uri(PlatformView.Url), message);
		}

		private void OnLoadFinished(object? sender, WebViewPageLoadEventArgs e)
		{
			//FocusManager.Instance.SetCurrentFocusView(NativeView);
			var url = PlatformView.Url;

			if (url == AppOrigin)
				PlatformView.EvaluateJavaScript(BlazorInitScript);
		}

		private static void OnRequestInterceptStaticCallback(WebHttpRequestInterceptor interceptor)
		{
			if (interceptor.Headers.TryGetValue(UserAgentHeaderKey, out var agent))
			{
				var idx = agent.IndexOf(BlazorWebViewIdentifier);
				if (idx >= 0)
				{
					var webviewKey = agent.Substring(idx + BlazorWebViewIdentifier.Length);
					if (s_webviewHandlerTable.TryGetValue(webviewKey, out var weakHandler)
						&& weakHandler.TryGetTarget(out var handler))
					{
						handler.OnRequestInterceptCallback(interceptor);
						return;
					}
				}
			}
			interceptor.Ignore();
		}

		private void OnRequestInterceptCallback(WebHttpRequestInterceptor interceptor)
		{
			var url = interceptor.Url;
			if (url.StartsWith(AppOrigin))
			{
				var allowFallbackOnHostPage = url.EndsWith("/");
				url = QueryStringHelper.RemovePossibleQueryString(url);
				if (_webviewManager!.TryGetResponseContentInternal(url, allowFallbackOnHostPage, out var statusCode, out var statusMessage, out var content, out var headers))
				{
					var header = $"HTTP/1.0 200 OK\r\n";
					foreach (var item in headers)
					{
						header += $"{item.Key}:{item.Value}\r\n";
					}
					header += "\r\n";
					MemoryStream memstream = new MemoryStream();
					content.CopyTo(memstream);
					interceptor.SetResponse(header, memstream.ToArray());
					return;
				}
			}
			interceptor.Ignore();
		}

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet ||
				_webviewManager != null)
			{
				return;
			}
			if (PlatformView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without platform web view instance.");
			}

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = System.IO.Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = System.IO.Path.GetRelativePath(contentRootDir, HostPage!);

			var fileProvider = VirtualView.CreateFileProvider(contentRootDir);

			_webviewManager = new TizenWebViewManager(
				this,
				PlatformView,
				Services!,
				new MauiDispatcher(Services!.GetRequiredService<IDispatcher>()),
				fileProvider,
				VirtualView.JSComponents,
				contentRootDir,
				hostPageRelativePath);

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
					// Since the page isn't loaded yet, this will always complete synchronously
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}
			_webviewManager.Navigate(VirtualView.StartPath);
		}

		internal IFileProvider CreateFileProvider(string contentRootDir)
		{
			return new TizenMauiAssetFileProvider(contentRootDir);
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
