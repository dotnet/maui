using Android.Webkit;
using Java.Interop;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Platform;

internal static class RefreshViewWebViewScrollCapture
{
	const string JavaScriptInterfaceName = "mauiRefreshViewHost";
	const int ScrollCaptureStateKey = 0x4D415549;

	const string ObserverScript =
		"""
		(function () {
			if (window.__mauiRefreshViewObserverInstalled) {
				return;
			}

			var host = window.mauiRefreshViewHost;
			if (!host || typeof host.setCanScrollUp !== 'function') {
				return;
			}

			window.__mauiRefreshViewObserverInstalled = true;

			function isScrollableElement(node) {
				if (!node || node.nodeType !== Node.ELEMENT_NODE) {
					return false;
				}

				var style = window.getComputedStyle(node);
				var overflowY = style ? style.overflowY : '';
				return (overflowY === 'auto' || overflowY === 'scroll' || overflowY === 'overlay') &&
					node.scrollHeight > node.clientHeight + 1;
			}

			function getScrollableElement(startNode) {
				for (var node = startNode; node && node.nodeType === Node.ELEMENT_NODE; node = node.parentElement) {
					if (isScrollableElement(node)) {
						return node;
					}
				}

				return document.scrollingElement || document.documentElement || document.body;
			}

			function getScrollTopForElement(element) {
				if (!element) {
					return 0;
				}

				if (element === document.body || element === document.documentElement || element === document.scrollingElement) {
					return window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop || 0;
				}

				return element.scrollTop || 0;
			}

			function report(target) {
				try {
					var scrollable = getScrollableElement(target);
					host.setCanScrollUp(getScrollTopForElement(scrollable) > 0);
				} catch (e) {
				}
			}

			document.addEventListener('touchstart', function (event) {
				report(event.target);
			}, true);

			document.addEventListener('touchmove', function (event) {
				report(event.target);
			}, true);

			report(document.body);
		})();
		""";

	internal static void Attach(WebView webView)
	{
		if (GetState(webView) is not null)
			return;

		var state = new ScrollCaptureState();
		webView.SetTag(ScrollCaptureStateKey, state);
		webView.AddJavascriptInterface(state, JavaScriptInterfaceName);
	}

	internal static void Detach(WebView? webView)
	{
		if (webView is null)
			return;

		if (GetState(webView) is not ScrollCaptureState state)
			return;

		webView.RemoveJavascriptInterface(JavaScriptInterfaceName);
		webView.SetTag(ScrollCaptureStateKey, null);
		state.Dispose();
	}

	internal static void Reset(WebView? webView)
	{
		if (GetState(webView) is ScrollCaptureState state)
			state.Reset();
	}

	internal static void InjectObserver(WebView? webView)
	{
		if (webView is null)
			return;

		webView.EvaluateJavascript(ObserverScript, null);
	}

	internal static bool IsAttached(WebView? webView) => GetState(webView) is not null;

	internal static bool TryGetCanScrollUp(WebView? webView, out bool canScrollUp)
	{
		if (webView is null)
		{
			canScrollUp = false;
			return false;
		}

		var nativeCanScrollUp = webView.CanScrollVertically(-1) || webView.ScrollY > 0;

		if (GetState(webView) is ScrollCaptureState state && state.HasReportedState)
		{
			canScrollUp = state.CanScrollUp || nativeCanScrollUp;
			return true;
		}

		if (nativeCanScrollUp)
		{
			canScrollUp = true;
			return true;
		}

		canScrollUp = false;
		return false;
	}

	static ScrollCaptureState? GetState(WebView? webView) =>
		webView?.GetTag(ScrollCaptureStateKey) as ScrollCaptureState;

	sealed class ScrollCaptureState : Java.Lang.Object
	{
		// These fields are written from the JavaBridge thread (via [JavascriptInterface])
		// and read from the UI thread, so they must be volatile to ensure visibility on ARM.
		volatile bool _canScrollUp;
		volatile bool _hasReportedState;

		internal bool CanScrollUp => _canScrollUp;

		internal bool HasReportedState => _hasReportedState;

		[JavascriptInterface]
		[RequiresUnreferencedCode("Java.Interop.Export uses dynamic features.")]
		[Export("setCanScrollUp")]
		public void SetCanScrollUp(bool canScrollUp)
		{
			_canScrollUp = canScrollUp;
			_hasReportedState = true;
		}

		internal void Reset()
		{
			_canScrollUp = false;
			_hasReportedState = false;
		}
	}
}
