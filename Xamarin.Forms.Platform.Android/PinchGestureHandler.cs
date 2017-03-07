using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal class PinchGestureHandler
	{
		double _pinchStartingScale = 1;

		public PinchGestureHandler(Func<View> getView)
		{
			GetView = getView;
		}

		public bool IsPinchSupported
		{
			get
			{
				View view = GetView();
				return view != null && view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>().Any();
			}
		}

		Func<View> GetView { get; }

		// A View can have at most one pinch gesture, so we just need to look for one (or none)
		PinchGestureRecognizer PinchGesture => GetView()?.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>().FirstOrDefault();

		public bool OnPinch(float scale, Point scalePoint)
		{
			View view = GetView();

			if (view == null)
				return false;

			PinchGestureRecognizer pinchGesture = PinchGesture;
			if (pinchGesture == null)
				return true;

			var scalePointTransformed = new Point(scalePoint.X / view.Width, scalePoint.Y / view.Height);
			((IPinchGestureController)pinchGesture).SendPinch(view, 1 + (scale - 1) * _pinchStartingScale, scalePointTransformed);

			return true;
		}

		public void OnPinchEnded()
		{
			View view = GetView();

			if (view == null)
				return;

			PinchGestureRecognizer pinchGesture = PinchGesture;
			((IPinchGestureController)pinchGesture)?.SendPinchEnded(view);
		}

		public bool OnPinchStarted(Point scalePoint)
		{
			View view = GetView();

			if (view == null)
				return false;

			PinchGestureRecognizer pinchGesture = PinchGesture;
			if (pinchGesture == null)
				return false;

			_pinchStartingScale = view.Scale;

			var scalePointTransformed = new Point(scalePoint.X / view.Width, scalePoint.Y / view.Height);

			((IPinchGestureController)pinchGesture).SendPinchStarted(view, scalePointTransformed);
			return true;
		}
	}
}