using System;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		internal event EventHandler<MotionEvent?>? DispatchTouchEvent;

		void OnWindowDispatchedTouch(object? sender, MotionEvent? e)
		{
			if (e is null)
			{
				return;
			}

			var page = GetEnclosingPage(FocusedView) ?? FocusedEnclosingPage;
			if (page is null ||
				!page.HasNavigatedTo ||
				!page.HideSoftInputOnTapped ||
				page.Handler is not IPlatformViewHandler pvh ||
				pvh.MauiContext?.Context is null)
			{
				return;
			}

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

		// This is called from InputViews as they are added to the visual tree
		IDisposable? SetupHideSoftInputOnTapped(AView aView)
		{
			if (aView is AViewGroup vg &&
				vg.GetFirstChildOfType<EditText>() is { } editText)
			{
				aView = editText;
			}

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
