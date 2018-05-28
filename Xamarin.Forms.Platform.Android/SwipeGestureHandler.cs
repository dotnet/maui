using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal class SwipeGestureHandler
	{
		readonly Func<double, double> _pixelTranslation;

		public SwipeGestureHandler(Func<View> getView, Func<double, double> pixelTranslation)
		{
			_pixelTranslation = pixelTranslation;
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
				((ISwipeGestureController)swipeGesture).SendSwipe(view, _pixelTranslation(x), _pixelTranslation(y));
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