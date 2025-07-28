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

			var transformedCoords = TransformSwipeCoordinatesWithRotation(x, y, view.Rotation);

			_totalX += PixelTranslation(transformedCoords.x);
			_totalY += PixelTranslation(transformedCoords.y);

			var result = false;
			foreach (SwipeGestureRecognizer swipeGesture in
					view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
			{
				((ISwipeGestureController)swipeGesture).SendSwipe(view, _totalX, _totalY);
				result = true;
			}

			return result;
		}

		(float x, float y) TransformSwipeCoordinatesWithRotation(float x, float y, double rotation)
		{
			var correctedX = x; 
			var correctedY = y;  

			if (Math.Abs(rotation) < 0.01)
			{
				return (correctedX, correctedY);
			}

			rotation = rotation % 360;
			if (rotation < 0) rotation += 360;

			var radians = rotation * Math.PI / 180.0;
			var cos = Math.Cos(radians);
			var sin = Math.Sin(radians);

			var transformedX = (float)(correctedX * cos - correctedY * sin);
			var transformedY = (float)(correctedX * sin + correctedY * cos);

			return (transformedX, transformedY);
		}

		public bool OnSwipeComplete()
		{
			View view = GetView();

			if (view == null)
				return false;

			var detected = false;
			foreach (SwipeGestureRecognizer swipeGesture in view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
			{
				var gestureDetected = ((ISwipeGestureController)swipeGesture).DetectSwipe(view, swipeGesture.Direction);
				if (gestureDetected)
				{
					detected = true;
				}
			}
			_totalX = 0;
			_totalY = 0;

			return detected;
		}

		public bool HasAnyGestures()
		{
			var view = GetView();
			return view != null && view.GestureRecognizers.OfType<SwipeGestureRecognizer>().Any();
		}
	}
}