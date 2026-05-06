using Android.Webkit;
using Java.Interop;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Platform;

internal static class RefreshViewWebViewScrollCapture
{
	const string JavaScriptInterfaceName = "mauiRefreshViewHost";
	const int ScrollCaptureStateKey = 0x4D415549;

	// Observer JS script — replaces static boolean guard (__mauiRefreshViewObserverInstalled) with
	// dynamic window.mauiRefreshViewHost lookup inside report(). Fixes scroll tracking after Shell tab-switch.
	//
	// After a Shell tab-switch the WebView is detached (Detach removes the old bridge) then re-attached
	// (Attach adds a fresh ScrollCaptureState, InjectObserver re-runs this script). A static guard would
	// prevent re-injection, so the new bridge object would never receive callbacks, silently breaking
	// pull-to-refresh protection until the next full page reload.
	//
	// Named global JS listener vars (window.__mauiTouchStartHandler/MoveHandler) so removeEventListener
	// works on re-inject, preventing listener stacking across Detach/re-Attach cycles.
	const string ObserverScript =
		"""
		(function () {
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
					var host = window.mauiRefreshViewHost;
					if (!host || typeof host.setCanScrollUp !== 'function') {
						return;
					}
					var scrollable = getScrollableElement(target);
					host.setCanScrollUp(getScrollTopForElement(scrollable) > 0);
				} catch (e) {
				}
			}

			// Remove any previously installed listeners to prevent accumulation
			// after Shell tab-switch (detach + re-attach without page reload).
			if (window.__mauiTouchStartHandler) {
				document.removeEventListener('touchstart', window.__mauiTouchStartHandler, true);
				document.removeEventListener('touchmove',  window.__mauiTouchMoveHandler,  true);
			}

			var touchStartHandler = function (event) { report(event.target); };
			var touchMoveHandler  = function (event) { report(event.target); };
			window.__mauiTouchStartHandler = touchStartHandler;
			window.__mauiTouchMoveHandler  = touchMoveHandler;

			document.addEventListener('touchstart', touchStartHandler, true);
			document.addEventListener('touchmove',  touchMoveHandler,  true);

			report(document.body);
		})();
		""";

	internal static void Attach(WebView webView)
	{
		if (GetState(webView) is not null)
		{
			return;
		}

		var state = new ScrollCaptureState();
		webView.SetTag(ScrollCaptureStateKey, state);
		webView.AddJavascriptInterface(state, JavaScriptInterfaceName);
	}

	internal static void Detach(WebView? webView)
	{
		if (webView is null)
		{
			return;
		}

		if (GetState(webView) is not ScrollCaptureState state)
		{
			return;
		}

		// Mark detached BEFORE removing the interface so any in-flight JNI
		// callbacks to SetCanScrollUp become no-ops instead of accessing a
		// disposed object.  RemoveJavascriptInterface is async from V8's
		// perspective — the JS bridge can still fire after this call returns.
		state.MarkDetached();
		webView.RemoveJavascriptInterface(JavaScriptInterfaceName);
		webView.SetTag(ScrollCaptureStateKey, null);
		// Do NOT call state.Dispose() here — V8 may still hold a reference to
		// the state object via the JS bridge.  The GC will collect it once V8
		// releases its last reference.
	}

	internal static void Reset(WebView? webView)
	{
		if (GetState(webView) is ScrollCaptureState state)
		{
			state.Reset();
		}
	}

	internal static void InjectObserver(WebView? webView)
	{
		if (webView is null)
		{
			return;
		}

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

	// Returns the cached ScrollCaptureState for the given WebView so callers on the
	// UI thread can read CanScrollUp (a volatile bool) directly without any JNI overhead.
	// Returns null when the WebView is not inside a RefreshView.
	internal static ScrollCaptureState? GetAttachedState(WebView? webView) => GetState(webView);

	internal sealed class ScrollCaptureState : Java.Lang.Object
	{
		// These fields are written from the JavaBridge thread (via [JavascriptInterface])
		// and read from the UI thread, so they must be volatile to ensure visibility on ARM.
		volatile bool _canScrollUp;
		volatile bool _hasReportedState;
		// Set before RemoveJavascriptInterface so any in-flight JNI callbacks become
		// no-ops rather than accessing a disposed object.
		volatile bool _detached;

		internal bool CanScrollUp => _canScrollUp;

		internal bool HasReportedState => _hasReportedState;

		[JavascriptInterface]
		[RequiresUnreferencedCode("Java.Interop.Export uses dynamic features.")]
		[Export("setCanScrollUp")]
		public void SetCanScrollUp(bool canScrollUp)
		{
			if (_detached)
			{
				return;
			}
			_canScrollUp = canScrollUp;
			_hasReportedState = true;
		}

		internal void MarkDetached() => _detached = true;

		internal void Reset()
		{
			_canScrollUp = false;
			_hasReportedState = false;
		}
	}
}
