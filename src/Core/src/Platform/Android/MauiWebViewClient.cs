using System;
using Android.Graphics;
using Android.Webkit;

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

		public override void OnPageStarted(WebView? view, string? url, Bitmap? favicon)
		{
			if (!_handler.TryGetTarget(out var handler) || handler.VirtualView == null || url == WebViewHandler.AssetBaseUrl)
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
			if (!_handler.TryGetTarget(out var handler) || handler.VirtualView == null || string.IsNullOrWhiteSpace(url) || url == WebViewHandler.AssetBaseUrl)
				return;

			bool navigate = _navigationResult != WebNavigationResult.Failure || !GetValidUrl(url).Equals(_lastUrlNavigatedCancel, StringComparison.OrdinalIgnoreCase);
			_lastUrlNavigatedCancel = _navigationResult == WebNavigationResult.Cancel ? url : null;

			if (navigate)
				handler.VirtualView.Navigated(handler.CurrentNavigationEvent, GetValidUrl(url), _navigationResult);

			handler.SyncPlatformCookiesToVirtualView(url);

			if (handler != null)
				handler.PlatformView.UpdateCanGoBackForward(handler.VirtualView);

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

		bool NavigatingCanceled(string? url) =>
			!_handler.TryGetTarget(out var handler) || handler.NavigatingCanceled(url);

		string GetValidUrl(string? url)
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