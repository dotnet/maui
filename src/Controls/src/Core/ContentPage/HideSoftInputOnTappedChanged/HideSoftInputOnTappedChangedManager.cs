using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
#if ANDROID || IOS
		WeakReference<IView>? _virtualView;
#endif

		IDisposable? _watchingForTaps;
		readonly ContentPage _contentPage;

		public HideSoftInputOnTappedChangedManager(ContentPage contentPage)
		{
			_contentPage = contentPage;
		}

		internal void PageAddedToPlatformVisualTree()
		{
#if ANDROID
			if (_contentPage.Window is not null)
				_contentPage.Window.DispatchTouchEvent += OnWindowDispatchedTouch;
#endif

			SetupHideSoftInputOnTapped();
		}

		internal void PageRemovedFromPlatformVisualTree(IWindow? oldWindow)
		{
#if ANDROID
			if (oldWindow is Window window)
				window.DispatchTouchEvent -= OnWindowDispatchedTouch;
#endif

#if ANDROID || IOS
			_virtualView = null;
#endif

			_watchingForTaps?.Dispose();
			_watchingForTaps = null;
		}

		internal void PageHideSoftInputOnTappedPropertyChanged(
			ContentPage page, bool oldValue, bool newValue)
		{
			SetupHideSoftInputOnTapped();
		}

		internal IDisposable? SetCurrentlyFocusedView(IView _view)
		{
			_watchingForTaps?.Dispose();
			_watchingForTaps = null;

#if IOS || ANDROID
			_virtualView = new WeakReference<IView>(_view);

			if (_view is not VisualElement ve)
				return null;

			if (!ve.IsLoaded || !_view.IsFocused || ve.Window is null)
				return null;

			var contentPage =
				ve.FindParentOfType<ContentPage>();

			if (contentPage is null)
				return null;

			if (_view is not null)
			{
				_watchingForTaps = SetupHideSoftInputOnTapped((_view.Handler as IPlatformViewHandler)?.PlatformView);
			}
			else if (_virtualView?.TryGetTarget(out IView? view) == true)
			{
				_watchingForTaps = SetupHideSoftInputOnTapped((view.Handler as IPlatformViewHandler)?.PlatformView);
			}
#endif
			return _watchingForTaps;
		}

		internal void SetupHideSoftInputOnTapped()
		{
#if IOS || ANDROID
			if (_virtualView?.TryGetTarget(out IView? view) == true)
			{
				SetCurrentlyFocusedView(view);
			}
#endif
		}
	}
}
