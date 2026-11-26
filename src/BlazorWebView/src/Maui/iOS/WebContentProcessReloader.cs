using System;
using Foundation;
using Microsoft.Maui.Platform;
using UIKit;
using WebKit;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	// Handling webViewWebContentProcessDidTerminate: turns off the reload WebKit does on its own when the web content
	// process ends, so this does the same reload instead, following WebPageProxy::notifyClientOfProcessTermination,
	// maybeReloadAfterCrashWhenVisible and tryReloadAfterProcessTermination in
	// https://github.com/WebKit/WebKit/blob/main/Source/WebKit/UIProcess/WebPageProxy.cpp
	internal sealed class WebContentProcessReloader : IDisposable
	{
		// WebKit's resetRecentCrashCountDelay: a reload stays recent until a page has been loaded for this long.
		internal static readonly TimeSpan RecentReloadInterval = TimeSpan.FromSeconds(30);

		private readonly BlazorWebViewHandler _handler;
		private readonly WKWebView _webView;

		private bool _hasReloadedRecently;
		private long? _lastNavigationFinishedTimestamp;

		// WebKit's pending API request: the page last asked for with LoadRequest, until its navigation starts.
		private string? _requestedPageUrl;
		private NSObject? _sceneWillEnterForegroundObserver;
		private IDisposable? _movedToWindowSubscription;
		private bool _isDisposed;

		public WebContentProcessReloader(BlazorWebViewHandler handler, WKWebView webView)
		{
			_handler = handler;
			_webView = webView;
		}

		private bool IsWaitingUntilVisible => _sceneWillEnterForegroundObserver is not null;

		public void ProcessTerminated()
		{
			if (_isDisposed)
			{
				return;
			}

			if (IsVisible())
			{
				TryReload();
				return;
			}

			// WebKit also waits until the page is visible, instead of starting a new web content process for a page
			// nobody sees.
			_handler.Logger.DeferringReloadAfterWebContentProcessTermination();
			_sceneWillEnterForegroundObserver ??= NSNotificationCenter.DefaultCenter.AddObserver(
				UIScene.WillEnterForegroundNotification, OnSceneWillEnterForeground);
			WaitForWindowIfNeeded();
		}

		public void PageRequested(NSUrl url)
		{
			_requestedPageUrl = url.AbsoluteString;
		}

		public void NavigationStarted()
		{
			// Like WebKit, a new navigation replaces the pending reload: it starts a new web content process itself.
			StopWaitingUntilVisible();
			_requestedPageUrl = null;
		}

		public void NavigationFinished()
		{
			ForgetRecentReloadIfExpired();
			_lastNavigationFinishedTimestamp = _handler.TimeProvider.GetTimestamp();
		}

		public void Dispose()
		{
			_isDisposed = true;
			StopWaitingUntilVisible();
		}

		// WebKit's PageClientImpl::isViewVisible on iOS: in a window whose scene is not in the background.
		private bool IsVisible()
		{
			var window = _webView.Window;
			if (window is null)
			{
				return false;
			}

			var scene = window.WindowScene;
			if (scene is null)
			{
				return true;
			}

			return scene.ActivationState is not (UISceneActivationState.Background or UISceneActivationState.Unattached);
		}

		private void WaitForWindowIfNeeded()
		{
			if (_webView.Window is not null || _movedToWindowSubscription is not null)
			{
				return;
			}

			_movedToWindowSubscription = _webView.OnLoaded(OnMovedToWindow);
		}

		private void OnMovedToWindow()
		{
			// OnLoaded removes its own observers before calling back.
			_movedToWindowSubscription = null;

			// In a window of a background scene, OnSceneWillEnterForeground reloads once the scene comes back.
			if (IsWaitingUntilVisible && IsVisible())
			{
				ReloadNowThatItIsVisible();
			}
		}

		private void OnSceneWillEnterForeground(NSNotification notification)
		{
			var window = _webView.Window;
			if (window is null)
			{
				WaitForWindowIfNeeded();
				return;
			}

			// The scene is still in the background while this is posted, so its state cannot be checked yet.
			if (ReferenceEquals(notification.Object, window.WindowScene))
			{
				ReloadNowThatItIsVisible();
			}
		}

		private void ReloadNowThatItIsVisible()
		{
			StopWaitingUntilVisible();
			TryReload();
		}

		private void StopWaitingUntilVisible()
		{
			if (_sceneWillEnterForegroundObserver is not null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_sceneWillEnterForegroundObserver);
				_sceneWillEnterForegroundObserver = null;
			}

			if (_movedToWindowSubscription is not null)
			{
				_movedToWindowSubscription.Dispose();
				_movedToWindowSubscription = null;
			}
		}

		private void TryReload()
		{
			ForgetRecentReloadIfExpired();

			// WebKit stops its timer that forgets the last reload here, and starts it again only once a page has loaded.
			_lastNavigationFinishedTimestamp = null;

			if (_hasReloadedRecently)
			{
				// WebKit skips one reload and then counts from zero again, so a later termination reloads the page again.
				_hasReloadedRecently = false;
				_handler.Logger.NotReloadingAfterWebContentProcessTermination(RecentReloadInterval);
				return;
			}

			_hasReloadedRecently = true;
			_handler.Logger.ReloadingAfterWebContentProcessTermination();

			// Like WebKit, a page asked for whose navigation had not started yet is loaded again. By the time WebKit
			// reports the termination it has already cleared the web view's URL for that page, so it is remembered here.
			if (_requestedPageUrl is not null)
			{
				using var requestedUrl = new NSUrl(_requestedPageUrl);
				using var requestedPageRequest = new NSUrlRequest(requestedUrl);
				_webView.LoadRequest(requestedPageRequest);
				return;
			}

			// The process ended before the first page was committed, so there is no page to reload. The page that was
			// being loaded is still the web view's URL, and WebKit loads that one again too.
			var url = _webView.BackForwardList.CurrentItem is null ? _webView.Url : null;
			if (url is not null)
			{
				using var request = new NSUrlRequest(url);
				_webView.LoadRequest(request);
				return;
			}

			_webView.Reload();
		}

		private void ForgetRecentReloadIfExpired()
		{
			if (_lastNavigationFinishedTimestamp is not null
				&& _handler.TimeProvider.GetElapsedTime(_lastNavigationFinishedTimestamp.Value) >= RecentReloadInterval)
			{
				_hasReloadedRecently = false;
			}
		}
	}
}
