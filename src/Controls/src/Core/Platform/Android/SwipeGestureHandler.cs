#nullable disable
using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal class SwipeGestureHandler
	{
		double _totalX, _totalY;

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

		public SwipeGestureHandler(Func<View> getView)
		{
			GetView = getView;
		}

		Func<View> GetView { get; }

		public bool OnSwipe(float x, float y)
		{
			View view = GetView();

			if (view == null)
				return false;

			if (view.Rotation != 0)
			{
				var transformedCoords = SwipeGestureExtensions.TransformSwipeCoordinatesWithRotation(x, y, view.Rotation);
				_totalX = PixelTranslation(transformedCoords.x);
				_totalY = PixelTranslation(transformedCoords.y);
			}
			else
			{
				_totalX = PixelTranslation(x);
				_totalY = PixelTranslation(y);
			}

			var result = false;
			foreach (SwipeGestureRecognizer swipeGesture in view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
			{
				((ISwipeGestureController)swipeGesture).SendSwipe(view, _totalX, _totalY);
				result = true;
			}

			return result;
		}

		public bool OnSwipeComplete()
		{
			View view = GetView();

			if (view == null)
				return false;

			foreach (SwipeGestureRecognizer swipeGesture in view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
			{
				var detected = ((ISwipeGestureController)swipeGesture).DetectSwipe(view, swipeGesture.Direction);
				if (detected)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasAnyGestures()
		{
			var view = GetView();
			return view != null && view.GestureRecognizers.OfType<SwipeGestureRecognizer>().Any();
		}
	}
}