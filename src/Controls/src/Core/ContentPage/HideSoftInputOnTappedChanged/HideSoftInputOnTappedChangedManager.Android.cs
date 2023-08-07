using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		internal event EventHandler<MotionEvent?>? DispatchTouchEvent;

		void OnDispatchTouch(object? sender, MotionEvent? e)
		{
			if (!_contentPage.HasNavigatedTo || e is null)
			{
				return;
			}

			if (_contentPage.HasNavigatedTo &&
				_contentPage.HideSoftInputOnTapped &&
				_contentPage.Handler is IPlatformViewHandler pvh &&
				pvh.MauiContext?.Context is not null)
			{
				var location = pvh.PlatformView.GetBoundingBox();
				var androidContext = pvh.MauiContext.Context;

				var point =
					new Point
					(
						androidContext.FromPixels(e.RawX),
						androidContext.FromPixels(e.RawY)
					);

				if (location.Contains(point))
					DispatchTouchEvent?.Invoke(this, e);
			}
		}

		internal void AddedToPlatformVisualTree()
		{
			if (_contentPage.Window is not null)
				_contentPage.Window.DispatchTouchEvent += OnDispatchTouch;
		}

		internal void RemovedFromPlatformVisualTree(IWindow? oldWindow)
		{
			if (oldWindow is Window window)
				window.DispatchTouchEvent -= OnDispatchTouch;
		}

		// This is called from InputViews as they are added to the visual tree
		internal IDisposable? SetupHideSoftInputOnTapped(AView? aView)
		{
			if (aView is SearchView sv)
				aView = sv.GetFirstChildOfType<EditText>();

			if (aView is AndroidX.AppCompat.Widget.SearchView svX)
				aView = svX.GetFirstChildOfType<EditText>();

			if (aView is null)
				return null;

			if (!_contentPage.HideSoftInputOnTapped)
				return null;

			var tracker = new TapWindowTracker(aView, this);
			return new ActionDisposable(() =>
			{
				tracker.Disconnect();
				tracker = null;
			});
		}
	}
}
