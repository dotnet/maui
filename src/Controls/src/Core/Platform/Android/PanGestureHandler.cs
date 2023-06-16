#nullable disable
using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class PanGestureHandler
	{
		Func<double, double> PixelTranslation
		{
			get
			{
				return (input) =>
				{
					var context = GetView()?.Handler?.MauiContext?.Context;
					if (context == null)
						return 0;

					return context.FromPixels(input);
				};
			}
		}

		public PanGestureHandler(Func<View> getView)
		{
			GetView = getView;
		}

		Func<View> GetView { get; }

		public bool OnPan(float x, float y, float totalX, float totalY, int pointerCount)
		{
			View view = GetView();

			if (view == null)
				return false;

			if (!TouchInsideViewBounds(new Graphics.Point(x, y)))
			{
				OnPanComplete();
				return false;
			}

			var result = false;
			foreach (PanGestureRecognizer panGesture in
				view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>(g => g.TouchPoints == pointerCount))
			{
				((IPanGestureController)panGesture).SendPan(view, PixelTranslation(totalX), PixelTranslation(totalY), PanGestureRecognizer.CurrentId.Value);
				result = true;
			}

			return result;
		}

		public bool OnPanComplete()
		{
			View view = GetView();

			if (view == null)
				return false;

			var result = false;
			foreach (PanGestureRecognizer panGesture in view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>())
			{
				((IPanGestureController)panGesture).SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
				result = true;
			}
			PanGestureRecognizer.CurrentId.Increment();
			return result;
		}

		public bool OnPanStarted(int pointerCount)
		{
			View view = GetView();

			if (view == null)
				return false;

			var result = false;
			foreach (PanGestureRecognizer panGesture in
				view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>(g => g.TouchPoints == pointerCount))
			{
				((IPanGestureController)panGesture).SendPanStarted(view, PanGestureRecognizer.CurrentId.Value);
				result = true;
			}
			return result;
		}

		public bool HasAnyGestures()
		{
			var view = GetView();
			return view != null && view.GestureRecognizers.OfType<PanGestureRecognizer>().Any();
		}

		bool TouchInsideViewBounds(Graphics.Point touchPoint)
		{
			var view = GetView();

			if (view is null)
				return false;

			var bounds = view.Bounds;
			var density = DeviceDisplay.MainDisplayInfo.Density;

			var platformBounds = new Graphics.Rect(
				bounds.X * density,
				bounds.Y * density,
				bounds.Width * density,
				bounds.Height * density);

			if (platformBounds.Contains(touchPoint))
				return true;
			
			return false;
		}
	}
}