using System;
using Android.Graphics;
using Android.Webkit;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Platform
{
	public class MauiWebViewClient : WebViewClient
	{
		WebNavigationResult _navigationResult;
		WeakReference<WebViewHandler?> _handler;
		string? _lastUrlNavigatedCancel;

		public MauiWebViewClient(WebViewHandler handler)
		{
			_ = handler ?? throw new ArgumentNullException(nameof(handler));

			_handler = new WeakReference<WebViewHandler?>(handler);
			_navigationResult = WebNavigationResult.Success;
		}
		public override bool ShouldOverrideUrlLoading(WebView? view, IWebResourceRequest? request)
			=> NavigatingCanceled(request?.Url?.ToString());

		public override WebResourceResponse? ShouldInterceptRequest(WebView? view, IWebResourceRequest? request)
		{
			// Block sub-resource requests (iframes, images, scripts) to disallowed domains
			if (request?.Url is Android.Net.Uri url && _handler.TryGetTarget(out var handler) && handler?.VirtualView is not null)
			{
				var urlString = url.ToString();
				if (!string.IsNullOrEmpty(urlString) && !WebViewDomainAllowlist.IsUrlAllowed(urlString, handler.VirtualView))
				{
					// Return an empty response to block the request
					return new WebResourceResponse("text/plain", "UTF-8", null);
				}
			}

			return base.ShouldInterceptRequest(view, request);
		}

		public override void OnPageStarted(WebView? view, string? url, Bitmap? favicon)
		{
			if (!_handler.TryGetTarget(out var handler) || handler.VirtualView == null)
				return;

			if (!string.IsNullOrWhiteSpace(url))
			{
				handler.SyncPlatformCookiesToVirtualView(url);
			}

			var cancel = false;

			if (!GetValidUrl(url).Equals(handler.UrlCanceled, StringComparison.OrdinalIgnoreCase))
				cancel = NavigatingCanceled(url);

			handler.UrlCanceled = null;

			if (cancel)
			{
				_navigationResult = WebNavigationResult.Cancel;
				view?.StopLoading();
			}
			else
			{
				_navigationResult = WebNavigationResult.Success;
				base.OnPageStarted(view, url, favicon);
			}
		}

		public override void OnPageFinished(WebView? view, string? url)
		{
			if (!_handler.TryGetTarget(out var handler) || handler.VirtualView == null || string.IsNullOrWhiteSpace(url))
				return;

			bool navigate = _navigationResult != WebNavigationResult.Failure || !GetValidUrl(url).Equals(_lastUrlNavigatedCancel, StringComparison.OrdinalIgnoreCase);
			_lastUrlNavigatedCancel = _navigationResult == WebNavigationResult.Cancel ? url : null;

			// Skip Navigated event for about:blank to prevent unwanted events when Source is null
			if (navigate && !IsBlankNavigation(url))
				handler.VirtualView.Navigated(handler.CurrentNavigationEvent, GetValidUrl(url), _navigationResult);

			handler.SyncPlatformCookiesToVirtualView(url);

			handler?.PlatformView.UpdateCanGoBackForward(handler.VirtualView);

			base.OnPageFinished(view, url);
		}

		[System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
		public override void OnReceivedError(WebView? view, IWebResourceRequest? request, WebResourceError? error)
		{
			if (request != null && _handler.TryGetTarget(out var handler) && request.Url?.ToString() == handler?.PlatformView.Url)
			{
				_navigationResult = WebNavigationResult.Failure;

				if (error?.ErrorCode == ClientError.Timeout)
					_navigationResult = WebNavigationResult.Timeout;
			}

			base.OnReceivedError(view, request, error);
		}

		// The render process was observed to crash or killed by the system.
		[System.Runtime.Versioning.SupportedOSPlatform("android26.0")]
		public override bool OnRenderProcessGone(WebView? view, RenderProcessGoneDetail? detail)
		{
			if (_handler.TryGetTarget(out var handler))
			{
				handler.VirtualView.ProcessTerminated(new WebProcessTerminatedEventArgs(view, detail));
			}

			return base.OnRenderProcessGone(view, detail);
		}

		bool NavigatingCanceled(string? url) =>
			!_handler.TryGetTarget(out var handler) || handler.NavigatingCanceled(url);

		static bool IsBlankNavigation(string? url)
		{
			// Null/empty URLs are handled by the early return in OnPageFinished,
			// so we only need to check for the explicit "about:blank" URL
			if (string.IsNullOrWhiteSpace(url))
				return false;

			// Check if URL is about:blank (case insensitive)
			return string.Equals(url.Trim(), "about:blank", StringComparison.OrdinalIgnoreCase);
		}

		static string GetValidUrl(string? url)
		{
			if (string.IsNullOrEmpty(url))
				return string.Empty;

			return url;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Disconnect();

			base.Dispose(disposing);
		}

		internal void Disconnect()
		{
			_handler.SetTarget(null);
		}
	}
}