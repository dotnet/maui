using System;
using System.Runtime.Versioning;
using Android.Graphics;
using Android.Webkit;

namespace Microsoft.Maui.Platform
{
	public class MauiWebViewClient : WebViewClient
	{
		WebNavigationResult _navigationResult;
		WebViewHandler? _handler;
		string? _lastUrlNavigatedCancel;

		public MauiWebViewClient(WebViewHandler handler)
		{
			_handler = handler ?? throw new ArgumentNullException("handler");

			_navigationResult = WebNavigationResult.Success;
		}
		public override bool ShouldOverrideUrlLoading(WebView? view, IWebResourceRequest? request)
			=> NavigatingCanceled(request?.Url?.ToString());

		public override void OnPageStarted(WebView? view, string? url, Bitmap? favicon)
		{
			if (_handler?.VirtualView == null || url == WebViewHandler.AssetBaseUrl)
				return;

			// TODO: Sync Cookies

			var cancel = false;

			if (!GetValidUrl(url).Equals(_handler.UrlCanceled, StringComparison.OrdinalIgnoreCase))
				cancel = NavigatingCanceled(url);

			_handler.UrlCanceled = null;

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
			if (_handler?.VirtualView == null || string.IsNullOrWhiteSpace(url) || url == WebViewHandler.AssetBaseUrl)
				return;

			bool navigate = _navigationResult != WebNavigationResult.Failure || !GetValidUrl(url).Equals(_lastUrlNavigatedCancel, StringComparison.OrdinalIgnoreCase);
			_lastUrlNavigatedCancel = _navigationResult == WebNavigationResult.Cancel ? url : null;

			if (navigate)
				_handler.VirtualView.Navigated(_handler.CurrentNavigationEvent, GetValidUrl(url), _navigationResult);

			_handler.SyncPlatformCookiesToVirtualView(url);

			if (_handler != null)
				_handler.PlatformView.UpdateCanGoBackForward(_handler.VirtualView);

			base.OnPageFinished(view, url);
		}

		[SupportedOSPlatform("android23.0")]
		public override void OnReceivedError(WebView? view, IWebResourceRequest? request, WebResourceError? error)
		{
			if (request != null && request.Url?.ToString() == _handler?.PlatformView.Url)
			{
				_navigationResult = WebNavigationResult.Failure;

				if (error?.ErrorCode == ClientError.Timeout)
					_navigationResult = WebNavigationResult.Timeout;
			}

			base.OnReceivedError(view, request, error);
		}

		bool NavigatingCanceled(string? url) => _handler?.NavigatingCanceled(url) ?? true;

		string GetValidUrl(string? url)
		{
			if (string.IsNullOrEmpty(url))
				return string.Empty;

			return url;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_handler = null;

			base.Dispose(disposing);
		}
	}
}