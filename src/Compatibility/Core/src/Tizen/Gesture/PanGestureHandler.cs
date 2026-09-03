using GestureStateType = Tizen.NUI.Gesture.StateType;
using NGestureDetector = Tizen.NUI.GestureDetector;
using PanGestureDetector = Tizen.NUI.PanGestureDetector;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
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

		new IPanGestureController Recognizer => base.Recognizer as IPanGestureController;
		new PanGestureDetector NativeDetector => base.NativeDetector as PanGestureDetector;

		protected override NGestureDetector CreateNativeDetector(IGestureRecognizer recognizer)
		{
			return new PanGestureDetector();
		}

		void OnDetected(object source, PanGestureDetector.DetectedEventArgs e)
		{
			if (e.PanGesture.State == GestureStateType.Started)
			{
				_gestureId++;
				_totalX = 0;
				_totalY = 0;
				Recognizer.SendPanStarted(View, _gestureId);
			}
			else if (e.PanGesture.State == GestureStateType.Continuing)
			{
				_totalX += e.PanGesture.Displacement.X;
				_totalY += e.PanGesture.Displacement.Y;
				Recognizer.SendPan(View, Forms.ConvertToScaledDP(_totalX), Forms.ConvertToScaledDP(_totalY), _gestureId);
			}
			else if (e.PanGesture.State == GestureStateType.Cancelled)
			{
				Recognizer.SendPanCanceled(View, _gestureId);
			}
			else if (e.PanGesture.State == GestureStateType.Finished)
			{
				Recognizer.SendPanCompleted(View, _gestureId);
			}
		}

	}
}