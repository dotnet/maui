using Microsoft.Maui.Graphics;
using NGestureDetector = Tizen.NUI.GestureDetector;
using NGestureStateType = Tizen.NUI.Gesture.StateType;
using NPinchGestureDetector = Tizen.NUI.PinchGestureDetector;

namespace Microsoft.Maui.Controls.Platform
{
	public class PinchGestureHandler : GestureHandler
	{
		double _pinchStartingScale = 1;
		public PinchGestureHandler(IGestureRecognizer recognizer) : base(recognizer)
		{
			NativeDetector.Detected += OnDetected;
		}

		new IPinchGestureController Recognizer => (IPinchGestureController)base.Recognizer;
		new NPinchGestureDetector NativeDetector => (NPinchGestureDetector)base.NativeDetector;

		protected override NGestureDetector CreateNativeDetector(IGestureRecognizer recognizer)
		{
			return new NPinchGestureDetector();
		}

		void OnDetected(object source, NPinchGestureDetector.DetectedEventArgs e)
		{
			if (e.PinchGesture.State == NGestureStateType.Started)
			{
				_pinchStartingScale = View!.Scale;
				var initialScalePoint = new Point(e.PinchGesture.LocalCenterPoint.X / NativeView!.Size.Width, e.PinchGesture.LocalCenterPoint.Y / NativeView.Size.Height);
				Recognizer.SendPinchStarted(View, initialScalePoint);
			}
			else if (e.PinchGesture.State == NGestureStateType.Continuing)
			{
				var currentScalePoint = new Point(e.PinchGesture.LocalCenterPoint.X / NativeView!.Size.Width, e.PinchGesture.LocalCenterPoint.Y / NativeView.Size.Height);
				var scale = 1 + (e.PinchGesture.Scale - 1) * _pinchStartingScale;
				Recognizer.SendPinch(View, scale, currentScalePoint);
			}
			else if (e.PinchGesture.State == NGestureStateType.Finished)
			{
				Recognizer.SendPinchEnded(View);
			}
			else if (e.PinchGesture.State == NGestureStateType.Cancelled)
			{
				Recognizer.SendPinchCanceled(View);
			}
		}
	}
}
