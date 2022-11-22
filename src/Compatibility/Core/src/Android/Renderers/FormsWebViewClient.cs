using System;
using System.ComponentModel;
using System.Net;
using Android.Graphics;
using Android.Runtime;
using Android.Webkit;
using WView = Android.Webkit.WebView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete]
	public class FormsWebViewClient : WebViewClient
	{
		WebNavigationResult _navigationResult = WebNavigationResult.Success;
		WebViewRenderer _renderer;
		string _lastUrlNavigatedCancel;

		public FormsWebViewClient(WebViewRenderer renderer)
			=> _renderer = renderer ?? throw new ArgumentNullException("renderer");

		protected FormsWebViewClient(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		bool SendNavigatingCanceled(string url) => _renderer?.SendNavigatingCanceled(url) ?? true;

		[Obsolete("ShouldOverrideUrlLoading(view,url) is obsolete as of version 4.0.0. This method was deprecated in API level 24.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		// api 19-23
#pragma warning disable 809
		public override bool ShouldOverrideUrlLoading(WView view, string url) => SendNavigatingCanceled(url);
#pragma warning restore 809

		[PortHandler]
		// api 24+
		public override bool ShouldOverrideUrlLoading(WView view, IWebResourceRequest request)
			=> SendNavigatingCanceled(request?.Url?.ToString());

		[PortHandler]
		public override void OnPageStarted(WView view, string url, Bitmap favicon)
		{
			if (_renderer?.Element == null || string.IsNullOrWhiteSpace(url) || url == WebViewRenderer.AssetBaseUrl)
				return;

			_renderer.SyncNativeCookiesToElement(url);
			var cancel = false;
			if (!url.Equals(_renderer.UrlCanceled, StringComparison.OrdinalIgnoreCase))
				cancel = SendNavigatingCanceled(url);
			_renderer.UrlCanceled = null;

			if (cancel)
			{
				_navigationResult = WebNavigationResult.Cancel;
				view.StopLoading();
			}
			else
			{
				_navigationResult = WebNavigationResult.Success;
				base.OnPageStarted(view, url, favicon);
			}
		}

		[PortHandler("Partially ported")]
		public override void OnPageFinished(WView view, string url)
		{
			if (_renderer?.Element == null || url == WebViewRenderer.AssetBaseUrl)
				return;

			var source = new UrlWebViewSource { Url = url };
			_renderer.IgnoreSourceChanges = true;
			_renderer.ElementController.SetValueFromRenderer(WebView.SourceProperty, source);
			_renderer.IgnoreSourceChanges = false;

			bool navigate = _navigationResult == WebNavigationResult.Failure ? !url.Equals(_lastUrlNavigatedCancel, StringComparison.OrdinalIgnoreCase) : true;
			_lastUrlNavigatedCancel = _navigationResult == WebNavigationResult.Cancel ? url : null;

			if (navigate)
			{
				var args = new WebNavigatedEventArgs(_renderer.GetCurrentWebNavigationEvent(), source, url, _navigationResult);
				_renderer.SyncNativeCookiesToElement(url);
				_renderer.ElementController.SendNavigated(args);
			}

			_renderer.UpdateCanGoBackForward();

			base.OnPageFinished(view, url);
		}

		[Obsolete("OnReceivedError is obsolete as of version 2.3.0. This method was deprecated in API level 23.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable 809
		public override void OnReceivedError(WView view, ClientError errorCode, string description, string failingUrl)
#pragma warning restore 809
		{
			if (failingUrl == _renderer?.Control.Url)
			{
				_navigationResult = WebNavigationResult.Failure;
				if (errorCode == ClientError.Timeout)
					_navigationResult = WebNavigationResult.Timeout;
			}
#pragma warning disable 618
#pragma warning disable CA1416, CA1422 // Validate platform compatibility
			base.OnReceivedError(view, errorCode, description, failingUrl);
#pragma warning restore CA1416, CA1422 // Validate platform compatibility
#pragma warning restore 618
		}

		[PortHandler]
		[System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
		public override void OnReceivedError(WView view, IWebResourceRequest request, WebResourceError error)
		{
			if (request.Url.ToString() == _renderer?.Control.Url)
			{
				_navigationResult = WebNavigationResult.Failure;
				if (error.ErrorCode == ClientError.Timeout)
					_navigationResult = WebNavigationResult.Timeout;
			}
			base.OnReceivedError(view, request, error);
		}

		[PortHandler]
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				_renderer = null;
		}
	}
}
