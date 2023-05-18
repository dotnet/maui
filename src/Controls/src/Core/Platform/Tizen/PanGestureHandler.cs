using NGestureDetector = Tizen.NUI.GestureDetector;
using NGestureStateType = Tizen.NUI.Gesture.StateType;
using NPanGestureDetector = Tizen.NUI.PanGestureDetector;

namespace Microsoft.Maui.Controls.Platform
{
	public class PanGestureHandler : GestureHandler
	{
		int _gestureId = 0;

		double _totalX;
		double _totalY;

		public PanGestureHandler(IGestureRecognizer recognizer) : base(recognizer)
		{
			NativeDetector.Detected += OnDetected;
		}

		new IPanGestureController Recognizer => (IPanGestureController)base.Recognizer;
		new NPanGestureDetector NativeDetector => (NPanGestureDetector)base.NativeDetector;

		protected override NGestureDetector CreateNativeDetector(IGestureRecognizer recognizer)
		{
			return new NPanGestureDetector();
		}

		void OnDetected(object source, NPanGestureDetector.DetectedEventArgs e)
		{
			e.Handled = false;
			if (e.PanGesture.State == NGestureStateType.Started)
			{
				_gestureId++;
				_totalX = 0;
				_totalY = 0;
				Recognizer.SendPanStarted(View, _gestureId);
			}
			else if (e.PanGesture.State == NGestureStateType.Continuing)
			{
				_totalX += e.PanGesture.Displacement.X;
				_totalY += e.PanGesture.Displacement.Y;
				Recognizer.SendPan(View, _totalX.ToScaledDP(), _totalY.ToScaledDP(), _gestureId);
			}
			else if (e.PanGesture.State == NGestureStateType.Cancelled)
			{
				Recognizer.SendPanCanceled(View, _gestureId);
			}
			else if (e.PanGesture.State == NGestureStateType.Finished)
			{
				Recognizer.SendPanCompleted(View, _gestureId);
			}
		}
	}
}