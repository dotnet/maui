// This behavior isn't lit up for WinUI because it's never been supported on WinUI, event in Xamarin.Forms
// The primary purpose of this API is for XF migration purposes. 
// Ideally users would use behavior that's more accessible forward and consistent with platform expectations.
#if ANDROID || IOS
using System;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		IDisposable? _watchingForTaps;
		WeakReference<IView>? _focusedView;

		static ContentPage? GetEnclosingPage(IView? view)
		{
			Element? element = view as Element;
			while (element is not null)
			{
				if (element is ContentPage contentPage)
					return contentPage;
				element = element.Parent;
			}
			return null;
		}

		bool FeatureEnabled
		{
			get
			{
				var page = GetEnclosingPage(FocusedView);
				return page is not null && page.HideSoftInputOnTapped && page.HasNavigatedTo;
			}
		}

		internal void UpdatePage(ContentPage page)
		{
			// HideSoftInputOnTapped (or HasNavigatedTo) changed on this page.
			// FeatureEnabled is computed on-demand from the currently focused view's
			// enclosing page, so we just need to re-evaluate the tap watcher in case
			// the change flips FeatureEnabled for the focused view.
			SetupHideSoftInputOnTapped();
		}

		internal IDisposable? UpdateFocusForView(IView _view)
		{
			// Update to new focused view
			if (_view.IsFocused)
			{
				DisconnectFromPlatform();
				_focusedView = new WeakReference<IView>(_view);
			}
			// If currently tracked view became unfocused then disconnect from it
			else if (_view == FocusedView)
			{
				DisconnectFromPlatform();
				_focusedView = null;
			}

			if (!FeatureEnabled)
			{
				DisconnectFromPlatform();
				return null;
			}

			if (_view is not VisualElement ve)
				return null;

			if (!_view.IsFocused)
				return null;

			DisconnectFromPlatform();

			// This view has been set as focused but it's not currently loaded
			var platformView = (_view.Handler as IPlatformViewHandler)?.PlatformView;
			if (platformView is null)
			{
				return null;
			}

			if (ve.Window is null)
			{
				// This means the xplat IsFocused value has lagged behind navigation events.
				// This might happen if navigated has fired on the incoming page but the
				// "LostFocus" event hasn't propagated from the previous one
				return null;
			}

			IDisposable? platformToken = SetupHideSoftInputOnTapped(platformView);

#if ANDROID
			var window = ve.Window;
			window.DispatchTouchEvent += OnWindowDispatchedTouch;
#endif
			_watchingForTaps = new ActionDisposable(() =>
			{
				platformToken?.Dispose();
				platformToken = null;
#if ANDROID
				window.DispatchTouchEvent -= OnWindowDispatchedTouch;
				window = null;
#endif
			});

			return _watchingForTaps;
		}

		void DisconnectFromPlatform()
		{
			_watchingForTaps?.Dispose();
			_watchingForTaps = null;
		}

		IView? FocusedView
		{
			get
			{
				if (_focusedView?.TryGetTarget(out IView? view) == true)
				{
					return view;
				}

				return null;
			}
		}
		internal void SetupHideSoftInputOnTapped()
		{
			if (FocusedView is not null)
			{
				UpdateFocusForView(FocusedView);
			}
		}
	}
}
#endif