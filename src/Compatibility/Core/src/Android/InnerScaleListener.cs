using System;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal class InnerScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
	{
		Func<float, Point, bool> _pinchDelegate;
		Action _pinchEndedDelegate;
		readonly Func<double, double> _fromPixels;
		Func<Point, bool> _pinchStartedDelegate;

		public InnerScaleListener(PinchGestureHandler pinchGestureHandler, Func<double, double> fromPixels)
		{
			if (pinchGestureHandler == null)
			{
				throw new ArgumentNullException(nameof(pinchGestureHandler));
			}

			_pinchDelegate = pinchGestureHandler.OnPinch;
			_pinchStartedDelegate = pinchGestureHandler.OnPinchStarted;
			_pinchEndedDelegate = pinchGestureHandler.OnPinchEnded;
			_fromPixels = fromPixels;
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

			return _pinchDelegate(detector.ScaleFactor, new Point(_fromPixels(detector.FocusX), _fromPixels(detector.FocusY)));
		}

		public override bool OnScaleBegin(ScaleGestureDetector detector)
		{
			return _pinchStartedDelegate(new Point(_fromPixels(detector.FocusX), _fromPixels(detector.FocusY)));
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