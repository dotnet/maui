#nullable disable
using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal class SwipeGestureHandler
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

			var result = false;
			foreach (SwipeGestureRecognizer swipeGesture in
					 view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
			{
				((ISwipeGestureController)swipeGesture).SendSwipe(view, PixelTranslation(x), PixelTranslation(y));
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