using System;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		internal event EventHandler<MotionEvent?>? DispatchTouchEvent;

		void OnWindowDispatchedTouch(object? sender, MotionEvent? e)
		{
			if (!FeatureEnabled || e is null)
			{
				return;
			}

			foreach (var page in _contentPages)
			{
				if (page.HasNavigatedTo &&
					page.HideSoftInputOnTapped &&
					page.Handler is IPlatformViewHandler pvh &&
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
		}

		// This is called from InputViews as they are added to the visual tree
		IDisposable? SetupHideSoftInputOnTapped(AView aView)
		{
			if (aView is SearchView sv &&
				sv.GetFirstChildOfType<EditText>() is EditText editText)
			{
				aView = editText;
			}

			if (aView is AndroidX.AppCompat.Widget.SearchView svX &&
				svX.GetFirstChildOfType<EditText>() is EditText editTextX)
			{
				aView = editTextX;
			}

			if (aView is null)
				return null;

			if (!FeatureEnabled)
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
