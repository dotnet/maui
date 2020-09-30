using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal class PanGestureHandler
	{
		readonly Func<double, double> _pixelTranslation;

		public PanGestureHandler(Func<View> getView, Func<double, double> pixelTranslation)
		{
			_pixelTranslation = pixelTranslation;
			GetView = getView;
		}

		Func<View> GetView { get; }

		public bool OnPan(float x, float y, int pointerCount)
		{
			View view = GetView();

			if (view == null)
				return false;

			var result = false;
			foreach (PanGestureRecognizer panGesture in
				view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>(g => g.TouchPoints == pointerCount))
			{
				((IPanGestureController)panGesture).SendPan(view, _pixelTranslation(x), _pixelTranslation(y), Application.Current.PanGestureId);
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
				((IPanGestureController)panGesture).SendPanCompleted(view, Application.Current.PanGestureId);
				result = true;
			}
			Application.Current.PanGestureId++;
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
				((IPanGestureController)panGesture).SendPanStarted(view, Application.Current.PanGestureId);
				result = true;
			}
			return result;
		}

		public bool HasAnyGestures()
		{
			var view = GetView();
			return view != null && view.GestureRecognizers.OfType<PanGestureRecognizer>().Any();
		}
	}
}