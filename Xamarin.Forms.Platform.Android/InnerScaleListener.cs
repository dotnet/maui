using System;
using Android.Runtime;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class InnerScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
	{
		Func<float, Point, bool> _pinchDelegate;
		Action _pinchEndedDelegate;
		Func<Point, bool> _pinchStartedDelegate;

		public InnerScaleListener(Func<float, Point, bool> pinchDelegate, Func<Point, bool> pinchStarted, Action pinchEnded)
		{
			if (pinchDelegate == null)
				throw new ArgumentNullException("pinchDelegate");

			if (pinchStarted == null)
				throw new ArgumentNullException("pinchStarted");

			if (pinchEnded == null)
				throw new ArgumentNullException("pinchEnded");

			_pinchDelegate = pinchDelegate;
			_pinchStartedDelegate = pinchStarted;
			_pinchEndedDelegate = pinchEnded;
		}

		// This is needed because GestureRecognizer callbacks can be delayed several hundred milliseconds
		// which can result in the need to resurect this object if it has already been disposed. We dispose
		// eagerly to allow easier garbage collection of the renderer
		internal InnerScaleListener(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
		}

		public override bool OnScale(ScaleGestureDetector detector)
		{
			float cur = detector.CurrentSpan;
			float last = detector.PreviousSpan;

			if (Math.Abs(cur - last) < 10)
				return false;

			return _pinchDelegate(detector.ScaleFactor, new Point(Forms.Context.FromPixels(detector.FocusX), Forms.Context.FromPixels(detector.FocusY)));
		}

		public override bool OnScaleBegin(ScaleGestureDetector detector)
		{
			return _pinchStartedDelegate(new Point(Forms.Context.FromPixels(detector.FocusX), Forms.Context.FromPixels(detector.FocusY)));
		}

		public override void OnScaleEnd(ScaleGestureDetector detector)
		{
			_pinchEndedDelegate();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_pinchDelegate = null;
				_pinchStartedDelegate = null;
				_pinchEndedDelegate = null;
			}
			base.Dispose(disposing);
		}
	}
}