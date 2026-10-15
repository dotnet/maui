using Microsoft.Maui.Controls;
#if IOS || MACCATALYST
using WebKit;
#elif WINDOWS
using Microsoft.Web.WebView2.Core;
#endif

namespace Maui.Controls.Sample.HybridWebApp;

sealed class DevelopmentPlatform(Action startingDocument) : IDisposable
{
#if IOS || MACCATALYST
	internal const string AppScheme = "app";
	WKWebView? _native;
	NavigationObserver? _navigation;
#elif ANDROID
	internal const string AppScheme = "https";
	global::Android.Webkit.WebView? _native;
#elif WINDOWS
	internal const string AppScheme = "https";
	CoreWebView2? _native;
#else
	internal const string AppScheme = "https";
#endif

	internal void Attach(WebViewInitializedEventArgs e, bool observeNavigation)
	{
		Dispose();
#if ANDROID
		_ = startingDocument;
#endif
#if IOS || MACCATALYST || ANDROID || WINDOWS
		_native = e.PlatformArgs?.Sender;
#endif
#if IOS || MACCATALYST
		if (observeNavigation && _native is { } native)
		{
			if (native.NavigationDelegate is not null)
				throw new InvalidOperationException("The Debug sample must not replace an existing navigation delegate.");
			_navigation = new NavigationObserver(startingDocument);
			native.NavigationDelegate = _navigation;
		}
#elif ANDROID
		// HTTPS app-origin content must be able to reach the loopback ws:// Vite HMR channel.
		if (observeNavigation && _native is { } native)
			native.Settings.MixedContentMode = global::Android.Webkit.MixedContentHandling.AlwaysAllow;
#elif WINDOWS
		if (observeNavigation && _native is { } native)
			native.NavigationStarting += NavigationStarting;
#endif
	}

	internal bool IsAttached(HybridWebView view)
	{
#if IOS || MACCATALYST || ANDROID
		return _native is not null && view.Handler?.PlatformView == _native;
#elif WINDOWS
		return _native is not null &&
			(view.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.WebView2)?.CoreWebView2 == _native;
#else
		return false;
#endif
	}

	internal string? Uri =>
#if IOS || MACCATALYST
		_native?.Url?.AbsoluteString;
#elif ANDROID
		_native?.Url;
#elif WINDOWS
		_native?.Source;
#else
		null;
#endif

	internal async Task<byte[]> SnapshotAsync()
	{
#if IOS || MACCATALYST
		var native = _native ?? throw new InvalidOperationException("WebView unavailable.");
		using var configuration = new WKSnapshotConfiguration { Rect = native.Bounds, AfterScreenUpdates = true };
		using var image = await native.TakeSnapshotAsync(configuration);
		using var png = image.AsPNG() ?? throw new InvalidOperationException("Snapshot PNG encoding failed.");
		return png.ToArray();
#elif ANDROID
		var native = _native ?? throw new InvalidOperationException("WebView unavailable.");
		using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(native.Width, native.Height, global::Android.Graphics.Bitmap.Config.Argb8888!)
			?? throw new InvalidOperationException("Snapshot bitmap allocation failed.");
		using var canvas = new global::Android.Graphics.Canvas(bitmap);
		native.Draw(canvas);
		using var stream = new MemoryStream();
		if (!await bitmap.CompressAsync(global::Android.Graphics.Bitmap.CompressFormat.Png!, 100, stream))
			throw new InvalidOperationException("Snapshot PNG encoding failed.");
		return stream.ToArray();
#elif WINDOWS
		var native = _native ?? throw new InvalidOperationException("WebView unavailable.");
		using var stream = new MemoryStream();
		await native.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
		return stream.ToArray();
#else
		await Task.CompletedTask;
		throw new NotSupportedException("Development inspection is available on Android, iOS, Mac Catalyst and Windows.");
#endif
	}

	public void Dispose()
	{
#if IOS || MACCATALYST
		if (_navigation is not null && _native?.NavigationDelegate == _navigation)
			_native.WeakNavigationDelegate = null;
		_navigation?.Dispose();
		_navigation = null;
#elif WINDOWS
		if (_native is not null)
			_native.NavigationStarting -= NavigationStarting;
#endif
#if IOS || MACCATALYST || ANDROID || WINDOWS
		_native = null;
#endif
	}

#if IOS || MACCATALYST
	sealed class NavigationObserver(Action startingDocument) : WKNavigationDelegate
	{
		public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
		{
			// Observe only main-frame navigation, not Vite's index.html?html-proxy source modules.
			if (navigationAction.TargetFrame?.MainFrame == true)
				startingDocument();
			decisionHandler(WKNavigationActionPolicy.Allow);
		}
	}
#elif WINDOWS
	void NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e) => startingDocument();
#endif
}
